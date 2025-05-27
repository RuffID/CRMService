namespace CRMService.Interfaces.BaseRepository
{
    public interface ICreateRepository<T> where T : class
    {
        void Create(T item); // создание объекта
    }
}
