using System;
using AsyncCacheContract.Enums;

namespace AsyncCacheContract
{
    /// <summary>
    /// Custom Exception for AsyncCache.
    /// </summary>
    public class AsyncCacheException : Exception
    {
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="errorCode">ErrorCode enum</param>
        /// <param name="message">Associated message</param>
        public AsyncCacheException(AsyncCacheErrorCode errorCode, string message) : base($"Reason:{errorCode}. {message}")
        {
            ErrorCode = errorCode;
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="errorCode">ErrorCode enum</param>
        /// <param name="message">Associated message</param>
        /// <param name="inner">Inner exception</param>
        public AsyncCacheException(AsyncCacheErrorCode errorCode, string message, Exception inner)
            : base($"Reason:{errorCode}. {message}", inner)
        {
            ErrorCode = errorCode;
        }

        /// <summary>
        /// Gets the error code enum.
        /// </summary>
        public AsyncCacheErrorCode ErrorCode { get; private set; }
    }
}