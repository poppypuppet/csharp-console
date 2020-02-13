using System.Collections.Generic;

namespace PEngineModule.Logs
{
    public static class IEnumerableExtensions
    {
        private static IEnumerable<T> Take<T>(this IEnumerator<T> self, T current, int count)
        {
            yield return current;
            for (int i = 0; i < count - 1 && self.MoveNext(); i++)
            {
                yield return self.Current;
            }
        }
        public static IEnumerable<IEnumerable<T>> Batch<T>(this IEnumerable<T> self, int batchSize)
        {
            using (IEnumerator<T> iterator = self.GetEnumerator())
            {
                while (iterator.MoveNext())
                {
                    yield return iterator.Take(iterator.Current, batchSize);
                }
            }
        }
    }
}