namespace TCBExchangeRate.Application.Models.Responses
{
    public class Result<T>
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public T? Data { get; set; }
        public List<string>? Errors { get; set; }

        public static Result<T> Ok(T data, string? message = null)
            => new() { Success = true, Data = data, Message = message ?? "Success" };

        public static Result<T> Fail(string message, List<string>? errors = null)
            => new() { Success = false, Message = message, Errors = errors };
    }
}

