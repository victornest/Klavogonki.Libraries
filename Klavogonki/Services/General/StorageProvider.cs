using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Klavogonki
{
    public class StorageProvider<T> : IStorageProvider<T>
        where T : new()
    {
        private readonly string dbPath;

        public StorageProvider(string dbPath)
        {
            this.dbPath = dbPath;
        }

        public T Read()
        {
            T result;
            if (File.Exists(dbPath))
            {
                using (FileStream fs = new FileStream(dbPath, FileMode.Open))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    result = (T)formatter.Deserialize(fs);
                }
            }
            else result = new T();
            return result;
        }

        public void Save(T data)
        {
            using (FileStream fs = new FileStream(dbPath, FileMode.Create))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(fs, data);
            }
        }
    }
}
