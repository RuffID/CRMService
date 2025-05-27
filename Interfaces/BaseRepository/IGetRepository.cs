namespace CRMService.Interfaces.BaseRepository
{
    public interface IGetRepository<T> where T : class
    {
        Task<IEnumerable<T>?> GetItems(int startIndex, int limit);
        Task<T?> GetItem(T item, bool? trackable = null);
    }
}