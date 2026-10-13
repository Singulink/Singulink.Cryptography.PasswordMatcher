namespace Singulink.Cryptography.Tests.PasswordMatchers;

[PrefixTestClass]
public class AnyMatcherTests
{
    private static readonly PasswordMatcher Matcher = PasswordMatcher.Any([PasswordMatcher.Text("cat"), PasswordMatcher.Text("dog")]);

    [TestMethod]
    public void FirstOption_IsMatch()
    {
        var result = PasswordMatcher.GetFirstMatch("cat", [Matcher]);

        result.ShouldNotBeNull();
        result.Items.Select(m => m.MatchedText).ShouldBe(["cat"]);
    }

    [TestMethod]
    public void SecondOption_IsMatch()
    {
        var result = PasswordMatcher.GetFirstMatch("dog", [Matcher]);

        result.ShouldNotBeNull();
        result.Items.Select(m => m.MatchedText).ShouldBe(["dog"]);
    }

    [TestMethod]
    public void NoOption_IsNotMatch()
    {
        PasswordMatcher.GetFirstMatch("cow", [Matcher]).ShouldBeNull();
    }

    [TestMethod]
    public void OptionalMatcher_Throws()
    {
        Should.Throw<ArgumentException>(() => PasswordMatcher.Any([PasswordMatcher.Optional(PasswordMatcher.Text("cat"))]));
    }
}
