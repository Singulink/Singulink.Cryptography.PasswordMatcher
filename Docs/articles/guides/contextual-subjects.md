<div class="article">

# Contextual Subjects

A contextual subject is a value specific to the user or your application that a password should not be built from: the user's name, their email address, your product name, your website. This guide covers the subject types, how a subject value is turned into a matcher, and how to change the rules.

### Why context matters

A password list built for attacking a particular service always includes the service's name, and a list built for a particular account includes whatever is known about its owner. For a user named Jane on Acme's site, `AcmeJane2024!` is as weak as `password2024!` is anywhere, but no static dictionary can know that. Contextual subjects let the evaluator know it for each check.

## Creating Subjects

A <xref:Singulink.Cryptography.ContextualSubject> pairs a value with a <xref:Singulink.Cryptography.ContextualSubjectType>. The value is trimmed. Everything else about how it is interpreted is up to the provider that turns it into a matcher, described below. Pass the subjects for the current user to <xref:Singulink.Cryptography.PasswordMatchEvaluator.MatchPassword*> on each check:

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

1. **Normalize.** The parts that every address shares are removed from email and website values, as the table above shows.
2. **Split.** The value is split into words on the provider's <xref:Singulink.Cryptography.ContextualSubjectMatcherProvider.Delimiters>: space, comma, period, dash, underscore, plus sign, colon, semicolon and `@` by default. Words are then split again where the case changes from lower to upper and where letters change to digits or back. Parts shorter than two characters are dropped.
3. **Build.** A single word becomes a text matcher for that word. Several words become a permutations matcher, so any subset of the words matches in any order, each word at most once.

The split in step 2 gives these words:

| Value | Words |
|---|---|
| `MikeSmith` | `mike`, `smith` |
| `mike_smith` | `mike`, `smith` |
| `mike1985` | `mike`, `1985` |
| `ACMEWidgets` | `acme`, `widgets` |

Each word is then matched the same way as a common word: case-insensitively, with character substitutions for words of three or more letters, with repeats, and with an optional separator after each occurrence. For a name of `Jane Doe`:

```text
janedoe       matches
doejane       matches: any order
jane          matches: any subset
j4ne          matches: substitutions
janejane      matches: a repeat
jane.doe      matches: a separator
jane-doe      matches
j4ne_d0e      matches
janedoejane   matches: the subject "jane doe", then the subject "jane"
```

Each word is used at most once within a single subject match. The last line still matches because the default patterns allow several consecutive subjects. See [How Matching Works](../concepts/how-matching-works.md) for the matching rules in detail.

In the default patterns, the resulting matcher can stand wherever a subject word can, interchangeably with common words such as `password` or `admin`:

```text
Company "Singulink Business Solutions"
  iamthebusinessadmin                   matches: "i", "am", "the", "business", "admin"

Name "Mike", website "singulink.com"
  MikeMikeLogInToSingulinkSingulink1!   matches: "mike", "mike", "login", "to", "singulink", "singulink", "1!"
```

See [Default Patterns](../concepts/default-patterns.md) for the patterns a subject can appear in.

#### Short words

Short words are matched too. Substitutions are not applied to words of one or two letters, though, since a lone `4` or `@` is far more likely to be a decoration than the word `a`:

```text
Name "Al"
  al2024!   matches: "al", "2024", "!"
  4l        no match: no substitutions in a two-letter word
```

> [!NOTE]
> A password only matches when it consists entirely of matchable values, so short subject words never cause a strong password to be rejected.

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

Application subjects are matched exactly like per-check subjects, and can stand wherever a subject word can in every default pattern. With the provider above, these are rejected for every user:

```text
iamthewidgetproadmin   matches: "i", "am", "the", "widget", "pro", "admin"
AcmeWidgets2024!       matches: "acme", "widgets", "2024", "!"
```

This is also the easiest way to add words of your own to the dictionary without touching the patterns: anything registered as a subject takes part in the sentence-like patterns and the catch-all alongside the common words. See [Adding Words and Patterns](customizing-patterns.md) for when the patterns themselves need changing.

## Customizing the Rules

The subject provider is the extension point. The evaluator asks its <xref:Singulink.Cryptography.PasswordMatchersProvider> for matchers, and that provider asks its <xref:Singulink.Cryptography.IContextualSubjectMatcherProvider> for one matcher per subject. To change how subjects are interpreted while keeping the default patterns, replace the subject provider.

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

Return `null` for a subject that should contribute nothing. Build matchers with the factory methods on <xref:Singulink.Cryptography.PasswordMatcher>, passing <xref:Singulink.Cryptography.PasswordMatchType.ContextualSubject> as the match type so that the match reports them correctly. See [Matcher Building Blocks](building-blocks.md) for the factory methods.

#### Implementing the interface directly

Implement <xref:Singulink.Cryptography.IContextualSubjectMatcherProvider> when you want to start from scratch, for example to match subjects against a transliterated form, or to look words up in an external service. The interface has the single <xref:Singulink.Cryptography.IContextualSubjectMatcherProvider.GetMatcher*> method, and the only contract is that the returned matcher depends on nothing but the characters it is given to match.

## Next Steps

Continue with these related articles:

- [Explaining a Rejection](match-results.md) - How contextual matches are reported.
- [Adding Words and Patterns](customizing-patterns.md) - Adding words of your own, and changing the patterns subjects appear in.
- [Default Patterns](../concepts/default-patterns.md) - Where a subject can appear in a matched password.

</div>
