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
        private string _messsage;

        public ExceptionWithCode(int errorCode, string message)
        {
            _messsage = message;
            _errorCode = errorCode;
        }

        public int ErrorCode
        {
            get { return _errorCode; }
        }

        public string Message
        {
            get
            {
                return _messsage;
            }
        }
    }
}
