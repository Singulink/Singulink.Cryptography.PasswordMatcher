<div class="article">

# Customizing the Patterns

The default patterns cover the most common password structures, but every application has words of its own worth blocking, and some have structures of their own. This guide covers the building blocks that patterns are made of, the predefined word lists and decorations, and how to extend or replace the default patterns.

### Two layers

A check is driven by an <xref:Singulink.Cryptography.IPasswordMatchersProvider>, which returns the top-level patterns to try for a password. <xref:Singulink.Cryptography.PasswordMatchersProvider.Default> builds its patterns from the word lists and decorations in <xref:Singulink.Cryptography.PasswordMatchers.CommonMatchers> and the contextual subjects of the current check. Both layers are made of <xref:Singulink.Cryptography.PasswordMatcher> objects created with the factory methods described first, so everything below composes the same way.

## Building Blocks

The static factory methods on <xref:Singulink.Cryptography.PasswordMatcher> create every kind of matcher. Matchers are immutable and safe to build once and share, including from static fields.

#### Text

<xref:Singulink.Cryptography.PasswordMatcher.Text*> matches a word or phrase. Matching ignores case and, by default, accepts common character substitutions (`4` or `@` for `a`, `0` for `o`, `5` or `$` for `s` and so on), consecutive repeats of the word, and a single separator character after each occurrence:

```csharp
var matcher = PasswordMatcher.Text("hunter");

// Matches "hunter", "HUNTER", "hun73r", "hunterhunter", "hunter.hunter", "hunter-"
```

Pass a <xref:Singulink.Cryptography.PasswordMatchType> to control how the match is reported, and turn off substitutions, repeats or the trailing separator when they do not make sense for the word. Substitutions should be turned off for values that are themselves digits or symbols, since `69` should not also match `g9`, and for words of one or two letters, since a lone `1` or `@` is far more likely to be a decoration than the word `i` or `a`:

```csharp
var matcher = PasswordMatcher.Text("69", checkSubstitutions: false);
```

#### Sequence

<xref:Singulink.Cryptography.PasswordMatcher.Sequence*> matches its matchers one after another, in order. This is how phrases and sentence-like patterns are built:

```csharp
var matcher = PasswordMatcher.Sequence([
    PasswordMatcher.Text("let"),
    PasswordMatcher.Text("me"),
    PasswordMatcher.Text("in"),
]);

// Matches "letmein", "let me in", "l3tm31n"
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
```

#### Optional

<xref:Singulink.Cryptography.PasswordMatcher.Optional*> matches its matcher or nothing at all. Use it inside a sequence to make a part of a pattern optional. The absent case is tried first, so a sequence of `Text("pass")`, `Optional(Text("word"))` reports `password` as the two words `pass` and `word` only when the shorter match does not lead to a complete match of the whole password:

```csharp
var matcher = PasswordMatcher.Sequence([
    PasswordMatcher.Optional(PasswordMatcher.Text("the")),
    seasons,
    PasswordMatcher.Optional(CommonMatchers.YearMatcher),
]);

// Matches "summer", "thesummer", "summer2024", "the.winter.1999"
```

#### Repeat

<xref:Singulink.Cryptography.PasswordMatcher.Repeat*> matches its matcher a number of times in a row, within a range. Each occurrence is matched independently, so repeating an alternative lets a different option match each time, which is how "up to four words from this list, in any order" is expressed:

```csharp
var matcher = PasswordMatcher.Repeat(seasons, 1, 4);

// Matches "summer", "summerwinter", "fall.spring.summer"; not "summerfallwinterspringsummer" (five occurrences)
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

// Matches "red", "bluered", "greenbluered"; not "redred" or "redbluered"
```

Each matcher must be a distinct instance, since the permutation tracks which ones have been used by identity. Use it when "each at most once" is the actual rule, as the default contextual subject provider does for the words of a name; use <xref:Singulink.Cryptography.PasswordMatcher.Repeat*> over an <xref:Singulink.Cryptography.PasswordMatcher.Any*> when parts may recur.

#### CharSequence

<xref:Singulink.Cryptography.PasswordMatcher.CharSequence*> matches runs of at least three characters that are adjacent on a keyboard or keypad or consecutive in the alphabet, in either direction and, for keyboard sequences, with or without the shift key held. Combine the <xref:Singulink.Cryptography.PasswordMatchers.CharSequenceTypes> flags to choose which:

