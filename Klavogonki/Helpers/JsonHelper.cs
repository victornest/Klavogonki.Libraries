using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;

namespace Klavogonki
{
    public static class JsonHelper
    {
        /// <summary>
        /// Serializes an object to JSON
        /// </summary>
        public static string Serialize(object instance)
        {
            var serializer = new DataContractJsonSerializer(instance.GetType(), new DataContractJsonSerializerSettings()
            {
                UseSimpleDictionaryFormat = true
            });
            using (var stream = new MemoryStream())
            {
                serializer.WriteObject(stream, instance);
                return Encoding.Default.GetString(stream.ToArray());
            }
        }

        /// <summary>
        /// DeSerializes an object from JSON
        /// </summary>
        public static T Deserialize<T>(string json) where T : class
        {
            using (var stream = new MemoryStream(Encoding.Default.GetBytes(json)))
            {
                var serializer = new DataContractJsonSerializer(typeof(T), new DataContractJsonSerializerSettings()
                {
                    UseSimpleDictionaryFormat = true
                });
                return serializer.ReadObject(stream) as T;
            }
        }
    }
}
