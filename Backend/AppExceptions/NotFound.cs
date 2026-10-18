namespace Backend.Exceptions
{
    public class NotFoundException(string message): AppException(message, StatusCodes.Status404NotFound, "Resource not found")
    {
        public static NotFoundException For<T>(object id) => new($"{typeof(T).Name} with id '{id}' was not found.");
    }
}