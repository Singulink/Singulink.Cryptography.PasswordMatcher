<div class="article">

# Matcher Building Blocks

The static factory methods on <xref:Singulink.Cryptography.PasswordMatcher> create every kind of matcher, and the default patterns are built from nothing else. This page is the reference for composing patterns of your own: what each method matches, the rules for combining them, and the predefined word lists you can reuse.

### Before you start

Most applications never need this page. Adding your own words takes no building blocks at all: see [Adding Words and Patterns](customizing-patterns.md). Matchers are immutable and safe to build once and share, including from static fields.

## Factory Methods at a Glance

| Method | Matches | Example |
|---|---|---|
| <xref:Singulink.Cryptography.PasswordMatcher.Text*> | A word or phrase, with substitutions, repeats and a trailing separator | `hun73r` for `Text("hunter")` |
| <xref:Singulink.Cryptography.PasswordMatcher.Sequence*> | Its matchers one after another, in order | `letmein` |
| <xref:Singulink.Cryptography.PasswordMatcher.Any*> | Whichever one of its matchers matches | `summer` from a list of seasons |
| <xref:Singulink.Cryptography.PasswordMatcher.Optional*> | Its matcher, or nothing | `summer` and `thesummer` |
| <xref:Singulink.Cryptography.PasswordMatcher.Repeat*> | Its matcher a number of times in a row | `summerwinter` |
| <xref:Singulink.Cryptography.PasswordMatcher.Permutations*> | Its matchers in any order, each at most once | `bluered` |
| <xref:Singulink.Cryptography.PasswordMatcher.CharSequence*> | Adjacent keys on a keyboard or keypad, or consecutive letters | `qwerty`, `1qaz`, `abc` |
| <xref:Singulink.Cryptography.PasswordMatcher.FixedDigits*> | An exact number of digits, optionally within a range | `2024` |
| <xref:Singulink.Cryptography.PasswordMatcher.RepeatedChars*> | A run of the same character | `!!!` |
| <xref:Singulink.Cryptography.PasswordMatcher.RepeatedGroups*> | A short group of characters repeated | `121212` |
| <xref:Singulink.Cryptography.PasswordMatcher.AnyChars*> | A short run of arbitrary characters | `4792` |

