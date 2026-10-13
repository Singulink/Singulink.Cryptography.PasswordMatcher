using System.Diagnostics.CodeAnalysis;

namespace Singulink.Cryptography.PasswordMatchers;

/// <summary>
/// Base class for matchers that consume text from the password, with optional support for matching consecutive repeats of the value and a
/// trailing separator character.
/// </summary>
/// <remarks>
/// <para>Derived classes implement <see cref="GetValueMatches(PasswordMatchContext)"/> to match the value itself. Repeats and separators are
/// handled by this class: after each value match, the exact characters that were matched may be matched again any number of times, and a single
/// separator character (space, period, dash, underscore or comma) may follow each occurrence.</para>
/// </remarks>
public abstract class ValueMatcher : PasswordMatcher
{
    /// <summary>
    /// Gets the characters that are matched as separators after a value when <see cref="MatchTrailingSeparator"/> is <see langword="true"/>.
    /// </summary>
    public static ImmutableArray<char> Separators { get; } = [' ', '.', '-', '_', ','];

    /// <summary>
    /// Gets a value indicating whether a single separator character after the value is also matched.
    /// </summary>
    public bool MatchTrailingSeparator { get; }

    /// <summary>
    /// Gets a value indicating whether consecutive repeats of the matched value are also matched.
    /// </summary>
    public bool MatchRepeats { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValueMatcher"/> class.
    /// </summary>
    /// <param name="matchRepeats">Whether consecutive repeats of the matched value are also matched.</param>
    /// <param name="matchTrailingSeparator">Whether a single separator character after the value is also matched.</param>
    protected ValueMatcher(bool matchRepeats, bool matchTrailingSeparator)
    {
        MatchRepeats = matchRepeats;
        MatchTrailingSeparator = matchTrailingSeparator;
    }

    /// <inheritdoc/>
    protected sealed override IEnumerable<PasswordMatchContext> GetMatchesCore(PasswordMatchContext context)
    {
        foreach (var childContext in GetValueMatches(context))
        {
            yield return childContext;

            int valueLength = childContext.TotalMatchedLength - context.TotalMatchedLength;

            if (!MatchRepeats || valueLength is 0)
            {
                if (MatchTrailingSeparator && TryMatchSeparator(childContext, out var separatorContext))
                    yield return separatorContext;

                continue;
            }

            // Repeats of the exact characters that were matched, optionally joined by separators, form a tree of contexts: each occurrence
            // can be followed directly by another occurrence, by a separator, or by a separator and another occurrence. Explore it depth-first
            // with an explicit stack so that long chains do not nest iterators.

            string repeatValue = context.CheckedPassword.Substring(context.TotalMatchedLength, valueLength);
            var repeatItem = childContext.LastMatchItem is not null ? new PasswordMatchItem(childContext.LastMatchItem.MatchedText, PasswordMatchType.RepeatValue) : null;

            var pending = new Stack<PasswordMatchContext>();
            pending.Push(childContext);

            while (pending.Count > 0)
            {
                var c = pending.Pop();
                PasswordMatchContext? directRepeat = null;
                PasswordMatchContext? separatedRepeat = null;

                if (c.RemainingChars.StartsWith(repeatValue.AsSpan(), StringComparison.Ordinal))
                {
                    directRepeat = c.CreateChild(valueLength, repeatItem);
                    yield return directRepeat;
                }

                if (MatchTrailingSeparator && TryMatchSeparator(c, out var separatorContext))
                {
                    yield return separatorContext;

                    if (separatorContext.RemainingChars.StartsWith(repeatValue.AsSpan(), StringComparison.Ordinal))
                    {
                        separatedRepeat = separatorContext.CreateChild(valueLength, repeatItem);
                        yield return separatedRepeat;
                    }
                }

                if (separatedRepeat is not null)
                    pending.Push(separatedRepeat);

                if (directRepeat is not null)
                    pending.Push(directRepeat);
            }
        }
    }

    /// <summary>
    /// Gets the contexts that result from matching the value at the current position of the given context, in the order they should be tried.
    /// Repeats and trailing separators do not need to be handled by the implementation.
    /// </summary>
    /// <param name="context">The context to match against.</param>
    protected abstract IEnumerable<PasswordMatchContext> GetValueMatches(PasswordMatchContext context);

    private static bool TryMatchSeparator(PasswordMatchContext context, [NotNullWhen(true)] out PasswordMatchContext? separatorContext)
    {
        if (context.RemainingChars is [char s, ..] && Separators.Contains(s))
        {
            separatorContext = context.CreateChild(1, null);
            return true;
        }

        separatorContext = null;
        return false;
    }
}
