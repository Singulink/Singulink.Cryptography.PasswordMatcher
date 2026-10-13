namespace Singulink.Cryptography.Tests;

[PrefixTestClass]
public class PasswordMatchersProviderTests
{
    private static readonly PasswordMatchEvaluator Evaluator = new(new PasswordMatchersProvider(ContextualSubjectMatcherProvider.Default, [
        new("Acme Widgets", ContextualSubjectType.Company),
        new("https://www.acmewidgets.com", ContextualSubjectType.Website),
        new("Widget Pro"),
    ]));

    [TestMethod]
    public void ApplicationSubjects_AreMatchedWithoutPerCheckSubjects()
    {
        var result = Evaluator.MatchPassword("AcmeWidgets2024!", null);
        result.ShouldNotBeNull();
        result.Values.ShouldBe(["acme", "widgets", "2024", "!"]);

        result = Evaluator.MatchPassword("iamthewidgetproadmin", null);
        result.ShouldNotBeNull();
        result.Values.ShouldBe(["i", "am", "the", "widget", "pro", "admin"]);

        result = Evaluator.MatchPassword("acmewidgets#1", []);
        result.ShouldNotBeNull();
        result.Values.ShouldBe(["acme", "widgets", "#1"]);
    }

    [TestMethod]
    public void ApplicationSubjects_CombineWithPerCheckSubjects()
    {
        var result = Evaluator.MatchPassword("MikeAcme2024", [new("Mike Smith", ContextualSubjectType.Name)]);

        result.ShouldNotBeNull();
        result.Values.ShouldBe(["mike", "acme", "2024"]);
        result.Items[0].MatchType.ShouldBe(PasswordMatchType.ContextualSubject);
        result.Items[1].MatchType.ShouldBe(PasswordMatchType.ContextualSubject);
    }

    [TestMethod]
    public void NoApplicationSubjects_IsDefaultBehavior()
    {
        PasswordMatchEvaluator.Default.MatchPassword("AcmeWidgets2024!", null).ShouldBeNull();
        PasswordMatchersProvider.Default.ApplicationSubjects.ShouldBeEmpty();
    }

    [TestMethod]
    public void EmptyApplicationSubjects_AreIgnored()
    {
        var provider = new PasswordMatchersProvider(ContextualSubjectMatcherProvider.Default, [new(string.Empty), new("  ")]);

        provider.ApplicationSubjects.Length.ShouldBe(2);
        new PasswordMatchEvaluator(provider).MatchPassword("admin", null).ShouldNotBeNull();
    }
}
