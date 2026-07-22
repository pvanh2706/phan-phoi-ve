using Microsoft.EntityFrameworkCore;
using PpvRecon.Domain.Entities.Agencies;
using PpvRecon.Domain.Entities.Auditing;
using PpvRecon.Domain.Entities.Identity;
using PpvRecon.Domain.Entities.Jobs;
using PpvRecon.Domain.Entities.Parks;
using PpvRecon.Domain.Entities.Reconciliation;
using PpvRecon.Domain.Entities.Settings;
using PpvRecon.Domain.Entities.Summaries;
using PpvRecon.Domain.Entities.Workflow;
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
    public DbSet<Agency> Agencies => Set<Agency>();
    public DbSet<AgencyBooking> AgencyBookings => Set<AgencyBooking>();
    public DbSet<Park> Parks => Set<Park>();
    public DbSet<ParkTicketType> ParkTicketTypes => Set<ParkTicketType>();
    public DbSet<ParkRefund> ParkRefunds => Set<ParkRefund>();
    public DbSet<JobRun> JobRuns => Set<JobRun>();
    public DbSet<JobRunItem> JobRunItems => Set<JobRunItem>();
    public DbSet<ExternalApiRawResponse> ExternalApiRawResponses => Set<ExternalApiRawResponse>();
    public DbSet<DailyParkBalanceSnapshot> DailyParkBalanceSnapshots => Set<DailyParkBalanceSnapshot>();
    public DbSet<DailyTicketCostSummary> DailyTicketCostSummaries => Set<DailyTicketCostSummary>();
    public DbSet<TicketSaleCostDetail> TicketSaleCostDetails => Set<TicketSaleCostDetail>();
    public DbSet<BankTransactionDetail> BankTransactionDetails => Set<BankTransactionDetail>();
    public DbSet<DailyBankTransactionSummary> DailyBankTransactionSummaries => Set<DailyBankTransactionSummary>();
    public DbSet<ParkReconciliation> ParkReconciliations => Set<ParkReconciliation>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<WorkflowColumn> WorkflowColumns => Set<WorkflowColumn>();
    public DbSet<WorkflowTask> WorkflowTasks => Set<WorkflowTask>();

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
        ConfigureAgencies(modelBuilder);
        ConfigureParks(modelBuilder);
        ConfigureJobs(modelBuilder);
        ConfigureSummaries(modelBuilder);
        ConfigureReconciliation(modelBuilder);
        ConfigureAudit(modelBuilder);
        ConfigureWorkflow(modelBuilder);
        SeedSystemSettings(modelBuilder);
        SeedTicketSaleCostDetails(modelBuilder);
        SeedReconciliationDemo(modelBuilder);
        SeedBankTransactionDetails(modelBuilder);
        SeedWorkflowBoard(modelBuilder);
    }

    private static void ConfigureAgencies(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Agency>(entity =>
        {
            entity.ToTable("Agencies");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).ValueGeneratedOnAdd();
            entity.Property(x => x.Code).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(300).IsRequired();
            entity.Property(x => x.ParentCode).HasMaxLength(50);
            entity.Property(x => x.ParentName).HasMaxLength(300);
            entity.Property(x => x.Source).HasMaxLength(50).IsRequired();
            entity.HasIndex(x => x.Code).IsUnique();
            entity.HasIndex(x => x.Name);
            entity.HasIndex(x => x.IsDeleted);
            RestrictUser(entity, x => x.CreatedByUserId);
            RestrictUser(entity, x => x.UpdatedByUserId);
            RestrictUser(entity, x => x.DeletedByUserId);
        });

        modelBuilder.Entity<AgencyBooking>(entity =>
        {
            entity.ToTable("AgencyBookings");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).ValueGeneratedOnAdd();
            entity.Property(x => x.SourceSystem).HasMaxLength(50).IsRequired();
            entity.Property(x => x.SourceTransactionId).HasMaxLength(100).IsRequired();
            entity.Property(x => x.BookingCode).HasMaxLength(100).IsRequired();
            entity.Property(x => x.BuyingAgentCode).HasMaxLength(50).IsRequired();
            entity.Property(x => x.BuyingAgentName).HasMaxLength(300);
            entity.Property(x => x.ParentBuyingAgentCode).HasMaxLength(50);
            entity.Property(x => x.ParentBuyingAgentName).HasMaxLength(300);
            entity.Property(x => x.SellingAgentCode).HasMaxLength(50);
            entity.Property(x => x.SellingAgentName).HasMaxLength(300);
            entity.Property(x => x.ParkExternalCode).HasMaxLength(50);
            entity.Property(x => x.ParkExternalName).HasMaxLength(300);
            entity.Property(x => x.TicketTypeCode).HasMaxLength(100);
            entity.Property(x => x.TicketTypeName).HasMaxLength(500);
            entity.Property(x => x.TicketGroupName).HasMaxLength(500);

            // Chống trùng §10: mỗi giao dịch nguồn chỉ tồn tại 1 bản ghi.
            entity.HasIndex(x => new { x.SourceSystem, x.SourceTransactionId }).IsUnique();
            entity.HasIndex(x => x.BookingDate);
            entity.HasIndex(x => x.BookingCode);
            entity.HasIndex(x => x.BuyingAgentId);
            entity.HasIndex(x => x.BuyingAgentCode);
            entity.HasIndex(x => x.SourceType);

            entity.HasOne<Agency>().WithMany().HasForeignKey(x => x.BuyingAgentId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<JobRun>().WithMany().HasForeignKey(x => x.SourceJobRunId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<JobRunItem>().WithMany().HasForeignKey(x => x.SourceJobRunItemId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<ExternalApiRawResponse>().WithMany().HasForeignKey(x => x.RawResponseId).OnDelete(DeleteBehavior.Restrict);
            RestrictUser(entity, x => x.CreatedByUserId);
            RestrictUser(entity, x => x.UpdatedByUserId);
        });
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

        modelBuilder.Entity<TicketSaleCostDetail>(entity =>
        {
            entity.ToTable("TicketSaleCostDetails");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).ValueGeneratedOnAdd();
            entity.Property(x => x.BookingCode).HasMaxLength(100).IsRequired();
            entity.Property(x => x.TicketTypeName).HasMaxLength(500).IsRequired();
            entity.Property(x => x.TicketGroupName).HasMaxLength(500);
            entity.Property(x => x.SellingAgentCode).HasMaxLength(100);
            entity.Property(x => x.BuyingAgentCode).HasMaxLength(100);
            entity.Property(x => x.BuyingAgentName).HasMaxLength(500);
            entity.Property(x => x.ParkCodeSnapshot).HasMaxLength(50).IsRequired();
            entity.Property(x => x.ParkNameSnapshot).HasMaxLength(300).IsRequired();
            entity.Property(x => x.ExternalLineId).HasMaxLength(100);
            entity.Property(x => x.SellingAgentName).HasMaxLength(500);
            entity.Property(x => x.TicketTypeCode).HasMaxLength(100);
            entity.Property(x => x.ParentBuyingAgentName).HasMaxLength(500);
            entity.Property(x => x.ParentBuyingAgentCode).HasMaxLength(100);
            entity.HasIndex(x => x.BusinessDate);
            entity.HasIndex(x => x.PaymentType);
            entity.HasIndex(x => x.BookingCode);
            entity.HasIndex(x => x.ParkId);
            entity.HasIndex(x => x.SourceType);
            entity.HasOne<Park>().WithMany().HasForeignKey(x => x.ParkId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<JobRun>().WithMany().HasForeignKey(x => x.SourceJobRunId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<JobRunItem>().WithMany().HasForeignKey(x => x.SourceJobRunItemId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<ExternalApiRawResponse>().WithMany().HasForeignKey(x => x.RawResponseId).OnDelete(DeleteBehavior.Restrict);
            RestrictUser(entity, x => x.CreatedByUserId);
            RestrictUser(entity, x => x.UpdatedByUserId);
        });

        modelBuilder.Entity<BankTransactionDetail>(entity =>
        {
            entity.ToTable("BankTransactionDetails");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).ValueGeneratedOnAdd();
            entity.Property(x => x.Content).HasMaxLength(1000).IsRequired();
            entity.Property(x => x.BankAccount).HasMaxLength(100);
            entity.HasIndex(x => x.BusinessDate);
            entity.HasIndex(x => x.PaymentType);
            entity.HasIndex(x => x.ParkId);
            entity.HasIndex(x => x.SourceType);
            entity.HasOne<Park>().WithMany().HasForeignKey(x => x.ParkId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<JobRun>().WithMany().HasForeignKey(x => x.SourceJobRunId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<JobRunItem>().WithMany().HasForeignKey(x => x.SourceJobRunItemId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<ExternalApiRawResponse>().WithMany().HasForeignKey(x => x.RawResponseId).OnDelete(DeleteBehavior.Restrict);
            RestrictUser(entity, x => x.CreatedByUserId);
            RestrictUser(entity, x => x.UpdatedByUserId);
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

    private static void ConfigureWorkflow(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WorkflowColumn>(entity =>
        {
            entity.ToTable("WorkflowColumns");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).ValueGeneratedOnAdd();
            entity.Property(x => x.ColumnKey).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Title).HasMaxLength(200).IsRequired();
            entity.Property(x => x.HeadTone).HasMaxLength(20).IsRequired();
            entity.Property(x => x.CardStatusLabel).HasMaxLength(100).IsRequired();
            entity.Property(x => x.CardTone).HasMaxLength(20).IsRequired();
            entity.Property(x => x.VisibleFields).HasMaxLength(500);
            entity.Property(x => x.PermittedUserIds).HasMaxLength(1000);
            entity.HasIndex(x => x.ColumnKey).IsUnique();
            entity.HasIndex(x => x.SortOrder);
            RestrictUser(entity, x => x.CreatedByUserId);
            RestrictUser(entity, x => x.UpdatedByUserId);
        });

        modelBuilder.Entity<WorkflowTask>(entity =>
        {
            entity.ToTable("WorkflowTasks");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).ValueGeneratedOnAdd();
            entity.Property(x => x.Title).HasMaxLength(300).IsRequired();
            entity.Property(x => x.BankAccount).HasMaxLength(100);
            entity.Property(x => x.BankName).HasMaxLength(200);
            entity.Property(x => x.Note).HasMaxLength(2000);
            entity.HasIndex(x => x.ColumnId);
            entity.HasIndex(x => x.ParkId);
            entity.HasIndex(x => x.PaymentType);
            entity.HasOne<WorkflowColumn>().WithMany().HasForeignKey(x => x.ColumnId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<Park>().WithMany().HasForeignKey(x => x.ParkId).OnDelete(DeleteBehavior.Restrict);
            RestrictUser(entity, x => x.CreatedByUserId);
            RestrictUser(entity, x => x.UpdatedByUserId);
        });
    }

    private static void SeedWorkflowBoard(ModelBuilder modelBuilder)
    {
        static WorkflowColumn C(int id, string key, string title, string headTone,
            string statusLabel, string cardTone, int order)
            => new()
            {
                Id = id,
                ColumnKey = key,
                Title = title,
                HeadTone = headTone,
                CardStatusLabel = statusLabel,
                CardTone = cardTone,
                SortOrder = order,
                VisibleFields = "title,desc,amount,date,tag",
                PermittedUserIds = "",
                CreatedAtUtc = SeedCreatedAtUtc,
            };

        modelBuilder.Entity<WorkflowColumn>().HasData(
            C(1, "lap-phieu", "Kế toán / NVKD lập phiếu", "gray", "Lập phiếu", "gray", 1),
            C(2, "truong-bo-phan-duyet", "Trưởng bộ phận duyệt", "sky", "Chờ duyệt", "blue", 2),
            C(3, "kiem-tra-chuyen-khoan", "Kế toán kiểm tra & chuyển khoản", "indigo", "Chuyển khoản", "indigo", 3),
            C(4, "hoan-thanh", "Hoàn thành", "green", "Hoàn thành", "green", 4),
            C(5, "that-bai", "Thất bại", "red", "Thất bại", "red", 5));

        static WorkflowTask T(int id, string title, ParkPaymentType pay, int? parkId, string? account,
            string? bank, long amount, string date, string? note, int columnId, int order)
            => new()
            {
                Id = id,
                Title = title,
                PaymentType = pay,
                ParkId = parkId,
                BankAccount = account,
                BankName = bank,
                Amount = amount,
                ExecuteDate = DateOnly.ParseExact(date, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture),
                Note = note,
                ColumnId = columnId,
                SortOrder = order,
                CreatedAtUtc = SeedCreatedAtUtc,
            };

        modelBuilder.Entity<WorkflowTask>().HasData(
            T(9001, "Nạp tiền - Vin Nha Trang", ParkPaymentType.Prepaid, null, "19139932758899", "Techcombank", 50000000, "2026-06-24", "Cần ezCloud Key tạy tiền trên hệ thống", 1, 1),
            T(9002, "Nạp tiền - Vin Phú Quốc", ParkPaymentType.Prepaid, 9003, "0091000593278", "Vietcombank", 100000000, "2026-04-28", "Cần ezCloud Key tạy tiền trên hệ thống", 2, 1),
            T(9003, "Nạp tiền - Vin Nam Hội An", ParkPaymentType.Prepaid, 9002, "1029876329", "Vietcombank", 50000000, "2026-04-28", "Cần ezCloud Key tạy tiền trên hệ thống", 2, 2),
            T(9004, "Nạp tiền - Thủy Cung Lotte (Lần 1)", ParkPaymentType.Prepaid, 9004, "700029610000", "Shinhan Bank", 365625000, "2026-04-29", "Bộ phận công tác: Phân Phối Vé · Kế Toán", 3, 1),
            T(9005, "Nạp tiền - Thủy Cung Lotte (Lần 2)", ParkPaymentType.Prepaid, 9004, "700029610000", "Shinhan Bank", 237375000, "2026-04-29", "Bộ phận công tác: Phân Phối Vé · Kế Toán", 3, 2),
            T(9006, "Nạp tiền - Bản Mòng", ParkPaymentType.Prepaid, 9001, "1213776969", "NCB", 490000000, "2025-11-14", "Bộ phận công tác: Phân Phối Vé · Trưởng bộ", 4, 1),
            T(9007, "Nạp tiền - Sunworld", ParkPaymentType.Prepaid, null, "1SB2B24", "NCB", 490000000, "2026-04-28", "Bộ phận công tác: Phân Phối Vé · Kế Toán", 4, 2),
            T(9008, "Nạp tiền - Vin Cửa Hội", ParkPaymentType.Prepaid, 9005, "19139932758899", "Techcombank", 50000000, "2026-04-28", "Bộ phận công tác: Phân Phối Vé · Kế Toán", 4, 3),
            T(9009, "Nạp tiền - Vin Vũ Yên (Lần 2)", ParkPaymentType.Prepaid, null, null, null, 50000000, "2026-06-15", "Sai số tài khoản, cần kiểm tra lại", 5, 1),
            T(9010, "Thanh toán Công nợ - Sealinks", ParkPaymentType.Debt, 9006, "1100030038237", "Vietcombank", 8110000, "2025-09-16", "Sai thông tin tài khoản thụ hưởng", 5, 2));
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

    private static void SeedTicketSaleCostDetails(ModelBuilder modelBuilder)
    {
        static TicketSaleCostDetail D(int id, ParkPaymentType pay, string date, string booking, long unit,
            string ticketName, string groupName, long sales, long cost, string sellCode, int qty,
            string buyCode, string buyName, string parkCode, string parkName, long subtotal,
            string extId, string sellName, string typeCode)
            => new()
            {
                Id = id,
                BusinessDate = DateOnly.ParseExact(date, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture),
                PaymentType = pay,
                BookingCode = booking,
                UnitPrice = unit,
                TicketTypeName = ticketName,
                TicketGroupName = groupName,
                SalesAmount = sales,
                CostAmount = cost,
                SellingAgentCode = sellCode,
                Quantity = qty,
                BuyingAgentCode = buyCode,
                BuyingAgentName = buyName,
                ParkCodeSnapshot = parkCode,
                ParkNameSnapshot = parkName,
                Subtotal = subtotal,
                ExternalLineId = extId,
                SellingAgentName = sellName,
                TicketTypeCode = typeCode,
                ParentBuyingAgentName = "Oneinventory_Q R_ezCloud Mua_PL",
                ParentBuyingAgentCode = "5129",
                SourceType = SourceType.Api,
                CreatedAtUtc = SeedCreatedAtUtc,
            };

        modelBuilder.Entity<TicketSaleCostDetail>().HasData(
            D(1, ParkPaymentType.Prepaid, "2025-09-01", "60023151211", 177200, "Người lớn (Trên 1.4m)-Cuối tuần T7-CN", "SJC- Thủy cung Timescity", 886000, 875000, "7310", 5, "7034", "Oneinventory_SJC Thủy cung Lotte", "11810", "VinKE & Aquarium Times City ezCMT", 886000, "14409473", "Thủy cung VINKE-EZCMT", "347015"),
            D(2, ParkPaymentType.Prepaid, "2025-09-01", "60023151212", 120000, "Trẻ em (1.0–1.4m)-Cuối tuần T7-CN", "SJC- Thủy cung Timescity", 480000, 468000, "7310", 4, "7034", "Oneinventory_SJC Thủy cung Lotte", "11810", "VinKE & Aquarium Times City ezCMT", 480000, "14409474", "Thủy cung VINKE-EZCMT", "347016"),
            D(3, ParkPaymentType.Prepaid, "2025-09-02", "60023158801", 245000, "Người lớn-Ngày thường", "Sun Group - Sunworld", 1225000, 1200000, "6935", 5, "8012", "Oneinventory_SunWorld HN", "6935", "Sunworld Hà Nội", 1225000, "14510230", "Sun Group - ezCloud", "198432"),
            D(4, ParkPaymentType.Prepaid, "2025-09-02", "60023158802", 185000, "Trẻ em-Ngày thường", "Sun Group - Sunworld", 555000, 540000, "6935", 3, "8012", "Oneinventory_SunWorld HN", "6935", "Sunworld Hà Nội", 555000, "14510231", "Sun Group - ezCloud", "198433"),
            D(5, ParkPaymentType.Prepaid, "2025-09-03", "60023165411", 320000, "Người lớn-Cuối tuần", "Vinpearl - Cửa Hội", 960000, 945000, "9012", 3, "9034", "Oneinventory_VIN CUA HOI", "11682", "Vinpearl Cửa Hội", 960000, "14611020", "Vinpearl - ezCloud", "220110"),
            D(6, ParkPaymentType.Prepaid, "2025-09-03", "60023165500", 280000, "Người lớn-Ngày thường", "Vinpearl - Nam Hội An", 1120000, 1100000, "9014", 4, "9035", "Oneinventory_VIN NAM HOI AN", "11684", "Vinpearl Nam Hội An", 1120000, "14611100", "Vinpearl - ezCloud", "220115"),
            D(7, ParkPaymentType.Prepaid, "2025-09-04", "60023172011", 420000, "Người lớn-Ngày thường", "Vinpearl - Phú Quốc", 2100000, 2070000, "9016", 5, "9036", "Oneinventory_VIN PHU QUOC", "11686", "Vinpearl Phú Quốc", 2100000, "14712300", "Vinpearl - ezCloud", "220200"),
            D(8, ParkPaymentType.Prepaid, "2025-09-04", "60023172500", 195000, "Người lớn-Ngày thường", "Thủy cung Lotte", 975000, 956250, "7310", 5, "7034", "Oneinventory_SJC Thủy cung Lotte", "11810", "Thủy cung Lotte Hà Nội", 975000, "14712400", "Thủy cung VINKE-EZCMT", "347020"),
            D(9, ParkPaymentType.Prepaid, "2025-09-05", "60023179001", 177200, "Người lớn (Trên 1.4m)-Ngày thường", "SJC- Thủy cung Timescity", 708800, 695000, "7310", 4, "7034", "Oneinventory_SJC Thủy cung Lotte", "11810", "VinKE & Aquarium Times City ezCMT", 708800, "14813000", "Thủy cung VINKE-EZCMT", "347030"),
            D(10, ParkPaymentType.Debt, "2025-09-16", "60023199001", 85000, "Người lớn-Ngày thường", "Sơn Tiên - Công nợ", 425000, 415000, "5020", 5, "5034", "Oneinventory_Son Tien", "10360", "Sơn Tiên", 425000, "14300001", "Sơn Tiên - ezCloud", "180001"),
            D(11, ParkPaymentType.Debt, "2025-09-16", "60023199002", 120000, "Người lớn-Cuối tuần", "Mikazuki - Công nợ", 480000, 468000, "5022", 4, "5036", "Oneinventory_Mikazuki", "11423", "Mikazuki", 480000, "14300002", "Mikazuki - ezCloud", "180010"),
            D(12, ParkPaymentType.Debt, "2025-09-16", "60023199003", 95000, "Người lớn-Ngày thường", "Mekong - Công nợ", 285000, 278000, "5024", 3, "5038", "Oneinventory_Mekong", "11588", "Mekong", 285000, "14300003", "Mekong - ezCloud", "180020"),
            D(13, ParkPaymentType.Debt, "2025-09-17", "60023205001", 150000, "Người lớn-Cuối tuần", "Hồ Tràm - Công nợ", 600000, 585000, "5030", 4, "5044", "Oneinventory_Ho Tram", "11483", "Hồ Tràm", 600000, "14400010", "Hồ Tràm - ezCloud", "180050"),
            D(14, ParkPaymentType.Debt, "2025-09-17", "60023205002", 180000, "Người lớn-Ngày thường", "Nova Phan Thiết - Công nợ", 900000, 880000, "5032", 5, "5046", "Oneinventory_Nova", "11480", "Nova Phan Thiết", 900000, "14400011", "Nova - ezCloud", "180055"),
            D(15, ParkPaymentType.Debt, "2025-09-18", "60023211001", 110000, "Người lớn-Ngày thường", "Sealinks - Công nợ", 330000, 322000, "5040", 3, "5054", "Oneinventory_Sealinks", "11807", "Sealinks", 330000, "14500020", "Sealinks - ezCloud", "180090"));
    }

    private static void SeedReconciliationDemo(ModelBuilder modelBuilder)
    {
        static Park P(int id, string code, string name, ParkPaymentType pay, string bank)
            => new()
            {
                Id = id,
                Code = code,
                Name = name,
                PaymentType = pay,
                BankAccount = bank,
                Status = RecordStatus.Active,
                IsDeleted = false,
                CreatedAtUtc = SeedCreatedAtUtc,
            };

        modelBuilder.Entity<Park>().HasData(
            P(9001, "11681", "Bản Mòng", ParkPaymentType.Prepaid, "1SB2B24"),
            P(9002, "11682", "Vin Nam Hội An", ParkPaymentType.Prepaid, "1029876329"),
            P(9003, "11683", "Vin Phú Quốc", ParkPaymentType.Prepaid, "0091000593278"),
            P(9004, "11684", "Thủy Cung Lotte", ParkPaymentType.Prepaid, "700029610000"),
            P(9005, "11685", "Vin Cửa Hội", ParkPaymentType.Prepaid, "19139932758899"),
            P(9006, "11686", "Sealinks", ParkPaymentType.Prepaid, "1100030038237"),
            P(9007, "21001", "Sơn Tiên", ParkPaymentType.Debt, "57457"),
            P(9008, "21002", "Mikazuki", ParkPaymentType.Debt, "200077779999"),
            P(9009, "21003", "Mekong", ParkPaymentType.Debt, "60300641396"),
            P(9010, "21004", "Hồ Tràm", ParkPaymentType.Debt, "1027882298"),
            P(9011, "21005", "Nova Phan Thiết", ParkPaymentType.Debt, "3336333979"),
            P(9012, "21006", "Công viên nước Hồ Tây", ParkPaymentType.Debt, "11004009888"),
            P(9013, "21007", "Sightseeing HN", ParkPaymentType.Debt, "1100030038237"));

        static ParkReconciliation R(int id, int parkId, ParkPaymentType pay, string dateT, string dateT1,
            long prev, long add, long used, long expected, long actual, long variance)
            => new()
            {
                Id = id,
                ParkId = parkId,
                PaymentType = pay,
                BusinessDate = DateOnly.ParseExact(dateT, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture),
                PreviousBusinessDate = DateOnly.ParseExact(dateT1, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture),
                PreviousBalance = prev,
                AdditionalAmount = add,
                UsedAmount = used,
                ExpectedBalance = expected,
                ActualBalance = actual,
                VarianceAmount = variance,
                Status = variance == 0 ? ReconciliationStatus.Matched : ReconciliationStatus.Variance,
                RebuildCount = 0,
                CreatedAtUtc = SeedCreatedAtUtc,
            };

        modelBuilder.Entity<ParkReconciliation>().HasData(
            R(9001, 9001, ParkPaymentType.Prepaid, "2025-09-17", "2025-09-16", 0, 490000000, 0, 490000000, 490000000, 0),
            R(9002, 9002, ParkPaymentType.Prepaid, "2025-09-17", "2025-09-16", 150000000, 50000000, 35000000, 165000000, 165000000, 0),
            R(9003, 9003, ParkPaymentType.Prepaid, "2025-09-17", "2025-09-16", 200000000, 100000000, 45000000, 255000000, 250000000, -5000000),
            R(9004, 9004, ParkPaymentType.Prepaid, "2025-09-17", "2025-09-16", 80000000, 365625000, 120000000, 325625000, 327625000, 2000000),
            R(9005, 9005, ParkPaymentType.Prepaid, "2026-04-29", "2026-04-28", 0, 50000000, 12500000, 37500000, 37500000, 0),
            R(9006, 9006, ParkPaymentType.Prepaid, "2026-04-29", "2026-04-28", 25000000, 0, 8110000, 16890000, 16890000, 0),
            R(9007, 9007, ParkPaymentType.Debt, "2025-09-17", "2025-09-16", 120000000, 0, 42495000, 77505000, 77505000, 0),
            R(9008, 9008, ParkPaymentType.Debt, "2025-09-17", "2025-09-16", 95000000, 0, 35953000, 59047000, 59047000, 0),
            R(9009, 9009, ParkPaymentType.Debt, "2025-09-17", "2025-09-16", 150000000, 0, 49833500, 100166500, 99666500, -500000),
            R(9010, 9010, ParkPaymentType.Debt, "2025-09-17", "2025-09-16", 55000000, 0, 22850000, 32150000, 32150000, 0),
            R(9011, 9011, ParkPaymentType.Debt, "2025-09-17", "2025-09-16", 250000000, 0, 119995000, 130005000, 130005000, 0),
            R(9012, 9012, ParkPaymentType.Debt, "2025-09-17", "2025-09-16", 80000000, 0, 32082500, 47917500, 48917500, 1000000),
            R(9013, 9013, ParkPaymentType.Debt, "2025-09-17", "2025-09-16", 500000000, 0, 9996172999, -9496172999, -9496172999, 0));
    }

    private static void SeedBankTransactionDetails(ModelBuilder modelBuilder)
    {
        static BankTransactionDetail B(int id, ParkPaymentType pay, int y, int mo, int d, int hh, int mi, int ss,
            long debit, string content)
            => new()
            {
                Id = id,
                BusinessDate = new DateOnly(y, mo, d),
                TransactionAtUtc = new DateTime(y, mo, d, hh, mi, ss, DateTimeKind.Utc),
                PaymentType = pay,
                DebitAmount = debit,
                CreditAmount = 0,
                Content = content,
                SourceType = SourceType.Api,
                CreatedAtUtc = SeedCreatedAtUtc,
            };

        modelBuilder.Entity<BankTransactionDetail>().HasData(
            B(1, ParkPaymentType.Prepaid, 2025, 9, 17, 17, 33, 32, 490000000, "HBK-TKThe :1SB2B24, tại NCB. ND Top up Sunworld - ezCloud 17.09.2025 -CTLNHIDO000012817233009-1/1-PMT-002"),
            B(2, ParkPaymentType.Prepaid, 2026, 4, 28, 0, 0, 0, 50000000, "HBK-TKThe :19139932758899, tại Techcombank. ND ezCloud topup trien khai ban ve bang he thong API cho VIN CUA HOI ngay 28 04 2026 -CTLNHIDO000015124913428-1/1-PMT-002 244"),
            B(3, ParkPaymentType.Prepaid, 2026, 4, 28, 0, 0, 0, 50000000, "HBK-TKThe :1029876329, tại Vietcombank. ND ezCloud topup trien khai ban ve bang he thong API cho VIN NAM HOI AN ngay 28 04 2026 -CTLNHIDO000015124944210-1/1-PMT-002 245"),
            B(4, ParkPaymentType.Prepaid, 2026, 4, 28, 0, 0, 0, 490000000, "HBK-TKThe :1SB2B24, tại NCB. ND Top-up SUNWORLD ezCloud ngay 28 04 2026 -CTLNHIDO000015124947382-1/1-PMT-002 246"),
            B(5, ParkPaymentType.Prepaid, 2026, 4, 28, 0, 0, 0, 100000000, "HBK-TKThe :0091000593278, tại Vietcombank. ND ezCloud topup trien khai ban ve bang he thong API cho VIN PHU QUOC ngay 28 04 2026 -CTLNHIDO000015124982897-1/1-PMT-002 248"),
            B(6, ParkPaymentType.Prepaid, 2026, 4, 29, 0, 0, 0, 365625000, "HBK-TKThe :700029610000, tại Shinhan Bank V. ND ezCloud thanh toan nhap lo cho THUY CUNG LOTTE ngay 29 04 2026 lan 1 -CTLNHIDO000015134693760-1/1-PMT-002 243"),
            B(7, ParkPaymentType.Prepaid, 2026, 4, 29, 0, 0, 0, 237375000, "HBK-TKThe :700029610000, tại Shinhan Bank V. ND ezCloud thanh toan nhap lo cho THUY CUNG LOTTE ngay 29 04 2026 lan 2 -CTLNHIDO000015134727960-1/1-PMT-002 246"),
            B(8, ParkPaymentType.Debt, 2025, 9, 16, 0, 0, 0, 42495000, "HBK-TKThe :57457, tại Techcombank. ND ezCloud thanh toan cong no cho Son Tien ngay 16 09 2025 -CTLNHIDO-PMT-001"),
            B(9, ParkPaymentType.Debt, 2025, 9, 16, 0, 0, 0, 35953000, "HBK-TKThe :200077779999, tại Vietcombank. ND ezCloud thanh toan cong no cho Mikazuki ngay 16 09 2025 -CTLNHIDO-PMT-002"),
            B(10, ParkPaymentType.Debt, 2025, 9, 16, 0, 0, 0, 49833500, "HBK-TKThe :60300641396, tại Vietcombank. ND ezCloud thanh toan cong no cho Mekong ngay 16 09 2025 -CTLNHIDO-PMT-003"),
            B(11, ParkPaymentType.Debt, 2025, 9, 16, 0, 0, 0, 22850000, "HBK-TKThe :1027882298, tại Vietcombank. ND ezCloud thanh toan cong no cho Ho Tram ngay 16 09 2025 -CTLNHIDO-PMT-004"),
            B(12, ParkPaymentType.Debt, 2025, 9, 16, 0, 0, 0, 119995000, "HBK-TKThe :3336333979, tại Vietcombank. ND ezCloud thanh toan cong no cho Nova Phan Thiet ngay 16 09 2025 -CTLNHIDO-PMT-005"),
            B(13, ParkPaymentType.Debt, 2025, 9, 16, 0, 0, 0, 32082500, "HBK-TKThe :11004009888, tại Vietcombank. ND ezCloud thanh toan cong no cho Cong Vien Nuoc Ho Tay ngay 16 09 2025 -CTLNHIDO-PMT-006"),
            B(14, ParkPaymentType.Debt, 2025, 9, 16, 0, 0, 0, 8110000, "HBK-TKThe :1100030038237, tại Vietcombank. ND ezCloud thanh toan cong no cho Sealinks ngay 16 09 2025 -CTLNHIDO-PMT-007"),
            B(15, ParkPaymentType.Debt, 2025, 9, 16, 0, 0, 0, 9996172999, "HBK-TKThe :sightseeing, tại Vietcombank. ND ezCloud thanh toan cong no cho Sightseeing HN ngay 16 09 2025 -CTLNHIDO-PMT-008"));
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
