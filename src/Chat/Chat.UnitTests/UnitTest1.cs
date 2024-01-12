using Chat.Data.Validators;

namespace Chat.UnitTests;

public class Tests
{
    [Test]
    public void NotEmptyUsernameShouldValidateTrue()
    {
        var username = "test";
        var result = UsernameValidator.IsValid(username);
        Assert.IsTrue(result);
    }

    [Test]
    public void EmptyUsernameShouldValidateFalse()
    {
        var username = "";
        var result = UsernameValidator.IsValid(username);
        Assert.IsFalse(result);
    }
}
