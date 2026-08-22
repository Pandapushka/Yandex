using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApiEvent.Domain.Exceptions
{
    public class BookingLimitExceededException : Exception
    {
        public BookingLimitExceededException(int limit)
            : base($"Превышен лимит активных броней. Максимум: {limit}") { }
    }
}
