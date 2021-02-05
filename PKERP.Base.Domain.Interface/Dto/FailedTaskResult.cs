namespace PKERP.Base.Domain.Interface.Dto
{
    public class SuccessTaskResult<T>: TaskResult<T>
    {
        public SuccessTaskResult(T data)
        {
            IsSuccess = true;
            Result = data;
        }
        public SuccessTaskResult()
        {
            IsSuccess = true;
        }
    }
}
