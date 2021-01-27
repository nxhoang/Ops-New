namespace PKERP.Base.Domain.Interface.Dto
{
    public class FailedTaskResult<T> : TaskResult<T>
    {
        public FailedTaskResult(string message)
        {
            IsSuccess = false;
            Log = message;
        }
    }
}
