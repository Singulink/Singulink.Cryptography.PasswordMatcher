namespace Singulink.Cryptography.Tests;

[PrefixTestClass]
public sealed class PasswordMatchEvaluatorTests
{
    [TestMethod]
    public void Subject_Verb_Subject_Suffix_IsMatch()
    {
        string password = "iloveyou!!";

        var result = PasswordMatchEvaluator.Default.MatchPassword(password, null);

        result.ShouldNotBeNull();
        result.Values.ShouldBe(["i", "love", "you", "!!"]);
    }

    [TestMethod]
    public void KeyboardSequence_L33tSubject_Year_IsMatch()
    {
        string password = "QWERTp@ssword1975";

        var result = PasswordMatchEvaluator.Default.MatchPassword(password, null);

        result.ShouldNotBeNull();
        result.Values.ShouldBe(["QWERT", "password", "1975"]);
    }

    [TestMethod]
    public void L33tCompanySubject_PrefixedAdjective_IsMatch()
    {
        string password = "s1ngul1nkisthebest";

        var result = PasswordMatchEvaluator.Default.MatchPassword(password, [new("Singulink Business Solutions", ContextualSubjectType.Company)]);

        result.ShouldNotBeNull();
        result.Values.ShouldBe(["singulink", "is", "the", "best"]);
    }

    [TestMethod]
    public void TwoL33tEmailSubjectParts_RepeatDigitSuffix_TwoRepeatCommonCharSuffix_IsMatch()
    {
        string password = "s1ngul1nkmikem111!!";

        var result = PasswordMatchEvaluator.Default.MatchPassword(password, [new("mikem@singulink.com", ContextualSubjectType.Email)]);

        result.ShouldNotBeNull();
        result.Values.ShouldBe(["singulink", "mikem", "111", "!!"]);
    }

    [TestMethod]
    public void Subject_RepeatKeyboardSequence_WithSeparators_IsMatch()
    {
        string password = "password.123123.123123";

        var result = PasswordMatchEvaluator.Default.MatchPassword(password, null);

        result.ShouldNotBeNull();
        result.Values.ShouldBe(["password", "123", "123", "123", "123"]);
    }

    [TestMethod]
    public void Subject_KeyboardSequence_ShiftedKeyboardSequence_IsMatch()
    {
        string password = "password123IOP{}|";

        var result = PasswordMatchEvaluator.Default.MatchPassword(password, null);

        result.ShouldNotBeNull();
        result.Values.ShouldBe(["password", "123", "IOP{}|"]);
    }

    [TestMethod]
    public void RepeatSubject_IsMatch()
    {
        string password = "PasswordPassword PasswordPassword";

        var result = PasswordMatchEvaluator.Default.MatchPassword(password, null);

        result.ShouldNotBeNull();
        result.Values.ShouldBe(["password", "password", "password", "password"]);
    }

    [TestMethod]
    public void Subject_PrefixedAdjective_Subject_WithSeparators_IsMatch()
    {
        string password = "this is a bad password";

        var result = PasswordMatchEvaluator.Default.MatchPassword(password, null);

        result.ShouldNotBeNull();
        result.Values.ShouldBe(["this", "is", "a", "bad", "password"]);
    }

    [TestMethod]
    public void DeterminedAdjective_Subject_WithDifferentSeparators_IsMatch()
    {
        string password = "the.bad.admin";

        var result = PasswordMatchEvaluator.Default.MatchPassword(password, null);

        result.ShouldNotBeNull();
        result.Values.ShouldBe(["the", "bad", "admin"]);
    }

    [TestMethod]
    public void Subject_CopularVerb_DeterminedAdjective_L33tSubject_IsMatch()
    {
        string password = "IAmABad@dmin";

        var result = PasswordMatchEvaluator.Default.MatchPassword(password, null);

        result.ShouldNotBeNull();
        result.Values.ShouldBe(["i", "am", "a", "bad", "admin"]);
    }

    [TestMethod]
    public void Subject_CopularVerb_DeterminedCompanySubject_Subject_IsMatch()
    {
        string password = "iamthebusinessadmin";

        var result = PasswordMatchEvaluator.Default.MatchPassword(password, [new("Singulink Business Solutions", ContextualSubjectType.Company)]);

        result.ShouldNotBeNull();
        result.Values.ShouldBe(["i", "am", "the", "business", "admin"]);
    }

    [TestMethod]
    public void RepeatedNameSubject_SuffixedVerb_RepeatedWebsiteSubject_CommonSuffix_IsMatch()
    {
        string password = "MikeMikeLogInToSingulinkSingulink1!";

        var result = PasswordMatchEvaluator.Default.MatchPassword(password, [
            new("https://www.singulink.com", ContextualSubjectType.Website),
            new("Mike Smith", ContextualSubjectType.Name)
        ]);

        result.ShouldNotBeNull();
        result.Values.ShouldBe(["mike", "mike", "login", "to", "singulink", "singulink", "1!"]);
    }

