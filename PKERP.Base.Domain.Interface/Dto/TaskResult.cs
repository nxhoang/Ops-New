namespace PKERP.Base.Domain.Interface.Dto
{
    public class TaskResult<T>
    {
        public bool IsSuccess { get; set; }
        public int Code { get; set; }
        public string Log { get; set; }
        public T Result { get; set; }
        public T Data => Result;
    }
}
