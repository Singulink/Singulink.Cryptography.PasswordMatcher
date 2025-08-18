# Singulink.Cryptography.PasswordMatcher

[![Chat on Discord](https://img.shields.io/discord/906246067773923490)](https://discord.gg/EkQhJFsBu6)
[![View nuget packages](https://img.shields.io/nuget/v/Singulink.Cryptography.PasswordMatcher.svg)](https://www.nuget.org/packages/Singulink.Cryptography.PasswordMatcher/)
[![Build and Test](https://github.com/Singulink/Singulink.Cryptography.PasswordMatcher/workflows/build%20and%20test/badge.svg)](https://github.com/Singulink/Singulink.Cryptography.PasswordMatcher/actions?query=workflow%3A%22build+and+test%22)

**Password Matcher** is kind of like a dictionary attack password generator but in reverse - instead of generating passwords, it uses a composable matcher system and algorithms similar to password generators to check if the password matches a common password pattern with the most commonly used password words/phrases that dictionary attacks use to generate passwords, taking into account contextual information like your app/service name and the user's information (name, email address, company, etc). This makes it easy to detect passwords that are vulnerable to simple dynamic dictionary attacks and follow the latest [NIST password guidelines](https://pages.nist.gov/800-63-4/sp800-63b/passwords/), giving your users lots of flexibility when choosing their passwords while still protecting them, without having to maintain a specialized password dictionary.

### About Singulink

We are a small team of engineers and designers dedicated to building beautiful, functional, and well-engineered software solutions. We offer very competitive rates as well as fixed-price contracts and welcome inquiries to discuss any custom development / project support needs you may have.

This package is part of our **Singulink Libraries** collection. Visit https://github.com/Singulink to see our full list of publicly available libraries and other open-source projects.

## 🚧🚧 **UNDER DEVELOPMENT 🚧🚧** 

Project is currently in beta and still needs to be properly documented but you are welcome to use it as you see fit.

## Installation

The package is available on NuGet - simply install the `Singulink.Cryptography.PasswordMatcher` package.

**Supported Runtimes**: Everywhere .NET Standard 2.0 is supported, including:
- .NET
- .NET Framework
- Mono / Xamarin

End-of-life runtime versions that are no longer officially supported are not tested or supported by this library.

## Usage

**Password Matcher** is inteded to be used together with something like the [Have I Been Pwned](https://haveibeenpwned.com/) password database to disallow the use of top leaked passwords (or all leaked passwords) and rate-limiting (as per [NIST guidelines](https://pages.nist.gov/800-63-4/sp800-63b/authenticators/#throttle)) to get comprehensive dictionary attack protection.

More documentation is coming soon, but for now you can have a [look at the tests](https://github.com/Singulink/Singulink.Cryptography.PasswordMatcher/blob/main/Tests/Singulink.Cryptography.PasswordMatcher.Tests/PasswordMatchEvaluatorTests.cs) to get an idea of how checking passwords works.

The default [`PasswordMatchersProvider`](https://github.com/Singulink/Singulink.Cryptography.PasswordMatcher/blob/main/Source/Singulink.Cryptography.PasswordMatcher/PasswordMatchersProvider.cs) implementation returns matchers that cover the most easily dictionary attacked password patterns using curated common password data in [`CommonMatchers`](https://github.com/Singulink/Singulink.Cryptography.PasswordMatcher/blob/main/Source/Singulink.Cryptography.PasswordMatcher/PasswordMatchers/CommonMatchers.cs) and contextual subjects you provide (i.e. the name of your service, the user's name / email address / etc). It matches with ~100 of the most common password words in an easily predictable order.

If the check returns a match, you should display a message to the user something along the lines of:

> Your password is composed entirely of the top 100 most common words used in passwords or easily guessed contextual words in a predictable sequence. Suggestion: Add an uncommon word or character sequence to your password to make it less susceptible to attacks.

You can optionally display the list of matched texts to the user so they can see the phrase their password variation matched to.

The library is written to be extensible, so you can easily add your own matchers or override the default ones if you want to customize the behavior.

## Further Reading

API to be documented soon...
<!--
You can view the fully documented API on the [project documentation site](https://www.singulink.com/Docs/Singulink.Cryptography.PasswordMatcher/api/Singulink.Cryptography.PasswordMatcher.html).
-->

#### Related Projects:

- [**Singulink.Cryptography.PasswordHasher**](https://github.com/Singulink/Singulink.Cryptography.PasswordHasher): Upgradable hash algorithm password management library.
- [**Singulink.Cryptography.Pwned**](https://github.com/Singulink/Singulink.Cryptography.Pwned): Pwned client/server implementation for self-hosting a pwned password service.