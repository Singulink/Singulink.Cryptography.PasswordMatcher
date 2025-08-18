namespace System.Diagnostics;

#if NETSTANDARD

#pragma warning disable RCS1194 // Implement exception constructors

internal sealed class UnreachableException(string message) : Exception(message)
{
}

#endif