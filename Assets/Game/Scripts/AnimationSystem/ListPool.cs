using System.Collections.Generic;

namespace HuaHaiLiKanHua
{
    public static class ListPool<T>
    {
        private static readonly Stack<List<T>> pool = new();

        public static List<T> Get()
        {
            return pool.Count > 0 ? pool.Pop() : new List<T>();
        }

        public static void Release(List<T> list)
        {
            list.Clear();
            pool.Push(list);
        }
    }

}
