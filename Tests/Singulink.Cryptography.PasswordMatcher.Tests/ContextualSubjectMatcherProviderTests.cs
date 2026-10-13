namespace Singulink.Cryptography.Tests;

[PrefixTestClass]
public class ContextualSubjectMatcherProviderTests
{
    private static PasswordMatcher GetMatcher(string value, ContextualSubjectType type)
    {
        var matcher = ContextualSubjectMatcherProvider.Default.GetMatcher(new ContextualSubject(value, type));
        matcher.ShouldNotBeNull();
        return matcher;
    }

    [TestMethod]
    public void SingleWord_IsTextMatcher()
    {
        var matcher = GetMatcher("Singulink", ContextualSubjectType.Company);

        var result = PasswordMatcher.GetFirstMatch("singulink", [matcher]);

        result.ShouldNotBeNull();
        result.Items.Select(m => m.MatchedText).ShouldBe(["singulink"]);
        result.Items[0].MatchType.ShouldBe(PasswordMatchType.ContextualSubject);
    }

    [TestMethod]
    public void SingleWordWithTrailingDelimiter_IgnoresDelimiter()
    {
        var matcher = GetMatcher("Mike.", ContextualSubjectType.Name);

        var result = PasswordMatcher.GetFirstMatch("mike", [matcher]);

        result.ShouldNotBeNull();
        result.Items.Select(m => m.MatchedText).ShouldBe(["mike"]);
    }

    [TestMethod]
    public void MultipleWords_MatchInAnyOrder()
    {
        var matcher = GetMatcher("Mike Smith", ContextualSubjectType.Name);

        PasswordMatcher.GetFirstMatch("mikesmith", [matcher]).ShouldNotBeNull();
        PasswordMatcher.GetFirstMatch("smithmike", [matcher]).ShouldNotBeNull();
        PasswordMatcher.GetFirstMatch("smith", [matcher]).ShouldNotBeNull();
        PasswordMatcher.GetFirstMatch("smithsmith", [matcher]).ShouldNotBeNull(); // repeats
        PasswordMatcher.GetFirstMatch("smithmikesmith", [matcher]).ShouldBeNull();
    }

    [TestMethod]
    public void Website_StripsProtocolPrefixTldAndPath()
    {
        var matcher = GetMatcher("https://www.singulink.com/some/path", ContextualSubjectType.Website);

        PasswordMatcher.GetFirstMatch("singulink", [matcher]).ShouldNotBeNull();
        PasswordMatcher.GetFirstMatch("singulinkcom", [matcher]).ShouldBeNull();
        PasswordMatcher.GetFirstMatch("www", [matcher]).ShouldBeNull();
    }

    [TestMethod]
    public void WebsiteWithSubdomain_MatchesAllParts()
    {
        var matcher = GetMatcher("app.singulink.com", ContextualSubjectType.Website);

        PasswordMatcher.GetFirstMatch("appsingulink", [matcher]).ShouldNotBeNull();
        PasswordMatcher.GetFirstMatch("singulinkapp", [matcher]).ShouldNotBeNull();
        PasswordMatcher.GetFirstMatch("com", [matcher]).ShouldBeNull();
    }

    [TestMethod]
    public void Email_StripsTld()
    {
        var matcher = GetMatcher("mikem@singulink.com", ContextualSubjectType.Email);

        PasswordMatcher.GetFirstMatch("mikemsingulink", [matcher]).ShouldNotBeNull();
        PasswordMatcher.GetFirstMatch("singulinkmikem", [matcher]).ShouldNotBeNull();
        PasswordMatcher.GetFirstMatch("mikem", [matcher]).ShouldNotBeNull();
        PasswordMatcher.GetFirstMatch("com", [matcher]).ShouldBeNull();
    }

    [TestMethod]
    public void EmailWithDottedLocalPart_MatchesAllParts()
    {
        var matcher = GetMatcher("mike.smith@singulink.com", ContextualSubjectType.Email);

        PasswordMatcher.GetFirstMatch("smithsingulinkmike", [matcher]).ShouldNotBeNull();
    }

