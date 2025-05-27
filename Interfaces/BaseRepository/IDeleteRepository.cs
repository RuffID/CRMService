namespace CRMService.Interfaces.BaseRepository
{
    public interface IDeleteRepository<T> where T : class
    {
        void Delete(T item); // удаление объекта по id
    }
}
