
namespace Klavogonki
{
    public interface IStorageProvider<T>
    {
        T Read();

        void Save(T data);
    }
}