namespace Backend.Exceptions
{
    public class AppValidationException(string message): AppException(message, StatusCodes.Status400BadRequest, "Bad request")
    {
    }
}