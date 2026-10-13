namespace Singulink.Cryptography;

/// <summary>
/// The type of value that a <see cref="PasswordMatchItem"/> was matched to.
/// </summary>
public enum PasswordMatchType
{
    /// <summary>
    /// A common password word, phrase or value.
    /// </summary>
    Common,

    /// <summary>
    /// A contextual subject such as the user's name or the application name.
    /// </summary>
    ContextualSubject,

    /// <summary>
    /// A sequence of characters that are adjacent on a keyboard or keypad or consecutive in the alphabet.
    /// </summary>
    CharSequence,

    /// <summary>
    /// A run of the same character repeated.
    /// </summary>
    RepeatedChar,

    /// <summary>
    /// A short group of characters repeated.
    /// </summary>
    RepeatedGroup,

    /// <summary>
    /// A short run of arbitrary characters, such as a few digits or symbols appended to a word.
    /// </summary>
    AnyChars,

    /// <summary>
    /// A repeat of the value that was matched immediately before it.
    /// </summary>
    RepeatValue,
}
