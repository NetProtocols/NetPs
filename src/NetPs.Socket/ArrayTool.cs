namespace NetPs.Socket.Operations
{
    using System;
    using System.Collections.Generic;
#if NET35_CF
    using Array = System.Array2;
#endif

    /// <summary>
    /// 数组操作.
    /// </summary>
    public static class ArrayTool
    {
        /// <summary>
        /// 数组是否存在指定元素.
        /// </summary>
        /// <typeparam name="T">数组类型.</typeparam>
        /// <param name="array">数组.</param>
        /// <param name="match">匹配函数.</param>
        /// <returns>存在状态.</returns>
        public static bool Exist<T>(T[] array, Predicate<T> match)
        {
            return Array.Exists(array, match);
        }

        /// <summary>
        /// 获取数组指定元素清单.
        /// </summary>
        /// <typeparam name="T">数组类型.</typeparam>
        /// <param name="array">数组.</param>
        /// <param name="match">匹配函数.</param>
        /// <returns>匹配清单.</returns>
        public static T[] FindAll<T>(T[] array, Predicate<T> match)
        {
            return Array.FindAll(array, match);
        }

        /// <summary>
        /// 空数组.
        /// </summary>
        /// <typeparam name="T">类型.</typeparam>
        /// <returns>实例.</returns>
        public static T[] Empty<T>()
        {
            return Array.Empty<T>();
        }

        /// <summary>
        /// 转只读.
        /// </summary>
        /// <typeparam name="T">类型.</typeparam>
        /// <param name="array">数组.</param>
        /// <returns>实例.</returns>
        public static IReadOnlyList<T> ToReadOnly<T>(this T[] array)
        {
#if NET35_CF
            return array.AsReadOnly();
#else
            return (IReadOnlyList<T>)array;
#endif
        }
    }
}
