namespace Singulink.Cryptography;

/// <summary>
/// Represents the state of a password match in progress: the password being checked, how much of it has been matched so far, and the items that
/// were matched.
/// </summary>
/// <remarks>
/// <para>Contexts are immutable. Matchers advance a match by creating child contexts with <see cref="CreateChild(int, PasswordMatchItem)"/>.</para>
/// </remarks>
public sealed class PasswordMatchContext
{
    /// <summary>
    /// Gets the password exactly as it was provided for matching.
    /// </summary>
    public string OriginalPassword { get; }

    /// <summary>
    /// Gets the normalized password that matchers operate on: <see cref="OriginalPassword"/> trimmed, lower-cased and with runs of spaces collapsed
    /// into a single space.
    /// </summary>
    public string CheckedPassword { get; }

    /// <summary>
    /// Gets the characters of <see cref="CheckedPassword"/> that have not been matched yet.
    /// </summary>
    public ReadOnlySpan<char> RemainingChars => CheckedPassword.AsSpan(TotalMatchedLength);

    /// <summary>
    /// Gets the item that was matched to produce this context, or <see langword="null"/> if this is the initial context or the match consumed
    /// characters without producing an item (such as a separator).
    /// </summary>
    internal PasswordMatchItem? LastMatchItem { get; }

    /// <summary>
    /// Gets the total number of characters of <see cref="CheckedPassword"/> matched so far.
    /// </summary>
    public int TotalMatchedLength { get; }

    /// <summary>
    /// Gets the context this context was created from, or <see langword="null"/> if this is the initial context.
    /// </summary>
    internal PasswordMatchContext? ParentContext { get; }

    /// <summary>
    /// Gets the cache of matcher results shared by every context created during the same password check.
    /// </summary>
    internal MatchCache Cache { get; }

    // Splice contexts re-use a cached chain of results: the items between SpliceRoot (exclusive) and SpliceResult (inclusive) are logically
    // appended to ParentContext without copying them.

    private PasswordMatchContext? SpliceRoot { get; }

    private PasswordMatchContext? SpliceResult { get; }

    internal PasswordMatchContext(string password)
    {
        OriginalPassword = password;
        CheckedPassword = OriginalPassword.Trim().ToLowerInvariant();

        while (true)
        {
            string normalizedSpaces = CheckedPassword.Replace("  ", " ");

            if (normalizedSpaces == CheckedPassword)
                break;

            CheckedPassword = normalizedSpaces;
        }

        Cache = new MatchCache();
    }

    private PasswordMatchContext(PasswordMatchContext parent, int length, PasswordMatchItem? matchItem)
    {
        OriginalPassword = parent.OriginalPassword;
        CheckedPassword = parent.CheckedPassword;
        Cache = parent.Cache;
        ParentContext = parent;
        LastMatchItem = matchItem;
        TotalMatchedLength = parent.TotalMatchedLength + length;
    }

    private PasswordMatchContext(PasswordMatchContext parent, PasswordMatchContext spliceRoot, PasswordMatchContext spliceResult)
    {
        OriginalPassword = parent.OriginalPassword;
        CheckedPassword = parent.CheckedPassword;
        Cache = parent.Cache;
        ParentContext = parent;
        SpliceRoot = spliceRoot;
        SpliceResult = spliceResult;
        LastMatchItem = spliceResult.LastMatchItem;
        TotalMatchedLength = parent.TotalMatchedLength + (spliceResult.TotalMatchedLength - spliceRoot.TotalMatchedLength);
    }

    /// <summary>
    /// Creates a child context that extends this context by the given number of matched characters.
    /// </summary>
    /// <param name="length">The number of characters matched, starting at the current position.</param>
    /// <param name="matchItem">The item that was matched, or <see langword="null"/> if the characters do not represent a matched item (such as a
    /// separator).</param>
    /// <exception cref="ArgumentOutOfRangeException">Length is negative or extends past the end of the password.</exception>
    public PasswordMatchContext CreateChild(int length, PasswordMatchItem? matchItem)
    {
        if (length < 0)
            throw new ArgumentOutOfRangeException(nameof(length), "Length must be non-negative.");

        if (TotalMatchedLength + length > CheckedPassword.Length)
            throw new ArgumentOutOfRangeException(nameof(length), "Length exceeds the remaining length of the password.");

        if (matchItem is not null && length > 0)
            matchItem = matchItem.WithMatchedChars(CheckedPassword.Substring(TotalMatchedLength, length));

        return new PasswordMatchContext(this, length, matchItem);
    }

    /// <summary>
    /// Returns a context equivalent to re-applying the matches between <paramref name="root"/> (exclusive) and <paramref name="result"/>
    /// (inclusive) on top of this context, in constant time.
    /// </summary>
    internal PasswordMatchContext Splice(PasswordMatchContext root, PasswordMatchContext result)
    {
        return ReferenceEquals(result, root) ? this : new PasswordMatchContext(this, root, result);
    }

    /// <summary>
    /// Gets the items matched so far, in the order they were matched.
    /// </summary>
    internal List<PasswordMatchItem> GetItems()
    {
        var items = new List<PasswordMatchItem>();
        AppendItemsReversed(items, null);
        items.Reverse();
        return items;
    }

    private void AppendItemsReversed(List<PasswordMatchItem> items, PasswordMatchContext? stopAt)
    {
        for (var c = this; !ReferenceEquals(c, stopAt) && c.ParentContext is not null; c = c.ParentContext)
        {
            if (c.SpliceResult is not null)
                c.SpliceResult.AppendItemsReversed(items, c.SpliceRoot);
            else if (c.LastMatchItem is not null)
                items.Add(c.LastMatchItem);
        }
    }

    /// <summary>
    /// Returns a string describing the state of the match for debugging purposes.
    /// </summary>
    public override string ToString()
    {
        return $"Remaining: \"{RemainingChars.ToString()}\" ({RemainingChars.Length} chars), LastMatched: {LastMatchItem}, TotalMatched: {TotalMatchedLength}";
    }

    /// <summary>
    /// Caches the results of each matcher at each position of the password for the duration of a password check.
    /// </summary>
    internal sealed class MatchCache
    {
        private readonly Dictionary<(PasswordMatcher Matcher, int Position), (PasswordMatchContext Root, List<PasswordMatchContext> Results)> _entries = [];

        public bool TryGet(PasswordMatcher matcher, int position, out (PasswordMatchContext Root, List<PasswordMatchContext> Results) entry)
        {
            return _entries.TryGetValue((matcher, position), out entry);
        }

        public void Add(PasswordMatcher matcher, PasswordMatchContext root, List<PasswordMatchContext> results)
        {
            _entries[(matcher, root.TotalMatchedLength)] = (root, results);
        }
    }
}