| Type | Examples |
|---|---|
| <xref:Singulink.Cryptography.PasswordMatchers.CharSequenceTypes.KeyboardRow> | `qwerty`, `asdfgh`, `12345`, `ytrewq`, `!@#$` |
| <xref:Singulink.Cryptography.PasswordMatchers.CharSequenceTypes.KeyboardDiagonal> | `1qaz`, `2wsx`, `zaq1`, `4esz` |
| <xref:Singulink.Cryptography.PasswordMatchers.CharSequenceTypes.KeyboardZigzag> | `1q2w3e4r`, `q1w2e3r4` |
| <xref:Singulink.Cryptography.PasswordMatchers.CharSequenceTypes.NumericKeypad> | `789`, `741`, `7410`, `2580`, `159`, `753` |
| <xref:Singulink.Cryptography.PasswordMatchers.CharSequenceTypes.Alphabet> | `abc`, `abcdef`, `zyx` |

```csharp
var matcher = PasswordMatcher.CharSequence(CharSequenceTypes.KeyboardRow | CharSequenceTypes.KeyboardDiagonal);
```

The longest sequence is tried first, so `qwertyuiop` is reported as one sequence rather than several short ones. <xref:Singulink.Cryptography.PasswordMatchers.CharSequenceTypes.All> selects every type.

#### FixedDigits

<xref:Singulink.Cryptography.PasswordMatcher.FixedDigits*> matches an exact number of digits, optionally restricted to a range of values. <xref:Singulink.Cryptography.PasswordMatchers.CommonMatchers.YearMatcher> is a four-digit matcher restricted to 1900 through ten years from now. A two-digit matcher for day or month numbers would be:

```csharp
var dayOrMonth = PasswordMatcher.FixedDigits(2, minValue: 1, maxValue: 31, PasswordMatchType.Common);
```

#### RepeatedChars

<xref:Singulink.Cryptography.PasswordMatcher.RepeatedChars*> matches a run of the same character, optionally limited to a range of lengths and to characters that pass a filter:

```csharp
var symbols = PasswordMatcher.RepeatedChars(1, 8, c => c is '!' or '$' or '?' or '#' or '*');

// Matches "!", "!!!", "####"; not "!?" (different characters) or "!!!!!!!!!" (too long)
```

#### RepeatedGroups

<xref:Singulink.Cryptography.PasswordMatcher.RepeatedGroups*> matches a short group of characters repeated two or more times, which neither a run nor a sequence can describe. Groups consisting of a single repeated character are left to <xref:Singulink.Cryptography.PasswordMatcher.RepeatedChars*>:

```csharp
var groups = PasswordMatcher.RepeatedGroups(2, 3, 2, null);

// Matches "121212", "abcabc", "1!1!"; not "1213" (no repeat) or "1111" (a run, not a group)
```

#### AnyChars

<xref:Singulink.Cryptography.PasswordMatcher.AnyChars*> matches a short run of arbitrary characters, optionally limited to characters that pass a filter. It is the one value matcher that tries its shortest match first, so that it consumes as few arbitrary characters as possible and leaves the rest to structured matchers:

```csharp
var digits = PasswordMatcher.AnyChars(1, 4, char.IsDigit);

// Matches "4", "47", "4792"; not "47921" (too long) or "47a"
```

Use it sparingly. A few arbitrary characters are a decoration an attacker's rules generate; more than a few are entropy, and matching them would reject strong passwords.

### Composition rules

A few constraints keep patterns well-formed, and the factory methods throw <xref:System.ArgumentException> when they are violated:

- <xref:Singulink.Cryptography.PasswordMatcher.Any*>, <xref:Singulink.Cryptography.PasswordMatcher.Repeat*> and <xref:Singulink.Cryptography.PasswordMatcher.Permutations*> cannot directly contain optional matchers. Wrap the optional part in a sequence instead.
- <xref:Singulink.Cryptography.PasswordMatcher.Sequence*> needs at least one matcher that is not optional.
- <xref:Singulink.Cryptography.PasswordMatcher.Permutations*> needs distinct matcher instances and supports up to <xref:Singulink.Cryptography.PasswordMatchers.PermutationsMatcher.MaxMatchers> of them.

Order matters in two ways. The matchers returned by a provider are tried in order and the first complete match is reported, so put the patterns whose explanation is most useful first. Within a pattern, a value matcher tries its longest match first (except <xref:Singulink.Cryptography.PasswordMatcher.AnyChars*>, which tries its shortest), so a keyboard sequence or a run of repeated characters is reported whole rather than split.

