namespace Singulink.Cryptography.Tests;

[PrefixTestClass]
public sealed class PasswordMatchEvaluatorTests
{
    [TestMethod]
    public void Subject_Verb_Subject_Suffix_IsMatch()
    {
        string password = "iloveyou!!";

        var result = PasswordMatchEvaluator.Default.MatchPassword(password, null);

        result.Matched.ShouldBeTrue();
        result.MatchedValues.ShouldBe(["i", "love", "you", "!!"]);
    }

    [TestMethod]
    public void KeyboardSequence_L33tSubject_Year_IsMatch()
    {
        string password = "QWERTp@ssword1975";

        var result = PasswordMatchEvaluator.Default.MatchPassword(password, null);

        result.Matched.ShouldBeTrue();
        result.MatchedValues.ShouldBe(["QWERT", "password", "1975"]);
    }

    [TestMethod]
    public void L33tCompanySubject_PrefixedAdjective_IsMatch()
    {
        string password = "s1ngul1nkisthebest";

        var result = PasswordMatchEvaluator.Default.MatchPassword(password, [new("Singulink Business Solutions", ContextualSubjectType.Company)]);

        result.Matched.ShouldBeTrue();
        result.MatchedValues.ShouldBe(["singulink", "is", "the", "best"]);
    }

    [TestMethod]
    public void TwoL33tEmailSubjectParts_RepeatDigitSuffix_TwoRepeatCommonCharSuffix_IsMatch()
    {
        string password = "s1ngul1nkmikem111!!";

        var result = PasswordMatchEvaluator.Default.MatchPassword(password, [new("mikem@singulink.com", ContextualSubjectType.Email)]);

        result.Matched.ShouldBeTrue();
        result.MatchedValues.ShouldBe(["singulink", "mikem", "111", "!!"]);
    }

    [TestMethod]
    public void Subject_RepeatKeyboardSequence_WithSeparators_IsMatch()
    {
        string password = "password.123123.123123";

        var result = PasswordMatchEvaluator.Default.MatchPassword(password, null);

        result.Matched.ShouldBeTrue();
        result.MatchedValues.ShouldBe(["password", "123", "123", "123", "123"]);
    }

    [TestMethod]
    public void Subject_KeyboardSequence_ShiftedKeyboardSequence_IsMatch()
    {
        string password = "password123IOP{}|";

        var result = PasswordMatchEvaluator.Default.MatchPassword(password, null);

        result.Matched.ShouldBeTrue();
        result.MatchedValues.ShouldBe(["password", "123", "IOP{}|"]);
    }

    [TestMethod]
    public void RepeatSubject_IsMatch()
    {
        string password = "PasswordPassword PasswordPassword";

        var result = PasswordMatchEvaluator.Default.MatchPassword(password, null);

        result.Matched.ShouldBeTrue();
        result.MatchedValues.ShouldBe(["password", "password", "password", "password"]);
    }

    [TestMethod]
    public void Subject_PrefixedAdjective_Subject_WithSeparators_IsMatch()
    {
        string password = "this is a bad password";

        var result = PasswordMatchEvaluator.Default.MatchPassword(password, null);

        result.Matched.ShouldBeTrue();
        result.MatchedValues.ShouldBe(["this", "is", "a", "bad", "password"]);
    }

    [TestMethod]
    public void DeterminedAdjective_Subject_WithDifferentSeparators_IsMatch()
    {
        string password = "the.bad.admin";

        var result = PasswordMatchEvaluator.Default.MatchPassword(password, null);

        result.Matched.ShouldBeTrue();
        result.MatchedValues.ShouldBe(["the", "bad", "admin"]);
    }

    [TestMethod]
    public void Subject_CopularVerb_DeterminedAdjective_L33tSubject_IsMatch()
    {
        string password = "IAmABad@dmin";

        var result = PasswordMatchEvaluator.Default.MatchPassword(password, null);

        result.Matched.ShouldBeTrue();
        result.MatchedValues.ShouldBe(["i", "am", "a", "bad", "admin"]);
    }

    [TestMethod]
    public void Subject_CopularVerb_DeterminedCompanySubject_Subject_IsMatch()
    {
        string password = "iamthebusinessadmin";

        var result = PasswordMatchEvaluator.Default.MatchPassword(password, [new("Singulink Business Solutions", ContextualSubjectType.Company)]);

        result.Matched.ShouldBeTrue();
        result.MatchedValues.ShouldBe(["i", "am", "the", "business", "admin"]);
    }

    [TestMethod]
    public void RepeatedNameSubject_SuffixedVerb_RepeatedWebsiteSubject_CommonSuffix_IsMatch()
    {
        string password = "MikeMikeLogInToSingulinkSingulink1!";

        var result = PasswordMatchEvaluator.Default.MatchPassword(password, [
            new("https://www.singulink.com", ContextualSubjectType.Website),
            new("Mike Smith", ContextualSubjectType.Name)
        ]);

        result.Matched.ShouldBeTrue();
        result.MatchedValues.ShouldBe(["mike", "mike", "login", "to", "singulink", "singulink", "1", "!"]);
    }

