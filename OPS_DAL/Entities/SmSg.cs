using System;


namespace OPS_DAL.Entities
{
    public partial class SmSg
    {
        //SYSTEMID, MENUID, EVENT, MESSAGETYPE, MESSAGECONTEXT, TITLE, ENGLISH, VIETNAMESE, KOREAN, INDONESIAN, MYANMAR, SMSGID
        public string ContextSerial { get; set; }
        public string SystemId { get; set; }
        public string MenuId { get; set; }
        public string Function { get; set; }
        public string MessageType { get; set; }
        public string MessageContext { get; set; }
        public string ContextDesc { get; set; }
        public string Title { get; set; }
        public string English { get; set; }
        public string Vietnamese { get; set; }
        public string Korean { get; set; }
        public string Indonesian { get; set; }
        public string Myanmar { get; set; }
        public string Amharic { get; set; }
        public decimal Status { get; set; }
        public DateTime RegistryDate { get; set; }
        public DateTime UpdateDate { get; set; }
        public string RegisterId { get; set; }
        public string UpdateId { get; set; }
    }
}