    [TestMethod]
    public void SuffixedVerb_RepeatedWebsiteSubject_RepeatedNameSubject_ReverseKeyboardSequence_IsMatch()
    {
        string password = "LogInToSingulinkSingulinkMikeMike hgfds";

        var result = PasswordMatchEvaluator.Default.MatchPassword(password, [
            new("https://www.singulink.com", ContextualSubjectType.Website),
            new("Mike Smith", ContextualSubjectType.Name)
        ]);

        result.ShouldNotBeNull();
        result.Values.ShouldBe(["login", "to", "singulink", "singulink", "mike", "mike", "HGFDS"]);
    }

    [TestMethod]
    public void Subject_SubjectJoiner_PrefixedSubject_WithSeparators_IsMatch()
    {
        string password = "you are a noob";

        var result = PasswordMatchEvaluator.Default.MatchPassword(password, null);

        result.ShouldNotBeNull();
        result.Values.ShouldBe(["you", "are", "a", "noob"]);
    }

    [TestMethod]
    public void Subject_SubjectJoiner_PrefixedAdjective_Subject_WithSeparators_IsMatch()
    {
        string password = "you are a bad noob";

        var result = PasswordMatchEvaluator.Default.MatchPassword(password, null);

        result.ShouldNotBeNull();
        result.Values.ShouldBe(["you", "are", "a", "bad", "noob"]);
    }

    [TestMethod]
    public void WelcomeTo_Subject_IsMatch()
    {
        string password = "welcometoadmin";

        var result = PasswordMatchEvaluator.Default.MatchPassword(password, null);

        result.ShouldNotBeNull();
        result.Values.ShouldBe(["welcome", "to", "admin"]);
    }

    [TestMethod]
    public void LetMeIn_IsMatch()
    {
        string password = "letmein";

        var result = PasswordMatchEvaluator.Default.MatchPassword(password, null);

        result.ShouldNotBeNull();
        result.Values.ShouldBe(["let", "me", "in"]);

        password = "LetMeIn2024!";

        result = PasswordMatchEvaluator.Default.MatchPassword(password, null);

        result.ShouldNotBeNull();
        result.Values.ShouldBe(["let", "me", "in", "2024", "!"]);
    }

    [TestMethod]
    public void LetMeIn_Subject_IsMatch()
    {
        string password = "letmeinadmin";

        var result = PasswordMatchEvaluator.Default.MatchPassword(password, null);

        result.ShouldNotBeNull();
        result.Values.ShouldBe(["let", "me", "in", "admin"]);
    }

    [TestMethod]
    public void LetMeInto_Subject_IsMatch()
    {
        string password = "letmeintoadmin";

        var result = PasswordMatchEvaluator.Default.MatchPassword(password, null);

        result.ShouldNotBeNull();
        result.Values.ShouldBe(["let", "me", "into", "admin"]);
    }

    [TestMethod]
    public void Adjective_Subject_IsMatch()
    {
        string password = "coolninja";

        var result = PasswordMatchEvaluator.Default.MatchPassword(password, null);

        result.ShouldNotBeNull();
        result.Values.ShouldBe(["cool", "ninja"]);
    }

    [TestMethod]
    public void SuffixedAdjective_Subject_IsMatch()
    {
        string password = "coolasninja";

        var result = PasswordMatchEvaluator.Default.MatchPassword(password, null);

        result.ShouldNotBeNull();
        result.Values.ShouldBe(["cool", "as", "ninja"]);
    }

    [TestMethod]
    public void AdjectiveWithSuffix_PrefixedSubject_IsMatch()
    {
        string password = "coolasaninja";

        var result = PasswordMatchEvaluator.Default.MatchPassword(password, null);

        result.ShouldNotBeNull();
        result.Values.ShouldBe(["cool", "as", "a", "ninja"]);
    }

    [TestMethod]
    public void Subject_CopularVerb_AdjectiveWithSuffix_PrefixedSubject_IsMatch()
    {
        string password = "iamcoolasaninja";

        var result = PasswordMatchEvaluator.Default.MatchPassword(password, null);

        result.ShouldNotBeNull();
        result.Values.ShouldBe(["i", "am", "cool", "as", "a", "ninja"]);
    }

    [TestMethod]
    public void FourKeyboardSequences_Subject_IsMatch()
    {
        string password = @"1234asdfqwerzxcvadmin";

        var result = PasswordMatchEvaluator.Default.MatchPassword(password, null);
        result.ShouldNotBeNull();
    }

