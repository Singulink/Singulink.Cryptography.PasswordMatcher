namespace Singulink.Cryptography.Tests.PasswordMatchers;

[PrefixTestClass]
public class RepeatedCharMatcherTests
{
    [TestMethod]
    public void WithinRange_IsMatch()
    {
        var matcher = PasswordMatcher.RepeatedChars(2, 4, null);

        var result = PasswordMatcher.GetFirstMatch("aaa", [matcher]);

        result.ShouldNotBeNull();
        result.Items.Select(m => m.MatchedText).ShouldBe(["aaa"]);
        result.Items[0].MatchType.ShouldBe(PasswordMatchType.RepeatedChar);
    }

    [TestMethod]
    public void OutsideRange_IsNotMatch()
    {
        var matcher = PasswordMatcher.RepeatedChars(2, 4, null);

        PasswordMatcher.GetFirstMatch("a", [matcher]).ShouldBeNull();
        PasswordMatcher.GetFirstMatch("aaaaa", [matcher]).ShouldBeNull();
    }

    [TestMethod]
    public void DifferentChars_IsNotMatch()
    {
        var matcher = PasswordMatcher.RepeatedChars(null);

        PasswordMatcher.GetFirstMatch("aab", [matcher]).ShouldBeNull();
    }

    [TestMethod]
    public void CharFilter_IsMatch()
    {
        var matcher = PasswordMatcher.RepeatedChars(c => c is >= '0' and <= '9');

        PasswordMatcher.GetFirstMatch("111", [matcher]).ShouldNotBeNull();
        PasswordMatcher.GetFirstMatch("aaa", [matcher]).ShouldBeNull();
    }

    [TestMethod]
    public void LongestMatchFirst()
    {
        // The matcher must prefer the longest run so the following matcher sees "b" rather than more "a"s.

        var matcher = PasswordMatcher.Sequence([
            PasswordMatcher.RepeatedChars(null, matchTrailingSeparator: false),
            PasswordMatcher.Text("b"),
        ]);

        var result = PasswordMatcher.GetFirstMatch("aaaab", [matcher]);

        result.ShouldNotBeNull();
        result.Items.Select(m => m.MatchedText).ShouldBe(["aaaa", "b"]);
    }

    [TestMethod]
    public void TrailingSeparator_IsMatch()
    {
        var matcher = PasswordMatcher.RepeatedChars(null);

        var result = PasswordMatcher.GetFirstMatch("!!!.", [matcher]);

        result.ShouldNotBeNull();
        result.Items.Select(m => m.MatchedText).ShouldBe(["!!!"]);
    }

    [TestMethod]
    public void InvalidArguments_Throw()
    {
        Should.Throw<ArgumentOutOfRangeException>(() => PasswordMatcher.RepeatedChars(0, 4, null));
        Should.Throw<ArgumentOutOfRangeException>(() => PasswordMatcher.RepeatedChars(4, 2, null));
    }
}
