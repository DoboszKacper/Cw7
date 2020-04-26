using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace APBD5.Exceptions
{
    public class StudentCannotDefendException : Exception
    {
        public StudentCannotDefendException()
        {
         
        }

        public StudentCannotDefendException(string message) : base(message)
        {
        }

        public StudentCannotDefendException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected StudentCannotDefendException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
