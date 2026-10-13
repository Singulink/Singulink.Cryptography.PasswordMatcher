<div class="article">

# How Matching Works

Password Matcher is a small grammar engine: the patterns are a grammar, the password is the input, and a match is a parse that consumes the whole input. This article explains what happens inside a check, from normalizing the password to choosing which match is reported, and why the engine stays fast on adversarial input.

### Grammar, not dictionary

A dictionary check asks whether the whole password is in a list. A pattern check asks whether the password can be *built* from a list, the way a dictionary attack builds candidates: words combined in common orders, with substitutions, repeats, separators, years and keyboard sequences around them. The space of passwords that can be built this way is enormous, far too large to enumerate, but recognizing whether a given password belongs to it is cheap when the grammar is run in reverse. That is what the matchers do.

## Normalization

Before anything is matched, the password is normalized into the <xref:Singulink.Cryptography.PasswordMatchContext.CheckedPassword> that every matcher sees:

1. Leading and trailing whitespace is trimmed.
2. The result is lower-cased using the invariant culture.
3. Runs of spaces are collapsed into a single space.

Matchers therefore never deal with case, and `Password  Password` is matched the same as `password password`. The original password is kept in <xref:Singulink.Cryptography.PasswordMatchContext.OriginalPassword> for reference, but matching, lengths and positions all refer to the normalized form.

The evaluator's <xref:Singulink.Cryptography.PasswordMatchEvaluator.MaxPasswordLength> is checked against the original password before normalization, so that the cost of normalizing is bounded too.

## Matchers and Contexts

A <xref:Singulink.Cryptography.PasswordMatchContext> is a position in the password plus the chain of items matched to reach it. A <xref:Singulink.Cryptography.PasswordMatcher> takes a context and returns the contexts that result from matching at that position: none if it does not match, one for a word that matches one way, several for a matcher that can consume different numbers of characters (a keyboard sequence that could be `qwe`, `qwer` or `qwert`, for instance).

Composite matchers build on this. A sequence feeds each result of one matcher into the next; an alternative concatenates the results of its options; an optional returns the unchanged context followed by its matcher's results; a repeat feeds each result back into the same matcher up to a maximum number of times; a permutation explores every order of its matchers. Together they describe the full set of ways a pattern can account for a prefix of the password.

#### Full consumption

A password matches a pattern only when some result reaches the end of the password. Partial coverage is not a match: `passwordk4efjawei` begins with the most common word there is, but nothing accounts for the rest, so it does not match. This is the rule that keeps the check conservative. It never rejects a password for containing a common word; it rejects a password for being *nothing but* common words.

## Ordering and the First Match

The patterns from the provider are tried in order, and within each pattern the matchers explore their alternatives depth-first in the order they produce results. The first result to reach the end of the password is the match that is reported. Ordering therefore decides the explanation the user sees, not whether the password is rejected: a password that can be explained two ways is rejected either way, and the first explanation found is the one returned.

Three ordering rules give the explanations their shape:

- **Patterns are tried in the provider's order.** The default provider lists simple patterns before elaborate ones, so `password123` is explained as a word with a suffix rather than as a word followed by a one-word sentence.
- **Value matchers try their longest match first.** Character sequences and runs of repeated characters are reported whole: `qwertyuiop` is one sequence, not `qwe` followed by `rtyuiop`. The one exception is the matcher for a few arbitrary characters, which tries its shortest match first so that it consumes as little as possible and structured matchers get the chance to account for the rest.
- **Optionals try the absent case first.** A sequence with optional parts prefers the explanation that uses fewer of them, so `thebadadmin` is explained with `the` as a determiner only because no explanation without it reaches the end.

## Value Matching Rules

The <xref:Singulink.Cryptography.PasswordMatchers.ValueMatcher> base class gives every text, digit, character sequence, repeated character, repeated group and arbitrary character matcher the same three behaviors on top of recognizing its value.

#### Substitutions

