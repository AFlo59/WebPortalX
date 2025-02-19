namespace WebPortalX.Core.Common
{
    public class ServiceResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;

        public ServiceResult(bool success = false, string message = "")
        {
            Success = success;
            Message = message;
        }

        public static ServiceResult Ok(string message = "") => new ServiceResult(true, message);
        public static ServiceResult Error(string message) => new ServiceResult(false, message);
    }

    public class ServiceResult<T> : ServiceResult
    {
        public T? Data { get; set; }

        public ServiceResult(bool success = false, string message = "", T? data = default)
            : base(success, message)
        {
            Data = data;
        }

        public static ServiceResult<T> Ok(T data, string message = "") 
            => new ServiceResult<T>(true, message, data);

        public new static ServiceResult<T> Error(string message)
        {
            return new ServiceResult<T>
            {
                Success = false,
                Message = message,
                Data = default
            };
        }
    }
} 