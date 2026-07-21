// Script chan doan (read-only), khong thay doi gi tren server.
// Dung chung config voi deploy-ssh.js (SERVER_HOST/SERVER_USER/SERVER_PASS tu deploy/config.bat).

const { Client } = require('ssh2');

const host = process.env.SERVER_HOST;
const username = process.env.SERVER_USER;
const password = process.env.SERVER_PASS;

if (!host || !username || !password) {
  console.error('[LOI] Thieu SERVER_HOST / SERVER_USER / SERVER_PASS. Kiem tra deploy/config.bat');
  process.exit(1);
}

function run(conn, cmd) {
  return new Promise((resolve) => {
    conn.exec(cmd, { pty: false }, (err, stream) => {
      if (err) return resolve(`ERR: ${err.message}`);
      let out = '';
      let errOut = '';
      stream
        .on('close', () => resolve(out || errOut))
        .on('data', (d) => { out += d; })
        .stderr.on('data', (d) => { errOut += d; });
    });
  });
}

async function main() {
  const conn = new Client();
  await new Promise((resolve, reject) => {
    conn.on('ready', resolve).on('error', reject).connect({
      host, port: 22, username, password, readyTimeout: 20000, tryKeyboard: true,
    });
    conn.on('keyboard-interactive', (_n, _i, _l, _prompts, finish) => finish([password]));
  });
  console.log(`[OK] Da ket noi SSH toi ${host}\n`);

  const checks = [
    ['Symlink /opt/ppv/current ->', 'readlink -f /opt/ppv/current'],
    ['Danh sach releases (moi nhat truoc)', 'ls -1dt /opt/ppv/releases/rel-* | head -5'],
    ['Asset JS trong release dang chay', "grep -oE '/assets/index-[A-Za-z0-9]+\\.js' /opt/ppv/current/wwwroot/index.html"],
    ['Health/local content-hash tu app dotnet (127.0.0.1:5000)', "curl -s http://127.0.0.1:5000/ | grep -oE '/assets/index-[A-Za-z0-9]+\\.(js|css)'"],
    ['systemctl status ppv-recon (tom tat)', 'systemctl is-active ppv-recon; systemctl show ppv-recon -p ExecMainStartTimestamp -p MainPID'],
    ['Cac process dang lang nghe port 80/443/5000', 'ss -ltnp 2>/dev/null | grep -E ":80 |:443 |:5000 " || netstat -ltnp 2>/dev/null | grep -E ":80 |:443 |:5000 "'],
    ['Danh sach site nginx enabled', 'ls -la /etc/nginx/sites-enabled/ 2>/dev/null'],
    ['Nginx config co chua ppv.ezcloud.vn', "grep -rl 'ppv.ezcloud.vn' /etc/nginx/ 2>/dev/null"],
    ['Noi dung config nginx cho ppv (neu tim thay)', "grep -rl 'ppv.ezcloud.vn' /etc/nginx/ 2>/dev/null | xargs -r cat"],
    ['curl truc tiep vao domain tu server (qua nginx local)', "curl -s -H 'Host: ppv.ezcloud.vn' http://127.0.0.1/ | grep -oE '/assets/index-[A-Za-z0-9]+\\.(js|css)'"],
    ['Thong tin process dang bind port 5000', 'ps -p 716079 -o pid,lstart,cmd 2>/dev/null; echo ---; readlink -f /proc/716079/cwd 2>/dev/null; echo ---; ls -la /proc/716079/exe 2>/dev/null'],
    ['journalctl ppv-recon (50 dong gan nhat)', 'sudo -n journalctl -u ppv-recon -n 50 --no-pager 2>&1 || journalctl -u ppv-recon -n 50 --no-pager 2>&1'],
    ['Toan bo process PpvRecon.Api dang chay', "ps aux | grep -i PpvRecon.Api | grep -v grep"],
    ['Trang thai tai khoan user (Status/FailedLoginCount/Locked) tren DB that', "command -v python3 >/dev/null 2>&1 && python3 -c \"import sqlite3; c=sqlite3.connect('/opt/ppv/shared/App_Data/ppv-recon.db'); cur=c.cursor(); cur.execute('SELECT Id, Email, Role, Status, FailedLoginCount, LockedAtUtc, LockReason FROM Users WHERE IsDeleted=0'); [print(row) for row in cur.fetchall()]\" || echo 'python3 khong co san tren server'"],
    ['Thoi diem tao/doi mat khau gan nhat cua tung user', "python3 -c \"import sqlite3; c=sqlite3.connect('/opt/ppv/shared/App_Data/ppv-recon.db'); cur=c.cursor(); cur.execute('SELECT Id, Email, CreatedAtUtc, PasswordChangedAtUtc, UpdatedAtUtc, UpdatedByUserId FROM Users WHERE IsDeleted=0'); [print(row) for row in cur.fetchall()]\""],
    ['Lich su audit log lien quan Auth/User gan day (20 dong)', "python3 -c \"import sqlite3; c=sqlite3.connect('/opt/ppv/shared/App_Data/ppv-recon.db'); cur=c.cursor(); cur.execute(\\\"SELECT CreatedAtUtc, UserId, Module, Action, EntityId FROM AuditLogs WHERE Module IN ('Auth','Settings') ORDER BY CreatedAtUtc DESC LIMIT 20\\\"); [print(row) for row in cur.fetchall()]\" 2>&1"],
  ];

  for (const [label, cmd] of checks) {
    console.log(`--- ${label} ---`);
    const out = await run(conn, cmd);
    console.log(out.trim() || '(khong co output)');
    console.log('');
  }

  conn.end();
}

main().catch((err) => {
  console.error('[LOI]', err.message || err);
  process.exit(1);
});
