namespace NetPs.Socket
{
    using System;

    public interface IRx
    {
        byte[] Buffer { get; }
        int ReceivedSize { get; }
        bool Running { get; }
        void BindEvents(IRxEvents events);
    }
}
