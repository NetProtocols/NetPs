namespace NetPs.Socket
{
    using System;

    public interface ILimit
    {
        /// <summary>
        /// 限制值
        /// </summary>
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

    public static class ILimitExtra
    {
        /// <summary>
        /// 一秒
        /// </summary>
        public const int SECOND = 10000000;

        public static bool HasSecondPassed(this ILimit limit, long now)
        {
            return now > limit.LastTime + SECOND;
        }
        public static int GetWaitMillisecond(this ILimit limit, long now)
        {
            return (int)((limit.LastTime + SECOND - now) / 10000);
        }

        public static long GetMillisecondTicks(this ILimit limit, int time)
        {
            return time * 10000;
        }
    }
}
