namespace Singulink.Cryptography.PasswordMatchers;

#pragma warning disable SA1515 // Single-line comment should be preceded by blank line

/// <summary>
/// Provides predefined keyboard and keypad layout data.
/// </summary>
public static class KeyboardData
{
    /// <summary>
    /// Gets the rows of keys for the English (US) keyboard layout, from left to right.
    /// </summary>
    public static ImmutableArray<string> EnglishUSRows { get; } = [
        @"`1234567890-=\",
        @"qwertyuiop[]\",
        @"asdfghjkl;'",
        @"zxcvbnm,./",
    ];

    /// <summary>
    /// Gets the diagonals of keys for the English (US) keyboard layout that run from the top-left towards the bottom-right.
    /// </summary>
    public static ImmutableArray<string> EnglishUSForwardDiagonals { get; } = [
        @"1qaz",
        @"2wsx",
        @"3edc",
        @"4rfv",
        @"5tgb",
        @"6yhn",
        @"7ujm",
        @"8ik,",
        @"9ol.",
        @"0p;/",
        @"-['",
        @"=]",
    ];

    /// <summary>
    /// Gets the diagonals of keys for the English (US) keyboard layout that run from the top-right towards the bottom-left.
    /// </summary>
    public static ImmutableArray<string> EnglishUSBackwardDiagonals { get; } = [
        @"2q",
        @"3wa",
        @"4esz",
        @"5rdx",
        @"6tfc",
        @"7ygv",
        @"8uhb",
        @"9ijn",
        @"0okm",
        @"-pl,",
        @"=[;.",
        @"\]'/",
    ];

    /// <summary>
    /// Gets the lines of keys for the English (US) keyboard layout that zigzag between the number row and the row below it, starting with either
    /// row.
    /// </summary>
    public static ImmutableArray<string> EnglishUSZigzags { get; } = [
        @"1q2w3e4r5t6y7u8i9o0p",
        @"q1w2e3r4t5y6u7i8o9p0",
    ];

    /// <summary>
    /// Gets the rows, columns and diagonals of keys for numeric keypads and phone keypads. Columns include the zero key where it sits below the
    /// column on either layout.
    /// </summary>
    public static ImmutableArray<string> NumericKeypadLines { get; } = [
        // Rows (both layouts)
        @"123",
        @"456",
        @"789",
        // Columns (numeric keypad, zero below the first two columns)
        @"7410",
        @"8520",
        @"963",
        // Columns (phone keypad, zero below the middle column)
        @"2580",
        // Diagonals (both layouts)
        @"159",
        @"357",
    ];

    /// <summary>
    /// Gets the keys that produce a different character when shifted on the English (US) keyboard layout. Each entry is a two-character string
    /// containing the unshifted character followed by the shifted character.
    /// </summary>
    public static ImmutableArray<string> EnglishUSKeysToShiftedKeys { get; } = [
        @"`~",
        @"1!",
        @"2@",
        @"3#",
        @"4$",
        @"5%",
        @"6^",
        @"7&",
        @"8*",
        @"9(",
        @"0)",
        @"-_",
        @"=+",
        @"[{",
        @"]}",
        @"\|",
        @";:",
        @"'""",
        @",<",
        @".>",
        @"/?",
    ];
}
