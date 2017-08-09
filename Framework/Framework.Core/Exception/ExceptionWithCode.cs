using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Framework.Core
{
    public class ExceptionWithCode : Exception
    {
        private int _errorCode;

        public ExceptionWithCode(int errorCode, string message)
        {
            _errorCode = errorCode;
        }

        public ExceptionWithCode(int errorCode, string message, Exception innertException) : base(message, innertException)
        {
            _errorCode = errorCode;
        }

        public int ErrorCode
        {
            get { return _errorCode; }
        }
    }
}
