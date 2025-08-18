namespace Singulink.Cryptography;

public interface IContextualSubjectMatcherProvider
{
    PasswordMatcher? GetMatcher(ContextualSubject subject);
}