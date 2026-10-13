<div class="article">

# Singulink.Cryptography.PasswordMatcher

**Password Matcher** is kind of like a dictionary attack password generator but in reverse. Instead of generating passwords, it uses a composable matcher system and algorithms similar to password generators to check whether a password follows a common password pattern built from the words and phrases that dictionary attacks use, taking into account contextual information like your app or service name and the user's information (name, email address, company, etc). This makes it easy to detect passwords that are vulnerable to simple dynamic dictionary attacks and to follow the latest [NIST password guidelines](https://pages.nist.gov/800-63-4/sp800-63b/passwords/), giving your users lots of flexibility when choosing their passwords while still protecting them, without having to maintain a specialized password dictionary.

This package is part of our **Singulink Libraries** collection. Visit https://github.com/Singulink to see our full list of publicly available libraries and other open-source projects.

### Key Features

✔️ Detects passwords composed entirely of the most common password words, in the orders attackers actually try them  
✔️ Contextual matching against your app name and the user's name, email address, username, company, website, address or phone number  
✔️ Understands character substitutions (`p@55w0rd`), repeats, years, keyboard, keypad and alphabet sequences, and digit or symbol decorations  
✔️ Explains every match so you can tell users exactly which words their password was built from  
✔️ Composable, immutable matcher grammar that is easy to extend with your own words, patterns and matchers  
✔️ Memoized matching with bounded cost and a configurable password length limit  
✔️ Full AOT and WinRT support  
✔️ Extensive test coverage  

### Installation

The package is available on NuGet - simply install the `Singulink.Cryptography.PasswordMatcher` package.

**Supported Runtimes**: Everywhere .NET Standard 2.0 is supported, including:
- .NET
- .NET Framework
- Mono / Xamarin

End-of-life runtime versions that are no longer officially supported are not tested or supported by this library.

## Information and Links

Here are some additional links to get you started:

- [Getting Started](articles/guides/getting-started.md) - Visit here first for a quick walkthrough. It opens the guides: in-depth articles on contextual subjects, match results, customizing the patterns, writing your own matchers and fitting the check into a password policy, listed in the sidebar.
- [Concepts](articles/concepts/how-matching-works.md) - How matching, the default patterns and the threat model work under the hood, starting with how matching works.
- [API Documentation](api/Singulink.Cryptography.yml) - Browse the fully documented API here.
- [Chat on Discord](https://discord.gg/EkQhJFsBu6) - Have questions or want to discuss the library? This is the place for all Singulink project discussions.
- [GitHub Repo](https://github.com/Singulink/Singulink.Cryptography.PasswordMatcher) - File issues, contribute pull requests or check out the code for yourself!

</div>
