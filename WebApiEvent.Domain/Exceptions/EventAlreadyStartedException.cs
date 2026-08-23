using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApiEvent.Domain.Exceptions
{
    public class EventAlreadyStartedException : Exception
    {
        public EventAlreadyStartedException(string message) : base(message) { }
    }
}