A <xref:Singulink.Cryptography.PasswordMatchers.TextMatcher> accepts common character substitutions in place of each letter of its word, so that `p@55w0rd` matches `password` and `n1nja` matches `ninja`:

| Letter | Substitutes | Letter | Substitutes |
|---|---|---|---|
| a | `4` `@` `Д` | o | `0` `Ø` |
| b | `8` `ß` | r | `®` `Я` |
| c | `(` `<` `{` `[` `¢` | s | `5` `$` |
| e | `3` `€` `£` | t | `7` `+` |
| f | `ƒ` | u | `µ` `บ` |
| g | `6` `9` | w | `พ` `₩` `ω` |
| i | `1` `!` `\|` | x | `×` |
| l | `1` `\|` `7` | y | `¥` |
| n | `И` `ท` | z | `2` `%` |

Substitution is per character: each letter of the word is either itself or one of its substitutes. Substitutions can be turned off per matcher, which the default lists and the default contextual subject provider do for values that are themselves digits, such as `69` and `420`, and for words of one or two letters, since a lone `1` or `@` is far more likely to be a decoration than the word `i` or `a`.

#### Repeats

After a value matches, the exact characters that matched may match again immediately, any number of times. The repeat is of the characters, not the word, so `p@sswordp@ssword` matches but `p@sswordpassword` does not: an attacker who doubles a word doubles it verbatim. A repeat matched this way is reported as a <xref:Singulink.Cryptography.PasswordMatchType.RepeatValue> item with the same text as the original. When the surrounding pattern allows the same word again as a separate match, that explanation is found first and the repeat is reported as an ordinary match instead; see [Match Results](../guides/match-results.md).

#### Separators

A single separator character may follow each occurrence of a value: a space, period, dash, underscore or comma (see <xref:Singulink.Cryptography.PasswordMatchers.ValueMatcher.Separators>). Separators are consumed but not reported, so `the.bad.admin`, `the-bad-admin` and `the bad admin` all report `the`, `bad`, `admin`. Repeats and separators combine, so `me.me me-me` is one word repeated four times.

A trailing separator on its own is also consumed. `password.` and `password_` match as just `password`, since a single trailing punctuation character is one of the first variations any attack tries.

## Memoization

A grammar run depth-first can do an exponential amount of work on input that *almost* matches in many ways, which is exactly the shape of a long password made of common tokens. Password Matcher avoids this with two rules that are applied inside <xref:Singulink.Cryptography.PasswordMatcher.GetMatches*>, so that every matcher gets them for free:

- **Results are cached per matcher and position** for the duration of a check. The default provider uses the same subject matcher in ten patterns and at several places within each; the second and later uses at the same position cost a dictionary lookup.
- **Only the first result per end position is kept.** Two different ways of reaching position 12 lead to identical futures, since what follows depends only on the remaining characters. Keeping the first preserves the reported explanation exactly while collapsing the alternatives.

Sequences and repeats apply the same deduplication after each step, and permutations track which combinations of used matchers and positions have been explored. The work for a check is then bounded by the number of matchers times the square of the password length, rather than growing with the number of ways the password can be split. In practice:

| Password | Time |
|---|---|
| Typical passwords, matched or not | Under 0.5 ms |
| 128 characters of `123123…` followed by one character that cannot match | About 20 ms |
| 128 characters of `1111…` followed by one character that cannot match | About 90 ms |

The length limit on the evaluator caps the quadratic term. Together they make the cost of a check predictable for any input, which matters when the check runs on a sign-up endpoint that accepts input from anyone.

> [!NOTE]
> Caching is also why a matcher's results must depend only on the remaining characters, never on what was matched before it. See [Writing Custom Matchers](../guides/custom-matchers.md).

## Further Reading

These articles cover the related topics in more depth:

- [The Default Patterns](default-patterns.md) - The grammar the default provider runs.
- [Writing Custom Matchers](../guides/custom-matchers.md) - The contract a matcher implements.
- [Threat Model](threat-model.md) - What the full consumption rule means for false positives and negatives.

</div>
