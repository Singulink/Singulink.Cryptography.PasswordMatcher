namespace Singulink.Cryptography;

public interface IPasswordMatchersProvider
{
    IEnumerable<PasswordMatcher> GetMatchers(IEnumerable<ContextualSubject>? subjects);
}