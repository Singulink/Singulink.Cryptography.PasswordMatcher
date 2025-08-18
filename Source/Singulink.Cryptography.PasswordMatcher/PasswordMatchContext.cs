namespace Singulink.Cryptography;

public sealed record PasswordMatchContext
{
    public string OriginalPassword { get; }

    public string CheckedPassword { get; }

    public ReadOnlySpan<char> RemainingChars => CheckedPassword.AsSpan(TotalMatchedLength);

    /// <summary>
    /// Gets the last text that matched to the password (without separators).
    /// </summary>
    public PasswordMatchItem? LastMatchItem { get; private init; }

    /// <summary>
    /// Gets the total length of characters matched against <see cref="CheckedPassword"/> so far.
    /// </summary>
    public int TotalMatchedLength { get; private init; }

    public PasswordMatchContext? ParentContext { get; private init; }

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
    }

    public PasswordMatchContext CreateChild(PasswordMatcher matcher, int length, PasswordMatchItem? matchItem)
    {
        if (length < 0)
            throw new ArgumentOutOfRangeException(nameof(length), "Length must be non-negative.");

        int newTotalLength = TotalMatchedLength + length;

        if (newTotalLength > OriginalPassword.Length)
            throw new ArgumentOutOfRangeException(nameof(length), "Length exceeds the original password length.");

        return this with {
            LastMatchItem = matchItem,
            TotalMatchedLength = TotalMatchedLength + length,
            ParentContext = this,
        };
    }

    public bool Equals(PasswordMatchContext? other)
    {
        if (ReferenceEquals(this, other))
            return true;

        if (other is null)
            return false;

        return OriginalPassword == other.OriginalPassword &&
               LastMatchItem == other.LastMatchItem &&
               TotalMatchedLength == other.TotalMatchedLength &&
               ParentContext == other.ParentContext;
    }

    public override int GetHashCode() => HashCode.Combine(OriginalPassword, LastMatchItem, TotalMatchedLength, ParentContext);

    public override string ToString()
    {
        return $"Remaining: \"{RemainingChars.ToString()}\" ({RemainingChars.Length} chars), LastMatched: {LastMatchItem}, TotalMatched: {TotalMatchedLength}";
    }
}