    [TestMethod]
    public void DiagonalKeyboardSequences_IsMatch()
    {
        var result = PasswordMatchEvaluator.Default.MatchPassword("1qaz2wsx", null);
        result.ShouldNotBeNull();
        result.Values.ShouldBe(["1QAZ", "2WSX"]);

        result = PasswordMatchEvaluator.Default.MatchPassword("ZAQ!2wsx", null);
        result.ShouldNotBeNull();
        result.Values.ShouldBe(["ZAQ!", "2WSX"]);

        result = PasswordMatchEvaluator.Default.MatchPassword("admin1qaz", null);
        result.ShouldNotBeNull();
        result.Values.ShouldBe(["admin", "1QAZ"]);
    }

    [TestMethod]
    public void LongPassword_IsMatch()
    {
        string password = @"1234567890-=qwertyuiop[]\asdfghjkl;'zxcvbnm,./memememe1234567890";

        var result = PasswordMatchEvaluator.Default.MatchPassword(password, null);
        result.ShouldNotBeNull();
    }

    [TestMethod]
    public void LeadingAndTrailingWhitespace_IsMatch()
    {
        var result = PasswordMatchEvaluator.Default.MatchPassword("  password ", null);

        result.ShouldNotBeNull();
        result.Values.ShouldBe(["password"]);
    }

    [TestMethod]
    public void DashAndUnderscoreSeparators_IsMatch()
    {
        var result = PasswordMatchEvaluator.Default.MatchPassword("password-123", null);
        result.ShouldNotBeNull();
        result.Values.ShouldBe(["password", "123"]);

        result = PasswordMatchEvaluator.Default.MatchPassword("password_1234", null);
        result.ShouldNotBeNull();
        result.Values.ShouldBe(["password", "1234"]);

        result = PasswordMatchEvaluator.Default.MatchPassword("admin_admin_2024", null);
        result.ShouldNotBeNull();
        result.Values.ShouldBe(["admin", "admin", "2024"]);
    }

    [TestMethod]
    public void NoMatches()
    {
        string password = "thisisnotamatch";
        var result = PasswordMatchEvaluator.Default.MatchPassword(password, null);

        result.ShouldBeNull();

        password = "passwordk4efjawei";

        result = PasswordMatchEvaluator.Default.MatchPassword(password, null);

        result.ShouldBeNull();

        password = "9092394850923029384";

        result = PasswordMatchEvaluator.Default.MatchPassword(password, null);
        result.ShouldBeNull();

        password = "hello world!!";

        result = PasswordMatchEvaluator.Default.MatchPassword(password, null);
        result.ShouldBeNull();

        password = "correct horse battery staple";

        result = PasswordMatchEvaluator.Default.MatchPassword(password, null);
        result.ShouldBeNull();
    }

    [TestMethod]
    public void EmptyPassword_IsNotMatch()
    {
        PasswordMatchEvaluator.Default.MatchPassword(string.Empty, null).ShouldBeNull();
        PasswordMatchEvaluator.Default.MatchPassword("   ", null).ShouldBeNull();
    }

    [TestMethod]
    public void LongerThanMaxLength_IsNotMatch()
    {
        var evaluator = new PasswordMatchEvaluator(PasswordMatchersProvider.Default, maxPasswordLength: 16);

        evaluator.MatchPassword("passwordpassword", null).ShouldNotBeNull();
        evaluator.MatchPassword("passwordpassword1", null).ShouldBeNull();
        evaluator.MatchPassword(" passwordpassword", null).ShouldBeNull(); // Length is checked before trimming

        PasswordMatchEvaluator.Default.MaxPasswordLength.ShouldBe(PasswordMatchEvaluator.DefaultMaxPasswordLength);

        string maxLengthPassword = string.Concat(Enumerable.Repeat("password", PasswordMatchEvaluator.DefaultMaxPasswordLength / 8));
        maxLengthPassword.Length.ShouldBe(PasswordMatchEvaluator.DefaultMaxPasswordLength);
        PasswordMatchEvaluator.Default.MatchPassword(maxLengthPassword, null).ShouldNotBeNull();
        PasswordMatchEvaluator.Default.MatchPassword(maxLengthPassword + "!", null).ShouldBeNull();
    }

    [TestMethod]
    public void InvalidMaxLength_Throws()
    {
        Should.Throw<ArgumentOutOfRangeException>(() => new PasswordMatchEvaluator(PasswordMatchersProvider.Default, maxPasswordLength: 0));
    }

    [TestMethod]
    public void EmptyContextualSubjects_AreIgnored()
    {
        var result = PasswordMatchEvaluator.Default.MatchPassword("admin", [new(string.Empty), new("   ", ContextualSubjectType.Name)]);

        result.ShouldNotBeNull();
        result.Values.ShouldBe(["admin"]);
    }

