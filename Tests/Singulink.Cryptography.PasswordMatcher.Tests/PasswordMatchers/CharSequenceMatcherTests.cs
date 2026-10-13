namespace Singulink.Cryptography.Tests.PasswordMatchers;

[PrefixTestClass]
public class CharSequenceMatcherTests
{
    private static readonly PasswordMatcher RowMatcher = PasswordMatcher.CharSequence(CharSequenceTypes.KeyboardRow);
    private static readonly PasswordMatcher DiagonalMatcher = PasswordMatcher.CharSequence(CharSequenceTypes.KeyboardDiagonal);
    private static readonly PasswordMatcher ZigzagMatcher = PasswordMatcher.CharSequence(CharSequenceTypes.KeyboardZigzag);
    private static readonly PasswordMatcher KeypadMatcher = PasswordMatcher.CharSequence(CharSequenceTypes.NumericKeypad);
    private static readonly PasswordMatcher AlphabetMatcher = PasswordMatcher.CharSequence(CharSequenceTypes.Alphabet);
    private static readonly PasswordMatcher AllMatcher = PasswordMatcher.CharSequence(CharSequenceTypes.All);

    [TestMethod]
    public void Row_IsMatch()
    {
        var result = PasswordMatcher.GetFirstMatch("qwerty", [RowMatcher]);

        result.ShouldNotBeNull();
        result.Items.Select(m => m.MatchedText).ShouldBe(["QWERTY"]);
        result.Items[0].MatchType.ShouldBe(PasswordMatchType.CharSequence);
    }

    [TestMethod]
    public void RowFromMiddle_IsMatch()
    {
        PasswordMatcher.GetFirstMatch("fghjk", [RowMatcher]).ShouldNotBeNull();
        PasswordMatcher.GetFirstMatch("567", [RowMatcher]).ShouldNotBeNull();
    }

    [TestMethod]
    public void RowBackward_IsMatch()
    {
        var result = PasswordMatcher.GetFirstMatch("ytrewq", [RowMatcher]);

        result.ShouldNotBeNull();
        result.Items.Select(m => m.MatchedText).ShouldBe(["YTREWQ"]);
    }

    [TestMethod]
    public void RowShifted_IsMatch()
    {
        var result = PasswordMatcher.GetFirstMatch("!@#$", [RowMatcher]);
        result.ShouldNotBeNull();
        result.Items.Select(m => m.MatchedText).ShouldBe(["!@#$"]);

        result = PasswordMatcher.GetFirstMatch("iop{}|", [RowMatcher]);
        result.ShouldNotBeNull();
        result.Items.Select(m => m.MatchedText).ShouldBe(["IOP{}|"]);
    }

    [TestMethod]
    public void TooShort_IsNotMatch()
    {
        PasswordMatcher.GetFirstMatch("qw", [AllMatcher]).ShouldBeNull();
        PasswordMatcher.GetFirstMatch("ab", [AllMatcher]).ShouldBeNull();
    }

    [TestMethod]
    public void Skipped_IsNotMatch()
    {
        PasswordMatcher.GetFirstMatch("qwety", [AllMatcher]).ShouldBeNull();
        PasswordMatcher.GetFirstMatch("abce", [AllMatcher]).ShouldBeNull();
    }

    [TestMethod]
    public void Diagonal_IsMatch()
    {
        var result = PasswordMatcher.GetFirstMatch("1qaz", [DiagonalMatcher]);
        result.ShouldNotBeNull();
        result.Items.Select(m => m.MatchedText).ShouldBe(["1QAZ"]);

        result = PasswordMatcher.GetFirstMatch("zaq1", [DiagonalMatcher]);
        result.ShouldNotBeNull();
        result.Items.Select(m => m.MatchedText).ShouldBe(["ZAQ1"]);

        result = PasswordMatcher.GetFirstMatch("4esz", [DiagonalMatcher]);
        result.ShouldNotBeNull();
        result.Items.Select(m => m.MatchedText).ShouldBe(["4ESZ"]);

        result = PasswordMatcher.GetFirstMatch("!qaz", [DiagonalMatcher]);
        result.ShouldNotBeNull();
        result.Items.Select(m => m.MatchedText).ShouldBe(["!QAZ"]);
    }

