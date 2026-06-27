-- Seed template for PpvRecon Parks and ParkTicketTypes.
-- Run this script after applying EF migrations.
-- DB path in development:
--   backend/src/PpvRecon.Api/App_Data/ppv-recon.db
--
-- Notes:
-- - Keep Park.Code unique forever, including soft-deleted rows.
-- - PaymentType: Prepaid or Debt.
-- - Status: Active or Inactive.
-- - Money fields are INTEGER VND.
-- - BalanceTransformRule examples: None, MultiplyMinusOne.

-- Example prepaid park.
INSERT INTO Parks (
    Code,
    Name,
    PaymentType,
    SearchCode,
    Location,
    BankAccount,
    BankName,
    CreditLimit,
    ApiSiteId,
    ApiProfileId,
    BalanceTransformRule,
    ApiNote,
    Status,
    CreatedAtUtc,
    IsDeleted
)
VALUES (
    'KVC_PREPAID_001',
    'Ten khu vui choi nap truoc',
    'Prepaid',
    'KVC_PREPAID_001',
    'Tinh/Thanh pho',
    'So tai khoan hoac ma tai khoan',
    'Ten ngan hang',
    NULL,
    'siteID-tu-api',
    'profileID-tu-api',
    'None',
    'Ghi chu cau hinh API neu co',
    'Active',
    datetime('now'),
    0
);

-- Example debt park.
INSERT INTO Parks (
    Code,
    Name,
    PaymentType,
    SearchCode,
    Location,
    BankAccount,
    BankName,
    CreditLimit,
    ApiSiteId,
    ApiProfileId,
    BalanceTransformRule,
    ApiNote,
    Status,
    CreatedAtUtc,
    IsDeleted
)
VALUES (
    'KVC_DEBT_001',
    'Ten khu vui choi cong no',
    'Debt',
    'KVC_DEBT_001',
    'Tinh/Thanh pho',
    'So tai khoan hoac ma tai khoan',
    'Ten ngan hang',
    100000000,
    'siteID-tu-api',
    'profileID-tu-api',
    'MultiplyMinusOne',
    'Vi du KVC can xu ly balance * -1',
    'Active',
    datetime('now'),
    0
);

-- Example ticket types.
-- Replace ParkId with IDs created above, or use subqueries by Code as below.
INSERT INTO ParkTicketTypes (
    ParkId,
    Code,
    TicketTypeCode,
    Name,
    TicketGroupName,
    CostPrice,
    Status,
    CreatedAtUtc,
    IsDeleted
)
VALUES (
    (SELECT Id FROM Parks WHERE Code = 'KVC_PREPAID_001'),
    'CHILD_001',
    'TICKET_ADULT',
    'Ve nguoi lon',
    'Ve vao cong',
    150000,
    'Active',
    datetime('now'),
    0
);

INSERT INTO ParkTicketTypes (
    ParkId,
    Code,
    TicketTypeCode,
    Name,
    TicketGroupName,
    CostPrice,
    Status,
    CreatedAtUtc,
    IsDeleted
)
VALUES (
    (SELECT Id FROM Parks WHERE Code = 'KVC_PREPAID_001'),
    'CHILD_002',
    'TICKET_CHILD',
    'Ve tre em',
    'Ve vao cong',
    100000,
    'Active',
    datetime('now'),
    0
);