## The Common Word Lists and Decorations

<xref:Singulink.Cryptography.PasswordMatchers.CommonMatchers> exposes the word lists that the default patterns are built from, grouped by the role a word plays in a sentence, along with the decoration matchers. Each is a matcher you can use in your own patterns:

| Matcher | Role | Examples |
|---|---|---|
| <xref:Singulink.Cryptography.PasswordMatchers.CommonMatchers.GeneralSubjectMatcher> | The nouns and pronouns that passwords are made of | `password`, `admin`, `i`, `you`, `dragon`, `ninja`, `letmein` |
| <xref:Singulink.Cryptography.PasswordMatchers.CommonMatchers.AdjectiveMatcher> | Adjectives | `best`, `cool`, `bad`, `sexy` |
| <xref:Singulink.Cryptography.PasswordMatchers.CommonMatchers.VerbMatcher> | Verbs | `love`, `hate`, `want`, `login` |
| <xref:Singulink.Cryptography.PasswordMatchers.CommonMatchers.SubjectDeterminerMatcher> | Words before a subject | `the`, `my`, `your`, `this` |
| <xref:Singulink.Cryptography.PasswordMatchers.CommonMatchers.SubjectConjunctionMatcher> | Words joining two subjects | `and`, `or`, `to`, `in`, `into` |
| <xref:Singulink.Cryptography.PasswordMatchers.CommonMatchers.SubjectToSubjectCopularVerbMatcher> | "is", "am" and "are" between subjects | `i am admin` |
| <xref:Singulink.Cryptography.PasswordMatchers.CommonMatchers.SubjectToAdjectiveCopularVerbMatcher> | "is", "am" and "are" before an adjective, with optional "so" and "the", "a" or "an" | `is so the best` |
| <xref:Singulink.Cryptography.PasswordMatchers.CommonMatchers.AdjectiveToSubjectConjunctionMatcher> | Words joining an adjective to a subject | `cool as`, `cool like` |
| <xref:Singulink.Cryptography.PasswordMatchers.CommonMatchers.VerbSuffixMatcher> | "to" after a verb | `want to`, `login to` |
| <xref:Singulink.Cryptography.PasswordMatchers.CommonMatchers.AllWordsMatcher> | Any word from any of the lists above | |
| <xref:Singulink.Cryptography.PasswordMatchers.CommonMatchers.YearMatcher> | Four-digit years from 1900 to ten years from now | `1975`, `2024` |
| <xref:Singulink.Cryptography.PasswordMatchers.CommonMatchers.AllCharSequencesMatcher> | Every type of character sequence | `qwerty`, `1qaz`, `1q2w3e`, `7410`, `abc` |
| <xref:Singulink.Cryptography.PasswordMatchers.CommonMatchers.DigitRunMatcher> | Two or more of the same digit | `11`, `000000` |
| <xref:Singulink.Cryptography.PasswordMatchers.CommonMatchers.SymbolRunMatcher> | Two or more of the same symbol | `!!`, `####` |
| <xref:Singulink.Cryptography.PasswordMatchers.CommonMatchers.DigitOrSymbolGroupMatcher> | A repeated group of two or three digits or symbols | `121212`, `1!1!` |
| <xref:Singulink.Cryptography.PasswordMatchers.CommonMatchers.StructuredDecorationMatcher> | Any one of the five decorations above | |
| <xref:Singulink.Cryptography.PasswordMatchers.CommonMatchers.DecorationRunMatcher> | One to six structured decorations in any order | `2024!!`, `1234qwerty`, `123123` |
| <xref:Singulink.Cryptography.PasswordMatchers.CommonMatchers.ShortDecorationMatcher> | One to four arbitrary digits or symbols other than separators | `47`, `#1`, `4792` |
| <xref:Singulink.Cryptography.PasswordMatchers.CommonMatchers.JoinerMatcher> | A single digit or symbol joining words | the `!` in `ninja!admin` |
| <xref:Singulink.Cryptography.PasswordMatchers.CommonMatchers.DecorationMatcher> | What commonly precedes or follows the words: a decoration run with at most one short decoration | `1`, `#1`, `2024!!`, `@123`, `!!!2024` |

See [The Default Patterns](../concepts/default-patterns.md) for the complete lists and the patterns they are combined in.

## Extending the Default Provider

Derive from <xref:Singulink.Cryptography.PasswordMatchersProvider> and override <xref:Singulink.Cryptography.PasswordMatchersProvider.GetMatchers*> to add patterns before or after the defaults. The base implementation returns the default patterns for the given subjects, so adding to them is a matter of yielding your own alongside:

