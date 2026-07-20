// Upload goi build (.tar.gz) len server va trien khai theo mo hinh:
//   /opt/ppv/shared/App_Data/...   -> du lieu SQLite, KHONG bao gio bi ghi de
//   /opt/ppv/shared/.env           -> Jwt SigningKey, tao 1 lan duy nhat
//   /opt/ppv/releases/rel-<ts>/    -> moi lan deploy la 1 thu muc moi
//   /opt/ppv/current -> symlink toi release moi nhat (chuyen atomically)
//
// Script nay idempotent: chay lai nhieu lan khong lam hong server.

const { Client } = require('ssh2');
const fs = require('fs');
const path = require('path');
const os = require('os');

const host = process.env.SERVER_HOST;
const username = process.env.SERVER_USER;
const password = process.env.SERVER_PASS;

if (!host || !username || !password) {
  console.error('[LOI] Thieu SERVER_HOST / SERVER_USER / SERVER_PASS. Kiem tra deploy/config.bat');
  process.exit(1);
}

const localTarball = path.join(os.tmpdir(), 'ppv-release.tar.gz');
const remoteTarball = '/tmp/ppv-release.tar.gz';
const releaseId = 'rel-' + Date.now();
const releasePath = `/opt/ppv/releases/${releaseId}`;
const SQLITE_SYSTEM_LIB = '/usr/lib/x86_64-linux-gnu/libsqlite3.so.0';

const SERVICE_UNIT = `[Unit]
Description=PPV Recon API
After=network.target

[Service]
Type=simple
WorkingDirectory=/opt/ppv/current
ExecStart=/opt/ppv/current/PpvRecon.Api
Restart=on-failure
RestartSec=5
User=${username}
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false
EnvironmentFile=/opt/ppv/shared/.env

[Install]
WantedBy=multi-user.target
`;

function run(conn, cmd) {
  return new Promise((resolve, reject) => {
    conn.exec(cmd, { pty: false }, (err, stream) => {
      if (err) return reject(err);
      let out = '';
      let errOut = '';
      stream
        .on('close', (code) => {
          if (code !== 0) {
            reject(new Error(`Lenh loi (exit ${code}): ${cmd}\n${errOut || out}`));
          } else {
            resolve(out);
          }
        })
        .on('data', (d) => {
          out += d;
          process.stdout.write(d);
        })
        .stderr.on('data', (d) => {
          errOut += d;
          process.stderr.write(d);
        });
    });
  });
}

