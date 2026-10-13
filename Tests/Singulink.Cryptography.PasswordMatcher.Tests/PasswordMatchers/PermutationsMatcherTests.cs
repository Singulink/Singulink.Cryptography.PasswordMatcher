namespace Singulink.Cryptography.Tests.PasswordMatchers;

[PrefixTestClass]
public class PermutationsMatcherTests
{
    private static PasswordMatcher CreateMatcher(bool mustMatchAll)
    {
        PasswordMatcher[] matchers = [
            PasswordMatcher.Text("red", matchRepeats: false),
            PasswordMatcher.Text("blue", matchRepeats: false),
            PasswordMatcher.Text("green", matchRepeats: false),
        ];

        return PasswordMatcher.Permutations(matchers, mustMatchAll);
    }

    [TestMethod]
    public void AnyOrder_IsMatch()
    {
        var matcher = CreateMatcher(mustMatchAll: false);

        var result = PasswordMatcher.GetFirstMatch("bluered", [matcher]);
        result.ShouldNotBeNull();
        result.Items.Select(m => m.MatchedText).ShouldBe(["blue", "red"]);

        result = PasswordMatcher.GetFirstMatch("greenbluered", [matcher]);
        result.ShouldNotBeNull();
        result.Items.Select(m => m.MatchedText).ShouldBe(["green", "blue", "red"]);
    }

    [TestMethod]
    public void Subset_IsMatch()
    {
        var matcher = CreateMatcher(mustMatchAll: false);

        var result = PasswordMatcher.GetFirstMatch("green", [matcher]);

        result.ShouldNotBeNull();
        result.Items.Select(m => m.MatchedText).ShouldBe(["green"]);
    }

    [TestMethod]
    public void MatcherUsedTwice_IsNotMatch()
    {
        var matcher = CreateMatcher(mustMatchAll: false);

        PasswordMatcher.GetFirstMatch("redred", [matcher]).ShouldBeNull();
        PasswordMatcher.GetFirstMatch("redbluered", [matcher]).ShouldBeNull();
    }

    [TestMethod]
    public void MustMatchAll_Subset_IsNotMatch()
    {
        var matcher = CreateMatcher(mustMatchAll: true);

        PasswordMatcher.GetFirstMatch("red", [matcher]).ShouldBeNull();
        PasswordMatcher.GetFirstMatch("bluered", [matcher]).ShouldBeNull();
    }

    [TestMethod]
    public void MustMatchAll_All_IsMatch()
    {
        var matcher = CreateMatcher(mustMatchAll: true);

        var result = PasswordMatcher.GetFirstMatch("redgreenblue", [matcher]);

        result.ShouldNotBeNull();
        result.Items.Select(m => m.MatchedText).ShouldBe(["red", "green", "blue"]);
    }

    [TestMethod]
    public void Empty_IsNotMatch()
    {
        PasswordMatcher.GetFirstMatch(string.Empty, [CreateMatcher(mustMatchAll: false)]).ShouldBeNull();
    }

    [TestMethod]
    public void DuplicateMatcher_Throws()
    {
        var text = PasswordMatcher.Text("red");
        Should.Throw<ArgumentException>(() => PasswordMatcher.Permutations([text, text]));
    }

    [TestMethod]
    public void OptionalMatcher_Throws()
    {
        Should.Throw<ArgumentException>(() => PasswordMatcher.Permutations([PasswordMatcher.Optional(PasswordMatcher.Text("red"))]));
    }

    [TestMethod]
    public void TooManyMatchers_Throws()
    {
        var matchers = Enumerable.Range(0, PermutationsMatcher.MaxMatchers + 1).Select(i => PasswordMatcher.Text("m" + i));
        Should.Throw<ArgumentException>(() => PasswordMatcher.Permutations(matchers));
    }
}
