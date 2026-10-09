using System.Text.RegularExpressions;

namespace Lab02.UsersApi;

public sealed record UserWriteRequest(string? Login, string? PassHash);
public sealed record UserResponse(int Id, string Login);
public sealed record ErrorResponse(string Message);
public sealed record DeleteResponse(int Id, string Message);

public static class UserValidation
{
    public static string? Validate(UserWriteRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Login) ||
            request.Login.Length is < 3 or > 50 ||
            !Regex.IsMatch(request.Login, @"\A[\p{L}\p{N}_.-]+\z"))
            return "Login must contain 3-50 letters, digits, _, . or -";
        if (request.PassHash is null ||
            !Regex.IsMatch(request.PassHash, @"\A[0-9a-fA-F]{64}\z"))
            return "PassHash must be a SHA-256 hash: 64 hexadecimal characters";
        return null;
    }
}
