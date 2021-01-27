namespace OPS_DAL.Entities
{
    public class OpMaster : StyleMaster
    {
        public string OpRevNo { get; set; }
    }

    public class OpdtKey : OpMaster
    {
        public string OpSerial { get; set; }
    }
}