    [TestMethod]
    public void SuffixedVerb_RepeatedWebsiteSubject_RepeatedNameSubject_ReverseKeyboardSequence_IsMatch()
    {
        string password = "LogInToSingulinkSingulinkMikeMike hgfds";

        var result = PasswordMatchEvaluator.Default.MatchPassword(password, [
            new("https://www.singulink.com", ContextualSubjectType.Website),
            new("Mike Smith", ContextualSubjectType.Name)
        ]);

        result.Matched.ShouldBeTrue();
        result.MatchedValues.ShouldBe(["login", "to", "singulink", "singulink", "mike", "mike", "HGFDS"]);
    }

    [TestMethod]
    public void Subject_SubjectJoiner_PrefixedSubject_WithSeparators_IsMatch()
    {
        string password = "you are a noob";

        var result = PasswordMatchEvaluator.Default.MatchPassword(password, null);

        result.Matched.ShouldBeTrue();
        result.MatchedValues.ShouldBe(["you", "are", "a", "noob"]);
    }

    [TestMethod]
    public void Subject_SubjectJoiner_PrefixedAdjective_Subject_WithSeparators_IsMatch()
    {
        string password = "you are a bad noob";

        var result = PasswordMatchEvaluator.Default.MatchPassword(password, null);

        result.Matched.ShouldBeTrue();
        result.MatchedValues.ShouldBe(["you", "are", "a", "bad", "noob"]);
    }

    [TestMethod]
    public void WelcomeTo_Subject_IsMatch()
    {
        string password = "welcometoadmin";

        var result = PasswordMatchEvaluator.Default.MatchPassword(password, null);

        result.Matched.ShouldBeTrue();
        result.MatchedValues.ShouldBe(["welcome", "to", "admin"]);
    }

    [TestMethod]
    public void LetMeIn_Subject_IsMatch()
    {
        string password = "letmeinadmin";

        var result = PasswordMatchEvaluator.Default.MatchPassword(password, null);

        result.Matched.ShouldBeTrue();
        result.MatchedValues.ShouldBe(["let", "me", "in", "admin"]);
    }

    [TestMethod]
    public void LetMeInto_Subject_IsMatch()
    {
        string password = "letmeintoadmin";

        var result = PasswordMatchEvaluator.Default.MatchPassword(password, null);

        result.Matched.ShouldBeTrue();
        result.MatchedValues.ShouldBe(["let", "me", "into", "admin"]);
    }

    [TestMethod]
    public void Adjective_Subject_IsMatch()
    {
        string password = "coolninja";

        var result = PasswordMatchEvaluator.Default.MatchPassword(password, null);

        result.Matched.ShouldBeTrue();
        result.MatchedValues.ShouldBe(["cool", "ninja"]);
    }

    [TestMethod]
    public void SuffixedAdjective_Subject_IsMatch()
    {
        string password = "coolasninja";

        var result = PasswordMatchEvaluator.Default.MatchPassword(password, null);

        result.Matched.ShouldBeTrue();
        result.MatchedValues.ShouldBe(["cool", "as", "ninja"]);
    }

    [TestMethod]
    public void AdjectiveWithSuffix_PrefixedSubject_IsMatch()
    {
        string password = "coolasaninja";

        var result = PasswordMatchEvaluator.Default.MatchPassword(password, null);

        result.Matched.ShouldBeTrue();
        result.MatchedValues.ShouldBe(["cool", "as", "a", "ninja"]);
    }

    [TestMethod]
    public void Subject_CopularVerb_AdjectiveWithSuffix_PrefixedSubject_IsMatch()
    {
        string password = "iamcoolasaninja";

        var result = PasswordMatchEvaluator.Default.MatchPassword(password, null);

        result.Matched.ShouldBeTrue();
        result.MatchedValues.ShouldBe(["i", "am", "cool", "as", "a", "ninja"]);
    }

    [TestMethod]
    public void FourKeyboardSequences_Subject_IsMatch()
    {
        string password = @"1234asdfqwerzxcvadmin";

        var result = PasswordMatchEvaluator.Default.MatchPassword(password, null);
        result.Matched.ShouldBeTrue();
    }

    [TestMethod]
    public void LongPassword_IsMatch()
    {
        string password = @"1234567890-=qwertyuiop[]\asdfghjkl;'zxcvbnm,./memememe1234567890";

        var result = PasswordMatchEvaluator.Default.MatchPassword(password, null);
        result.Matched.ShouldBeTrue();
    }

    [TestMethod]
    public void NoMatches()
    {
        string password = "thisisnotamatch";
        var result = PasswordMatchEvaluator.Default.MatchPassword(password, null);

        result.Matched.ShouldBeFalse();
        result.MatchedValues.ShouldBeEmpty();

        password = "passwordk4efjawei";

        result = PasswordMatchEvaluator.Default.MatchPassword(password, null);

        result.Matched.ShouldBeFalse();
        result.MatchedValues.ShouldBeEmpty();

        password = "9092394850923029384";

        result = PasswordMatchEvaluator.Default.MatchPassword(password, null);
        result.Matched.ShouldBeFalse();

        password = "hello world!!";

        result = PasswordMatchEvaluator.Default.MatchPassword(password, null);
        result.Matched.ShouldBeFalse();
    }
}
