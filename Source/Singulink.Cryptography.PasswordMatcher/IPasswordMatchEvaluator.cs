namespace Singulink.Cryptography;

public interface IPasswordMatchEvaluator
{
    PasswordMatchResult MatchPassword(string password, IEnumerable<ContextualSubject>? subjects);
}