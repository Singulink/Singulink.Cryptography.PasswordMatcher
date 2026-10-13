<div class="article">

# Match Results

A successful check returns a <xref:Singulink.Cryptography.PasswordMatch> that describes exactly what the password was built from. This guide covers what the match contains, what each match type means, and how to turn a match into a message the user can act on.

## The Match Object

<xref:Singulink.Cryptography.PasswordMatchEvaluator.MatchPassword*> returns `null` when the password does not match, so the match object itself only ever describes a rejection. It has two views of the same information:

- <xref:Singulink.Cryptography.PasswordMatch.Values> is the list of matched values as strings, in the order they appear in the password. This is what you show the user.
- <xref:Singulink.Cryptography.PasswordMatch.Items> is the list of <xref:Singulink.Cryptography.PasswordMatchItem> objects, each pairing a value with its <xref:Singulink.Cryptography.PasswordMatchItem.MatchType>. Use it when you want to word the explanation differently for different kinds of matches, or to log what kinds of weak passwords your users attempt.

```csharp
var match = PasswordMatchEvaluator.Default.MatchPassword("QWERTp@ssword1975", null);

match.Values;                 // ["QWERT", "password", "1975"]
match.Items[0].MatchType;     // CharSequence
match.Items[1].MatchType;     // Common
match.ToString();             // "\"QWERT\" (CharSequence) \"password\" (Common) \"1975\" (Common)"
```

#### Values are the dictionary form

<xref:Singulink.Cryptography.PasswordMatchItem.MatchedText> holds the value in the form the user will recognize, not the characters that were actually in the password. A password of `p@55w0rd` reports `password`, because that is the word the attacker's dictionary contains and the word the user needs to get rid of. Character sequences are reported in upper case (`QWERTY`, `ABC`) to stand out as sequences rather than words. The characters that were actually matched are available as <xref:Singulink.Cryptography.PasswordMatchItem.MatchedChars> (`p@55w0rd`, `qwerty`), taken from the normalized password, for highlighting the matched part of what the user typed or for showing both forms side by side. Separators between values (`password.123` or `the-bad-admin`) are consumed by the match but are not reported, since they are not part of what made the password weak.

## Match Types

<xref:Singulink.Cryptography.PasswordMatchType> says what kind of value each item is:

| Type | Meaning | Example item |
|---|---|---|
| <xref:Singulink.Cryptography.PasswordMatchType.Common> | A word, phrase or value from the common password lists. | `password`, `love`, `2024` |
| <xref:Singulink.Cryptography.PasswordMatchType.ContextualSubject> | A word from one of the contextual subjects passed to the check. | `jane`, `acmewidgets` |
| <xref:Singulink.Cryptography.PasswordMatchType.CharSequence> | A run of adjacent keys on a keyboard row, diagonal or zigzag, a line of a numeric keypad, or consecutive letters of the alphabet, in either direction, shifted or not. | `QWERTY`, `1QAZ`, `!@#$`, `1Q2W3E`, `7410`, `ABC` |
| <xref:Singulink.Cryptography.PasswordMatchType.RepeatedChar> | A run of the same character. | `111`, `!!` |
| <xref:Singulink.Cryptography.PasswordMatchType.RepeatedGroup> | A short group of characters repeated. | `121212`, `1!1!` |
| <xref:Singulink.Cryptography.PasswordMatchType.AnyChars> | A short run of arbitrary digits or symbols decorating the words. | `47`, `#1`, `1!` |
| <xref:Singulink.Cryptography.PasswordMatchType.RepeatValue> | A repeat of the value matched immediately before it. | the second `you` in `iloveyouyou` |

A value that occurs several times is reported once per occurrence. The default patterns allow a few consecutive words, so the first occurrences are usually reported as separate `Common` or `ContextualSubject` matches, and `RepeatValue` appears once the pattern has no room for another: `memememememe` is reported as three `Common` items followed by three `RepeatValue` items, all with the text `me`. Treat the two the same way when wording a message, and deduplicate <xref:Singulink.Cryptography.PasswordMatch.Values> if you only want to tell the user which distinct words were found.

## Explaining the Rejection

The point of returning the values is to let the user fix the problem on the first try. A message that names the words works far better than a generic one:

```csharp
static string? GetRejectionMessage(PasswordMatch? match)
{
    if (match is null)
        return null;

    var words = match.Values.Distinct().Select(v => $"\"{v}\"");

    return $"Your password is made up entirely of words that are commonly used in passwords or that are easy to guess " +
           $"from your account details ({string.Join(", ", words)}). Add an uncommon word or character sequence to " +
           $"make it harder to guess.";
}
```

#### Wording by match type

<xref:Singulink.Cryptography.PasswordMatch.Items> lets you be more specific. Contextual matches deserve their own explanation, since the user may not realize that their own name or your product name is an obvious guess:

```csharp
var contextual = match.Items
    .Where(i => i.MatchType is PasswordMatchType.ContextualSubject)
    .Select(i => i.MatchedText)
    .Distinct()
    .ToList();

if (contextual.Count > 0)
{
    message += $" Avoid using your name, email address or the name of this service ({string.Join(", ", contextual)}).";
}
```

> [!TIP]
> Keep the suggestion constructive. The matched words tell the user what to remove; suggesting they add an uncommon word tells them what to do instead, and keeps them from simply appending another digit, which the suffix patterns will catch anyway.

## Matching Without an Evaluator

<xref:Singulink.Cryptography.PasswordMatcher.GetFirstMatch*> runs a password against any set of matchers directly and returns the same <xref:Singulink.Cryptography.PasswordMatch>. It is what the evaluator uses internally, and it is the easiest way to test a matcher in isolation, since a match is only returned when the matchers consume the entire password:

```csharp
var matcher = PasswordMatcher.Text("hunter");

PasswordMatcher.GetFirstMatch("hunter", [matcher]);     // match with values ["hunter"]
PasswordMatcher.GetFirstMatch("hunter2", [matcher]);    // null: the "2" is not consumed
```

The evaluator adds the length limit and the matchers provider on top of this. See [Writing Custom Matchers](custom-matchers.md) for testing matchers this way.

## Next Steps

Continue with these related articles:

- [Password Policy Integration](password-policy.md) - Where the message fits in a complete password check.
- [Contextual Subjects](contextual-subjects.md) - Where contextual items come from.
- [How Matching Works](../concepts/how-matching-works.md) - Why the first match found is the one reported.

</div>
