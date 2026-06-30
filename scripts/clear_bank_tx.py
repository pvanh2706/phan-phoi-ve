"""
Xoa du lieu bang BankTransactionDetails (man "Danh sach nap tien KVC theo ngay")
de test lai luong Get API.

Yeu cau: Python 3 (module sqlite3 co san, khong can cai them).

Cach dung (chay tu thu muc goc du an hoac bat ky dau):

    # Xoa TOAN BO du lieu (mac dinh)
    python scripts/clear_bank_tx.py

    # Chi xoa du lieu nguon API (giu lai cac dong nhap tay neu co)
    python scripts/clear_bank_tx.py --api-only

    # Chi xoa mot ngay (dinh dang yyyy-MM-dd)
    python scripts/clear_bank_tx.py --date 2026-06-30

    # Chi xoa mot khoang ngay
    python scripts/clear_bank_tx.py --from 2026-06-01 --to 2026-06-30

    # Tro toi file DB khac (mac dinh: backend/src/PpvRecon.Api/App_Data/ppv-recon.db)
    python scripts/clear_bank_tx.py --db D:\\duong\\dan\\toi\\ppv-recon.db

Luu y: backend dang chay van xoa duoc (SQLite cho phep), khong can restart;
chi can refresh lai man hinh la thay trong.
"""

import argparse
import os
import sqlite3
import sys

# Mac dinh: file DB nam canh project API, suy ra tu vi tri script (../backend/...).
DEFAULT_DB = os.path.normpath(
    os.path.join(
        os.path.dirname(os.path.abspath(__file__)),
        "..", "backend", "src", "PpvRecon.Api", "App_Data", "ppv-recon.db",
    )
)


def build_where(args):
    """Tao menh de WHERE + tham so tu cac tuy chon dong lenh."""
    clauses, params = [], []
    if args.api_only:
        # SourceType luu duoi dang chuoi 'Api' trong DB.
        clauses.append("SourceType = ?")
        params.append("Api")
    if args.date:
        clauses.append("BusinessDate = ?")
        params.append(args.date)
    if getattr(args, "from_date", None):
        clauses.append("BusinessDate >= ?")
        params.append(args.from_date)
    if args.to:
        clauses.append("BusinessDate <= ?")
        params.append(args.to)
    where = (" WHERE " + " AND ".join(clauses)) if clauses else ""
    return where, params


def main():
    parser = argparse.ArgumentParser(description="Xoa du lieu BankTransactionDetails.")
    parser.add_argument("--db", default=DEFAULT_DB, help="Duong dan file SQLite .db")
    parser.add_argument("--api-only", action="store_true", help="Chi xoa dong nguon API")
    parser.add_argument("--date", help="Chi xoa 1 ngay (yyyy-MM-dd)")
    parser.add_argument("--from", dest="from_date", help="Tu ngay (yyyy-MM-dd)")
    parser.add_argument("--to", help="Den ngay (yyyy-MM-dd)")
    args = parser.parse_args()

    if not os.path.exists(args.db):
        print(f"Khong tim thay file DB: {args.db}")
        sys.exit(1)

    where, params = build_where(args)

    con = sqlite3.connect(args.db, timeout=15)
    try:
        con.execute("PRAGMA busy_timeout = 15000")
        cur = con.cursor()

        before = cur.execute(
            f"SELECT COUNT(*) FROM BankTransactionDetails{where}", params
        ).fetchone()[0]
        print(f"So dong khop dieu kien: {before}")

        if before == 0:
            print("Khong co gi de xoa.")
            return

        cur.execute(f"DELETE FROM BankTransactionDetails{where}", params)
        con.commit()
        print(f"Da xoa {cur.rowcount} dong.")

        total_left = cur.execute("SELECT COUNT(*) FROM BankTransactionDetails").fetchone()[0]
        print(f"Tong con lai trong bang: {total_left} dong.")
    finally:
        con.close()


if __name__ == "__main__":
    main()
