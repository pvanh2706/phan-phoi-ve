namespace PpvRecon.Domain.Enums;

public enum UserRole
{
    Admin,
    Member,
    Accountant,
}

public enum UserStatus
{
    Active,
    Inactive,
    Locked,
}

public enum ThemeMode
{
    Dark,
    Light,
    System,
}

public enum ParkPaymentType
{
    Prepaid,
    Debt,
}

public enum RecordStatus
{
    Active,
    Inactive,
}

public enum SourceType
{
    Api,
    Manual,
}

public enum JobRunStatus
{
    Running,
    Succeeded,
    CompletedWithErrors,
    Failed,
    Canceled,
}

public enum JobRunItemStatus
{
    Pending,
    Running,
    Succeeded,
    Failed,
    Skipped,
    ManualResolved,
}

public enum JobTriggerType
{
    Schedule,
    Manual,
    System,
}

public enum ExternalApiSource
{
    ParkBalance,
    TicketCost,
    BankTransaction,
    AgencyBooking,
}

public enum BankTransactionType
{
    TopUp,
    DebtPayment,
    Refund,
    Other,
}

public enum ReconciliationStatus
{
    Matched,
    Variance,
    MissingData,
    Resolved,
}

public enum AuditAction
{
    Create,
    Update,
    SetInactive,
    Restore,
    SoftDelete,
    RunJob,
    ManualEntry,
    ResolveVariance,
    Login,
    Logout,
    LockUser,
    UnlockUser,
    ResetPassword,
    RevokeSession,
    ResetData,
}

public enum NotificationType
{
    SyncErrorSummary,
    DailyReport,
    ReconciliationVarianceAlert,
}

public enum SettingValueType
{
    String,
    Int,
    Bool,
    Decimal,
    Json,
}

public enum ParkRefundStatus
{
    Completed,
    Processing,
    Rejected,
}

public enum CustomerRefundStatus
{
    Refunded,
    Transferring,
    WaitingConfirmation,
    NotProcessed,
    NoRefund,
}
