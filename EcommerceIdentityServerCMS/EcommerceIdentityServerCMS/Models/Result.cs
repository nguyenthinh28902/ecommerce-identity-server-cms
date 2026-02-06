namespace EcommerceIdentityServerCMS.Models
{
    public class Result<T>
    {
        public Result() { }
        public bool IsSuccess { get; }
        public string? Error { get; }
        public T? Data { get; }

        protected Result(bool isSuccess, T? data, string? error)
        {
            IsSuccess = isSuccess;
            Data = data;
            Error = error;
        }
        public static Result<T> Success(T data, string mess)
       => new(true, data, mess);

        public static Result<T> Failure(string error, bool IsSuccess = false)
            => new(IsSuccess, default, error);
    }
}
