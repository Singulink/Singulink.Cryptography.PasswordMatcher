using System.Diagnostics;

namespace Singulink.Cryptography.Tests;

[PrefixTestClass]
public class PerformanceTests
{
    private static readonly ContextualSubject[] Subjects = [
        new("mikem@singulink.com", ContextualSubjectType.Email),
        new("Mike Smith", ContextualSubjectType.Name),
        new("https://www.singulink.com", ContextualSubjectType.Website),
    ];

    [TestMethod]
    public void AdversarialNonMatchingPasswords_CompleteQuickly()
    {
        // Long passwords made of many matchable tokens that never fully match are the worst case for a backtracking matcher: without the
        // per-position memoization the work would grow exponentially with length. The limit is generous to allow for slow CI machines and
        // debug builds.

        PasswordMatchEvaluator.Default.MatchPassword("warmup", Subjects);

        foreach (string token in new[] { "qwerty", "123", "1111", "me", "password", "asdf", "qwe123", "admin1!" })
        {
            string password = string.Concat(Enumerable.Repeat(token, (128 / token.Length) + 1)).Substring(0, 127) + "ÿ";

            var stopwatch = Stopwatch.StartNew();
            var result = PasswordMatchEvaluator.Default.MatchPassword(password, Subjects);
            stopwatch.Stop();

            result.ShouldBeNull(token);
            stopwatch.Elapsed.ShouldBeLessThan(TimeSpan.FromSeconds(2), token);
        }
    }

    [TestMethod]
    public void LongMatchingPasswords_CompleteQuickly()
    {
        PasswordMatchEvaluator.Default.MatchPassword("warmup", Subjects);

        foreach (string password in new[] {
            string.Concat(Enumerable.Repeat("qwerty", 20)),
            string.Concat(Enumerable.Repeat("me", 60)),
            string.Concat(Enumerable.Repeat("password", 16)),
            string.Concat(Enumerable.Repeat("123", 40)),
            @"1234567890-=qwertyuiop[]\asdfghjkl;'zxcvbnm,./memememe1234567890",
        })
        {
            var stopwatch = Stopwatch.StartNew();
            var result = PasswordMatchEvaluator.Default.MatchPassword(password, Subjects);
            stopwatch.Stop();

            result.ShouldNotBeNull(password);
            stopwatch.Elapsed.ShouldBeLessThan(TimeSpan.FromSeconds(2), password);
        }
    }
}
