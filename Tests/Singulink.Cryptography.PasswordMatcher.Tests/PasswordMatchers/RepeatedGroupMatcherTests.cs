namespace Singulink.Cryptography.Tests.PasswordMatchers;

[PrefixTestClass]
public class RepeatedGroupMatcherTests
{
    private static readonly PasswordMatcher Matcher = PasswordMatcher.RepeatedGroups(2, 3, 2, null);

    [TestMethod]
    public void TwoCharGroup_IsMatch()
    {
        var result = PasswordMatcher.GetFirstMatch("121212", [Matcher]);

        result.ShouldNotBeNull();
        result.Items.Select(m => m.MatchedText).ShouldBe(["121212"]);
        result.Items[0].MatchType.ShouldBe(PasswordMatchType.RepeatedGroup);
    }

    [TestMethod]
    public void ThreeCharGroup_IsMatch()
    {
        var result = PasswordMatcher.GetFirstMatch("abcabc", [Matcher]);

        result.ShouldNotBeNull();
        result.Items.Select(m => m.MatchedText).ShouldBe(["abcabc"]);
    }

    [TestMethod]
    public void SingleOccurrence_IsNotMatch()
    {
        PasswordMatcher.GetFirstMatch("12", [Matcher]).ShouldBeNull();
        PasswordMatcher.GetFirstMatch("1213", [Matcher]).ShouldBeNull();
    }

    [TestMethod]
    public void SameCharGroup_IsNotMatch()
    {
        // Runs of one character are left to the repeated character matcher.
        PasswordMatcher.GetFirstMatch("1111", [Matcher]).ShouldBeNull();
    }

    [TestMethod]
    public void PartialGroup_IsNotMatch()
    {
        PasswordMatcher.GetFirstMatch("12121", [Matcher]).ShouldBeNull();
    }

    [TestMethod]
    public void LongestMatchFirst()
    {
        var matcher = PasswordMatcher.Sequence([Matcher, PasswordMatcher.Text("x")]);

        var result = PasswordMatcher.GetFirstMatch("12121212x", [matcher]);

        result.ShouldNotBeNull();
        result.Items.Select(m => m.MatchedText).ShouldBe(["12121212", "x"]);
    }

    [TestMethod]
    public void CharFilter_IsMatch()
    {
        var matcher = PasswordMatcher.RepeatedGroups(2, 3, 2, char.IsDigit);

        PasswordMatcher.GetFirstMatch("131313", [matcher]).ShouldNotBeNull();
        PasswordMatcher.GetFirstMatch("abab", [matcher]).ShouldBeNull();
    }

    [TestMethod]
    public void MinRepeats_IsRespected()
    {
        var matcher = PasswordMatcher.RepeatedGroups(2, 2, 3, null);

        PasswordMatcher.GetFirstMatch("1212", [matcher]).ShouldBeNull();
        PasswordMatcher.GetFirstMatch("121212", [matcher]).ShouldNotBeNull();
    }

    [TestMethod]
    public void InvalidArguments_Throw()
    {
        Should.Throw<ArgumentOutOfRangeException>(() => PasswordMatcher.RepeatedGroups(1, 3, 2, null));
        Should.Throw<ArgumentOutOfRangeException>(() => PasswordMatcher.RepeatedGroups(3, 2, 2, null));
        Should.Throw<ArgumentOutOfRangeException>(() => PasswordMatcher.RepeatedGroups(2, 3, 1, null));
    }
}
