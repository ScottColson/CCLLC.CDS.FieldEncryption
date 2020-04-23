using System;
using System.Collections;
using System.Collections.Generic;


namespace CCLLC.CDS.FieldEncryption.Test.Fakes
{
    using CCLLC.Core;
    using CCLLC.Azure.Secrets;

    public class FakeSecretProviderFactory<T> : ISecretProviderFactory where T : class, ISecretProvider, new()
    {
        public ISecretProvider Create(IProcessExecutionContext executionContext, bool disableCache = false)
        {
            return new T();
        }
    }

    public class FakeSecretProvider : ISecretProvider
    {
        public string this[string key] => throw new NotImplementedException();

        public IEnumerable<string> Keys => throw new NotImplementedException();

        public IEnumerable<string> Values => throw new NotImplementedException();

        public int Count => throw new NotImplementedException();

        public bool ContainsKey(string key)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public T GetValue<T>(string key, T defaultValue = default)
        {
            if(typeof(T) == typeof(string))
            {
                return (T)(object)"eYT34kaslkdjfasdasdfluoueeq457Dousnd";
            }

            throw new Exception("Unsupported Fake");
            
        }

        public bool TryGetValue(string key, out string value)
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }

}
