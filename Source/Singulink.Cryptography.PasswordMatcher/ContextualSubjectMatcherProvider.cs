using System.Runtime.InteropServices;

namespace Singulink.Cryptography;

/// <summary>
/// Creates matchers for contextual subjects by splitting the subject value into words and matching the words in any order.
/// </summary>
/// <remarks>
/// <para>Values are split on the provider's delimiters, and additionally where the case changes from lower to upper and where letters change to
/// digits or back, so that "MikeSmith", "mike_smith" and "mike1985" all yield the word "mike". Parts shorter than two characters are
/// discarded.</para>
/// <para>Email subjects have their top-level domain removed, and website subjects have their protocol, "www." prefix, top-level domain and path
/// removed before being split, so that "mikem@singulink.com" matches "mikem", "singulink", "mikemsingulink" or "singulinkmikem" but not "com".</para>
/// </remarks>
public class ContextualSubjectMatcherProvider : IContextualSubjectMatcherProvider
{
    private const int MinWordLength = 2;

    private static readonly ImmutableArray<char> DefaultDelimiters = [' ', ',', '.', '-', '_', '+', ':', ';', '@'];

    /// <summary>
    /// Gets the default provider, which splits subjects on spaces, commas, periods, dashes, underscores, plus signs, colons, semicolons and "@"
    /// characters.
    /// </summary>
    public static ContextualSubjectMatcherProvider Default { get; } = new ContextualSubjectMatcherProvider();

    /// <summary>
    /// Gets the characters that subject values are split into words on.
    /// </summary>
    public ImmutableArray<char> Delimiters { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ContextualSubjectMatcherProvider"/> class with the default delimiters.
    /// </summary>
    protected ContextualSubjectMatcherProvider()
    {
        Delimiters = DefaultDelimiters;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ContextualSubjectMatcherProvider"/> class with the given delimiters.
    /// </summary>
    /// <param name="delimiters">The characters that subject values are split into words on.</param>
    public ContextualSubjectMatcherProvider(IEnumerable<char> delimiters)
    {
        Delimiters = delimiters.ToImmutableArray();
    }

    /// <inheritdoc/>
    public virtual PasswordMatcher? GetMatcher(ContextualSubject subject)
    {
        string value = subject.Value;

        if (subject.Type is ContextualSubjectType.Website)
        {
            ReadOnlySpan<char> span = value;

            // Remove protocol prefix if present

            if (span.StartsWith("http://".AsSpan(), StringComparison.OrdinalIgnoreCase))
                span = span["http://".Length..];
            else if (span.StartsWith("https://".AsSpan(), StringComparison.OrdinalIgnoreCase))
                span = span["https://".Length..];

            // Remove "www." prefix if present

            if (span.StartsWith("www.".AsSpan(), StringComparison.OrdinalIgnoreCase))
                span = span["www.".Length..];

            // Remove anything after the first slash

            int slashIndex = span.IndexOf('/');

            if (slashIndex >= 0)
                span = span[..slashIndex];

            // Remove the TLD

            int tldDotIndex = span.LastIndexOf('.');

            if (tldDotIndex > 0)
                span = span[..tldDotIndex];

            if (span.Length != value.Length)
                value = span.ToString();
        }
        else if (subject.Type is ContextualSubjectType.Email)
        {
            ReadOnlySpan<char> span = value;

            // Remove the TLD

            int atIndex = span.LastIndexOf('@');
            int tldDotIndex = span.LastIndexOf('.');

            if (atIndex > 0 && tldDotIndex > atIndex)
                span = span[..tldDotIndex];

            if (span.Length != value.Length)
                value = span.ToString();
        }

        var words = value
            .Split(ImmutableCollectionsMarshal.AsArray(Delimiters), StringSplitOptions.RemoveEmptyEntries)
            .SelectMany(SplitOnBoundaries)
            .Where(w => w.Length >= MinWordLength)
            .ToList();

        if (words.Count is 0)
            return null;

        if (words.Count is 1)
            return WordMatcher(words[0]);

        return PasswordMatcher.Permutations(words.Select(WordMatcher));

        // Character substitutions are only checked for words longer than two letters, since a substituted one or two letter word (such as "1"
        // for "i") is far more likely to be a decoration than the word.

        static PasswordMatcher WordMatcher(string word) => PasswordMatcher.Text(word, PasswordMatchType.ContextualSubject, checkSubstitutions: word.Length > 2);
    }

    /// <summary>
    /// Splits a word where the case changes from lower to upper ("MikeSmith", "ACMEWidgets") and where letters change to digits or back
    /// ("mike1985").
    /// </summary>
    private static IEnumerable<string> SplitOnBoundaries(string word)
    {
        int start = 0;

        for (int i = 1; i < word.Length; i++)
        {
            char prev = word[i - 1];
            char c = word[i];
            bool boundary;

            if (char.IsDigit(prev) != char.IsDigit(c))
                boundary = true;
            else if (char.IsLower(prev) && char.IsUpper(c))
                boundary = true;
            else if (char.IsUpper(prev) && char.IsUpper(c) && i + 1 < word.Length && char.IsLower(word[i + 1]))
                boundary = true; // End of an upper-case run followed by a capitalized word, e.g. "ACME|Widgets"
            else
                boundary = false;

            if (boundary)
            {
                yield return word[start..i];
                start = i;
            }
        }

        yield return word[start..];
    }
}