    [TestMethod]
    public void Zigzag_IsMatch()
    {
        var result = PasswordMatcher.GetFirstMatch("1q2w3e4r", [ZigzagMatcher]);
        result.ShouldNotBeNull();
        result.Items.Select(m => m.MatchedText).ShouldBe(["1Q2W3E4R"]);

        result = PasswordMatcher.GetFirstMatch("q1w2e3", [ZigzagMatcher]);
        result.ShouldNotBeNull();
        result.Items.Select(m => m.MatchedText).ShouldBe(["Q1W2E3"]);

        result = PasswordMatcher.GetFirstMatch("!q@w#e", [ZigzagMatcher]);
        result.ShouldNotBeNull();
        result.Items.Select(m => m.MatchedText).ShouldBe(["!Q@W#E"]);

        result = PasswordMatcher.GetFirstMatch("r4e3w2q1", [ZigzagMatcher]);
        result.ShouldNotBeNull();
        result.Items.Select(m => m.MatchedText).ShouldBe(["R4E3W2Q1"]);
    }

    [TestMethod]
    public void Keypad_IsMatch()
    {
        foreach (string password in new[] { "789", "741", "147", "7410", "0147", "8520", "2580", "0852", "963", "369", "159", "951", "357", "753" })
        {
            var result = PasswordMatcher.GetFirstMatch(password, [KeypadMatcher]);
            result.ShouldNotBeNull(password);
            result.Items.Select(m => m.MatchedText).ShouldBe([password]);
        }
    }

    [TestMethod]
    public void Alphabet_IsMatch()
    {
        var result = PasswordMatcher.GetFirstMatch("abcdef", [AlphabetMatcher]);
        result.ShouldNotBeNull();
        result.Items.Select(m => m.MatchedText).ShouldBe(["ABCDEF"]);

        result = PasswordMatcher.GetFirstMatch("zyx", [AlphabetMatcher]);
        result.ShouldNotBeNull();
        result.Items.Select(m => m.MatchedText).ShouldBe(["ZYX"]);

        result = PasswordMatcher.GetFirstMatch("XYZ", [AlphabetMatcher]);
        result.ShouldNotBeNull();
        result.Items.Select(m => m.MatchedText).ShouldBe(["XYZ"]);
    }

    [TestMethod]
    public void OtherTypes_AreNotMatched()
    {
        PasswordMatcher.GetFirstMatch("1qaz", [RowMatcher]).ShouldBeNull();
        PasswordMatcher.GetFirstMatch("qwerty", [DiagonalMatcher]).ShouldBeNull();
        PasswordMatcher.GetFirstMatch("1q2w3e", [RowMatcher]).ShouldBeNull();
        PasswordMatcher.GetFirstMatch("abc", [RowMatcher]).ShouldBeNull();
        PasswordMatcher.GetFirstMatch("741", [RowMatcher]).ShouldBeNull();
        PasswordMatcher.GetFirstMatch("qwerty", [AlphabetMatcher]).ShouldBeNull();
    }

    [TestMethod]
    public void All_IsMatch()
    {
        foreach (string password in new[] { "1qaz", "qwerty", "1q2w3e", "7410", "abcdef" })
            PasswordMatcher.GetFirstMatch(password, [AllMatcher]).ShouldNotBeNull(password);
    }

    [TestMethod]
    public void Repeats_IsMatch()
    {
        var result = PasswordMatcher.GetFirstMatch("123123", [RowMatcher]);

        result.ShouldNotBeNull();
        result.Items.Select(m => m.MatchedText).ShouldBe(["123", "123"]);
    }

    [TestMethod]
    public void LongestMatchFirst()
    {
        var matcher = PasswordMatcher.Sequence([AllMatcher, PasswordMatcher.Text("x")]);

        var result = PasswordMatcher.GetFirstMatch("qwertyx", [matcher]);

        result.ShouldNotBeNull();
        result.Items.Select(m => m.MatchedText).ShouldBe(["QWERTY", "x"]);
    }

    [TestMethod]
    public void InvalidArguments_Throw()
    {
        Should.Throw<ArgumentException>(() => PasswordMatcher.CharSequence(CharSequenceTypes.None));
        Should.Throw<ArgumentOutOfRangeException>(() => PasswordMatcher.CharSequence((CharSequenceTypes)1000));
    }
}
