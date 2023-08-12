﻿namespace NetPs.Webdav
{
    /// <summary>
    /// Represents a lock owner.
    /// </summary>
    public abstract class LockOwner
    {
        /// <summary>
        /// Gets a value representing an owner.
        /// </summary>
        public abstract string Value { get; }
    }
}
