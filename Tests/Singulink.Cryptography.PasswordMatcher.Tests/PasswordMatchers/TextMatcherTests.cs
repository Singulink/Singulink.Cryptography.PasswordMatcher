namespace Singulink.Cryptography.Tests.PasswordMatchers;

[PrefixTestClass]
public class TextMatcherTests
{
    [TestMethod]
    public void Repeats_IsMatch()
    {
        var matcher = PasswordMatcher.Text("me", checkSubstitutions: false, matchRepeats: true, matchTrailingSeparator: false);

        var result = PasswordMatcher.GetFirstMatch("memememememememememe", [matcher]);

        result.ShouldNotBeNull();
        result.Items.Select(m => m.MatchedText).ShouldBe(["me", "me", "me", "me", "me", "me", "me", "me", "me", "me"]);
        result.Items[0].MatchType.ShouldBe(PasswordMatchType.Common);
        result.Items[1].MatchType.ShouldBe(PasswordMatchType.RepeatValue);
    }

    [TestMethod]
    public void NoRepeats_IsNotMatch()
    {
        var matcher = PasswordMatcher.Text("me", matchRepeats: false);

        PasswordMatcher.GetFirstMatch("meme", [matcher]).ShouldBeNull();
        PasswordMatcher.GetFirstMatch("me", [matcher]).ShouldNotBeNull();
    }

    [TestMethod]
    public void Substitutions_IsMatch()
    {
        var matcher = PasswordMatcher.Text("password");

        var result = PasswordMatcher.GetFirstMatch("p@55w0rd", [matcher]);

        result.ShouldNotBeNull();
        result.Items.Select(m => m.MatchedText).ShouldBe(["password"]);
    }

    [TestMethod]
    public void SubstitutionsDisabled_IsNotMatch()
    {
        var matcher = PasswordMatcher.Text("password", checkSubstitutions: false);

        PasswordMatcher.GetFirstMatch("p@ssword", [matcher]).ShouldBeNull();
        PasswordMatcher.GetFirstMatch("password", [matcher]).ShouldNotBeNull();
    }

    [TestMethod]
    public void CaseInsensitive_IsMatch()
    {
        var matcher = PasswordMatcher.Text("PassWord");

        var result = PasswordMatcher.GetFirstMatch("PASSWORD", [matcher]);

        result.ShouldNotBeNull();
        result.Items.Select(m => m.MatchedText).ShouldBe(["password"]);
    }

    [TestMethod]
    public void TrailingSeparators_IsMatch()
    {
        var matcher = PasswordMatcher.Text("password");

        foreach (string separator in new[] { " ", ".", "-", "_", "," })
        {
            var result = PasswordMatcher.GetFirstMatch("password" + separator, [matcher]);
            result.ShouldNotBeNull(separator);
            result.Items.Select(m => m.MatchedText).ShouldBe(["password"]);
        }

        PasswordMatcher.GetFirstMatch("password!", [matcher]).ShouldBeNull();
        PasswordMatcher.GetFirstMatch("password..", [matcher]).ShouldBeNull();
    }

    [TestMethod]
    public void TrailingSeparatorDisabled_IsNotMatch()
    {
        var matcher = PasswordMatcher.Text("password", matchTrailingSeparator: false);

        PasswordMatcher.GetFirstMatch("password.", [matcher]).ShouldBeNull();
    }

    [TestMethod]
    public void RepeatsWithSeparators_IsMatch()
    {
        var matcher = PasswordMatcher.Text("me");

        var result = PasswordMatcher.GetFirstMatch("me.me me-me", [matcher]);

        result.ShouldNotBeNull();
        result.Items.Select(m => m.MatchedText).ShouldBe(["me", "me", "me", "me"]);
    }

    [TestMethod]
    public void RepeatsRequireSameCharacters()
    {
        // Repeats must use the exact same characters as the original match, substitutions and all.

        var matcher = PasswordMatcher.Text("password");

        PasswordMatcher.GetFirstMatch("p@sswordp@ssword", [matcher]).ShouldNotBeNull();
        PasswordMatcher.GetFirstMatch("p@sswordpassword", [matcher]).ShouldBeNull();
    }

    [TestMethod]
    public void Partial_IsNotMatch()
    {
        var matcher = PasswordMatcher.Text("password");

        PasswordMatcher.GetFirstMatch("passwor", [matcher]).ShouldBeNull();
        PasswordMatcher.GetFirstMatch("passwords", [matcher]).ShouldBeNull();
    }

    [TestMethod]
    public void EmptyText_Throws()
    {
        Should.Throw<ArgumentException>(() => PasswordMatcher.Text(string.Empty));
        Should.Throw<ArgumentException>(() => PasswordMatcher.Text("   "));
    }

    [TestMethod]
    public void MatchedChars_AreTheActualCharacters()
    {
        var result = PasswordMatcher.GetFirstMatch("P@55w0rd.p@55w0rd", [PasswordMatcher.Text("password")]);

        result.ShouldNotBeNull();
        result.Items.Select(m => m.MatchedText).ShouldBe(["password", "password"]);
        result.Items.Select(m => m.MatchedChars).ShouldBe(["p@55w0rd", "p@55w0rd"]);

        result = PasswordMatcher.GetFirstMatch("QWErty", [PasswordMatcher.CharSequence(CharSequenceTypes.KeyboardRow)]);

        result.ShouldNotBeNull();
        result.Items[0].MatchedText.ShouldBe("QWERTY");
        result.Items[0].MatchedChars.ShouldBe("qwerty");

        new PasswordMatchItem("password", PasswordMatchType.Common).MatchedChars.ShouldBe("password");
    }

    [TestMethod]
    public void Properties()
    {
        var matcher = (TextMatcher)PasswordMatcher.Text(" PassWord ", PasswordMatchType.ContextualSubject, checkSubstitutions: false, matchRepeats: false, matchTrailingSeparator: false);

        matcher.Value.ShouldBe("password");
        matcher.MatchType.ShouldBe(PasswordMatchType.ContextualSubject);
        matcher.CheckSubstitutions.ShouldBeFalse();
        matcher.MatchRepeats.ShouldBeFalse();
        matcher.MatchTrailingSeparator.ShouldBeFalse();
    }
}