To try any of them, run a password through <xref:Singulink.Cryptography.PasswordMatcher.GetFirstMatch*>, which returns a match only when the matchers consume the whole password (see [Testing a Matcher](custom-matchers.md#testing-a-matcher)).

## Combining Matchers

These methods combine other matchers into a pattern.

#### Sequence

<xref:Singulink.Cryptography.PasswordMatcher.Sequence*> matches its matchers one after another, in order. This is how phrases and sentence-like patterns are built:

```csharp
var matcher = PasswordMatcher.Sequence([
    PasswordMatcher.Text("let"),
    PasswordMatcher.Text("me"),
    PasswordMatcher.Text("in"),
]);

// "letmein"     matches
// "let me in"   matches: a separator may follow each word
// "l3tm31n"     matches: substitutions apply to each word
// "letme"       no match: "in" is missing
```

#### Any

<xref:Singulink.Cryptography.PasswordMatcher.Any*> matches whichever of its matchers matches, trying them in order. It is how a word list becomes a single matcher:

```csharp
var seasons = PasswordMatcher.Any([
    PasswordMatcher.Text("spring"),
    PasswordMatcher.Text("summer"),
    PasswordMatcher.Text("autumn"),
    PasswordMatcher.Text("fall"),
    PasswordMatcher.Text("winter"),
]);

// "summer"         matches
// "summerwinter"   no match: Any matches one of them once (use Repeat for more)
```

#### Optional

<xref:Singulink.Cryptography.PasswordMatcher.Optional*> matches its matcher or nothing at all. Use it inside a sequence to make a part of a pattern optional:

```csharp
var matcher = PasswordMatcher.Sequence([
    PasswordMatcher.Optional(PasswordMatcher.Text("the")),
    seasons,
    PasswordMatcher.Optional(CommonMatchers.YearMatcher),
]);

// "summer"            matches
// "thesummer"         matches
// "summer2024"        matches
// "the.winter.1999"   matches
```

The absent case is tried first. The optional part is only used when leaving it out does not lead to a complete match of the whole password:

```csharp
var matcher = PasswordMatcher.Sequence([
    PasswordMatcher.Text("pass"),
    PasswordMatcher.Optional(PasswordMatcher.Text("word")),
]);

// "pass"       reports "pass": leaving out "word" consumes the whole password
// "password"   reports "pass", "word": leaving out "word" would leave "word" unmatched
```

#### Repeat

<xref:Singulink.Cryptography.PasswordMatcher.Repeat*> matches its matcher a number of times in a row, within a range. Each occurrence is matched independently, so repeating an alternative lets a different option match each time. That is how "up to four words from this list, in any order" is expressed:

```csharp
var matcher = PasswordMatcher.Repeat(seasons, 1, 4);

// "summer"                         matches
// "summerwinter"                   matches
// "fall.spring.summer"             matches
// "summerfallwinterspringsummer"   no match: five occurrences
```

Fewer occurrences are tried before more, so the explanation with the fewest parts is preferred.

#### Permutations

<xref:Singulink.Cryptography.PasswordMatcher.Permutations*> matches its matchers in any order, using each at most once. By default any non-empty subset of them matches; pass `mustMatchAllMatchers: true` to require every one:

```csharp
var matcher = PasswordMatcher.Permutations([
    PasswordMatcher.Text("red", matchRepeats: false),
    PasswordMatcher.Text("green", matchRepeats: false),
    PasswordMatcher.Text("blue", matchRepeats: false),
]);

// "red"            matches
// "bluered"        matches
// "greenbluered"   matches
// "redred"         no match: "red" is used twice
// "redbluered"     no match: "red" is used twice
```

The permutation tracks which matchers have been used by identity, so each one must be a distinct instance. Use it when "each at most once" is the actual rule, as the default contextual subject provider does for the words of a name. Use <xref:Singulink.Cryptography.PasswordMatcher.Repeat*> over an <xref:Singulink.Cryptography.PasswordMatcher.Any*> when parts may recur.

## Matching Values

These methods consume characters from the password. Each of them also matches a single separator character after what it matched, unless you turn that off. <xref:Singulink.Cryptography.PasswordMatcher.Text*>, <xref:Singulink.Cryptography.PasswordMatcher.FixedDigits*> and <xref:Singulink.Cryptography.PasswordMatcher.CharSequence*> also match repeats of the exact characters they matched. See [How Matching Works](../concepts/how-matching-works.md#value-matching-rules) for both.

#### Text

<xref:Singulink.Cryptography.PasswordMatcher.Text*> matches a word or phrase. Matching ignores case and, by default, accepts common character substitutions (`4` or `@` for `a`, `0` for `o`, `5` or `$` for `s` and so on), consecutive repeats of the word, and a single separator character after each occurrence:

```csharp
var matcher = PasswordMatcher.Text("hunter");

// "hunter"          matches
// "HUNTER"          matches: case is ignored
// "hun73r"          matches: substitutions
// "hunterhunter"    matches: a repeat
// "hunter.hunter"   matches: a separator, then a repeat
// "hunter-"         matches: a trailing separator
// "hunter2"         no match: nothing accounts for the "2"
```

Pass a <xref:Singulink.Cryptography.PasswordMatchType> to control how the match is reported, and turn off substitutions, repeats or the trailing separator when they do not make sense for the word. Turn substitutions off for words of one or two letters, since a lone `1` or `@` is far more likely to be a decoration than the word `i` or `a`:

```csharp
var matcher = PasswordMatcher.Text("i", checkSubstitutions: false);

// "i"   matches
// "1"   no match: substitutions are off
```

Substitutions replace the letters of the word only, so they never change how digits and symbols match: `Text("69")` does not match `g9`, whether substitutions are on or off.

#### CharSequence

<xref:Singulink.Cryptography.PasswordMatcher.CharSequence*> matches runs of at least three characters that are adjacent on a keyboard or keypad or consecutive in the alphabet. They may run in either direction and, for keyboard sequences, with or without the shift key held. Combine the <xref:Singulink.Cryptography.PasswordMatchers.CharSequenceTypes> flags to choose which:

| Type | Examples |
|---|---|
| <xref:Singulink.Cryptography.PasswordMatchers.CharSequenceTypes.KeyboardRow> | `qwerty`, `asdfgh`, `12345`, `ytrewq`, `!@#$` |
| <xref:Singulink.Cryptography.PasswordMatchers.CharSequenceTypes.KeyboardDiagonal> | `1qaz`, `2wsx`, `zaq1`, `4esz` |
| <xref:Singulink.Cryptography.PasswordMatchers.CharSequenceTypes.KeyboardZigzag> | `1q2w3e4r`, `q1w2e3r4` |
| <xref:Singulink.Cryptography.PasswordMatchers.CharSequenceTypes.NumericKeypad> | `789`, `741`, `7410`, `2580`, `159`, `753` |
| <xref:Singulink.Cryptography.PasswordMatchers.CharSequenceTypes.Alphabet> | `abc`, `abcdef`, `zyx` |

```csharp
var matcher = PasswordMatcher.CharSequence(CharSequenceTypes.KeyboardRow | CharSequenceTypes.KeyboardDiagonal);

// "qwerty"   matches: a keyboard row
// "1qaz"     matches: a keyboard diagonal
// "abc"      no match: alphabet sequences are not selected
```

The longest sequence is tried first, so `qwertyuiop` is reported as one sequence rather than several short ones. <xref:Singulink.Cryptography.PasswordMatchers.CharSequenceTypes.All> selects every type.

#### FixedDigits

<xref:Singulink.Cryptography.PasswordMatcher.FixedDigits*> matches an exact number of digits, optionally restricted to a range of values. <xref:Singulink.Cryptography.PasswordMatchers.CommonMatchers.YearMatcher> is a four-digit matcher restricted to 1900 through ten years from now. A two-digit matcher for day or month numbers would be:

```csharp
var dayOrMonth = PasswordMatcher.FixedDigits(2, minValue: 1, maxValue: 31, PasswordMatchType.Common);

// "01"   matches
// "31"   matches
// "32"   no match: out of range
// "1"    no match: two digits are needed
```

#### RepeatedChars

<xref:Singulink.Cryptography.PasswordMatcher.RepeatedChars*> matches a run of the same character, optionally limited to a range of lengths and to characters that pass a filter:

```csharp
var symbols = PasswordMatcher.RepeatedChars(1, 8, c => c is '!' or '$' or '?' or '#' or '*');

// "!"           matches
// "!!!"         matches
// "####"        matches
// "!?"          no match: different characters
// "!!!!!!!!!"   no match: nine characters, more than eight
```

#### RepeatedGroups

<xref:Singulink.Cryptography.PasswordMatcher.RepeatedGroups*> matches a short group of characters repeated two or more times, which neither a run nor a sequence can describe. Groups consisting of a single repeated character are left to <xref:Singulink.Cryptography.PasswordMatcher.RepeatedChars*>:

```csharp
var groups = PasswordMatcher.RepeatedGroups(2, 3, 2, null);

// "121212"   matches
// "abcabc"   matches
// "1!1!"     matches
// "1213"     no match: no repeat
// "1111"     no match: a run, not a group
```

#### AnyChars

<xref:Singulink.Cryptography.PasswordMatcher.AnyChars*> matches a short run of arbitrary characters, optionally limited to characters that pass a filter. It is the one value matcher that tries its shortest match first, so that it consumes as few arbitrary characters as possible and leaves the rest to structured matchers:

```csharp
var digits = PasswordMatcher.AnyChars(1, 4, char.IsDigit);

// "4"       matches
// "47"      matches
// "4792"    matches
// "47921"   no match: five characters, more than four
// "47a"     no match: "a" is not a digit
```

> [!CAUTION]
> Use it sparingly. A few arbitrary characters are a decoration that an attacker's rules generate. More than a few are entropy, and matching them would reject strong passwords.

## Composition Rules

A few constraints keep patterns well-formed. The factory methods throw <xref:System.ArgumentException> when one is broken, so a mistake shows up when the pattern is built, not when a password is checked:

| Rule | Notes |
|---|---|
| <xref:Singulink.Cryptography.PasswordMatcher.Any*>, <xref:Singulink.Cryptography.PasswordMatcher.Repeat*> and <xref:Singulink.Cryptography.PasswordMatcher.Permutations*> cannot directly contain optional matchers. | Wrap the optional part in a sequence instead. |
| <xref:Singulink.Cryptography.PasswordMatcher.Sequence*> needs at least one matcher that is not optional. | |
| <xref:Singulink.Cryptography.PasswordMatcher.Permutations*> needs distinct matcher instances. | It tracks which matchers have been used by identity. |
| <xref:Singulink.Cryptography.PasswordMatcher.Permutations*> supports up to <xref:Singulink.Cryptography.PasswordMatchers.PermutationsMatcher.MaxMatchers> matchers. | |

#### Order

Order matters in two ways:

- **The matchers returned by a provider are tried in order,** and the first complete match is reported. Put the patterns whose explanation is most useful first.
- **Within a pattern, a value matcher tries its longest match first,** except <xref:Singulink.Cryptography.PasswordMatcher.AnyChars*>, which tries its shortest. This is why a keyboard sequence or a run of repeated characters is reported whole rather than split.

## Common Word Lists and Decorations

<xref:Singulink.Cryptography.PasswordMatchers.CommonMatchers> exposes the word lists that the default patterns are built from, grouped by the role a word plays in a sentence, along with the decoration matchers. Each is a matcher you can use in your own patterns. [Default Patterns](../concepts/default-patterns.md) lists the words in each and what each decoration matches.

| Matcher | Role |
|---|---|
| <xref:Singulink.Cryptography.PasswordMatchers.CommonMatchers.GeneralSubjectMatcher> | The nouns and pronouns that passwords are made of |
| <xref:Singulink.Cryptography.PasswordMatchers.CommonMatchers.AdjectiveMatcher> | Adjectives |
| <xref:Singulink.Cryptography.PasswordMatchers.CommonMatchers.VerbMatcher> | Verbs |
| <xref:Singulink.Cryptography.PasswordMatchers.CommonMatchers.SubjectDeterminerMatcher> | Words before a subject |
| <xref:Singulink.Cryptography.PasswordMatchers.CommonMatchers.SubjectConjunctionMatcher> | Words joining two subjects |
| <xref:Singulink.Cryptography.PasswordMatchers.CommonMatchers.SubjectToSubjectCopularVerbMatcher> | "is", "am" and "are" between subjects |
| <xref:Singulink.Cryptography.PasswordMatchers.CommonMatchers.SubjectToAdjectiveCopularVerbMatcher> | "is", "am" and "are" before an adjective |
| <xref:Singulink.Cryptography.PasswordMatchers.CommonMatchers.AdjectiveToSubjectConjunctionMatcher> | Words joining an adjective to a subject |
| <xref:Singulink.Cryptography.PasswordMatchers.CommonMatchers.VerbSuffixMatcher> | "to" after a verb |
| <xref:Singulink.Cryptography.PasswordMatchers.CommonMatchers.AllWordsMatcher> | Any word from any of the lists above |
| <xref:Singulink.Cryptography.PasswordMatchers.CommonMatchers.YearMatcher> | Four-digit years |
| <xref:Singulink.Cryptography.PasswordMatchers.CommonMatchers.AllCharSequencesMatcher> | Every type of character sequence |
| <xref:Singulink.Cryptography.PasswordMatchers.CommonMatchers.DigitRunMatcher> | Two or more of the same digit |
| <xref:Singulink.Cryptography.PasswordMatchers.CommonMatchers.SymbolRunMatcher> | Two or more of the same symbol |
| <xref:Singulink.Cryptography.PasswordMatchers.CommonMatchers.DigitOrSymbolGroupMatcher> | A repeated group of two or three digits or symbols |
| <xref:Singulink.Cryptography.PasswordMatchers.CommonMatchers.StructuredDecorationMatcher> | Any one of the five decorations above |
| <xref:Singulink.Cryptography.PasswordMatchers.CommonMatchers.DecorationRunMatcher> | One to six structured decorations in any order |
| <xref:Singulink.Cryptography.PasswordMatchers.CommonMatchers.ShortDecorationMatcher> | One to four arbitrary digits or symbols other than separators |
| <xref:Singulink.Cryptography.PasswordMatchers.CommonMatchers.JoinerMatcher> | A single digit or symbol joining words, such as the `!` in `ninja!admin` |
| <xref:Singulink.Cryptography.PasswordMatchers.CommonMatchers.DecorationMatcher> | What commonly precedes or follows the words: a decoration run with at most one short decoration |

## Next Steps

Continue with these related articles:

- [Adding Words and Patterns](customizing-patterns.md) - Putting a pattern built from these blocks into the check.
- [Writing Custom Matchers](custom-matchers.md) - When the building blocks are not enough.
- [Default Patterns](../concepts/default-patterns.md) - The words in each list and the patterns they are combined in.

</div>
