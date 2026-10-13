<div class="article">

# Singulink.Cryptography.PasswordMatcher

**Password Matcher** can be thought of as a dictionary attack password generator, but in reverse. Instead of generating candidate passwords, it recognizes them: a composable matcher grammar checks whether a password is built entirely from the words, phrases and decorations that dictionary attacks combine, including contextual values such as your application or service name and the user's name, email address and company. Passwords that are vulnerable to simple dynamic dictionary attacks are detected without maintaining a specialized password dictionary, which makes it easy to follow the latest [NIST password guidelines](https://pages.nist.gov/800-63-4/sp800-63b/passwords/) and give users wide latitude in choosing their passwords while still protecting them.

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
