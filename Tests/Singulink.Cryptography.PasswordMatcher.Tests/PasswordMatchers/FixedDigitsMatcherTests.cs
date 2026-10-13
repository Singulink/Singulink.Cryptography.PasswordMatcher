namespace Singulink.Cryptography.Tests.PasswordMatchers;

[PrefixTestClass]
public class FixedDigitsMatcherTests
{
    private static readonly PasswordMatcher YearMatcher = PasswordMatcher.FixedDigits(4, 1900, 2100, PasswordMatchType.Common);

    [TestMethod]
    public void InRange_IsMatch()
    {
        var result = PasswordMatcher.GetFirstMatch("1985", [YearMatcher]);

        result.ShouldNotBeNull();
        result.Items.Select(m => m.MatchedText).ShouldBe(["1985"]);
        result.Items[0].MatchType.ShouldBe(PasswordMatchType.Common);
    }

    [TestMethod]
    public void OutOfRange_IsNotMatch()
    {
        PasswordMatcher.GetFirstMatch("1899", [YearMatcher]).ShouldBeNull();
        PasswordMatcher.GetFirstMatch("2101", [YearMatcher]).ShouldBeNull();
    }

    [TestMethod]
    public void WrongLength_IsNotMatch()
    {
        PasswordMatcher.GetFirstMatch("198", [YearMatcher]).ShouldBeNull();
        PasswordMatcher.GetFirstMatch("19855", [YearMatcher]).ShouldBeNull();
    }

    [TestMethod]
    public void NonDigits_IsNotMatch()
    {
        PasswordMatcher.GetFirstMatch("19x5", [YearMatcher]).ShouldBeNull();
        PasswordMatcher.GetFirstMatch("+985", [YearMatcher]).ShouldBeNull();
    }

    [TestMethod]
    public void Repeats_IsMatch()
    {
        var result = PasswordMatcher.GetFirstMatch("19851985", [YearMatcher]);

        result.ShouldNotBeNull();
        result.Items.Select(m => m.MatchedText).ShouldBe(["1985", "1985"]);
    }

    [TestMethod]
    public void AnyValue_IsMatch()
    {
        var matcher = PasswordMatcher.FixedDigits(3, PasswordMatchType.Common);

        PasswordMatcher.GetFirstMatch("000", [matcher]).ShouldNotBeNull();
        PasswordMatcher.GetFirstMatch("999", [matcher]).ShouldNotBeNull();
        PasswordMatcher.GetFirstMatch("99", [matcher]).ShouldBeNull();
    }

    [TestMethod]
    public void InvalidArguments_Throw()
    {
        Should.Throw<ArgumentOutOfRangeException>(() => PasswordMatcher.FixedDigits(0, PasswordMatchType.Common));
        Should.Throw<ArgumentOutOfRangeException>(() => PasswordMatcher.FixedDigits(10, PasswordMatchType.Common));
        Should.Throw<ArgumentOutOfRangeException>(() => PasswordMatcher.FixedDigits(2, 50, 10, PasswordMatchType.Common));
        Should.Throw<ArgumentOutOfRangeException>(() => PasswordMatcher.FixedDigits(2, 0, 100, PasswordMatchType.Common));
        Should.Throw<ArgumentOutOfRangeException>(() => PasswordMatcher.FixedDigits(2, -1, 10, PasswordMatchType.Common));
        Should.Throw<ArgumentOutOfRangeException>(() => PasswordMatcher.FixedDigits(2, (PasswordMatchType)100));
    }
}
