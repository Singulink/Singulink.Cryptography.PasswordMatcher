<div class="article">

# Getting Started

This guide walks through checking a password in a few minutes: evaluating it against the default patterns, telling the evaluator about the user and your application, and showing the user why a password was rejected. Each section points to the in-depth guide for that area.

### Packages

Install `Singulink.Cryptography.PasswordMatcher`. It contains everything shown in this guide, including the curated lists of common password words and the default patterns they are matched in.

## Evaluating a Password

Call <xref:Singulink.Cryptography.PasswordMatchEvaluator.MatchPassword*> on <xref:Singulink.Cryptography.PasswordMatchEvaluator.Default>. It returns a <xref:Singulink.Cryptography.PasswordMatch> that describes how the password matched, or `null` if it did not. A non-null result means the password is composed entirely of common password words and values in a predictable order and should be rejected:

```csharp
using Singulink.Cryptography;

var match = PasswordMatchEvaluator.Default.MatchPassword("P@ssw0rd2024", subjects: null);

if (match is not null)
{
    // match.Values is ["password", "2024"]
}
```

A password only matches when the whole password is accounted for:

```text
P@ssw0rd2024        matches: "password" with substituted characters, then a year
P@ssw0rdk4efjawei   no match: the trailing characters are not a common value
```

The result is a yes-or-no answer with an explanation, not a strength score. A password that does not match may still be weak for other reasons, which is why the check is meant to be combined with a leaked password database. See [Password Policy Integration](password-policy.md).

> [!TIP]
> Trying it with weak passwords of your own? `summer2024` and `hunter2` do not match, because `summer` and `hunter` are not in the word lists. A leaked password database catches those. See [What It Catches and What It Doesn't](../concepts/threat-model.md).

The evaluator gets its patterns from the common words in <xref:Singulink.Cryptography.PasswordMatchers.CommonMatchers> and from the contextual subjects you pass in, which the next section describes. You only need to work with the patterns if you want to change what is matched.

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

Subject values are split into words that can appear in any order:

```text
Name "Jane Doe"
  janedoe   matches
  doejane   matches
  jane      matches
  doe       matches
```

Email and website subjects ignore the parts that every address shares, so `com` and `www` never count as words. See [Contextual Subjects](contextual-subjects.md) for the subject types, how values are split, and how to customize that.

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

Showing the matched words is worth doing, because a generic "password is too weak" message does not get the problem across. A user named Mike on singulink.com who sees what their password was built from understands it immediately:

```text
MikeMikeLogInToSingulink1!   built from "mike", "mike", "login", "to", "singulink", "1!"
```

See [Explaining a Rejection](match-results.md) for the match types and more on presenting them.

## Password Length

The evaluator only checks passwords up to <xref:Singulink.Cryptography.PasswordMatchEvaluator.MaxPasswordLength> characters (128 by default) and returns `null` for longer ones. Matching cost grows with password length, so skipping longer passwords keeps the cost of evaluating adversarial input bounded. Nothing is lost by it: a password that long made entirely of common words is not a realistic risk. Create your own evaluator to change the limit:

```csharp
var evaluator = new PasswordMatchEvaluator(PasswordMatchersProvider.Default, maxPasswordLength: 64);
```

> [!TIP]
> Validate the length your application accepts before running any other checks. The evaluator's limit is a safety net, not a replacement for your own maximum.

## Next Steps

Continue with these related articles:

- [Password Policy Integration](password-policy.md) - Combining the check with length rules, a leaked password database and rate limiting.
- [Contextual Subjects](contextual-subjects.md) - Subject types, how values are turned into matchers, and customizing the splitting rules.
- [Explaining a Rejection](match-results.md) - Everything a match tells you and how to present it.
- [Adding Words and Patterns](customizing-patterns.md) - Adding words and patterns of your own.

</div>