    [TestMethod]
    public void DecorationOnly_IsMatch()
    {
        foreach (string password in new[] { "111111", "0000", "7777777", "0123456789", "!!!!", "121212", "qwerty!!!", "1q2w3e4r", "159753", "abc123", "abcd1234", "2024" })
            PasswordMatchEvaluator.Default.MatchPassword(password, null).ShouldNotBeNull(password);
    }

    [TestMethod]
    public void SymbolDecorations_IsMatch()
    {
        var result = PasswordMatchEvaluator.Default.MatchPassword("Welcome@123", null);
        result.ShouldNotBeNull();
        result.Values.ShouldBe(["welcome", "@", "123"]);

        result = PasswordMatchEvaluator.Default.MatchPassword("Password#1", null);
        result.ShouldNotBeNull();
        result.Values.ShouldBe(["password", "#1"]);

        result = PasswordMatchEvaluator.Default.MatchPassword("password!1!", null);
        result.ShouldNotBeNull();
        result.Values.ShouldBe(["password", "!1!"]);

        result = PasswordMatchEvaluator.Default.MatchPassword("112233", null);
        result.ShouldNotBeNull();
        result.Values.ShouldBe(["11", "22", "33"]);

        result = PasswordMatchEvaluator.Default.MatchPassword("iloveyou!!!!!!!!!", null);
        result.ShouldNotBeNull();
        result.Values.ShouldBe(["i", "love", "you", "!!!!!!!!!"]);
    }

    [TestMethod]
    public void ShortArbitraryDecorations_IsMatch()
    {
        var result = PasswordMatchEvaluator.Default.MatchPassword("password4792", null);
        result.ShouldNotBeNull();
        result.Values.ShouldBe(["password", "4792"]);

        result = PasswordMatchEvaluator.Default.MatchPassword("4792password", null);
        result.ShouldNotBeNull();
        result.Values.ShouldBe(["4792", "password"]);

        result = PasswordMatchEvaluator.Default.MatchPassword("password4747", null);
        result.ShouldNotBeNull();
        result.Values.ShouldBe(["password", "4747"]);

        // At most four arbitrary characters per decoration
        PasswordMatchEvaluator.Default.MatchPassword("password47921", null).ShouldBeNull();
        PasswordMatchEvaluator.Default.MatchPassword("password4792#5", null).ShouldBeNull();
    }

    [TestMethod]
    public void ContextualWordWithDecorations_IsMatch()
    {
        ContextualSubject[] subjects = [new("Acme Widgets", ContextualSubjectType.Company), new("Mike Smith", ContextualSubjectType.Name)];

        foreach (string password in new[] { "Acme#1", "acme*2024", "acme1q2w3e4r", "mike111111", "acme121212", "Mike@Acme2024!", "acmeabc123", "mike7410", "ACME!@#$" })
            PasswordMatchEvaluator.Default.MatchPassword(password, subjects).ShouldNotBeNull(password);
    }

    [TestMethod]
    public void CatchAllWordPattern_IsMatch()
    {
        ContextualSubject[] subjects = [new("Acme Widgets", ContextualSubjectType.Company), new("Mike Smith", ContextualSubjectType.Name)];

        var result = PasswordMatchEvaluator.Default.MatchPassword("mike123acme", subjects);
        result.ShouldNotBeNull();
        result.Values.ShouldBe(["mike", "123", "acme"]);

        result = PasswordMatchEvaluator.Default.MatchPassword("cool2024!", null);
        result.ShouldNotBeNull();
        result.Values.ShouldBe(["cool", "2024", "!"]);

        result = PasswordMatchEvaluator.Default.MatchPassword("loveyou", null);
        result.ShouldNotBeNull();
        result.Values.ShouldBe(["love", "you"]);

        result = PasswordMatchEvaluator.Default.MatchPassword("trustno1", null);
        result.ShouldNotBeNull();
        result.Values.ShouldBe(["trust", "no1"]);

        result = PasswordMatchEvaluator.Default.MatchPassword("acmelovemike", subjects);
        result.ShouldNotBeNull();
        result.Values.ShouldBe(["acme", "love", "mike"]);

        // More than six tokens are not matched by the catch-all pattern
        PasswordMatchEvaluator.Default.MatchPassword("loveyoulovemelovehimloveher", null).ShouldBeNull();
    }

    [TestMethod]
    public void UncommonWord_IsNotMatch()
    {
        foreach (string password in new[] { "summer2024", "hunter2", "password4792x", "mikeacmerocks", "correct horse battery staple", "passwordk" })
            PasswordMatchEvaluator.Default.MatchPassword(password, null).ShouldBeNull(password);
    }
}
