namespace Singulink.Cryptography.Tests.PasswordMatchers;

[PrefixTestClass]
public class AnyCharsMatcherTests
{
    private static readonly PasswordMatcher Digits = PasswordMatcher.AnyChars(1, 4, char.IsDigit);

    [TestMethod]
    public void WithinRange_IsMatch()
    {
        var result = PasswordMatcher.GetFirstMatch("4792", [Digits]);

        result.ShouldNotBeNull();
        result.Items.Select(m => m.MatchedText).ShouldBe(["4792"]);
        result.Items[0].MatchType.ShouldBe(PasswordMatchType.AnyChars);

        PasswordMatcher.GetFirstMatch("4", [Digits]).ShouldNotBeNull();
    }

    [TestMethod]
    public void OutsideRange_IsNotMatch()
    {
        PasswordMatcher.GetFirstMatch("47921", [Digits]).ShouldBeNull();
        PasswordMatcher.GetFirstMatch(string.Empty, [Digits]).ShouldBeNull();
    }

    [TestMethod]
    public void CharFilter_IsRespected()
    {
        PasswordMatcher.GetFirstMatch("47a", [Digits]).ShouldBeNull();
        PasswordMatcher.GetFirstMatch("47a", [PasswordMatcher.AnyChars(1, 4, null)]).ShouldNotBeNull();
    }

    [TestMethod]
    public void ShortestMatchFirst()
    {
        // The arbitrary characters should give way to the structured matcher that follows.

        var matcher = PasswordMatcher.Sequence([Digits, PasswordMatcher.CharSequence(CharSequenceTypes.KeyboardRow)]);

        var result = PasswordMatcher.GetFirstMatch("7123", [matcher]);

        result.ShouldNotBeNull();
        result.Items.Select(m => m.MatchedText).ShouldBe(["7", "123"]);
    }

    [TestMethod]
    public void InvalidArguments_Throw()
    {
        Should.Throw<ArgumentOutOfRangeException>(() => PasswordMatcher.AnyChars(0, 4, null));
        Should.Throw<ArgumentOutOfRangeException>(() => PasswordMatcher.AnyChars(4, 2, null));
    }
}
