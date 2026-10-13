<div class="article">

# Contextual Subjects

A contextual subject is a value specific to the user or your application that a password should not be built from: the user's name, their email address, your product name, your website. This guide covers the subject types, how a subject value is turned into a matcher, and how to change the rules.

### Why context matters

A password list built for attacking a particular service always includes the service's name, and a list built for a particular account includes whatever is known about its owner. `AcmeJane2024!` is as weak on Acme's site for a user named Jane as `password2024!` is anywhere, but no static dictionary can know that. Contextual subjects let the evaluator know it for each check.

## Creating Subjects

A <xref:Singulink.Cryptography.ContextualSubject> pairs a value with a <xref:Singulink.Cryptography.ContextualSubjectType>. The value is trimmed; everything else about how it is interpreted is up to the provider that turns it into a matcher (see below). Pass the subjects for the current user to <xref:Singulink.Cryptography.PasswordMatchEvaluator.MatchPassword*> on each check:

```csharp
ContextualSubject[] subjects = [
    new("Acme Widgets", ContextualSubjectType.Company),
    new("https://www.acmewidgets.com", ContextualSubjectType.Website),
    new("Jane Doe", ContextualSubjectType.Name),
    new("jdoe", ContextualSubjectType.Username),
    new("jane.doe@example.com", ContextualSubjectType.Email),
    new("555-123-4567", ContextualSubjectType.PhoneNumber),
];

var match = PasswordMatchEvaluator.Default.MatchPassword(password, subjects);
```

Subjects with an empty value, or a value that contains only delimiters, are ignored, so it is fine to pass a field the user left blank.

#### Subject types

The type tells the provider how to interpret the value. The default provider treats most types the same way and gives two of them special handling:

| Type | Treatment |
|---|---|
| <xref:Singulink.Cryptography.ContextualSubjectType.General> | The value is split into words. Use this for anything without a more specific type, such as a product name. |
| <xref:Singulink.Cryptography.ContextualSubjectType.Name>, <xref:Singulink.Cryptography.ContextualSubjectType.Username>, <xref:Singulink.Cryptography.ContextualSubjectType.Company>, <xref:Singulink.Cryptography.ContextualSubjectType.Address>, <xref:Singulink.Cryptography.ContextualSubjectType.PhoneNumber> | The value is split into words. |
| <xref:Singulink.Cryptography.ContextualSubjectType.Email> | The top-level domain is removed first, so `jane.doe@example.com` becomes the words `jane`, `doe` and `example`. |
| <xref:Singulink.Cryptography.ContextualSubjectType.Website> | The protocol, `www.` prefix, top-level domain and path are removed first, so `https://www.acmewidgets.com/login` becomes the word `acmewidgets`, and `app.acmewidgets.com` becomes `app` and `acmewidgets`. |