function sudoRun(conn, cmd) {
  const escapedPass = password.replace(/'/g, `'\\''`);
  return run(conn, `echo '${escapedPass}' | sudo -S -p '' ${cmd}`);
}

function uploadFile(sftp, localPath, remotePath) {
  return new Promise((resolve, reject) => {
    // "warm up" call - can vi mot so ban sshd tra loi "No such file"
    // neu OPEN duoc goi ngay sau khi phien SFTP vua mo (van de quan sat duoc thuc te).
    sftp.realpath('.', () => {
      sftp.open(remotePath, 'w', (err, handle) => {
        if (err) return reject(err);
        const data = fs.readFileSync(localPath);
        const CHUNK = 256 * 1024;
        let offset = 0;
        (function next() {
          if (offset >= data.length) {
            sftp.close(handle, () => resolve());
            return;
          }
          const end = Math.min(offset + CHUNK, data.length);
          const chunk = data.subarray(offset, end);
          sftp.write(handle, chunk, 0, chunk.length, offset, (werr) => {
            if (werr) return reject(werr);
            offset = end;
            next();
          });
        })();
      });
    });
  });
}

async function main() {
  if (!fs.existsSync(localTarball)) {
    throw new Error(`Khong tim thay goi build: ${localTarball}`);
  }

  const conn = new Client();
  await new Promise((resolve, reject) => {
    conn.on('ready', resolve).on('error', reject).connect({
      host,
      port: 22,
      username,
      password,
      readyTimeout: 20000,
      tryKeyboard: true,
    });
    conn.on('keyboard-interactive', (_n, _i, _l, _prompts, finish) => finish([password]));
  });
  console.log(`[OK] Da ket noi SSH toi ${host}`);

  console.log('[1/6] Upload goi build len server...');
  await new Promise((resolve, reject) => {
    conn.sftp((err, sftp) => {
      if (err) return reject(err);
      uploadFile(sftp, localTarball, remoteTarball).then(resolve, reject);
    });
  });

  console.log('[2/6] Chuan bi thu muc dung chung (App_Data, .env)...');
  await run(conn, 'mkdir -p /opt/ppv/shared/App_Data /opt/ppv/releases');
  await run(
    conn,
    `test -f /opt/ppv/shared/.env || printf 'Jwt__SigningKey=%s\\n' "$(head -c48 /dev/urandom | base64 | tr -d '\\n')" > /opt/ppv/shared/.env`
  );
  // Di chuyen du lieu cu (neu day la lan chay dau tien tren mot ban trien khai thu cong truoc do)
  await run(
    conn,
    `if [ -f /opt/ppv/current/App_Data/ppv-recon.db ] && [ ! -L /opt/ppv/current/App_Data ] && [ ! -f /opt/ppv/shared/App_Data/ppv-recon.db ]; then cp -a /opt/ppv/current/App_Data/. /opt/ppv/shared/App_Data/; fi`
  );
  await run(
    conn,
    `if [ -d /opt/ppv/current ] && [ ! -L /opt/ppv/current ]; then rm -rf /opt/ppv/current; fi`
  );

  console.log(`[3/6] Giai nen release moi: ${releaseId}`);
  await run(conn, `mkdir -p ${releasePath}`);
  await run(conn, `tar -xzf ${remoteTarball} -C ${releasePath}`);
  await run(conn, `rm -rf ${releasePath}/App_Data && ln -s /opt/ppv/shared/App_Data ${releasePath}/App_Data`);
  // Thu vien SQLite tu build .NET yeu cau glibc moi hon Ubuntu 20.04 dang co -> dung ban he thong
  await run(conn, `rm -f ${releasePath}/libe_sqlite3.so && ln -s ${SQLITE_SYSTEM_LIB} ${releasePath}/libe_sqlite3.so`);
  await run(conn, `chmod +x ${releasePath}/PpvRecon.Api`);

  console.log('[4/6] Cap nhat systemd service...');
  const svcB64 = Buffer.from(SERVICE_UNIT).toString('base64');
  await run(conn, `echo ${svcB64} | base64 -d > /tmp/ppv-recon.service`);
  await sudoRun(conn, 'cp /tmp/ppv-recon.service /etc/systemd/system/ppv-recon.service');
  await sudoRun(conn, 'systemctl daemon-reload');
  await sudoRun(conn, 'systemctl enable ppv-recon');

  console.log('[5/6] Chuyen symlink current -> release moi va khoi dong lai service...');
  await run(conn, `ln -sfn ${releasePath} /opt/ppv/current`);
  await sudoRun(conn, 'systemctl restart ppv-recon');

  console.log('[6/6] Kiem tra health check...');
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

  if (!healthy) {
    console.error('[CANH BAO] Service khong healthy sau khi deploy. Kiem tra: sudo journalctl -u ppv-recon -n 100');
  } else {
    console.log('[OK] Service da chay va healthy.');
  }

  // Don dep: xoa goi tam va giu lai 5 release gan nhat de co the rollback
  await run(conn, `rm -f ${remoteTarball}`);
  await run(conn, `cd /opt/ppv/releases && ls -1dt rel-* | tail -n +6 | xargs -r rm -rf`);

  console.log(`\nRelease da trien khai: ${releaseId}`);
  console.log(`Link test: http://${host}/`);

  conn.end();
  if (!healthy) process.exit(1);
}

main().catch((err) => {
  console.error('\n[DEPLOY THAT BAI]', err.message || err);
  process.exit(1);
});
