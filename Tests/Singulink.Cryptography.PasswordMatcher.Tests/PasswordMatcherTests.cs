namespace Singulink.Cryptography.Tests;

[PrefixTestClass]
public class PasswordMatcherTests
{
    [TestMethod]
    public void LeadingAndTrailingWhitespace_IsTrimmed()
    {
        var matcher = PasswordMatcher.Text("password");

        PasswordMatcher.GetFirstMatch("  password", [matcher]).ShouldNotBeNull();
        PasswordMatcher.GetFirstMatch("password  ", [matcher]).ShouldNotBeNull();
        PasswordMatcher.GetFirstMatch("\tpassword\r\n", [matcher]).ShouldNotBeNull();
    }

    [TestMethod]
    public void RepeatedSpaces_AreCollapsed()
    {
        var matcher = PasswordMatcher.Text("pass word");

        PasswordMatcher.GetFirstMatch("pass    word", [matcher]).ShouldNotBeNull();
    }

    [TestMethod]
    public void EmptyPassword_IsNotMatch()
    {
        PasswordMatcher.GetFirstMatch(string.Empty, [PasswordMatcher.Text("password")]).ShouldBeNull();
        PasswordMatcher.GetFirstMatch("   ", [PasswordMatcher.Text("password")]).ShouldBeNull();
    }

    [TestMethod]
    public void PartialMatch_IsNotMatch()
    {
        PasswordMatcher.GetFirstMatch("passwordX", [PasswordMatcher.Text("password")]).ShouldBeNull();
    }

    [TestMethod]
    public void MultipleMatchers_FirstMatchWins()
    {
        PasswordMatcher[] matchers = [
            PasswordMatcher.Text("pass"),
            PasswordMatcher.Sequence([PasswordMatcher.Text("pass"), PasswordMatcher.Text("word")]),
            PasswordMatcher.Text("password"),
        ];

        var result = PasswordMatcher.GetFirstMatch("password", matchers);

        result.ShouldNotBeNull();
        result.Items.Select(m => m.MatchedText).ShouldBe(["pass", "word"]);
    }

    [TestMethod]
    public void CustomValueMatcher_IsMatch()
    {
        var matcher = PasswordMatcher.Sequence([
            new VowelsMatcher(),
            PasswordMatcher.Text("x"),
        ]);

        var result = PasswordMatcher.GetFirstMatch("aeiouAEIOU.x", [matcher]);

        result.ShouldNotBeNull();
        result.Items.Select(m => m.MatchedText).ShouldBe(["aeiouaeiou", "x"]);
    }

    [TestMethod]
    public void CustomCompositeMatcher_IsMatch()
    {
        // Matches the inner matcher exactly twice.
        var matcher = new TwiceMatcher(PasswordMatcher.Text("ab", matchRepeats: false));

        var result = PasswordMatcher.GetFirstMatch("abab", [matcher]);
        result.ShouldNotBeNull();
        result.Items.Select(m => m.MatchedText).ShouldBe(["ab", "ab"]);

        PasswordMatcher.GetFirstMatch("ab", [matcher]).ShouldBeNull();
        PasswordMatcher.GetFirstMatch("ababab", [matcher]).ShouldBeNull();
    }

    [TestMethod]
    public void CreateChild_InvalidLength_Throws()
    {
        var matcher = new ContextProbeMatcher();

        Should.Throw<ArgumentOutOfRangeException>(() => PasswordMatcher.GetFirstMatch("abc", [matcher]));
    }

    private sealed class VowelsMatcher : ValueMatcher
    {
        public VowelsMatcher() : base(matchRepeats: false, matchTrailingSeparator: true) { }

        protected override IEnumerable<PasswordMatchContext> GetValueMatches(PasswordMatchContext context)
        {
            int length = 0;

            while (length < context.RemainingChars.Length && "aeiou".Contains(context.RemainingChars[length]))
                length++;

            if (length is 0)
                return [];

            return [context.CreateChild(length, new PasswordMatchItem(context.RemainingChars.Slice(0, length).ToString(), PasswordMatchType.Common))];
        }
    }

    private sealed class TwiceMatcher(PasswordMatcher inner) : PasswordMatcher
    {
        protected override IEnumerable<PasswordMatchContext> GetMatchesCore(PasswordMatchContext context)
        {
            foreach (var first in inner.GetMatches(context))
            {
                foreach (var second in inner.GetMatches(first))
                    yield return second;
            }
        }
    }

    private sealed class ContextProbeMatcher : PasswordMatcher
    {
        protected override IEnumerable<PasswordMatchContext> GetMatchesCore(PasswordMatchContext context)
        {
            context.OriginalPassword.ShouldBe("abc");
            context.CheckedPassword.ShouldBe("abc");
            context.TotalMatchedLength.ShouldBe(0);
            context.RemainingChars.ToString().ShouldBe("abc");

            var child = context.CreateChild(2, new PasswordMatchItem("ab", PasswordMatchType.Common));
            child.TotalMatchedLength.ShouldBe(2);
            child.RemainingChars.ToString().ShouldBe("c");

            Should.Throw<ArgumentOutOfRangeException>(() => child.CreateChild(-1, null));
            child.CreateChild(2, null); // Exceeds the remaining length
            yield break;
        }
    }
}
