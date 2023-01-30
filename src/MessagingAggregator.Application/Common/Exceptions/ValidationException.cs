namespace MessagingAggregator.Application.Common.Exceptions;

[Serializable]
public class ValidationException : Exception
{
    public ValidationException(string message) : base(message)
    {
    }

    public ValidationException(string message, Exception inner) : base(message, inner)
    {
    }
}
