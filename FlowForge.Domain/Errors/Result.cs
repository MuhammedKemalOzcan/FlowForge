namespace FlowForge.Domain.Errors
{
    public class Result<T>
    {
        public bool IsSuccess { get; private set; }
        public T Data { get; private set; }
        public Error Error { get; private set; }

        private readonly T _data;

        private Result(bool isSuccess, T? data, Error? error)
        {
            IsSuccess = isSuccess;
            Data = data;
            Error = error;
        }

        public T Value => IsSuccess
        ? _data
        : throw new InvalidOperationException("Cannot access Value on a failed result.");

        public static Result<T> Success(T data) => new(true, data, Error.None);

        public static Result<T> Failure(Error error) => new(false, default, error);
    }

    public class Result
    {
        public bool IsSuccess { get; private set; }
        public Error Error { get; private set; }

        private Result(bool isSuccess, Error error)
        {
            IsSuccess = isSuccess;
            Error = error;
        }

        public static Result Success() => new(true, Error.None);

        public static Result Failure(Error error) => new(false, error);
    }
}