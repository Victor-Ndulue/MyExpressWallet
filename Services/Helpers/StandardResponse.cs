using System.Text.Json;

namespace Services.Helpers
{
    public class StandardResponse<T>
    {
        public T Data { get; set; }
        public bool Succeeded { get; set; }
        public string Message { get; set; }
        public int StatusCode { get; set; }
        public DateTime? ExpireDate { get; set; } = DateTime.Now;

        public StandardResponse(int statusCode, bool success, string msg, T data)
        {
            Data = data;
            Succeeded = success;
            StatusCode = statusCode;
            Message = msg;
        }
        public StandardResponse()
        {
        }

        /// <summary>
        /// Application custom response message, 99 response means failed
        /// </summary>
        /// <param name="errorMessage"></param>
        /// <param name="statusCode"></param>
        /// <returns></returns>
        public static StandardResponse<T> Failed(string errorMessage, int statusCode = 400)
        {
            return new StandardResponse<T> { Succeeded = false, Message = errorMessage, StatusCode = statusCode };
        }

        /// <summary>
        /// Application custom response message, 00 means successful
        /// </summary>
        /// <param name="successMessage"></param>
        /// <param name="data"></param>
        /// <param name="statusCode"></param>
        /// <returns></returns>
        public static StandardResponse<T> Success(string successMessage, T data, int statusCode = 200)
        {
            return new StandardResponse<T> { Succeeded = true, Message = successMessage, Data = data, StatusCode = statusCode };
        }

        /// <summary>
        /// Application custom response message, 66 means third party error
        /// </summary>
        /// <param name="message"></param>
        /// <param name="data"></param>
        /// <param name="statusCode"></param>
        /// <returns></returns>
        public static StandardResponse<T> UnExpectedError(string message, T data, int statusCode = 500)
        {
            return new StandardResponse<T> { Succeeded = false, Message = message, Data = data, StatusCode = statusCode };
        }

        public override string ToString() => JsonSerializer.Serialize(this);
    }
}
