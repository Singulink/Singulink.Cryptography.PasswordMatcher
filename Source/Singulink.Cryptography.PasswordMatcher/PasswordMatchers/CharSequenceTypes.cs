namespace Singulink.Cryptography.PasswordMatchers;

/// <summary>
/// The types of character sequences that a <see cref="CharSequenceMatcher"/> matches.
/// </summary>
[Flags]
public enum CharSequenceTypes
{
    /// <summary>
    /// No sequence types.
    /// </summary>
    None = 0,

    /// <summary>
    /// Sequences along a row of keyboard keys, such as "qwerty", "ytrewq" or "!@#$".
    /// </summary>
    KeyboardRow = 1,

    /// <summary>
    /// Sequences along a diagonal of keyboard keys, such as "1qaz", "zaq1" or "4esz".
    /// </summary>
    KeyboardDiagonal = 2,

    /// <summary>
    /// Sequences that zigzag between the number row and the row below it, such as "1q2w3e4r" or "q1w2e3r4".
    /// </summary>
    KeyboardZigzag = 4,

    /// <summary>
    /// Sequences along a row, column or diagonal of a numeric keypad or phone keypad, such as "789", "741", "2580" or "159".
    /// </summary>
    NumericKeypad = 8,

    /// <summary>
    /// Sequences of consecutive letters of the alphabet, such as "abc" or "zyx".
    /// </summary>
    Alphabet = 16,

    /// <summary>
    /// All sequence types.
    /// </summary>
    All = KeyboardRow | KeyboardDiagonal | KeyboardZigzag | NumericKeypad | Alphabet,
}
