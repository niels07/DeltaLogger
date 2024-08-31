using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeltaLogger.Networking
{
    public class TCPException : Exception
    {
        public TCPException(string message): base(message){}
    }
}
