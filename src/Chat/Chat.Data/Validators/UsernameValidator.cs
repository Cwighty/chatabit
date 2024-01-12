namespace Chat.Data.Validators;

public class UsernameValidator
{
    public static bool IsValid(string username)
    {
        return !string.IsNullOrWhiteSpace(username);
    }
}
