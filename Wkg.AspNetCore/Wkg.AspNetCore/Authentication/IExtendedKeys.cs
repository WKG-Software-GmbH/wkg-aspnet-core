namespace Wkg.AspNetCore.Authentication;

public interface IExtendedKeys<TSelf> where TSelf : IExtendedKeys<TSelf>
{
    static abstract TSelf Generate();
}