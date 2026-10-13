<div class="article">

# Threat Model

Password Matcher targets one specific attack and is deliberately conservative about everything else. This article describes the attack it defends against, what it catches and what it leaves to other checks, and the kinds of false positives and negatives to expect.

### Dynamic dictionary attacks

A dictionary attack does not try random strings. It starts from a short list of the words people actually use in passwords, generates the common variations of each (capitalization, character substitutions, a year or a few digits or symbols on the end), and combines them in the orders people actually combine them. A *dynamic* dictionary attack adds words specific to the target: the name of the service, and whatever is known about the account owner from their profile, their email address or earlier breaches.

The first few million candidates such an attack produces cover a remarkable share of real passwords, and they are exactly the passwords an online attacker with a rate limit in their way will try. Password Matcher recognizes passwords that fall in that set, with particular attention to the contextual ones, because those are the passwords that no leaked password database can know about.

## What Is Caught

A password is rejected when it is composed *entirely* of values from the default word lists and the contextual subjects, in one of the default patterns, with the variations an attack generates:

- **Common words in common structures.** `password`, `iloveyou`, `letmein`, `iamthebest`, `welcometoadmin`, `coolasaninja`. See [The Default Patterns](default-patterns.md).
- **Contextual words.** The user's name, username, email address words, and your service, company and product names, alone or in the same structures: `mikepassword`, `singulinkadmin`, `iamthebusinessadmin`.
- **Any short combination of those words.** Up to six words or structured decorations in any order, so `loveyou`, `trustno1`, `mike123acme` and `ninja!admin` are caught even though no sentence pattern describes them.
- **Character substitutions.** `p@55w0rd`, `n1nja`, `s1ngul1nk`.
- **Repeats and separators.** `passwordpassword`, `mike.mike`, `the-bad-admin`.
- **Decorations.** Years, keyboard, keypad and alphabet sequences, runs of repeated digits or symbols, repeated groups, and up to four arbitrary digits or symbols, in any order, at either end: `password123`, `2024admin!`, `Acme#1`, `Welcome@123`, `mike4792`, `acme121212`, `1q2w3e4rpassword`.

The matching is exhaustive within those rules. There is no sampling and no heuristic scoring: if a password can be built from the lists in one of the patterns, it is found.

## What Is Not Caught

The check is one layer of a password policy, not the whole of it. It does not attempt to catch:

- **Words outside the lists.** The lists hold roughly a hundred of the most common words, chosen for how often they appear rather than for coverage. `summer2024` and `hunter2` are weak and are not matched, because `summer` and `hunter` are not in the lists. A leaked password database catches these: they have been leaked many times over.
- **Passphrases of ordinary words.** `correct horse battery staple` is not matched, and should not be: with a large enough vocabulary a passphrase is strong. The check cannot tell a four-word passphrase from a four-word weak password except by the words, which is why the lists are kept short and specific.
- **Structure without the words.** A password shaped like `Word1!` is not rejected for its shape. The check never penalizes a pattern that happens to contain an uncommon word, because the uncommon word is what gives the password its strength.
- **More than a few arbitrary characters.** Five or more arbitrary digits or symbols are not a decoration: `password47921` is not matched. A dictionary attack appends a few characters to each word it tries, not a random string.
- **Passwords that are strong but known.** A long random password that was leaked from another site is as compromised as `password`. Only a leaked password database can know that.

The intended complement is a leaked password database such as [Have I Been Pwned](https://haveibeenpwned.com/Passwords) for the compromised side, and rate limiting for the online guessing attack itself. See [Password Policy Integration](../guides/password-policy.md).

## False Positives

The full consumption rule makes false positives rare: a password is only rejected when nothing in it falls outside the lists. A user who adds any uncommon word, or any sequence of characters that is not a sequence, a repeat or a few arbitrary digits or symbols, is never rejected by this check. The cases worth knowing about are:

- **Short contextual words.** A name of `Al` or a username of `jd` makes `al` or `jd` a matchable word. This cannot reject a password that contains anything else, but it does mean that `al2024!` is rejected for that user, which is correct, and that the explanation can name a word the user did not think of as a word. Substitutions are not applied to words of one or two letters, so `4l` is not taken for the name.
- **Short decorations.** Up to four arbitrary digits or symbols count as a decoration at each end of the words, so `password4792` and `Acme#1` are rejected. This is deliberate: a word followed by a four-digit number is one of the first things a dictionary attack tries, and `4792` could as easily be a birth year, a PIN or a street number as a random choice.
- **Joining characters.** In the catch-all pattern a single digit or symbol may join a word to what precedes it, so `ninja!admin` and `mike1admin` are rejected. The explanation reports the joiner as a value of its own.

## False Negatives

A password that is nearly all common values but has one character out of place escapes the check: `passwordk` is not matched because `k` is not accounted for. This is by design. Rejecting passwords that *contain* common words would reject a large share of strong passwords too, and the attacker's dictionary does not generate `passwordk` early either. The leaked password database is the backstop for near misses that have been used often enough to leak.

> [!NOTE]
> The check is strongest when the contextual subjects are complete. A dynamic dictionary attack uses everything public about the account, so pass everything public about the account: display name, username, email address, company, website and any other profile fields an attacker could read.

## Further Reading

These articles cover the related topics in more depth:

- [Password Policy Integration](../guides/password-policy.md) - Combining the check with a leaked password database and rate limiting.
- [The Default Patterns](default-patterns.md) - Exactly which words and structures are matched.
- [Customizing the Patterns](../guides/customizing-patterns.md) - Extending the lists for your application.

</div>
