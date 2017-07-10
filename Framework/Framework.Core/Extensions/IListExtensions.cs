using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Framework.Core
{
    public static class IListExtensions
    {
        public static void Foreach<T>(this IList<T> list, Action<T> func)
        {
            if (list == null || !list.Any())
            {
                return;
            }

            foreach (var item in list)
            {
                func(item);
            }
        }
    }
}
