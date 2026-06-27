namespace PpvRecon.Application.Auth;

public static class PasswordPolicy
{
    public static string? Validate(string password)
    {
        if (password.Length < 8)
        {
            return "Mật khẩu phải có ít nhất 8 ký tự.";
        }

        if (!password.Any(char.IsUpper))
        {
            return "Mật khẩu phải có ít nhất 1 chữ hoa.";
        }

        if (!password.Any(char.IsLower))
        {
            return "Mật khẩu phải có ít nhất 1 chữ thường.";
        }

        if (!password.Any(char.IsDigit))
        {
            return "Mật khẩu phải có ít nhất 1 chữ số.";
        }

        if (!password.Any(ch => !char.IsLetterOrDigit(ch)))
        {
            return "Mật khẩu phải có ít nhất 1 ký tự đặc biệt.";
        }

        return null;
    }
}
