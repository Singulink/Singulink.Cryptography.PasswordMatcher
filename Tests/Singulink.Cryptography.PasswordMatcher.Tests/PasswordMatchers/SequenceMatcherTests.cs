namespace Singulink.Cryptography.Tests.PasswordMatchers;

[PrefixTestClass]
public class SequenceMatcherTests
{
    [TestMethod]
    public void AllMatchers_IsMatch()
    {
        var matcher = PasswordMatcher.Sequence([PasswordMatcher.Text("hello"), PasswordMatcher.Text("world")]);

        var result = PasswordMatcher.GetFirstMatch("helloworld", [matcher]);

        result.ShouldNotBeNull();
        result.Items.Select(m => m.MatchedText).ShouldBe(["hello", "world"]);
    }

    [TestMethod]
    public void MissingMatcher_IsNotMatch()
    {
        var matcher = PasswordMatcher.Sequence([PasswordMatcher.Text("hello"), PasswordMatcher.Text("world")]);

        PasswordMatcher.GetFirstMatch("hello", [matcher]).ShouldBeNull();
        PasswordMatcher.GetFirstMatch("world", [matcher]).ShouldBeNull();
        PasswordMatcher.GetFirstMatch("worldhello", [matcher]).ShouldBeNull();
    }

    [TestMethod]
    public void OptionalInMiddle_IsMatch()
    {
        var matcher = PasswordMatcher.Sequence([
            PasswordMatcher.Text("hello"),
            PasswordMatcher.Optional(PasswordMatcher.Text("big")),
            PasswordMatcher.Text("world"),
        ]);

        var result = PasswordMatcher.GetFirstMatch("hellobigworld", [matcher]);
        result.ShouldNotBeNull();
        result.Items.Select(m => m.MatchedText).ShouldBe(["hello", "big", "world"]);

        result = PasswordMatcher.GetFirstMatch("helloworld", [matcher]);
        result.ShouldNotBeNull();
        result.Items.Select(m => m.MatchedText).ShouldBe(["hello", "world"]);
    }

    [TestMethod]
    public void Backtracking_IsMatch()
    {
        // The first matcher can match "aa" or "aaaa" (via repeats); only the shorter match lets the second matcher succeed.

        var matcher = PasswordMatcher.Sequence([
            PasswordMatcher.Text("aa", checkSubstitutions: false),
            PasswordMatcher.Text("aab", checkSubstitutions: false),
        ]);

        var result = PasswordMatcher.GetFirstMatch("aaaab", [matcher]);

        result.ShouldNotBeNull();
        result.Items.Select(m => m.MatchedText).ShouldBe(["aa", "aab"]);
    }

    [TestMethod]
    public void OnlyOptionalMatchers_Throws()
    {
        Should.Throw<ArgumentException>(() => PasswordMatcher.Sequence([PasswordMatcher.Optional(PasswordMatcher.Text("cat"))]));
    }
}
