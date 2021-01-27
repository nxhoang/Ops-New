using System;

namespace OPS_DAL.Entities
{
    /// <summary>
    /// Refer to T_RD_ACTL table
    /// </summary>
    /// Author: Son Nguyen Cao
    public class Actl : StyleMaster
    {
        public Actl()
        {
            
        }

        public Actl(string styleCode, string styleSize, string styleColorSerial, string revNo, string opRevNo, string userId,
            string roleId, string functionId, string operationId, string refNo, string success, DateTime transactionTime, 
            string remark, string systemId) : base(styleCode, styleSize, styleColorSerial, revNo)
        {
            OpRevNo = opRevNo;
            UserId = userId;
            RoleId = roleId;
            FunctionId = functionId;
            OperationId = operationId;
            RefNo = refNo;
            Success = success;
            TransactionTime = transactionTime;
            Remark = remark;
            SystemId = systemId;
        }

        public string OpRevNo { get; set; }
        public string UserId { get; set; }
        public string RoleId { get; set; }
        public string FunctionId { get; set; }
        public string OperationId { get; set; }
        public string RefNo { get; set; }
        public string Success { get; set; }
        public string Edition { get; set; }
        public DateTime TransactionTime { get; set; }
        public string Remark { get; set; }
        public string SystemId { get; set; }

        public string BuyerStyleCode { get; set; }
        public string Buyer { get; set; }
        public string StyleGroup { get; set; }
        public string SubGroup { get; set; }
        public string SubSubGroup { get; set; }
    }
}
