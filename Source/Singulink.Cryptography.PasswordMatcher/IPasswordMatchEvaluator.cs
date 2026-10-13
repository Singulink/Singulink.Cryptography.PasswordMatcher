namespace Singulink.Cryptography;

/// <summary>
/// Evaluates whether passwords follow common or contextual patterns that make them easy to guess.
/// </summary>
public interface IPasswordMatchEvaluator
{
    /// <summary>
    /// Evaluates a password, taking into account contextual subjects such as the user's name or the application name.
    /// </summary>
    /// <param name="password">The password to evaluate.</param>
    /// <param name="subjects">Contextual subjects that the password should not be composed of, or <see langword="null"/> if there are none.</param>
    /// <returns>A description of how the password matched, meaning it is composed entirely of common or contextual values in a predictable order
    /// and should be rejected, or <see langword="null"/> if it did not match.</returns>
    PasswordMatch? MatchPassword(string password, IEnumerable<ContextualSubject>? subjects);
}
