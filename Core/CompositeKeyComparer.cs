namespace CRMService.Core
{

    /// <summary>
    /// Универсальный компаратор сущностей по составному ключу из двух полей.
    /// </summary>
    public class CompositeKeyComparer<T, K1, K2> : IEqualityComparer<T>
        where K1 : notnull
        where K2 : notnull
    {
        private readonly Func<T, K1> _key1;
        private readonly Func<T, K2> _key2;

        public CompositeKeyComparer(Func<T, K1> key1, Func<T, K2> key2)
        {
            _key1 = key1 ?? throw new ArgumentNullException(nameof(key1));
            _key2 = key2 ?? throw new ArgumentNullException(nameof(key2));
        }

        public bool Equals(T? x, T? y)
        {
            if (ReferenceEquals(x, y)) 
                return true;

            if (x is null || y is null) 
                return false;

            return EqualityComparer<K1>.Default.Equals(_key1(x), _key1(y)) &&
                   EqualityComparer<K2>.Default.Equals(_key2(x), _key2(y));
        }

        public int GetHashCode(T obj)
        {
            return HashCode.Combine(_key1(obj), _key2(obj));
        }
    }

    /// <summary>
    /// Фабрика для удобного создания компараторов.
    /// </summary>
    public class CompositeKeyComparer
    {
        public static IEqualityComparer<T> For<T, K1, K2>(Func<T, K1> key1, Func<T, K2> key2)
            where K1 : notnull
            where K2 : notnull
            => new CompositeKeyComparer<T, K1, K2>(key1, key2);
    }
}
