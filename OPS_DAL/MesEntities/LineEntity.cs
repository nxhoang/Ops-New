using System;

namespace OPS_DAL.MesEntities
{
    public class LineEntity
    {
        public decimal LineSerial { get; set; }
        public string LineName { get; set; }
        public string Factory { get; set; }
        public string Description { get; set; }
        public string LineNo { get; set; }
        public int? TotalTables { get; set; }
        public long? LineMan { get; set; }
        public long Capacity { get; set; }
        public string BackgroundColor { get; set; }
        public string InUse { get; set; }

        //START ADD) SON - 22/Jul/2019
        public string LineCombination { get; set; }
        public int Workers { get; set; }
        public int MappingSeats { get; set; }
        public int ConnectedIot { get; set; }
        public string IsActive { get; set; }
        public DateTime LAST_IOT_DATA_RECEIVE_TIME { get; set; }
        
        //END ADD) SON - 19/Feb/2019
    }
}
