<div class="article">

# Getting Started

This guide walks through checking a password in a few minutes: evaluating it against the default patterns, telling the evaluator about the user and your application, and showing the user why a password was rejected. Each section points to the in-depth guide for that area.

### Packages

Install `Singulink.Cryptography.PasswordMatcher`. It contains everything shown in this guide, including the curated lists of common password words and the default patterns they are matched in.

### How the pieces fit

A <xref:Singulink.Cryptography.PasswordMatchEvaluator> checks a password and returns a <xref:Singulink.Cryptography.PasswordMatch> describing how it matched, or `null` if it did not. The evaluator gets its patterns from an <xref:Singulink.Cryptography.IPasswordMatchersProvider>, which combines the common words in <xref:Singulink.Cryptography.PasswordMatchers.CommonMatchers> with the <xref:Singulink.Cryptography.ContextualSubject> values you pass in for the current user. The patterns themselves are built from composable <xref:Singulink.Cryptography.PasswordMatcher> objects, which you only need to touch if you want to customize what is matched.

## Evaluating a Password

Call <xref:Singulink.Cryptography.PasswordMatchEvaluator.MatchPassword*> on <xref:Singulink.Cryptography.PasswordMatchEvaluator.Default>. A non-null result means the password is composed entirely of common password words and values in a predictable order and should be rejected:

```csharp
using Singulink.Cryptography;

var match = PasswordMatchEvaluator.Default.MatchPassword("P@ssw0rd2024", subjects: null);

if (match is not null)
{
    // match.Values is ["password", "2024"]
}
```

A password only matches when the whole password is accounted for. `P@ssw0rd2024` matches because "password" (with substituted characters) followed by a year is a pattern attackers try early, but `P@ssw0rdk4efjawei` does not match because the trailing characters are not a common value. The result is a yes-or-no answer with an explanation, not a strength score: a password that does not match may still be weak for other reasons, which is why the check is meant to be combined with a leaked password database. See [Password Policy Integration](password-policy.md).

## Providing Context

Attackers tailor dictionaries to their target, so a password made from the name of your service or the user's own name is just as guessable as `password123`. Pass the values that are specific to the user and your application as contextual subjects, and they become part of the patterns for that check:

```csharp
ContextualSubject[] subjects = [
    new("Acme Widgets", ContextualSubjectType.Company),
    new("https://www.acmewidgets.com", ContextualSubjectType.Website),
    new("Jane Doe", ContextualSubjectType.Name),
    new("jane.doe@example.com", ContextualSubjectType.Email),
];

var match = PasswordMatchEvaluator.Default.MatchPassword("AcmeJane2024!", subjects);

// match.Values is ["acme", "jane", "2024", "!"]
```

Subject values are split into words that can appear in any order, so `Jane Doe` matches `janedoe`, `doejane`, `jane` and `doe`. Email and website subjects ignore the parts that every address shares, so `com` and `www` never count as words. See [Contextual Subjects](contextual-subjects.md) for the subject types, how values are split, and how to customize that.

## Explaining the Result to the User

The match describes exactly what the password was built from. <xref:Singulink.Cryptography.PasswordMatch.Values> lists the matched words in order, and <xref:Singulink.Cryptography.PasswordMatch.Items> adds the type of each match (a common word, a contextual subject, a keyboard sequence and so on):

```csharp
if (match is not null)
{
    string words = string.Join(", ", match.Values.Select(v => $"\"{v}\""));

    return $"Your password is made up of words that are commonly used in passwords or that are easy to guess " +
           $"({words}). Add an uncommon word or character sequence to make it harder to guess.";
}
```

Showing the matched words is worth doing: a user who sees that `MikeMikeLogInToSingulink1!` was rejected because it is "mike, mike, login, to, singulink, 1!" understands the problem immediately, which a generic "password is too weak" message does not achieve. See [Match Results](match-results.md) for the match types and more on presenting them.

## Password Length

The evaluator only checks passwords up to <xref:Singulink.Cryptography.PasswordMatchEvaluator.MaxPasswordLength> characters (128 by default) and returns `null` for longer ones. Matching cost grows with password length, and a password that long made entirely of common words is not a realistic risk, so skipping the check keeps the cost of evaluating adversarial input bounded. Create your own evaluator to change the limit:

```csharp
var evaluator = new PasswordMatchEvaluator(PasswordMatchersProvider.Default, maxPasswordLength: 64);
```

> [!TIP]
> Validate the length your application accepts before running any other checks. The evaluator's limit is a safety net, not a replacement for your own maximum.

## Next Steps

Continue with these related articles:

- [Contextual Subjects](contextual-subjects.md) - Subject types, how values are turned into matchers, and customizing the splitting rules.
- [Match Results](match-results.md) - Everything a match tells you and how to present it.
- [Password Policy Integration](password-policy.md) - Combining the check with length rules, a leaked password database and rate limiting.
- [Customizing the Patterns](customizing-patterns.md) - Adding words and patterns of your own.
- [How Matching Works](../concepts/how-matching-works.md) - What happens inside a check.

</div>
