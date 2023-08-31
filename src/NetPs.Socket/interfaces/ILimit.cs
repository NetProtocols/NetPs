namespace NetPs.Socket
{
    using System;

    public interface ILimit
    {
        int Limit { get; }
        void SetLimit(int value);
    }
}
