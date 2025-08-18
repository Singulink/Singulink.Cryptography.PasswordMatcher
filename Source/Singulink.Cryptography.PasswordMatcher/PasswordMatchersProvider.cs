using static Singulink.Cryptography.PasswordMatcher;
using static Singulink.Cryptography.PasswordMatchers.CommonMatchers;

namespace Singulink.Cryptography;

public class PasswordMatchersProvider : IPasswordMatchersProvider
{
    public static PasswordMatchersProvider Default { get; } = new PasswordMatchersProvider(ContextualSubjectMatcherProvider.Default);

    private readonly IContextualSubjectMatcherProvider _subjectMatcherProvider;

    public PasswordMatchersProvider(IContextualSubjectMatcherProvider subjectMatcherProvider)
    {
        _subjectMatcherProvider = subjectMatcherProvider;
    }

    public virtual IEnumerable<PasswordMatcher> GetMatchers(IEnumerable<ContextualSubject>? subjects)
    {
        // Subject matchers

        var contextualSubjectMatchers = subjects?
            .Select(_subjectMatcherProvider.GetMatcher)
            .OfType<PasswordMatcher>()
            .ToList() ?? [];

        var singleSubjectMatcher = Any([
            ..contextualSubjectMatchers,
            GeneralSubjectMatcher,
        ]);

        var subjectMatcher = Sequence([
            singleSubjectMatcher,
            Optional(singleSubjectMatcher),
        ]);

        var subjectWithOptionalDeterminerMatcher = Sequence([
            Optional(SubjectDeterminerMatcher),
            subjectMatcher,
        ]);

        // Adjective matchers

        // Pattern: [prefix] adjective
        // Example: "[is] great"
        var adjectiveAfterSubjectMatcher = Sequence([
            Optional(SubjectToAdjectiveCopularVerbMatcher),
            AdjectiveMatcher,
        ]);

        // Pattern: [determiner] adjective
        // Example: "[the] great"
        var adjectiveBeforeSubjectMatcher = Sequence([
            Optional(SubjectDeterminerMatcher),
            AdjectiveMatcher,
        ]);

        // Verb matchers

        // Pattern: verb [suffix]
        // Example: "loves [to]"
        var verbWithOptionalSuffixMatcher = Sequence([
            VerbMatcher,
            Optional(VerbSuffixMatcher),
        ]);

        // ----------------------------------
        // Resulting common sequence matchers
        // ----------------------------------

        // Pattern: (pwd prefix) [pwd suffix]
        // Example: qwerty [!!!]
        yield return Sequence([
            PasswordPrefixMatcher,
            Optional(PasswordSuffixMatcher),
        ]);

        // Pattern: subject [subject] [subject] [subject]
        // Example: password [password] [password] [password]
        yield return PasswordSequence([
            subjectMatcher, // matches up to 2 consecutive subjects
            Optional(subjectMatcher),
        ]);

        // Pattern: [determiner]subject [[prefix]adjective]
        // Example: [this] password [[is] great]
        yield return PasswordSequence([
            subjectWithOptionalDeterminerMatcher,
            Optional(adjectiveAfterSubjectMatcher),
        ]);

        // Pattern: [determiner]subject (copular verb) [determiner]subject
        // Example: [the] password [is] [a] password
        yield return PasswordSequence([
            subjectWithOptionalDeterminerMatcher,
            SubjectToSubjectCopularVerbMatcher,
            subjectWithOptionalDeterminerMatcher,
        ]);

        // Pattern: [determiner]adjective subject
        // Example: [the] great password
        yield return PasswordSequence([
            adjectiveBeforeSubjectMatcher,
            subjectMatcher,
        ]);

        // Pattern: [determiner]subject [prefix]adjective [determiner]subject
        // Example: this [is a] bad password
        yield return PasswordSequence([
            subjectWithOptionalDeterminerMatcher,
            adjectiveAfterSubjectMatcher,
            subjectWithOptionalDeterminerMatcher,
        ]);

        // Pattern: [[determiner]subject [copular verb]] adjective [conjunction] [determiner]subject
        // Example: [i [am]] cool as [a] ninja
        yield return PasswordSequence([
            Optional(Sequence([subjectWithOptionalDeterminerMatcher, SubjectToSubjectCopularVerbMatcher])),
            Sequence([AdjectiveMatcher, Optional(AdjectiveToSubjectConjunctionMatcher)]),
            subjectWithOptionalDeterminerMatcher,
        ]);

        // Pattern: [determiner]subject [conjunction] [determiner]subject [[prefix]adjective]
        // Example: [this] password [and] [the] appname [[is] great]
        yield return PasswordSequence([
            subjectWithOptionalDeterminerMatcher,
            Optional(SubjectConjunctionMatcher),
            subjectWithOptionalDeterminerMatcher,
            Optional(adjectiveAfterSubjectMatcher),
        ]);

        // Pattern: [determiner]subject verb[suffix] verb
        // Example: [this] admin loves [to] login
        yield return PasswordSequence([
            subjectWithOptionalDeterminerMatcher,
            verbWithOptionalSuffixMatcher,
            VerbMatcher,
        ]);

        // Pattern: [determiner]subject verb[suffix] [verb[suffix]] [determiner]subject
        // Example: [this] admin loves [to] log in[to] appname
        yield return PasswordSequence([
            subjectWithOptionalDeterminerMatcher,
            verbWithOptionalSuffixMatcher,
            Optional(verbWithOptionalSuffixMatcher),
            subjectWithOptionalDeterminerMatcher,
        ]);

        PasswordMatcher PasswordSequence(IEnumerable<PasswordMatcher> matchers)
        {
            return Sequence([
                Optional(PasswordPrefixMatcher),
                ..matchers,
                Optional(PasswordSuffixMatcher)
            ]);
        }
    }
}