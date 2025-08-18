namespace System.Diagnostics.CodeAnalysis;

#if NETSTANDARD

[AttributeUsage(AttributeTargets.All)]
internal sealed class DynamicallyAccessedMembers : Attribute
{
    public DynamicallyAccessedMemberTypes MemberTypes { get; }

    public DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes memberTypes)
    {
        MemberTypes = memberTypes;
    }
}

#endif