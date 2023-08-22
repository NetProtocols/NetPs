namespace NetPs.Socket.Eggs
{
    using System;

    public interface IHeat
    {
        void Start(IHeatingWatch watch);
    }
    public interface IHeatingWatch
    {
        void Heat_Progress();

        void Heat_End();
    }
}
