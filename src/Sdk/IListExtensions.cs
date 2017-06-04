using System.Collections.Generic;
using System.ComponentModel;

namespace Moq.Sdk
{
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public static class IListExtensions
    {
        public static void Enqueue<T>(this IList<T> list, T item)
        {
            lock (list)
                list.Add(item);
        }

        public static T Dequeue<T>(this IList<T> list)
        {
            var item = default(T);
            lock (list)
            {
                if (list.Count > 0)
                {
                    item = list[0];
                    list.RemoveAt(0);
                }
            }

            return item;
        }

        public static void Push<T>(this IList<T> list, T item)
        {
            lock (list)
                list.Add(item);
        }

        public static T Pop<T>(this IList<T> list)
        {
            var item = default(T);
            lock (list)
            {
                var count = list.Count;
                if (count > 0)
                {
                    item = list[count - 1];
                    list.RemoveAt(count - 1);
                }
            }

            return item;
        }
    }
}