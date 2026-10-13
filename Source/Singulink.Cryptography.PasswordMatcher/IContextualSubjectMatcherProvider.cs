namespace Singulink.Cryptography;

/// <summary>
/// Creates matchers for contextual subjects.
/// </summary>
public interface IContextualSubjectMatcherProvider
{
    /// <summary>
    /// Gets a matcher for the given subject, or <see langword="null"/> if the subject has no value worth matching.
    /// </summary>
    /// <param name="subject">The subject to create a matcher for.</param>
    PasswordMatcher? GetMatcher(ContextualSubject subject);
}