    [TestMethod]
    public void EmptyValue_IsNull()
    {
        ContextualSubjectMatcherProvider.Default.GetMatcher(new ContextualSubject(string.Empty)).ShouldBeNull();
        ContextualSubjectMatcherProvider.Default.GetMatcher(new ContextualSubject("  ")).ShouldBeNull();
        ContextualSubjectMatcherProvider.Default.GetMatcher(new ContextualSubject(" . - ")).ShouldBeNull();
    }

    [TestMethod]
    public void CustomDelimiters()
    {
        var provider = new ContextualSubjectMatcherProvider(['/']);
        var matcher = provider.GetMatcher(new ContextualSubject("mike/smith"));

        matcher.ShouldNotBeNull();
        PasswordMatcher.GetFirstMatch("smithmike", [matcher]).ShouldNotBeNull();

        matcher = provider.GetMatcher(new ContextualSubject("mike smith"));

        matcher.ShouldNotBeNull();
        PasswordMatcher.GetFirstMatch("mike smith", [matcher]).ShouldNotBeNull();
        PasswordMatcher.GetFirstMatch("smithmike", [matcher]).ShouldBeNull();
    }

    [TestMethod]
    public void InvalidSubjectType_Throws()
    {
        Should.Throw<ArgumentOutOfRangeException>(() => new ContextualSubject("x", (ContextualSubjectType)100));
    }

    [TestMethod]
    public void CamelCase_IsSplit()
    {
        var matcher = GetMatcher("MikeSmith", ContextualSubjectType.Username);

        PasswordMatcher.GetFirstMatch("mike", [matcher]).ShouldNotBeNull();
        PasswordMatcher.GetFirstMatch("smithmike", [matcher]).ShouldNotBeNull();
        PasswordMatcher.GetFirstMatch("mikesmith", [matcher]).ShouldNotBeNull();
    }

    [TestMethod]
    public void UpperCaseRunFollowedByWord_IsSplit()
    {
        var matcher = GetMatcher("ACMEWidgets", ContextualSubjectType.Company);

        PasswordMatcher.GetFirstMatch("acme", [matcher]).ShouldNotBeNull();
        PasswordMatcher.GetFirstMatch("widgets", [matcher]).ShouldNotBeNull();
        PasswordMatcher.GetFirstMatch("widgetsacme", [matcher]).ShouldNotBeNull();
    }

    [TestMethod]
    public void LetterDigitBoundaries_AreSplit()
    {
        var matcher = GetMatcher("mike1985", ContextualSubjectType.Username);

        PasswordMatcher.GetFirstMatch("mike", [matcher]).ShouldNotBeNull();
        PasswordMatcher.GetFirstMatch("1985", [matcher]).ShouldNotBeNull();
        PasswordMatcher.GetFirstMatch("1985mike", [matcher]).ShouldNotBeNull();

        matcher = GetMatcher("42acme7", ContextualSubjectType.General);

        PasswordMatcher.GetFirstMatch("acme42", [matcher]).ShouldNotBeNull();
        PasswordMatcher.GetFirstMatch("7", [matcher]).ShouldBeNull(); // Single-character parts are discarded
    }

    [TestMethod]
    public void PlusAddressedEmail_IsSplit()
    {
        var matcher = GetMatcher("mike+work@singulink.com", ContextualSubjectType.Email);

        PasswordMatcher.GetFirstMatch("mike", [matcher]).ShouldNotBeNull();
        PasswordMatcher.GetFirstMatch("worksingulink", [matcher]).ShouldNotBeNull();
    }

    [TestMethod]
    public void SingleCharacterValue_IsNull()
    {
        ContextualSubjectMatcherProvider.Default.GetMatcher(new ContextualSubject("x", ContextualSubjectType.Username)).ShouldBeNull();
    }
}
