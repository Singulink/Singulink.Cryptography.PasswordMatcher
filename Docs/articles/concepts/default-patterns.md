<div class="article">

# The Default Patterns

<xref:Singulink.Cryptography.PasswordMatchersProvider.Default> matches passwords against ten sentence-like patterns built from curated lists of the words that appear most often in passwords, plus a catch-all pattern for short combinations of those words. This article lists the patterns, the word lists, and the decorations that can surround them, so that you know exactly what the default check rejects.

### Sentences, not words

Most weak passwords are not single words. They are tiny sentences (`iloveyou`, `letmein`, `iamthebest`), a word with a name or a service attached (`mikepassword`, `singulinkadmin`), or one of those with a year or a few symbols stuck on the end. The default patterns describe those sentences in terms of parts of speech: subjects, adjectives, verbs and the small words that join them. Contextual subjects (the user's name, your service name) are subjects in these patterns, interchangeable with `password` or `admin`.

## Subjects

A subject is one or two consecutive subject words. A subject word is either a contextual subject for the check or an entry in <xref:Singulink.Cryptography.PasswordMatchers.CommonMatchers.GeneralSubjectMatcher>:

| Group | Words |
|---|---|
| Pronouns | `i`, `me`, `you`, `this` |
| Access | `password`, `pass`, `login`, `secret`, `letmein`, `letmeinto` |
| Greetings | `welcome`, `hello`, `hi` |
| Roles | `master`, `admin`, `user`, `nobody`, `no1` |
| Sports | `football`, `baseball`, `hockey`, `soccer` |
| Favorites | `noob`, `google`, `monkey`, `dragon`, `ninja`, `batman`, `hottie`, `shadow`, `sunshine`, `princess`, `freedom`, `jesus`, `god`, `whatever`, `love`, `sex`, `starwars` |
| Numbers | `69`, `420` |

A handful of common profanities are included as well. Every word accepts repeats and a trailing separator, and words of three or more letters accept character substitutions, as described in [How Matching Works](how-matching-works.md). A subject may be preceded by a determiner from <xref:Singulink.Cryptography.PasswordMatchers.CommonMatchers.SubjectDeterminerMatcher>: `the`, `a`, `an`, `this`, `my`, `your`, `their`, `trusted`, `into`, `let`.

## The Patterns

Each pattern below is tried in order, and each except the first may start and end with a decoration (described in the next section). Square brackets mark optional parts.

| # | Pattern | Example |
|---|---|---|
| 1 | decoration | `qwerty!!!`, `111111`, `1q2w3e4r`, `abc123` |
| 2 | subject [subject] | `password`, `mikepassword`, `Welcome@123` |
| 3 | [determiner] subject [[copular verb] adjective] | `myadmin`, `passwordisgreat`, `s1ngul1nkisthebest` |
| 4 | [determiner] subject copular verb [determiner] subject | `iamadmin`, `thepasswordisapassword` |
| 5 | [determiner] adjective subject | `coolninja`, `thegreatpassword` |
| 6 | [determiner] subject [copular verb] adjective [determiner] subject | `thisisabadpassword`, `youareabadnoob` |
| 7 | [[determiner] subject copular verb] adjective [conjunction] [determiner] subject | `coolasaninja`, `iamcoolasaninja` |
| 8 | [determiner] subject [conjunction] [determiner] subject [[copular verb] adjective] | `youandme`, `letmeinadmin`, `thepasswordandtheadminaregreat` |
| 9 | [determiner] subject verb [to] verb | `ilovetologin` |
| 10 | [determiner] subject verb [to] [verb [to]] [determiner] subject | `iloveyou`, `welcometoadmin`, `MikeMikeLogInToSingulinkSingulink` |
| 11 | up to six words or structured decorations in any order, where a word may be joined to what precedes it by a single digit or symbol | `loveyou`, `trustno1`, `cool2024!`, `ninja!admin`, `mike1admin` |

Pattern 1 catches passwords that are nothing but decorations: keyboard sequences, years, repeated digits and symbols. Pattern 2 is the one most weak passwords fall into, since a single subject with a decoration covers `password123`, `admin2024!`, `Acme#1` and `n1nja!`. Pattern 11 is the safety net for combinations the sentence patterns do not anticipate: any word from any list, any contextual subject and any structured decoration, in any order, up to <xref:Singulink.Cryptography.PasswordMatchersProvider.MaxCatchAllTokens> of them. Since every token is a weak value, it cannot reject a password that contains anything uncommon.

#### Adjectives, verbs and joining words

The other word lists fill the remaining roles:

| Matcher | Words |
|---|---|
| <xref:Singulink.Cryptography.PasswordMatchers.CommonMatchers.AdjectiveMatcher> | `best`, `worst`, `good`, `bad`, `cool`, `great`, `awesome`, `amazing`, `lovely`, `sexy`, `hot`, `trusted` |
| <xref:Singulink.Cryptography.PasswordMatchers.CommonMatchers.VerbMatcher> | `love`, `like`, `hate`, `am`, `want`, `trust`, `login`, `log`, `let` |
| <xref:Singulink.Cryptography.PasswordMatchers.CommonMatchers.VerbSuffixMatcher> | `to` |
| <xref:Singulink.Cryptography.PasswordMatchers.CommonMatchers.SubjectToSubjectCopularVerbMatcher> | `is`, `am`, `are` |
| <xref:Singulink.Cryptography.PasswordMatchers.CommonMatchers.SubjectToAdjectiveCopularVerbMatcher> | `is`, `am`, `are`, each optionally followed by `so` and by `the`, `a` or `an` |
| <xref:Singulink.Cryptography.PasswordMatchers.CommonMatchers.SubjectConjunctionMatcher> | `and`, `or`, `to`, `in`, `into` |
| <xref:Singulink.Cryptography.PasswordMatchers.CommonMatchers.AdjectiveToSubjectConjunctionMatcher> | `like`, `as` |

<xref:Singulink.Cryptography.PasswordMatchers.CommonMatchers.AllWordsMatcher> combines every list above with the subjects, and is what the catch-all pattern uses.

## Decorations

Every pattern except the first may start and end with <xref:Singulink.Cryptography.PasswordMatchers.CommonMatchers.DecorationMatcher>, and the first pattern is a decoration on its own. A decoration is made of two kinds of parts:

- **Structured decorations** (<xref:Singulink.Cryptography.PasswordMatchers.CommonMatchers.StructuredDecorationMatcher>), which an attacker's rules generate without needing to guess individual characters:
  - A year from 1900 to ten years from now (<xref:Singulink.Cryptography.PasswordMatchers.CommonMatchers.YearMatcher>).
  - A character sequence of at least three characters along a keyboard row, diagonal or zigzag, a numeric keypad line, or the alphabet, in either direction, shifted or not (<xref:Singulink.Cryptography.PasswordMatchers.CommonMatchers.AllCharSequencesMatcher>): `qwerty`, `1qaz`, `1q2w3e4r`, `7410`, `abc`, `!@#$`.
  - A run of two or more of the same digit (<xref:Singulink.Cryptography.PasswordMatchers.CommonMatchers.DigitRunMatcher>) or the same symbol (<xref:Singulink.Cryptography.PasswordMatchers.CommonMatchers.SymbolRunMatcher>): `111`, `!!!!`.
  - A group of two or three digits or symbols repeated two or more times (<xref:Singulink.Cryptography.PasswordMatchers.CommonMatchers.DigitOrSymbolGroupMatcher>): `121212`, `1!1!`.
- **A short decoration** (<xref:Singulink.Cryptography.PasswordMatchers.CommonMatchers.ShortDecorationMatcher>): one to four arbitrary digits or symbols other than separators, such as `47`, `#1` or `4792`.

A decoration is a run of one to six structured decorations in any order (<xref:Singulink.Cryptography.PasswordMatchers.CommonMatchers.DecorationRunMatcher>), with at most one short decoration before, after or between runs. So `password` can be followed by `1`, `#1`, `47`, `4792`, `123`, `2024!!`, `1234qwerty`, `!!!2024`, `123123`, `121212` or `@123`, and preceded by the same, but not by five arbitrary digits (`password47921` is not matched). Years and sequences also accept repeats and separators, so `password.123123.123123` matches.

The limit of four arbitrary characters is what separates a decoration from entropy. A dictionary attack appends a few digits or symbols to every word it tries; it does not append five random ones.

## Examples

A few passwords and how the default patterns explain them, with a company subject of `Singulink Business Solutions`, a website subject of `singulink.com` and a name subject of `Mike Smith`:

| Password | Values | Pattern |
|---|---|---|
| `P@ssw0rd2024` | password, 2024 | 2 |
| `QWERTp@ssword1975` | QWERT, password, 1975 | 2 |
| `Welcome@123` | welcome, @, 123 | 2 |
| `Password#1` | password, #1 | 2 |
| `1qaz2wsx` | 1QAZ, 2WSX | 1 |
| `1q2w3e4r` | 1Q2W3E4R | 1 |
| `iloveyou!!` | i, love, you, !! | 10 |
| `LetMeIn2024!` | let, me, in, 2024, ! | 2 |
| `this is a bad password` | this, is, a, bad, password | 6 |
| `IAmABad@dmin` | i, am, a, bad, admin | 6 |
| `s1ngul1nkisthebest` | singulink, is, the, best | 3 |
| `iamthebusinessadmin` | i, am, the, business, admin | 4 |
| `MikeMikeLogInToSingulinkSingulink1!` | mike, mike, login, to, singulink, singulink, 1! | 10 |
| `admin_admin_2024` | admin, admin, 2024 | 2 |
| `trustno1` | trust, no1 | 11 |
| `cool2024!` | cool, 2024, ! | 11 |
| `mike1admin` | mike, 1, admin | 11 |

And a few that do not match, because something in them is not a common value: `thisisnotamatch` (`not` and `match` are not in the lists), `hello world!!` (`world`), `summer2024` (`summer`), `correct horse battery staple` and `Tr0ub4dor&3`.

## Further Reading

These articles cover the related topics in more depth:

- [Customizing the Patterns](../guides/customizing-patterns.md) - Adding words and patterns, or replacing the provider.
- [How Matching Works](how-matching-works.md) - Substitutions, repeats, separators and ordering.
- [Threat Model](threat-model.md) - What these patterns do and do not protect against.

</div>
