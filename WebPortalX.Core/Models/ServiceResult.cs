namespace WebPortalX.Core.Models
{
    public class ServiceResult<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public string? Token { get; set; }

        public static ServiceResult<T> Ok(T data, string message = "")
        {
            return new ServiceResult<T>
            {
                Success = true,
                Data = data,
                Message = message
            };
        }

        public static ServiceResult<T> Error(string message)
        {
            return new ServiceResult<T>
            {
                Success = false,
                Message = message
            };
        }

        public static ServiceResult<T> SuccessResult(T data, string? token = null)
        {
            return new ServiceResult<T>
            {
                Success = true,
                Data = data,
                Token = token
            };
        }

        public static ServiceResult<T> ErrorResult(string message)
        {
            return new ServiceResult<T>
            {
                Success = false,
                Message = message
            };
        }
    }

    // Version non générique pour les opérations simples
    public class ServiceResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;

        public ServiceResult(bool success, string message = "")
        {
            Success = success;
            Message = message;
        }

        public static ServiceResult Ok(string message = "")
        {
            return new ServiceResult(true, message);
        }

        public static ServiceResult Error(string message)
        {
            return new ServiceResult(false, message);
        }
    }
} 