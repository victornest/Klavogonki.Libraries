using System;

namespace Klavogonki
{
    public abstract class FetchResult
    {
        protected bool isSuccessfulDownload;
        protected bool userExists;
        protected bool isOpen;

        public bool IsSuccessfulDownload
        {
            get => isSuccessfulDownload;
            protected set
            {
                isSuccessfulDownload = value;
            }
        }

        public bool UserExists
        {
            get => userExists;
            protected set
            {
                IsSuccessfulDownload = true;
                userExists = value;
            }
        }

        public bool IsOpen
        {
            get => isOpen;
            protected set
            {
                UserExists = true;
                isOpen = value;
            }
        }
    }

    public class FetchResult<T> : FetchResult where T : class
    {
        private T value;

        public FetchResult(
            bool isSuccessfulDownload = false,
            bool userExists = false,
            bool isOpen = false,
            T value = null)
        {
            if (value != null)
            {
                Value = value;
            }
            else if (isOpen)
            {
                IsOpen = isOpen;
            }
            else if (userExists)
            {
                UserExists = userExists;
            }
            else if (isSuccessfulDownload)
            {
                IsSuccessfulDownload = isSuccessfulDownload;
            }
        }

        public FetchResult(FetchResult fetchResult, T value = null) 
            : this(fetchResult.IsSuccessfulDownload, fetchResult.UserExists, fetchResult.IsOpen, value)
        { }

        public FetchResult(T value)
        {
            Value = value;
        }

        public T Value
        {
            get => value;
            private set
            {
                IsOpen = true;
                this.value = value;
            }
        }
    }
}