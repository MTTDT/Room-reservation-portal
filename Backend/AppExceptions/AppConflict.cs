namespace Backend.Exceptions
{
    public class AppConflict(string message): AppException(message, StatusCodes.Status400BadRequest, "Action not allowed")
    {
    }
}