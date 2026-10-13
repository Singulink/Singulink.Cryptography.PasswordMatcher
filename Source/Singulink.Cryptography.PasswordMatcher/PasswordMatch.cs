namespace Singulink.Cryptography;

/// <summary>
/// Describes how a password was matched: the sequence of common or contextual values that it is composed of.
/// </summary>
public sealed class PasswordMatch
{
    /// <summary>
    /// Gets the items that were matched, in order.
    /// </summary>
    public IReadOnlyList<PasswordMatchItem> Items { get; }

    /// <summary>
    /// Gets the values that were matched, in order. This is a convenience view of <see cref="Items"/> suitable for showing the user why their
    /// password was rejected.
    /// </summary>
    public IReadOnlyList<string> Values { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PasswordMatch"/> class.
    /// </summary>
    /// <param name="items">The items that were matched, in order.</param>
    public PasswordMatch(IEnumerable<PasswordMatchItem> items)
    {
        var itemsArray = items.ToImmutableArray();

        Items = itemsArray;
        Values = itemsArray.Select(i => i.MatchedText).ToImmutableArray();
    }

    /// <summary>
    /// Returns the matched items separated by spaces.
    /// </summary>
    public override string ToString() => string.Join(" ", Items);
}
