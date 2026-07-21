// Don dep moi process PpvRecon.Api "mo coi" (khong phai process dang duoc systemd quan ly),
// de tranh loi cong 5000 bi chiem giu boi ban cu sau khi deploy (xem deploy/fix-stale-process.js).
// An toan: neu khong co process mo coi nao, script khong lam gi ca.

const { Client } = require('ssh2');

const host = process.env.SERVER_HOST;
const username = process.env.SERVER_USER;
const password = process.env.SERVER_PASS;

if (!host || !username || !password) {
  console.error('[LOI] Thieu SERVER_HOST / SERVER_USER / SERVER_PASS. Kiem tra deploy/config.bat');
  process.exit(1);
}

function run(conn, cmd) {
  return new Promise((resolve, reject) => {
    conn.exec(cmd, { pty: false }, (err, stream) => {
      if (err) return reject(err);
      let out = '';
      let errOut = '';
      stream
        .on('close', (code) => {
          if (code !== 0) reject(new Error(`Lenh loi (exit ${code}): ${cmd}\n${errOut || out}`));
          else resolve(out);
        })
        .on('data', (d) => { out += d; })
        .stderr.on('data', (d) => { errOut += d; });
    });
  });
}

function sudoRun(conn, cmd) {
  const escapedPass = password.replace(/'/g, `'\\''`);
  return run(conn, `echo '${escapedPass}' | sudo -S -p '' ${cmd}`);
}

async function main() {
  const conn = new Client();
  await new Promise((resolve, reject) => {
    conn.on('ready', resolve).on('error', reject).connect({
      host, port: 22, username, password, readyTimeout: 20000, tryKeyboard: true,
    });
    conn.on('keyboard-interactive', (_n, _i, _l, _prompts, finish) => finish([password]));
  });
  console.log(`[OK] Da ket noi SSH toi ${host}`);

  const mainPidOut = (await run(conn, 'systemctl show ppv-recon -p MainPID --value')).trim();
  const mainPid = mainPidOut || '0';
  console.log(`[INFO] systemd MainPID hien tai: ${mainPid}`);

  const allPids = (await run(conn, "pgrep -f '/PpvRecon.Api$' || true")).trim().split(/\s+/).filter(Boolean);
  console.log(`[INFO] Tat ca PID PpvRecon.Api dang chay: ${allPids.join(', ') || '(khong co)'}`);

  const orphans = allPids.filter((pid) => pid !== mainPid);

  if (orphans.length === 0) {
    console.log('[OK] Khong co process mo coi nao. Khong can don dep.');
  } else {
    console.log(`[CANH BAO] Phat hien ${orphans.length} process mo coi: ${orphans.join(', ')}. Dang kill...`);
    for (const pid of orphans) {
      await run(conn, `kill ${pid} 2>/dev/null; sleep 1; kill -0 ${pid} 2>/dev/null && kill -9 ${pid} || true`);
      console.log(`  Da kill PID ${pid}`);
    }
  }

  conn.end();
}

main().catch((err) => {
  console.error('\n[THAT BAI]', err.message || err);
  process.exit(1);
});
