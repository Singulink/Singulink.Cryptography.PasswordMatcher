namespace Singulink.Cryptography.Tests.PasswordMatchers;

[PrefixTestClass]
public class SegmentMatcherTests
{
    [TestMethod]
    public void Repeats_IsMatch()
    {
        var matcher = PasswordMatcher.Text("me", checkSubstitutions: false, matchRepeats: true, matchTrailingSeparator: false);

        var result = PasswordMatcher.GetFirstMatch("memememememememememe", [matcher]);

        result.ShouldNotBeNull();
        result.Items.Select(m => m.MatchedText).ShouldBe(["me", "me", "me", "me", "me", "me", "me", "me", "me", "me"]);
    }
}
