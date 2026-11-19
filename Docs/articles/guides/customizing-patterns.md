<div class="article">

# Adding Words and Patterns

The default patterns cover the most common password structures, but every application has words of its own worth blocking, and some have structures of their own. There are two levels of change:

| You want to | Do this | Section |
|---|---|---|
| Block your product names and other words of your own | Register them as application subjects. No pattern changes. | [Adding Your Own Words](#adding-your-own-words) |
| Block a structure the default patterns do not describe | Add a pattern to the default provider | [Adding a Pattern](#adding-a-pattern) |
| Use different patterns altogether, for example for another language | Write a provider of your own | [Replacing the Patterns Entirely](#replacing-the-patterns-entirely) |

### How the check is put together

A check is driven by an <xref:Singulink.Cryptography.IPasswordMatchersProvider>, which returns the top-level patterns to try for a password. <xref:Singulink.Cryptography.PasswordMatchersProvider.Default> builds its patterns from the word lists and decorations in <xref:Singulink.Cryptography.PasswordMatchers.CommonMatchers> and the contextual subjects of the current check. The patterns, and the word lists and decorations they are built from, are all <xref:Singulink.Cryptography.PasswordMatcher> objects, described in [Matcher Building Blocks](building-blocks.md).

## Adding Your Own Words

Words that should take part in the sentence-like default patterns are best added as contextual subjects, since the default provider treats subjects and common words identically. Register them once as the provider's application subjects, with <xref:Singulink.Cryptography.ContextualSubjectType.General> as the type for plain words:

```csharp
var provider = new PasswordMatchersProvider(ContextualSubjectMatcherProvider.Default, [
    new("Widget Pro"),                                  // General is the default type
    new("Widget Lite"),
    new("Acme Widgets", ContextualSubjectType.Company),
]);

var evaluator = new PasswordMatchEvaluator(provider);
```

The words now take part in every default pattern, for every user, without any pattern changing:

```csharp
evaluator.MatchPassword("iamthewidgetproadmin", null);   // match: "i", "am", "the", "widget", "pro", "admin"
evaluator.MatchPassword("AcmeWidgets2024!", null);       // match: "acme", "widgets", "2024", "!"
```

It works the same way as `iamthebusinessadmin` does for a company subject containing "business". See [Contextual Subjects](contextual-subjects.md#application-subjects) for more on application subjects.

> [!TIP]
> This covers most needs. Deriving from the provider, as below, is only needed when the *structure* of the patterns should change.

## Adding a Pattern

Derive from <xref:Singulink.Cryptography.PasswordMatchersProvider> and override <xref:Singulink.Cryptography.PasswordMatchersProvider.GetMatchers*> to add patterns before or after the defaults. The base implementation returns the default patterns for the given subjects, so to add patterns you yield your own alongside them:

```csharp
public class MyMatchersProvider : PasswordMatchersProvider
{
    private static readonly PasswordMatcher ProductNames = PasswordMatcher.Any([
        PasswordMatcher.Text("widgetpro"),
        PasswordMatcher.Text("widgetlite"),
        PasswordMatcher.Sequence([PasswordMatcher.Text("widget"), PasswordMatcher.Text("pro")]),
        PasswordMatcher.Sequence([PasswordMatcher.Text("widget"), PasswordMatcher.Text("lite")]),
    ]);

    // Pattern: [decoration] product [product] [decoration]
    private static readonly PasswordMatcher ProductPattern = PasswordMatcher.Sequence([
        PasswordMatcher.Optional(CommonMatchers.DecorationMatcher),
        PasswordMatcher.Repeat(ProductNames, 1, 2),
        PasswordMatcher.Optional(CommonMatchers.DecorationMatcher),
    ]);

    public MyMatchersProvider() : base(ContextualSubjectMatcherProvider.Default) { }

    public override IEnumerable<PasswordMatcher> GetMatchers(IEnumerable<ContextualSubject>? subjects)
    {
        yield return ProductPattern;                    // tried first

        foreach (var matcher in base.GetMatchers(subjects))
            yield return matcher;                       // then the default patterns
    }
}
```

Then build an evaluator around it, and use that evaluator everywhere instead of <xref:Singulink.Cryptography.PasswordMatchEvaluator.Default>:

```csharp
var evaluator = new PasswordMatchEvaluator(new MyMatchersProvider());
```

The patterns are tried in the order they are returned, and the first complete match is the one reported. See [Matcher Building Blocks](building-blocks.md) for the factory methods and the predefined word lists and decorations.

## Replacing the Patterns Entirely

Implement <xref:Singulink.Cryptography.IPasswordMatchersProvider> directly when the default patterns are not what you want at all, for example for a non-English user base. The single <xref:Singulink.Cryptography.IPasswordMatchersProvider.GetMatchers*> method returns the patterns to try for a password, in order, given the contextual subjects for the check. You can still use the <xref:Singulink.Cryptography.PasswordMatchers.CommonMatchers> word lists and decorations, and <xref:Singulink.Cryptography.ContextualSubjectMatcherProvider.Default> to turn subjects into matchers:

```csharp
public class SimpleMatchersProvider : IPasswordMatchersProvider
{
    public IEnumerable<PasswordMatcher> GetMatchers(IEnumerable<ContextualSubject>? subjects)
    {
        var subjectMatchers = subjects?
            .Select(ContextualSubjectMatcherProvider.Default.GetMatcher)
            .OfType<PasswordMatcher>()
            .ToList() ?? [];

        var word = PasswordMatcher.Any([..subjectMatchers, CommonMatchers.GeneralSubjectMatcher]);

        // Pattern: [decoration] word [word] [decoration]
        yield return PasswordMatcher.Sequence([
            PasswordMatcher.Optional(CommonMatchers.DecorationMatcher),
            PasswordMatcher.Repeat(word, 1, 2),
            PasswordMatcher.Optional(CommonMatchers.DecorationMatcher),
        ]);
    }
}
```

> [!NOTE]
> Matchers that depend on the contextual subjects have to be built per check, as above, because the subjects differ for every user. Matchers that do not depend on them should be built once and stored in static fields, as the default provider does with the common word lists and decorations.

## Next Steps

Continue with these related articles:

- [Matcher Building Blocks](building-blocks.md) - The factory methods, the composition rules and the predefined word lists.
- [Writing Custom Matchers](custom-matchers.md) - When the building blocks are not enough, write a matcher of your own.
- [Default Patterns](../concepts/default-patterns.md) - The complete word lists, the decorations and the default patterns.

</div>
