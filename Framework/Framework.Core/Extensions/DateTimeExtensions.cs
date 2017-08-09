using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Framework.Core
{
    public static class DateTimeExtensions
    {
        public static long ToUnixTimeSeconds(this DateTime datetime)
        {
            DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            return Convert.ToInt64((datetime.ToUniversalTime() - epoch).TotalSeconds);
        }

        public static long? ToUnixTimeSeconds(this DateTime? datetime)
        {
            if (datetime == null)
            {
                return null;
            }

            return datetime.Value.ToUnixTimeSeconds();
        }
    }
}
