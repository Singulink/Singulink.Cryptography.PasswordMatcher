namespace Singulink.Cryptography.Tests.PasswordMatchers;

[PrefixTestClass]
public class OptionalMatcherTests
{
    private static readonly PasswordMatcher Matcher = PasswordMatcher.Sequence([
        PasswordMatcher.Optional(PasswordMatcher.Text("the")),
        PasswordMatcher.Text("cat"),
    ]);

    [TestMethod]
    public void Present_IsMatch()
    {
        var result = PasswordMatcher.GetFirstMatch("thecat", [Matcher]);

        result.ShouldNotBeNull();
        result.Items.Select(m => m.MatchedText).ShouldBe(["the", "cat"]);
    }

    [TestMethod]
    public void Absent_IsMatch()
    {
        var result = PasswordMatcher.GetFirstMatch("cat", [Matcher]);

        result.ShouldNotBeNull();
        result.Items.Select(m => m.MatchedText).ShouldBe(["cat"]);
    }

    [TestMethod]
    public void OptionalAlone_DoesNotMatchEmpty()
    {
        var result = PasswordMatcher.GetFirstMatch(string.Empty, [PasswordMatcher.Optional(PasswordMatcher.Text("cat"))]);

        // An empty password trivially matches an optional matcher, which is why sequences require at least one non-optional matcher.
        result.ShouldNotBeNull();
        result.Items.ShouldBeEmpty();
    }
}
