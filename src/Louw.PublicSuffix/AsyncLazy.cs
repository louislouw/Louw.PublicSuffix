/*
 * Based on this article:
 * https://blogs.msdn.microsoft.com/pfxteam/2011/01/15/asynclazyt/
 */
using System;
using System.Threading.Tasks;

namespace Louw.PublicSuffix
{
    public class AsyncLazy<T> : Lazy<Task<T>> 
    { 
        public AsyncLazy(Func<T> valueFactory) : 
            base(() => Task.Factory.StartNew(valueFactory)) { }
        public AsyncLazy(Func<Task<T>> taskFactory) : 
            base(() => Task.Factory.StartNew(() => taskFactory()).Unwrap()) { } 
    }
}