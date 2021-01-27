using System;

namespace OPS_DAL.Entities
{
    public class Errl
    {
        public string UserId { get; set; }
        public string ScreenId { get; set; }
        public string EventId { get; set; }
        public string ErrorDesc { get; set; }
        public DateTime ErrorTime { get; set; }
        public string InnerExceptionType { get; set; }
        public string InnerExceptionMessage { get; set; }
        public string InnerExceptionSource { get; set; }
        public string InnerExceptionStackTrace { get; set; }
        public string SystemId { get; set; }
    }
}
