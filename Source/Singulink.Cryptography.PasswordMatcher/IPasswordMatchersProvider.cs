namespace Singulink.Cryptography;

/// <summary>
/// Supplies the matchers that a password is evaluated against.
/// </summary>
public interface IPasswordMatchersProvider
{
    /// <summary>
    /// Gets the matchers that a password should be evaluated against, in the order they should be tried.
    /// </summary>
    /// <param name="subjects">Contextual subjects that the password should not be composed of, or <see langword="null"/> if there are none.</param>
    IEnumerable<PasswordMatcher> GetMatchers(IEnumerable<ContextualSubject>? subjects);
}
