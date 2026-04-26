namespace FlowForge.Domain.Enums
{
    public enum ErrorType
    {
        None = 0,
        Validation = 1,
        NotFound = 2,
        Conflict = 3,
        LimitExceeded = 4, //429 too many requests veya 403 forbidden
        Forbidden = 5, //403 forbidden
        BadRequest = 6 //400
    }
}