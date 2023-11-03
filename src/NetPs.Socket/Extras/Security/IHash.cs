namespace NetPs.Socket.Extras.Security
{
    using System;
    public interface IHash
    {
        string Make(byte[] data);
    }
}
