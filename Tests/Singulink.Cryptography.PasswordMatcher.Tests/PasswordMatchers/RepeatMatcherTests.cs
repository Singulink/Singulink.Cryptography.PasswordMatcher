namespace Singulink.Cryptography.Tests.PasswordMatchers;

[PrefixTestClass]
public class RepeatMatcherTests
{
    private static readonly PasswordMatcher Word = PasswordMatcher.Any([
        PasswordMatcher.Text("red", matchRepeats: false),
        PasswordMatcher.Text("blue", matchRepeats: false),
    ]);

    [TestMethod]
    public void WithinRange_IsMatch()
    {
        var matcher = PasswordMatcher.Repeat(Word, 2, 3);

        var result = PasswordMatcher.GetFirstMatch("redblue", [matcher]);
        result.ShouldNotBeNull();
        result.Items.Select(m => m.MatchedText).ShouldBe(["red", "blue"]);

        result = PasswordMatcher.GetFirstMatch("redredblue", [matcher]);
        result.ShouldNotBeNull();
        result.Items.Select(m => m.MatchedText).ShouldBe(["red", "red", "blue"]);
    }

    [TestMethod]
    public void OutsideRange_IsNotMatch()
    {
        var matcher = PasswordMatcher.Repeat(Word, 2, 3);

        PasswordMatcher.GetFirstMatch("red", [matcher]).ShouldBeNull();
        PasswordMatcher.GetFirstMatch("redbluebluered", [matcher]).ShouldBeNull();
    }

    [TestMethod]
    public void NonMatchingOccurrence_IsNotMatch()
    {
        var matcher = PasswordMatcher.Repeat(Word, 1, 3);

        PasswordMatcher.GetFirstMatch("redgreen", [matcher]).ShouldBeNull();
    }

    [TestMethod]
    public void FewestOccurrencesFirst()
    {
        // "aa" can be one occurrence of "aa" or two of "a"; the single occurrence is preferred.

        var options = PasswordMatcher.Any([
            PasswordMatcher.Text("a", checkSubstitutions: false, matchRepeats: false),
            PasswordMatcher.Text("aa", checkSubstitutions: false, matchRepeats: false),
        ]);

        var matcher = PasswordMatcher.Repeat(options, 1, 4);

        var result = PasswordMatcher.GetFirstMatch("aa", [matcher]);

        result.ShouldNotBeNull();
        result.Items.Select(m => m.MatchedText).ShouldBe(["aa"]);
    }

    [TestMethod]
    public void LongInput_CompletesQuickly()
    {
        // Each occurrence can end at many positions; the matcher must not explore every combination.

        var matcher = PasswordMatcher.Repeat(PasswordMatcher.RepeatedChars(1, 8, null, matchTrailingSeparator: false), 1, 6);
        string password = new string('a', 120) + "b";

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        PasswordMatcher.GetFirstMatch(password, [matcher]).ShouldBeNull();
        stopwatch.Elapsed.ShouldBeLessThan(TimeSpan.FromSeconds(1));
    }

    [TestMethod]
    public void InvalidArguments_Throw()
    {
        Should.Throw<ArgumentOutOfRangeException>(() => PasswordMatcher.Repeat(Word, 0, 3));
        Should.Throw<ArgumentOutOfRangeException>(() => PasswordMatcher.Repeat(Word, 3, 2));
        Should.Throw<ArgumentException>(() => PasswordMatcher.Repeat(PasswordMatcher.Optional(Word), 1, 3));
    }
}