```csharp
public class MyMatchersProvider : PasswordMatchersProvider
{
    private static readonly PasswordMatcher ProductNames = PasswordMatcher.Any([
        PasswordMatcher.Text("widgetpro"),
        PasswordMatcher.Text("widgetlite"),
        PasswordMatcher.Sequence([PasswordMatcher.Text("widget"), PasswordMatcher.Text("pro")]),
        PasswordMatcher.Sequence([PasswordMatcher.Text("widget"), PasswordMatcher.Text("lite")]),
    ]);

    // Pattern: [decoration] product [product] [decoration]
    private static readonly PasswordMatcher ProductPattern = PasswordMatcher.Sequence([
        PasswordMatcher.Optional(CommonMatchers.DecorationMatcher),
        PasswordMatcher.Repeat(ProductNames, 1, 2),
        PasswordMatcher.Optional(CommonMatchers.DecorationMatcher),
    ]);

    public MyMatchersProvider() : base(ContextualSubjectMatcherProvider.Default) { }

    public override IEnumerable<PasswordMatcher> GetMatchers(IEnumerable<ContextualSubject>? subjects)
    {
        yield return ProductPattern;

        foreach (var matcher in base.GetMatchers(subjects))
            yield return matcher;
    }
}
```

Then build an evaluator around it, and use that evaluator everywhere instead of <xref:Singulink.Cryptography.PasswordMatchEvaluator.Default>:

```csharp
var evaluator = new PasswordMatchEvaluator(new MyMatchersProvider());
```

#### Adding words to the default patterns

Words that should take part in the sentence-like default patterns, rather than only in a pattern of their own, are best added as contextual subjects, since the default provider treats subjects and common words identically. Register them once as the provider's application subjects, with <xref:Singulink.Cryptography.ContextualSubjectType.General> as the type for plain words:

```csharp
var provider = new PasswordMatchersProvider(ContextualSubjectMatcherProvider.Default, [
    new("Widget Pro"),
    new("Widget Lite"),
    new("Acme Widgets", ContextualSubjectType.Company),
]);

var evaluator = new PasswordMatchEvaluator(provider);
```

This makes `iamthewidgetproadmin` match the same way `iamthebusinessadmin` does for a company subject containing "business", for every user, without changing any pattern. Deriving from the provider, as above, is only needed when the *structure* of the patterns should change.

## Replacing the Provider

Implement <xref:Singulink.Cryptography.IPasswordMatchersProvider> directly when the default patterns are not what you want at all, for example for a non-English user base. The single <xref:Singulink.Cryptography.IPasswordMatchersProvider.GetMatchers*> method returns the patterns to try for a password, in order, given the contextual subjects for the check. You can still use the <xref:Singulink.Cryptography.PasswordMatchers.CommonMatchers> word lists and decorations and <xref:Singulink.Cryptography.ContextualSubjectMatcherProvider.Default> to turn subjects into matchers:

```csharp
public class SimpleMatchersProvider : IPasswordMatchersProvider
{
    public IEnumerable<PasswordMatcher> GetMatchers(IEnumerable<ContextualSubject>? subjects)
    {
        var subjectMatchers = subjects?
            .Select(ContextualSubjectMatcherProvider.Default.GetMatcher)
            .OfType<PasswordMatcher>()
            .ToList() ?? [];

        var word = PasswordMatcher.Any([..subjectMatchers, CommonMatchers.GeneralSubjectMatcher]);

        // Pattern: [decoration] word [word] [decoration]
        yield return PasswordMatcher.Sequence([
            PasswordMatcher.Optional(CommonMatchers.DecorationMatcher),
            PasswordMatcher.Repeat(word, 1, 2),
            PasswordMatcher.Optional(CommonMatchers.DecorationMatcher),
        ]);
    }
}
```

> [!NOTE]
> Matchers that depend on the contextual subjects have to be built per check, as above, because the subjects differ for every user. Matchers that do not depend on them should be built once and stored in static fields, as the default provider does with the common word lists and decorations.

## Next Steps

Continue with these related articles:

- [Writing Custom Matchers](custom-matchers.md) - When the building blocks are not enough, write a matcher of your own.
- [The Default Patterns](../concepts/default-patterns.md) - The complete word lists, the decorations and the default patterns.
- [How Matching Works](../concepts/how-matching-works.md) - Why ordering works the way it does.

</div>
