using Microsoft.EntityFrameworkCore;
using PpvRecon.Application.Auth;
using PpvRecon.Domain.Entities.Identity;
using PpvRecon.Domain.Enums;
using PpvRecon.Infrastructure.Persistence;

namespace PpvRecon.Api.Commands;

public static class AdminSeedCommand
{
    public static bool ShouldRun(string[] args)
    {
        return args.Any(x => string.Equals(x, "seed-admin", StringComparison.OrdinalIgnoreCase));
    }

    public static async Task RunAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PpvReconDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        await dbContext.Database.MigrateAsync(cancellationToken);

        Console.WriteLine("Tao tai khoan Admin dau tien cho PpvRecon.");
        Console.WriteLine("Email se la dinh danh dang nhap va khong nen doi sau khi tao.");
        Console.WriteLine();

        var hasAdmin = await dbContext.Users
            .AnyAsync(x => x.Role == UserRole.Admin && !x.IsDeleted, cancellationToken);

        if (hasAdmin && !Confirm("Da co tai khoan Admin. Ban co muon tao them Admin khong? (y/N): "))
        {
            Console.WriteLine("Da huy tao Admin.");
            return;
        }

        var fullName = ReadRequired("Ho ten: ");
        var email = ReadRequired("Email: ").Trim();
        var normalizedEmail = email.ToUpperInvariant();
        var phoneNumber = ReadOptional("So dien thoai (bo trong neu khong co): ");

        var emailExists = await dbContext.Users
            .AnyAsync(x => x.NormalizedEmail == normalizedEmail && !x.IsDeleted, cancellationToken);

        if (emailExists)
        {
            Console.WriteLine("Email nay da ton tai. Vui long dung email khac.");
            return;
        }

        string password;
        while (true)
        {
            password = ReadPassword("Mat khau: ");
            var confirmPassword = ReadPassword("Nhap lai mat khau: ");

            if (password != confirmPassword)
            {
                Console.WriteLine("Mat khau nhap lai khong khop.");
                continue;
            }

            var passwordError = ValidatePassword(password);
            if (passwordError is not null)
            {
                Console.WriteLine(passwordError);
                continue;
            }

            break;
        }

        var nowUtc = DateTime.UtcNow;
        var user = new User
        {
            FullName = fullName,
            Email = email,
            NormalizedEmail = normalizedEmail,
            PhoneNumber = string.IsNullOrWhiteSpace(phoneNumber) ? null : phoneNumber,
            PasswordHash = passwordHasher.HashPassword(password),
            Role = UserRole.Admin,
            Status = UserStatus.Active,
            FailedLoginCount = 0,
            PasswordChangedAtUtc = nowUtc,
            CreatedAtUtc = nowUtc,
            IsDeleted = false,
        };

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync(cancellationToken);

        Console.WriteLine();
        Console.WriteLine($"Da tao Admin: {email}");
    }

    private static string ReadRequired(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            var value = Console.ReadLine()?.Trim();
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            Console.WriteLine("Gia tri nay la bat buoc.");
        }
    }

    private static string? ReadOptional(string prompt)
    {
        Console.Write(prompt);
        return Console.ReadLine()?.Trim();
    }

    private static bool Confirm(string prompt)
    {
        Console.Write(prompt);
        var value = Console.ReadLine()?.Trim();
        return string.Equals(value, "y", StringComparison.OrdinalIgnoreCase)
            || string.Equals(value, "yes", StringComparison.OrdinalIgnoreCase);
    }

    private static string ReadPassword(string prompt)
    {
        Console.Write(prompt);
        if (Console.IsInputRedirected)
        {
            return Console.ReadLine() ?? string.Empty;
        }

        var password = new List<char>();

        while (true)
        {
            var key = Console.ReadKey(intercept: true);
            if (key.Key == ConsoleKey.Enter)
            {
                Console.WriteLine();
                return new string(password.ToArray());
            }

            if (key.Key == ConsoleKey.Backspace)
            {
                if (password.Count > 0)
                {
                    password.RemoveAt(password.Count - 1);
                }

                continue;
            }

            password.Add(key.KeyChar);
        }
    }

    private static string? ValidatePassword(string password)
    {
        if (password.Length < 8)
        {
            return "Mat khau phai co it nhat 8 ky tu.";
        }

        if (!password.Any(char.IsUpper))
        {
            return "Mat khau phai co it nhat 1 chu hoa.";
        }

        if (!password.Any(char.IsLower))
        {
            return "Mat khau phai co it nhat 1 chu thuong.";
        }

        if (!password.Any(char.IsDigit))
        {
            return "Mat khau phai co it nhat 1 chu so.";
        }

        if (!password.Any(ch => !char.IsLetterOrDigit(ch)))
        {
            return "Mat khau phai co it nhat 1 ky tu dac biet.";
        }

        return null;
    }
}
