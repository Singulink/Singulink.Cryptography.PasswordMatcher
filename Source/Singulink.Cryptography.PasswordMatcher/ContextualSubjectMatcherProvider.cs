using System.Runtime.InteropServices;

namespace Singulink.Cryptography;

public class ContextualSubjectMatcherProvider : IContextualSubjectMatcherProvider
{
    private static readonly ImmutableArray<char> DefaultDelimiters = ImmutableArray.Create(' ', ',', '.', '-', '_', '.', ':', ';', '@');

    public static ContextualSubjectMatcherProvider Default { get; } = new ContextualSubjectMatcherProvider();

    public ImmutableArray<char> Delimiters { get; }

    protected ContextualSubjectMatcherProvider()
    {
        Delimiters = DefaultDelimiters;
    }

    public ContextualSubjectMatcherProvider(IEnumerable<char> delimiters)
    {
        Delimiters = delimiters.ToImmutableArray();
    }

    public virtual PasswordMatcher? GetMatcher(ContextualSubject subject)
    {
        string value = subject.Value;

        if (subject.Type is ContextualSubjectType.Website)
        {
            ReadOnlySpan<char> span = value;

            // Remove protocol prefix if present

            if (span.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
                span = span["http://".Length..];
            else if (span.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                span = span["https://".Length..];

            // Remove "www." prefix if present

            if (span.StartsWith("www.", StringComparison.OrdinalIgnoreCase))
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

        return GetPermutationsMatcher(value);

        PasswordMatcher? GetPermutationsMatcher(string value)
        {
            string[] values = value.Split(ImmutableCollectionsMarshal.AsArray(Delimiters), StringSplitOptions.RemoveEmptyEntries);

            if (values.Length is 0)
                return null;

            if (values.Length is 1)
                return PasswordMatcher.Text(value, PasswordMatchType.ContextualSubject);

            return PasswordMatcher.Permutations(values.Select(v => PasswordMatcher.Text(v, PasswordMatchType.ContextualSubject)));
        }
    }
}
