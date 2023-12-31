﻿using System;

namespace NetPs.Webdav
{
    internal static class Guard
    {
        public static void NotNull(object obj, string paramName)
        {
            if (obj == null)
                throw new ArgumentNullException(paramName);
        }

        public static void NotNullOrEmpty(string str, string paramName)
        {
            if (string.IsNullOrEmpty(str))
                throw new ArgumentNullException(paramName);
        }
    }
}
