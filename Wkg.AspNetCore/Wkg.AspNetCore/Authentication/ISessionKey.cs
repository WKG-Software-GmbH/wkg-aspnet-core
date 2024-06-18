namespace Wkg.AspNetCore.Authentication;

public interface ISessionKey<TExtendedKeys> where TExtendedKeys : IExtendedKeys<TExtendedKeys>
{
    TExtendedKeys ExtendedKeys { get; }

    ref long CreatedAt { get; }

    int Size { get; }

    void WriteKey(Span<byte> destination);
}