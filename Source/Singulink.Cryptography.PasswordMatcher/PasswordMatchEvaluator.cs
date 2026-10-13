namespace Singulink.Cryptography;

/// <summary>
/// Evaluates passwords against the matchers supplied by an <see cref="IPasswordMatchersProvider"/>.
/// </summary>
/// <remarks>
/// <para>Evaluators are immutable and safe to share between threads. <see cref="Default"/> uses <see cref="PasswordMatchersProvider.Default"/> and
/// is suitable for most applications.</para>
/// </remarks>
public class PasswordMatchEvaluator : IPasswordMatchEvaluator
{
    /// <summary>
    /// The default value of <see cref="MaxPasswordLength"/>.
    /// </summary>
    public const int DefaultMaxPasswordLength = 128;

    /// <summary>
    /// Gets the default evaluator, which uses <see cref="PasswordMatchersProvider.Default"/> and <see cref="DefaultMaxPasswordLength"/>.
    /// </summary>
    public static PasswordMatchEvaluator Default { get; } = new PasswordMatchEvaluator(PasswordMatchersProvider.Default);

    /// <summary>
    /// Gets the provider that supplies the matchers used to evaluate passwords.
    /// </summary>
    public IPasswordMatchersProvider MatchersProvider { get; }

    /// <summary>
    /// Gets the maximum length of password that is evaluated. Longer passwords are reported as not matching without being evaluated.
    /// </summary>
    /// <remarks>
    /// <para>Evaluation time and memory grow roughly quadratically with password length, so the limit bounds the cost of evaluating very long
    /// passwords. A password longer than the limit that consists entirely of common words is not a realistic risk, so skipping evaluation is
    /// appropriate.</para>
    /// </remarks>
    public int MaxPasswordLength { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PasswordMatchEvaluator"/> class.
    /// </summary>
    /// <param name="matchersProvider">The provider that supplies the matchers used to evaluate passwords.</param>
    /// <param name="maxPasswordLength">The maximum length of password that is evaluated. Longer passwords are reported as not matching without being
    /// evaluated.</param>
    public PasswordMatchEvaluator(IPasswordMatchersProvider matchersProvider, int maxPasswordLength = DefaultMaxPasswordLength)
    {
        if (maxPasswordLength < 1)
            throw new ArgumentOutOfRangeException(nameof(maxPasswordLength), "Maximum password length must be at least 1.");

        MatchersProvider = matchersProvider;
        MaxPasswordLength = maxPasswordLength;
    }

    /// <inheritdoc/>
    public virtual PasswordMatch? MatchPassword(string password, IEnumerable<ContextualSubject>? subjects)
    {
        if (password.Length > MaxPasswordLength)
            return null;

        return PasswordMatcher.GetFirstMatch(password, MatchersProvider.GetMatchers(subjects));
    }
}
