using System;

namespace PipelineService.Exceptions
{
    public class InvalidIdException : Exception
    {
        public InvalidIdException(string? message) : base(message)
        {
        }

        public InvalidIdException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}