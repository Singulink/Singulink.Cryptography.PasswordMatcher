<div class="article">

# Password Policy Integration

Password Matcher answers one question: is this password built entirely from words an attacker would try first? A complete password policy needs a few more pieces. This guide shows where the check fits alongside length rules, a leaked password database and rate limiting, and how to host the evaluator in an application.

### What the guidelines ask for

Modern guidance, such as [NIST SP 800-63B](https://pages.nist.gov/800-63-4/sp800-63b/passwords/), has moved away from composition rules (requiring digits, symbols and mixed case) and from forced rotation. Both push users towards predictable patterns like `Password1!`. The guidance asks instead for a small set of checks that actually block the passwords attackers try:

- Require a reasonable minimum length and allow long passwords (at least 64 characters).
- Screen new passwords against a blocklist of commonly used, expected or compromised passwords.
- Rate limit authentication attempts.
- Let users see what they typed, and accept any printable character including spaces.

Password Matcher is the "expected" part of the blocklist: the passwords that are not in any leaked list yet but that a dictionary attack tailored to your service and the user would generate. A leaked password database such as [Have I Been Pwned](https://haveibeenpwned.com/Passwords) is the "compromised" part. Neither covers the other.

## A Complete Check

Run the cheap checks first, then the matcher, then the leaked password lookup. The matcher runs in well under a millisecond for ordinary passwords, while a leaked password lookup usually involves a network call or a large local database:

```csharp
public async Task<string?> ValidatePasswordAsync(string password, User user, CancellationToken cancellationToken)
{
    if (password.Length < 8)
        return "Your password must be at least 8 characters long.";

    if (password.Length > 64)
        return "Your password must be at most 64 characters long.";

    ContextualSubject[] subjects = [
        new(user.DisplayName, ContextualSubjectType.Name),
        new(user.UserName, ContextualSubjectType.Username),
        new(user.Email, ContextualSubjectType.Email),
    ];

    if (_evaluator.MatchPassword(password, subjects) is { } match)
    {
        string words = string.Join(", ", match.Values.Distinct().Select(v => $"\"{v}\""));
        return $"Your password is made up entirely of words that are commonly used in passwords or that are easy to guess " +
               $"from your account details ({words}). Add an uncommon word or character sequence to make it harder to guess.";
    }

    if (await _pwnedPasswords.IsPwnedAsync(password, cancellationToken))
        return "This password has appeared in a data breach and cannot be used. Choose a different password.";

    return null;
}
```

The subjects should include everything the attacker could plausibly know: the user's name, username, email address, and any other profile details that are visible or guessable. The names of your service, company and products apply to every check, so register them once on the provider as application subjects (shown in [Hosting the Evaluator](#hosting-the-evaluator)) rather than passing them each time. See [Contextual Subjects](contextual-subjects.md).

> [!TIP]
> Run the same check when a password is changed, not only at sign-up, and consider passing the user's previous display names or email addresses as subjects as well, since users tend to reuse them.

## Length Limits

Two limits are involved, and they serve different purposes:

- **Your application's maximum** is a policy decision. The guidelines ask for at least 64 characters to be accepted; 64 to 128 is typical, and the cost of the password hash is usually what bounds it.
- <xref:Singulink.Cryptography.PasswordMatchEvaluator.MaxPasswordLength> is a safety net inside the evaluator, 128 by default. The evaluator returns longer passwords as not matched without evaluating them, so an unusually long input can never make a check expensive. Matching cost grows roughly quadratically with length: a 128 character password built entirely of repeated common tokens takes tens of milliseconds, where an ordinary password takes a fraction of one.

Enforce your own maximum before calling the evaluator, as the example does. If your maximum is higher than 128, decide whether passwords above the evaluator's limit should be checked at all. A 200 character password composed entirely of common words is not a realistic attack target, so leaving the default and skipping the check is reasonable. If you prefer, raise the limit with the evaluator's constructor.

## Hosting the Evaluator

<xref:Singulink.Cryptography.PasswordMatchEvaluator> is immutable and safe to use from any number of threads, so one instance serves the whole application. Build it once with your application subjects, register it as a singleton, and inject it through <xref:Singulink.Cryptography.IPasswordMatchEvaluator>:

```csharp
var matchersProvider = new PasswordMatchersProvider(ContextualSubjectMatcherProvider.Default, [
    new("Acme Widgets", ContextualSubjectType.Company),
    new("https://www.acmewidgets.com", ContextualSubjectType.Website),
    new("Widget Pro"),
]);

services.AddSingleton<IPasswordMatchEvaluator>(new PasswordMatchEvaluator(matchersProvider, maxPasswordLength: 64));
```

<xref:Singulink.Cryptography.PasswordMatchEvaluator.Default> is fine to use directly when there are no application subjects to register.

The first check in a process pays a one-time cost of a few tens of milliseconds to build the common word lists; every check after that reuses them. Nothing is allocated per check beyond the match state for that password.

#### Testing with a fake evaluator

Code that depends on <xref:Singulink.Cryptography.IPasswordMatchEvaluator> can be tested with a stub that returns a fixed <xref:Singulink.Cryptography.PasswordMatch> or `null`. To create the match, pass its items to the constructor:

```csharp
var stubMatch = new PasswordMatch([
    new PasswordMatchItem("password", PasswordMatchType.Common),
    new PasswordMatchItem("2024", PasswordMatchType.Common),
]);
```

## Using It With ASP.NET Core Identity

ASP.NET Core Identity checks a password with every registered [IPasswordValidator&lt;TUser&gt;](https://learn.microsoft.com/dotnet/api/microsoft.aspnetcore.identity.ipasswordvalidator-1) when a user is created with a password, and when a password is added or changed. Plug the check in as one more validator.

#### Writing the validator

The validator builds the subjects from the user and reports the matched words as an [IdentityError](https://learn.microsoft.com/dotnet/api/microsoft.aspnetcore.identity.identityerror):

```csharp
public sealed class PasswordMatchValidator<TUser>(IPasswordMatchEvaluator evaluator) : IPasswordValidator<TUser>
    where TUser : class
{
    public async Task<IdentityResult> ValidateAsync(UserManager<TUser> manager, TUser user, string? password)
    {
        if (string.IsNullOrEmpty(password))
            return IdentityResult.Success;                  // nothing to check

        List<ContextualSubject> subjects = [];

        if (await manager.GetUserNameAsync(user) is { } userName)
            subjects.Add(new(userName, ContextualSubjectType.Username));

        if (manager.SupportsUserEmail && await manager.GetEmailAsync(user) is { } email)
            subjects.Add(new(email, ContextualSubjectType.Email));

        if (evaluator.MatchPassword(password, subjects) is not { } match)
            return IdentityResult.Success;

        string words = string.Join(", ", match.Values.Distinct().Select(v => $"\"{v}\""));

        return IdentityResult.Failed(new IdentityError {
            Code = "CommonPassword",
            Description = $"Your password is made up entirely of words that are commonly used in passwords or that are easy to " +
                          $"guess from your account details ({words}). Add an uncommon word or character sequence to make it harder to guess.",
        });
    }
}
```

Add any other profile fields your user type has, such as a display name, as further subjects.

#### Registering it

Register the evaluator as a singleton, as shown above, and add the validator to Identity:

```csharp
builder.Services.AddSingleton<IPasswordMatchEvaluator>(new PasswordMatchEvaluator(matchersProvider, maxPasswordLength: 64));

builder.Services.AddIdentity<IdentityUser, IdentityRole>(options => {
        options.Password.RequiredLength = 8;
        options.Password.RequireDigit = false;              // composition rules off, as NIST advises
        options.Password.RequireLowercase = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;
    })
    .AddPasswordValidator<PasswordMatchValidator<IdentityUser>>();  // runs alongside Identity's own validator
```

Identity runs your validator in addition to its own, which applies the options. It has no option for a maximum length, so enforce yours where the password is accepted, as in [A Complete Check](#a-complete-check).

> [!NOTE]
> By default Identity requires a digit, a lower-case letter, an upper-case letter and a symbol. Composition rules like these push users towards passwords like `Password1!`, so turn them off and rely on length and this check, as [the guidelines](#what-the-guidelines-ask-for) advise.

## Next Steps

Continue with these related articles:

- [What It Catches and What It Doesn't](../concepts/threat-model.md) - What the matcher catches, what it leaves to the leaked password database, and the false positives to expect.
- [Explaining a Rejection](match-results.md) - Wording the rejection message.
- [Adding Words and Patterns](customizing-patterns.md) - Adding your product names and other words.

</div>
