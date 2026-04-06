namespace WebApiEvent.Models.DTOs
{
    public class ResponseServerDto<T>
    {

        public bool IsSuccess { get; set; }
        public int StatusCode { get; set; }
        public string? ErrorMessage { get; set; }
        public T? Result { get; set; }

        public static ResponseServerDto<T> Success(T data, int statusCode = 200)
        {
            return new ResponseServerDto<T>
            {
                StatusCode = statusCode,
                IsSuccess = true,
                Result = data
            };
        }
        public static ResponseServerDto<T> Error(string message, int statusCode = 400)
        {
            return new ResponseServerDto<T>
            {
                StatusCode = statusCode,
                IsSuccess = false,
                ErrorMessage = message
            };
        }
    }
}
