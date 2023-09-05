namespace NetPs.Socket
{
    using System;

    /// <summary>
    /// 速度控制
    /// </summary>
    /// <remarks>
    /// 用于限制传输速度。以秒为单位，当大于限制流量则Wait该秒结束。
    /// </remarks>
    public interface ISpeedLimit
    {
        /// <summary>
        /// 限制值
        /// </summary>
        /// <remarks>
        /// 单位byte; 1M则为 1&#60;&#60;20 个单位。
        /// </remarks>
        int Limit { get; }
        /// <summary>
        /// 最近周期开始时间
        /// </summary>
        long LastTime { get; }
        /// <summary>
        /// 流速控制
        /// </summary>
        /// <param name="value">带宽byte</param>
        void SetLimit(int value);
    }

    public static class ISpeedLimitExtra
    {
        /// <summary>
        /// 一秒
        /// </summary>
        public const int SECOND = 10000000;

        public static bool HasSecondPassed(this ISpeedLimit limit, long now)
        {
            return now > limit.LastTime + SECOND;
        }
        public static int GetWaitMillisecond(this ISpeedLimit limit, long now)
        {
            return (int)((limit.LastTime + SECOND - now) / 10000);
        }

        public static long GetMillisecondTicks(this ISpeedLimit limit, int time)
        {
            return time * 10000;
        }
    }
}
