using System;

namespace CCLLC.CDS.FieldEncryption.Test.Fakes
{
    public class FakeCacheProvider : CCLLC.Core.ICache
    {
        public void Add(string key, object data, TimeSpan lifetime)
        {
           
        }

        public void Add(string key, object data, int seconds)
        {
            
        }

        public void Add<T>(string key, T data, TimeSpan lifetime)
        {
            
        }

        public void Add<T>(string key, T data, int seconds)
        {
            
        }

        public bool Exists(string key)
        {
            return false;
        }

        public object Get(string key)
        {
            return null;
        }

        public T Get<T>(string key)
        {
            return (T)(Object)Get(key);
        }

        public void Remove(string key)
        {
            
        }
    }
}
