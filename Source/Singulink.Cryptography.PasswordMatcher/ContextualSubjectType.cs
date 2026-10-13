namespace Singulink.Cryptography;

/// <summary>
/// The type of a <see cref="ContextualSubject"/>, which determines how its value is interpreted.
/// </summary>
public enum ContextualSubjectType
{
    /// <summary>
    /// A general value with no special interpretation, such as an application or product name.
    /// </summary>
    General,

    /// <summary>
    /// A person's name.
    /// </summary>
    Name,

    /// <summary>
    /// An email address. The top-level domain is ignored.
    /// </summary>
    Email,

    /// <summary>
    /// A username.
    /// </summary>
    Username,

    /// <summary>
    /// A company name.
    /// </summary>
    Company,

    /// <summary>
    /// A website address. The protocol, "www." prefix, top-level domain and path are ignored.
    /// </summary>
    Website,

    /// <summary>
    /// A street address.
    /// </summary>
    Address,

    /// <summary>
    /// A phone number.
    /// </summary>
    PhoneNumber,
}
