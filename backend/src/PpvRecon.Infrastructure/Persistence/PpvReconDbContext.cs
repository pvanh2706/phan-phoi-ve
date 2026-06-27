using Microsoft.EntityFrameworkCore;
using PpvRecon.Domain.Entities.Auditing;
using PpvRecon.Domain.Entities.Identity;
using PpvRecon.Domain.Entities.Jobs;
using PpvRecon.Domain.Entities.Parks;
using PpvRecon.Domain.Entities.Reconciliation;
using PpvRecon.Domain.Entities.Settings;
using PpvRecon.Domain.Entities.Summaries;
using PpvRecon.Domain.Enums;

namespace PpvRecon.Infrastructure.Persistence;

public sealed class PpvReconDbContext(DbContextOptions<PpvReconDbContext> options)
    : DbContext(options)
{
    private static readonly DateTime SeedCreatedAtUtc = new(2026, 6, 27, 0, 0, 0, DateTimeKind.Utc);

    public DbSet<User> Users => Set<User>();
    public DbSet<UserSession> UserSessions => Set<UserSession>();
    public DbSet<UserPreference> UserPreferences => Set<UserPreference>();
    public DbSet<NotificationPreference> NotificationPreferences => Set<NotificationPreference>();
    public DbSet<SystemSetting> SystemSettings => Set<SystemSetting>();
    public DbSet<NotificationRecipient> NotificationRecipients => Set<NotificationRecipient>();
    public DbSet<Park> Parks => Set<Park>();
    public DbSet<ParkTicketType> ParkTicketTypes => Set<ParkTicketType>();
    public DbSet<ParkRefund> ParkRefunds => Set<ParkRefund>();
    public DbSet<JobRun> JobRuns => Set<JobRun>();
    public DbSet<JobRunItem> JobRunItems => Set<JobRunItem>();
    public DbSet<ExternalApiRawResponse> ExternalApiRawResponses => Set<ExternalApiRawResponse>();
    public DbSet<DailyParkBalanceSnapshot> DailyParkBalanceSnapshots => Set<DailyParkBalanceSnapshot>();
    public DbSet<DailyTicketCostSummary> DailyTicketCostSummaries => Set<DailyTicketCostSummary>();
    public DbSet<DailyBankTransactionSummary> DailyBankTransactionSummaries => Set<DailyBankTransactionSummary>();
    public DbSet<ParkReconciliation> ParkReconciliations => Set<ParkReconciliation>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<DateOnly>()
            .HaveConversion<PpvDateOnlyConverter>()
            .HaveColumnType("TEXT");

        configurationBuilder.Properties<UserRole>().HaveConversion<string>().HaveMaxLength(50);
        configurationBuilder.Properties<UserStatus>().HaveConversion<string>().HaveMaxLength(50);
        configurationBuilder.Properties<ThemeMode>().HaveConversion<string>().HaveMaxLength(50);
        configurationBuilder.Properties<ParkPaymentType>().HaveConversion<string>().HaveMaxLength(50);
        configurationBuilder.Properties<RecordStatus>().HaveConversion<string>().HaveMaxLength(50);
        configurationBuilder.Properties<SourceType>().HaveConversion<string>().HaveMaxLength(50);
        configurationBuilder.Properties<JobRunStatus>().HaveConversion<string>().HaveMaxLength(50);
        configurationBuilder.Properties<JobRunItemStatus>().HaveConversion<string>().HaveMaxLength(50);
        configurationBuilder.Properties<JobTriggerType>().HaveConversion<string>().HaveMaxLength(50);
        configurationBuilder.Properties<ExternalApiSource>().HaveConversion<string>().HaveMaxLength(100);
        configurationBuilder.Properties<BankTransactionType>().HaveConversion<string>().HaveMaxLength(50);
        configurationBuilder.Properties<ReconciliationStatus>().HaveConversion<string>().HaveMaxLength(50);
        configurationBuilder.Properties<AuditAction>().HaveConversion<string>().HaveMaxLength(100);
        configurationBuilder.Properties<NotificationType>().HaveConversion<string>().HaveMaxLength(100);
        configurationBuilder.Properties<SettingValueType>().HaveConversion<string>().HaveMaxLength(50);
        configurationBuilder.Properties<ParkRefundStatus>().HaveConversion<string>().HaveMaxLength(50);
        configurationBuilder.Properties<CustomerRefundStatus>().HaveConversion<string>().HaveMaxLength(50);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigureIdentity(modelBuilder);
        ConfigureSettings(modelBuilder);
        ConfigureParks(modelBuilder);
        ConfigureJobs(modelBuilder);
        ConfigureSummaries(modelBuilder);
        ConfigureReconciliation(modelBuilder);
        ConfigureAudit(modelBuilder);
        SeedSystemSettings(modelBuilder);
    }

    private static void ConfigureIdentity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).ValueGeneratedOnAdd();
            entity.Property(x => x.FullName).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(320).IsRequired();
            entity.Property(x => x.NormalizedEmail).HasMaxLength(320).IsRequired();
            entity.Property(x => x.PhoneNumber).HasMaxLength(30);
            entity.Property(x => x.PasswordHash).IsRequired();
            entity.Property(x => x.LockReason).HasMaxLength(500);
            entity.Property(x => x.AvatarPath).HasMaxLength(500);
            entity.HasIndex(x => x.NormalizedEmail).IsUnique();
            entity.HasIndex(x => x.Role);
            entity.HasIndex(x => x.Status);
            entity.HasIndex(x => x.IsDeleted);
            RestrictUser(entity, x => x.CreatedByUserId);
            RestrictUser(entity, x => x.UpdatedByUserId);
            RestrictUser(entity, x => x.DeletedByUserId);
        });

        modelBuilder.Entity<UserSession>(entity =>
        {
            entity.ToTable("UserSessions");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).ValueGeneratedOnAdd();
            entity.Property(x => x.RefreshTokenHash).HasMaxLength(512).IsRequired();
            entity.Property(x => x.JwtId).HasMaxLength(100);
            entity.Property(x => x.CreatedByIp).HasMaxLength(100);
            entity.Property(x => x.UserAgent).HasMaxLength(1000);
            entity.Property(x => x.DeviceName).HasMaxLength(300);
            entity.Property(x => x.LastUsedIp).HasMaxLength(100);
            entity.Property(x => x.RevokeReason).HasMaxLength(300);
            entity.HasIndex(x => x.RefreshTokenHash).IsUnique();
            entity.HasIndex(x => x.UserId);
            entity.HasIndex(x => x.ExpiresAtUtc);
            entity.HasIndex(x => x.RevokedAtUtc);
            entity.HasOne<User>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
            RestrictUser(entity, x => x.RevokedByUserId);
            entity.HasOne<UserSession>().WithMany().HasForeignKey(x => x.ReplacedBySessionId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<UserPreference>(entity =>
        {
            entity.ToTable("UserPreferences");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).ValueGeneratedOnAdd();
            entity.Property(x => x.Language).HasMaxLength(20);
            entity.HasIndex(x => x.UserId).IsUnique();
            entity.HasOne<User>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<NotificationPreference>(entity =>
        {
            entity.ToTable("NotificationPreferences");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).ValueGeneratedOnAdd();
            entity.Property(x => x.EventType).HasMaxLength(100).IsRequired();
            entity.HasIndex(x => new { x.UserId, x.EventType }).IsUnique();
            entity.HasOne<User>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureSettings(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SystemSetting>(entity =>
        {
            entity.ToTable("SystemSettings");
            entity.HasKey(x => x.Key);
            entity.Property(x => x.Key).HasMaxLength(200);
            entity.Property(x => x.Value).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(1000);
            RestrictUser(entity, x => x.UpdatedByUserId);
        });

        modelBuilder.Entity<NotificationRecipient>(entity =>
        {
            entity.ToTable("NotificationRecipients");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).ValueGeneratedOnAdd();
            entity.Property(x => x.Email).HasMaxLength(320).IsRequired();
            entity.Property(x => x.DisplayName).HasMaxLength(200);
            entity.HasIndex(x => new { x.NotificationType, x.Email }).IsUnique();
            entity.HasIndex(x => x.IsActive);
            RestrictUser(entity, x => x.CreatedByUserId);
            RestrictUser(entity, x => x.UpdatedByUserId);
        });
    }

    private static void ConfigureParks(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Park>(entity =>
        {
            entity.ToTable("Parks");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).ValueGeneratedOnAdd();
            entity.Property(x => x.Code).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(300).IsRequired();
            entity.Property(x => x.SearchCode).HasMaxLength(100);
            entity.Property(x => x.Location).HasMaxLength(300);
            entity.Property(x => x.BankAccount).HasMaxLength(100);
            entity.Property(x => x.BankName).HasMaxLength(200);
            entity.Property(x => x.ApiSiteId).HasMaxLength(100);
            entity.Property(x => x.ApiProfileId).HasMaxLength(100);
            entity.Property(x => x.BalanceTransformRule).HasMaxLength(100);
            entity.Property(x => x.ApiNote).HasMaxLength(1000);
            entity.HasIndex(x => x.Code).IsUnique();
            entity.HasIndex(x => x.Name);
            entity.HasIndex(x => x.PaymentType);
            entity.HasIndex(x => x.Status);
            entity.HasIndex(x => x.IsDeleted);
            RestrictUser(entity, x => x.CreatedByUserId);
            RestrictUser(entity, x => x.UpdatedByUserId);
            RestrictUser(entity, x => x.DeletedByUserId);
        });

        modelBuilder.Entity<ParkTicketType>(entity =>
        {
            entity.ToTable("ParkTicketTypes");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).ValueGeneratedOnAdd();
            entity.Property(x => x.Code).HasMaxLength(100).IsRequired();
            entity.Property(x => x.TicketTypeCode).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(500).IsRequired();
            entity.Property(x => x.TicketGroupName).HasMaxLength(500);
            entity.HasIndex(x => x.ParkId);
            entity.HasIndex(x => x.Status);
            entity.HasIndex(x => x.IsDeleted);
            entity.HasIndex(x => new { x.ParkId, x.TicketTypeCode }).IsUnique();
            entity.HasIndex(x => new { x.ParkId, x.Code });
            entity.HasOne<Park>().WithMany().HasForeignKey(x => x.ParkId).OnDelete(DeleteBehavior.Restrict);
            RestrictUser(entity, x => x.CreatedByUserId);
            RestrictUser(entity, x => x.UpdatedByUserId);
            RestrictUser(entity, x => x.DeletedByUserId);
        });

        modelBuilder.Entity<ParkRefund>(entity =>
        {
            entity.ToTable("ParkRefunds");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).ValueGeneratedOnAdd();
            entity.Property(x => x.BookingCode).HasMaxLength(100).IsRequired();
            entity.Property(x => x.ParkCodeSnapshot).HasMaxLength(50);
            entity.Property(x => x.ParkNameSnapshot).HasMaxLength(300).IsRequired();
            entity.Property(x => x.TicketTypeCode).HasMaxLength(100);
            entity.Property(x => x.TicketTypeName).HasMaxLength(500);
            entity.Property(x => x.Reason).HasMaxLength(2000);
            entity.HasIndex(x => x.BookingCode);
            entity.HasIndex(x => x.RefundDate);
            entity.HasIndex(x => x.ParkId);
            entity.HasIndex(x => x.PaymentType);
            entity.HasIndex(x => x.ParkRefundStatus);
            entity.HasIndex(x => x.CustomerRefundStatus);
            entity.HasIndex(x => x.IsDeleted);
            entity.HasOne<Park>().WithMany().HasForeignKey(x => x.ParkId).OnDelete(DeleteBehavior.Restrict);
            RestrictUser(entity, x => x.CreatedByUserId);
            RestrictUser(entity, x => x.UpdatedByUserId);
            RestrictUser(entity, x => x.DeletedByUserId);
        });
    }

    private static void ConfigureJobs(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<JobRun>(entity =>
        {
            entity.ToTable("JobRuns");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).ValueGeneratedOnAdd();
            entity.Property(x => x.JobName).HasMaxLength(100).IsRequired();
            entity.HasIndex(x => new { x.JobName, x.BusinessDate });
            entity.HasIndex(x => x.Status);
            entity.HasIndex(x => x.StartedAtUtc);
            RestrictUser(entity, x => x.TriggeredByUserId);
        });

        modelBuilder.Entity<JobRunItem>(entity =>
        {
            entity.ToTable("JobRunItems");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).ValueGeneratedOnAdd();
            entity.Property(x => x.ErrorCode).HasMaxLength(100);
            entity.Property(x => x.ManualResolutionNote).HasMaxLength(1000);
            entity.HasIndex(x => x.JobRunId);
            entity.HasIndex(x => new { x.BusinessDate, x.ParkId, x.Source });
            entity.HasIndex(x => x.Status);
            entity.HasIndex(x => x.ResolvedAtUtc);
            entity.HasOne<JobRun>().WithMany().HasForeignKey(x => x.JobRunId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<Park>().WithMany().HasForeignKey(x => x.ParkId).OnDelete(DeleteBehavior.Restrict);
            RestrictUser(entity, x => x.ResolvedByUserId);
        });

        modelBuilder.Entity<ExternalApiRawResponse>(entity =>
        {
            entity.ToTable("ExternalApiRawResponses");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).ValueGeneratedOnAdd();
            entity.Property(x => x.RequestUrl).HasMaxLength(1000);
            entity.HasIndex(x => new { x.Source, x.BusinessDate, x.ParkId });
            entity.HasIndex(x => x.JobRunId);
            entity.HasIndex(x => x.ReceivedAtUtc);
            entity.HasOne<Park>().WithMany().HasForeignKey(x => x.ParkId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<JobRun>().WithMany().HasForeignKey(x => x.JobRunId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<JobRunItem>().WithMany().HasForeignKey(x => x.JobRunItemId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<JobRunItem>()
            .HasOne<ExternalApiRawResponse>()
            .WithMany()
            .HasForeignKey(x => x.RawResponseId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureSummaries(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DailyParkBalanceSnapshot>(entity =>
        {
            entity.ToTable("DailyParkBalanceSnapshots");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).ValueGeneratedOnAdd();
            entity.Property(x => x.BankAccountSnapshot).HasMaxLength(100);
            entity.Property(x => x.ManualReason).HasMaxLength(1000);
            entity.HasIndex(x => new { x.BusinessDate, x.ParkId }).IsUnique();
            entity.HasIndex(x => x.PaymentType);
            entity.HasIndex(x => x.SourceType);
            ConfigureSummaryRelationships(entity);
        });

        modelBuilder.Entity<DailyTicketCostSummary>(entity =>
        {
            entity.ToTable("DailyTicketCostSummaries");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).ValueGeneratedOnAdd();
            entity.Property(x => x.ManualReason).HasMaxLength(1000);
            entity.HasIndex(x => new { x.BusinessDate, x.ParkId }).IsUnique();
            entity.HasIndex(x => x.PaymentType);
            entity.HasIndex(x => x.SourceType);
            ConfigureSummaryRelationships(entity);
        });

        modelBuilder.Entity<DailyBankTransactionSummary>(entity =>
        {
            entity.ToTable("DailyBankTransactionSummaries");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).ValueGeneratedOnAdd();
            entity.Property(x => x.ManualReason).HasMaxLength(1000);
            entity.HasIndex(x => new { x.BusinessDate, x.ParkId, x.TransactionType }).IsUnique();
            entity.HasIndex(x => x.PaymentType);
            entity.HasIndex(x => x.SourceType);
            ConfigureSummaryRelationships(entity);
        });
    }

    private static void ConfigureReconciliation(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ParkReconciliation>(entity =>
        {
            entity.ToTable("ParkReconciliations");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).ValueGeneratedOnAdd();
            entity.Property(x => x.AdjustmentNote).HasMaxLength(2000);
            entity.Property(x => x.LastSourceHash).HasMaxLength(128);
            entity.Property(x => x.ResolvedSourceHash).HasMaxLength(128);
            entity.HasIndex(x => new { x.BusinessDate, x.ParkId }).IsUnique();
            entity.HasIndex(x => x.Status);
            entity.HasIndex(x => x.PaymentType);
            entity.HasIndex(x => x.ResolvedAtUtc);
            entity.HasOne<Park>().WithMany().HasForeignKey(x => x.ParkId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<JobRun>().WithMany().HasForeignKey(x => x.LastBuiltJobRunId).OnDelete(DeleteBehavior.Restrict);
            RestrictUser(entity, x => x.ResolvedByUserId);
            RestrictUser(entity, x => x.CreatedByUserId);
            RestrictUser(entity, x => x.UpdatedByUserId);
        });
    }

    private static void ConfigureAudit(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.ToTable("AuditLogs");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).ValueGeneratedOnAdd();
            entity.Property(x => x.UserEmailSnapshot).HasMaxLength(320);
            entity.Property(x => x.UserRoleSnapshot).HasMaxLength(50);
            entity.Property(x => x.Module).HasMaxLength(100).IsRequired();
            entity.Property(x => x.EntityName).HasMaxLength(200).IsRequired();
            entity.Property(x => x.EntityId).HasMaxLength(100);
            entity.Property(x => x.IpAddress).HasMaxLength(100);
            entity.Property(x => x.UserAgent).HasMaxLength(1000);
            entity.Property(x => x.CorrelationId).HasMaxLength(100);
            entity.HasIndex(x => x.OccurredAtUtc);
            entity.HasIndex(x => x.UserId);
            entity.HasIndex(x => x.Module);
            entity.HasIndex(x => new { x.EntityName, x.EntityId });
            entity.HasIndex(x => x.Action);
            RestrictUser(entity, x => x.UserId);
        });
    }

    private static void SeedSystemSettings(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SystemSetting>().HasData(
            new SystemSetting
            {
                Key = "Jobs.ScheduleTime",
                Value = "23:59",
                ValueType = SettingValueType.String,
                Description = "Gio chay job hang ngay theo mui gio Asia/Bangkok.",
                IsSensitive = false,
                CreatedAtUtc = SeedCreatedAtUtc,
            },
            new SystemSetting
            {
                Key = "Jobs.ApiTimeoutSeconds",
                Value = "30",
                ValueType = SettingValueType.Int,
                Description = "Timeout cho tung API call.",
                IsSensitive = false,
                CreatedAtUtc = SeedCreatedAtUtc,
            },
            new SystemSetting
            {
                Key = "Jobs.ApiRetryCount",
                Value = "2",
                ValueType = SettingValueType.Int,
                Description = "So lan retry khi API loi.",
                IsSensitive = false,
                CreatedAtUtc = SeedCreatedAtUtc,
            },
            new SystemSetting
            {
                Key = "Jobs.ApiRetryDelaySeconds",
                Value = "5",
                ValueType = SettingValueType.Int,
                Description = "Thoi gian cho co dinh giua cac lan retry.",
                IsSensitive = false,
                CreatedAtUtc = SeedCreatedAtUtc,
            },
            new SystemSetting
            {
                Key = "Audit.RetentionDays",
                Value = "365",
                ValueType = SettingValueType.Int,
                Description = "So ngay giu audit log.",
                IsSensitive = false,
                CreatedAtUtc = SeedCreatedAtUtc,
            },
            new SystemSetting
            {
                Key = "Email.EnableSyncErrorSummary",
                Value = "true",
                ValueType = SettingValueType.Bool,
                Description = "Bat gui email tong hop loi dong bo.",
                IsSensitive = false,
                CreatedAtUtc = SeedCreatedAtUtc,
            });
    }

    private static void ConfigureSummaryRelationships<TEntity>(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<TEntity> entity)
        where TEntity : class
    {
        entity.HasOne<Park>().WithMany().HasForeignKey("ParkId").OnDelete(DeleteBehavior.Restrict);
        entity.HasOne<JobRun>().WithMany().HasForeignKey("SourceJobRunId").OnDelete(DeleteBehavior.Restrict);
        entity.HasOne<JobRunItem>().WithMany().HasForeignKey("SourceJobRunItemId").OnDelete(DeleteBehavior.Restrict);
        entity.HasOne<ExternalApiRawResponse>().WithMany().HasForeignKey("RawResponseId").OnDelete(DeleteBehavior.Restrict);
        entity.HasOne<User>().WithMany().HasForeignKey("CreatedByUserId").OnDelete(DeleteBehavior.Restrict);
        entity.HasOne<User>().WithMany().HasForeignKey("UpdatedByUserId").OnDelete(DeleteBehavior.Restrict);
    }

    private static void RestrictUser<TEntity>(
        Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<TEntity> entity,
        System.Linq.Expressions.Expression<Func<TEntity, object?>> foreignKeyExpression)
        where TEntity : class
    {
        entity.HasOne<User>().WithMany().HasForeignKey(foreignKeyExpression).OnDelete(DeleteBehavior.Restrict);
    }
}
