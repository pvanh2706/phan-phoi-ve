// Kill process PpvRecon.Api "mo coi" dang chiem cong 5000 (chay tu ngoai systemd),
// roi restart lai service ppv-recon de code moi nhat duoc nap.

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
        .on('data', (d) => { out += d; process.stdout.write(d); })
        .stderr.on('data', (d) => { errOut += d; process.stderr.write(d); });
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

  console.log('[1/4] Dung service systemd hien tai (dang crash-loop)...');
  await sudoRun(conn, 'systemctl stop ppv-recon').catch((e) => console.log('  (bo qua) ' + e.message));

  console.log('[2/4] Kill process PpvRecon.Api mo coi dang chiem cong 5000...');
  await run(conn, "pid=$(ss -ltnp 2>/dev/null | grep ':5000 ' | grep -oE 'pid=[0-9]+' | head -1 | cut -d= -f2); if [ -n \"$pid\" ]; then kill \"$pid\"; sleep 2; kill -0 \"$pid\" 2>/dev/null && kill -9 \"$pid\" || true; echo \"Da kill PID $pid\"; else echo 'Khong tim thay process nao dang chiem cong 5000'; fi");

  console.log('[3/4] Khoi dong lai service ppv-recon...');
  await sudoRun(conn, 'systemctl start ppv-recon');

  console.log('[4/4] Kiem tra health check va asset hash...');
  let healthy = false;
  for (let i = 0; i < 8 && !healthy; i++) {
    await new Promise((r) => setTimeout(r, 2000));
    try {
      await run(conn, 'curl -sf http://127.0.0.1:5000/health');
      healthy = true;
    } catch (e) {
      console.log(`  ... chua san sang, thu lai (${i + 1}/8)`);
    }
  }

  if (healthy) {
    console.log('[OK] Service da chay va healthy.');
    await run(conn, "curl -s http://127.0.0.1:5000/ | grep -oE '/assets/index-[A-Za-z0-9]+\\.(js|css)'");
  } else {
    console.error('[CANH BAO] Service khong healthy. Kiem tra: sudo journalctl -u ppv-recon -n 100');
  }

  conn.end();
  if (!healthy) process.exit(1);
}

main().catch((err) => {
  console.error('\n[THAT BAI]', err.message || err);
  process.exit(1);
});