The types that share the default treatment exist so that a custom provider can treat them differently without changing the calling code. See [Customizing the Rules](#customizing-the-rules).

## How a Subject Becomes a Matcher

<xref:Singulink.Cryptography.ContextualSubjectMatcherProvider.Default> turns each subject into a matcher in three steps:

1. **Normalize.** Email and website values have the parts every address shares removed, as described above.
2. **Split.** The value is split into words on the provider's <xref:Singulink.Cryptography.ContextualSubjectMatcherProvider.Delimiters>: space, comma, period, dash, underscore, plus sign, colon, semicolon and `@` by default. Words are then split again where the case changes from lower to upper and where letters change to digits or back, so `MikeSmith`, `mike_smith` and `mike1985` all yield `mike`, and `ACMEWidgets` yields `acme` and `widgets`. Parts shorter than two characters are dropped.
3. **Build.** A single word becomes a text matcher for that word. Several words become a permutations matcher, so any subset of the words matches in any order, each word at most once.

Each word is matched the same way as a common word: case-insensitively, with character substitutions for words of three or more letters (`j4ne` matches `jane`), with repeats (`janejane`), and with an optional separator after each occurrence (`jane.doe`). The result is that a name of `Jane Doe` matches `janedoe`, `doejane`, `jane`, `doe`, `jane-doe` and `j4ne_d0e`. Each word is used at most once within a single subject match, but the default patterns allow several consecutive subjects, so `janedoejane` still matches as the subject `jane doe` followed by the subject `jane`. See [How Matching Works](../concepts/how-matching-works.md) for the matching rules in detail.

The resulting matcher is a subject in the default patterns, interchangeable with common words such as `password` or `admin`. That is what makes `iamthebusinessadmin` match for a company named `Singulink Business Solutions`, and `MikeMikeLogInToSingulinkSingulink1!` match for a user named Mike on singulink.com. See [The Default Patterns](../concepts/default-patterns.md) for the patterns a subject can appear in.

> [!NOTE]
> Short words are matched too. A subject of `Al` makes `al` a matchable word, so `al2024!` is rejected for that user. Substitutions are not applied to words of one or two letters, since a lone `4` or `@` is far more likely to be a decoration than the word `a`, so `4l` is not matched as the name. Since a password only matches when it consists entirely of matchable values, short subject words never cause a strong password to be rejected.

## Application Subjects

Some subjects apply to every check: the names of your application, company and products, and your website. Rather than passing them on every call, register them once on the <xref:Singulink.Cryptography.PasswordMatchersProvider> as <xref:Singulink.Cryptography.PasswordMatchersProvider.ApplicationSubjects>, and pass only the user's subjects per check:

```csharp
var provider = new PasswordMatchersProvider(ContextualSubjectMatcherProvider.Default, [
    new("Acme Widgets", ContextualSubjectType.Company),
    new("https://www.acmewidgets.com", ContextualSubjectType.Website),
    new("Widget Pro"),
]);

var evaluator = new PasswordMatchEvaluator(provider);

// Later, per check:
var match = evaluator.MatchPassword(password, [
    new(user.DisplayName, ContextualSubjectType.Name),
    new(user.Email, ContextualSubjectType.Email),
]);
```

Application subjects are matched exactly like per-check subjects, as subjects in every default pattern, so `iamthewidgetproadmin` and `AcmeWidgets2024!` are rejected for every user. This is also the easiest way to add words of your own to the dictionary without touching the patterns: anything registered as a subject takes part in the sentence-like patterns and the catch-all alongside the common words. See [Customizing the Patterns](customizing-patterns.md) for when the patterns themselves need changing.

## Customizing the Rules

The provider is the extension point. The evaluator asks its <xref:Singulink.Cryptography.PasswordMatchersProvider> for matchers, and that provider asks its <xref:Singulink.Cryptography.IContextualSubjectMatcherProvider> for a matcher per subject. Replace the subject provider to change how subjects are interpreted while keeping the default patterns.

#### Changing the delimiters

Construct a <xref:Singulink.Cryptography.ContextualSubjectMatcherProvider> with your own delimiters when your subject values use different separators, then build a matchers provider and evaluator around it:

```csharp
var subjectProvider = new ContextualSubjectMatcherProvider([' ', '/', '|']);
var matchersProvider = new PasswordMatchersProvider(subjectProvider);
var evaluator = new PasswordMatchEvaluator(matchersProvider);
```

#### Overriding how a subject is interpreted

Derive from <xref:Singulink.Cryptography.ContextualSubjectMatcherProvider> and override <xref:Singulink.Cryptography.ContextualSubjectMatcherProvider.GetMatcher*> to handle some subject types yourself and defer the rest to the base implementation. This example strips a company suffix such as "Inc" before the company name is split, and treats phone numbers as a single run of digits rather than as words:

```csharp
public class MySubjectMatcherProvider : ContextualSubjectMatcherProvider
{
    public MySubjectMatcherProvider() : base([' ', ',', '.', '-', '_', '@']) { }

    public override PasswordMatcher? GetMatcher(ContextualSubject subject)
    {
        if (subject.Type is ContextualSubjectType.Company)
        {
            string value = Regex.Replace(subject.Value, @"\b(inc|llc|ltd)\.?$", "", RegexOptions.IgnoreCase);
            return base.GetMatcher(new ContextualSubject(value, subject.Type));
        }

        if (subject.Type is ContextualSubjectType.PhoneNumber)
        {
            string digits = new(subject.Value.Where(char.IsDigit).ToArray());
            return digits.Length > 0 ? PasswordMatcher.Text(digits, PasswordMatchType.ContextualSubject, checkSubstitutions: false) : null;
        }

        return base.GetMatcher(subject);
    }
}
```

Return `null` for a subject that should contribute nothing. Build matchers with the factory methods on <xref:Singulink.Cryptography.PasswordMatcher>, passing <xref:Singulink.Cryptography.PasswordMatchType.ContextualSubject> as the match type so that the match reports them correctly. See [Customizing the Patterns](customizing-patterns.md) for the factory methods.

#### Implementing the interface directly

Implement <xref:Singulink.Cryptography.IContextualSubjectMatcherProvider> when you want to start from scratch, for example to match subjects against a transliterated form, or to look words up in an external service. The interface has the single <xref:Singulink.Cryptography.IContextualSubjectMatcherProvider.GetMatcher*> method, and the only contract is that the returned matcher depends on nothing but the characters it is given to match.

## Next Steps

Continue with these related articles:

- [Match Results](match-results.md) - How contextual matches are reported.
- [Customizing the Patterns](customizing-patterns.md) - The factory methods used to build matchers, and changing the patterns subjects appear in.
- [The Default Patterns](../concepts/default-patterns.md) - Where a subject can appear in a matched password.

</div>
