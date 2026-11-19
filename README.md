# Singulink.Cryptography.PasswordMatcher

[![Chat on Discord](https://img.shields.io/discord/906246067773923490)](https://discord.gg/EkQhJFsBu6)
[![View nuget packages](https://img.shields.io/nuget/v/Singulink.Cryptography.PasswordMatcher.svg)](https://www.nuget.org/packages/Singulink.Cryptography.PasswordMatcher/)
[![Build and Test](https://github.com/Singulink/Singulink.Cryptography.PasswordMatcher/workflows/build%20and%20test/badge.svg)](https://github.com/Singulink/Singulink.Cryptography.PasswordMatcher/actions?query=workflow%3A%22build+and+test%22)

**Password Matcher** works like a dictionary attack password generator in reverse: instead of generating candidate passwords, it recognizes them. A composable matcher grammar checks whether a password is built entirely from the words, phrases and decorations that dictionary attacks combine. That includes contextual values such as your application or service name and the user's name, email address and company.

It detects passwords that are vulnerable to simple dynamic dictionary attacks, and you do not have to maintain a specialized password dictionary for it. That makes it easy to follow the latest [NIST password guidelines](https://pages.nist.gov/800-63-4/sp800-63b/passwords/) and give users wide latitude in choosing their passwords while still protecting them.

### About Singulink

We are a small team of engineers and designers dedicated to building beautiful, functional, and well-engineered software solutions. We offer very competitive rates as well as fixed-price contracts and welcome inquiries to discuss any custom development / project support needs you may have.

This package is part of our **Singulink Libraries** collection. Visit https://github.com/Singulink to see our full list of publicly available libraries and other open-source projects.

## Installation

The package is available on NuGet - simply install the `Singulink.Cryptography.PasswordMatcher` package.

**Supported Runtimes**: Everywhere .NET Standard 2.0 is supported, including:
- .NET
- .NET Framework
- Mono / Xamarin

End-of-life runtime versions that are no longer officially supported are not tested or supported by this library.

## Usage

For comprehensive protection against dictionary attacks, use **Password Matcher** together with a leaked password database such as [Have I Been Pwned](https://haveibeenpwned.com/), and with rate limiting as the [NIST guidelines](https://pages.nist.gov/800-63-4/sp800-63b/authenticators/#throttle) recommend. The database covers passwords that are known to be compromised. Password Matcher covers the contextual ones: easy to predict from your service and the user's details, but never going to appear in any database.

### Checking a password

```cs
using Singulink.Cryptography;

ContextualSubject[] subjects = [
    new("Acme Widgets", ContextualSubjectType.Company),
    new("https://www.acmewidgets.com", ContextualSubjectType.Website),
    new("Jane Doe", ContextualSubjectType.Name),
    new("jane.doe@example.com", ContextualSubjectType.Email),
];

var match = PasswordMatchEvaluator.Default.MatchPassword(password, subjects);

if (match is not null)
{
    // Reject the password. match.Values holds the values it was built from: for "AcmeJane2024!" it holds
    // ["acme", "jane", "2024", "!"]. match.Items also gives the type of each match.
}
```

A password matches when it is composed entirely of common password words, phrases and values or the contextual subjects you provide, in a predictable order. Common values include `password`, `iloveyou` and `qwerty`, along with common character substitutions like `p@55w0rd`. The words may be decorated with years, keyboard and keypad sequences, repeated characters and groups, and a few arbitrary digits or symbols, as in `Acme#1`, `mike2024!!` and `Welcome@123`. The default matchers match about 100 of the most common password words in the most common orders, plus any short combination of those words and decorations.

If a match is returned, you should display a message to the user along the lines of:

> Your password is composed entirely of the most common words used in passwords or easily guessed contextual words in a predictable sequence. Suggestion: Add an uncommon word or character sequence to your password to make it less susceptible to attacks.

You can optionally display the matched values to the user so they can see the phrase their password matched.

### Contextual subjects

Contextual subjects are values specific to your application or the user that a password should not be built from. Each subject value is split into words on spaces, periods, dashes, underscores, plus signs, commas, colons, semicolons and `@`, and the words can appear in any order:

```text
Name "Jane Doe"
  janedoe   matches
  doejane   matches
  jane      matches
  doe       matches
```

Email subjects ignore their top-level domain. Website subjects ignore their protocol, `www.` prefix, top-level domain and path. So `com` and `www` never count as matched words.

### Password length

Evaluation cost grows roughly quadratically with password length, so `PasswordMatchEvaluator` only evaluates passwords up to `MaxPasswordLength` characters (128 by default) and reports longer passwords as not matched. A password that long made entirely of common words is not a realistic risk, and skipping the check keeps the cost of evaluating adversarial input bounded. To change the limit, create your own evaluator:

```cs
var evaluator = new PasswordMatchEvaluator(PasswordMatchersProvider.Default, maxPasswordLength: 64);
```

### Customizing matching

The library is built from composable matchers so the default behavior can be extended or replaced:

- **Custom patterns**: Derive from `PasswordMatchersProvider` and override `GetMatchers()` to add or change the patterns passwords are checked against. The building blocks are the predefined word lists and decoration matchers in `CommonMatchers`, and the factory methods on `PasswordMatcher`: `Text()`, `Sequence()`, `Any()`, `Optional()`, `Repeat()`, `Permutations()`, `CharSequence()`, `FixedDigits()`, `RepeatedChars()`, `RepeatedGroups()` and `AnyChars()`.
- **Custom contextual subject handling**: Implement `IContextualSubjectMatcherProvider` (or derive from `ContextualSubjectMatcherProvider`) to change how subjects are turned into matchers.
- **Custom matchers**: Derive from `ValueMatcher` to create a matcher that consumes text, or from `PasswordMatcher` to compose other matchers.

Matchers and evaluators are immutable and safe to share between threads, so the `Default` instances can be used from anywhere.

## Further Reading

The [project documentation site](https://www.singulink.com/Docs/Singulink.Cryptography.PasswordMatcher/) has in-depth guides on contextual subjects, match results, customizing the patterns, writing your own matchers and fitting the check into a password policy, along with concept articles on how matching works and what it does and does not protect against. You can also browse the [fully documented API](https://www.singulink.com/Docs/Singulink.Cryptography.PasswordMatcher/api/Singulink.Cryptography.html) there.

#### Related Projects:

- [**Singulink.Cryptography.PasswordHasher**](https://github.com/Singulink/Singulink.Cryptography.PasswordHasher): Upgradable hash algorithm password management library.
- [**Singulink.Cryptography.Pwned**](https://github.com/Singulink/Singulink.Cryptography.Pwned): Pwned client/server implementation for self-hosting a pwned password service.
