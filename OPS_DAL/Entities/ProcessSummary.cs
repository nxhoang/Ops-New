namespace OPS_DAL.Entities
{
    /// <summary>
    /// The class to hold data from summary processes in T_SD_OPDT table
    /// </summary>
    /// Author: Nguyen Xuan Hoang
    /// <seealso cref="StyleMaster" />
    public class ProcessSummary : StyleMaster
    {
        public decimal ProcessCount { get; set; }
        public decimal OperationTime { get; set; }
        public decimal TaktTime { get; set; }
        public decimal TargetPerHour { get; set; }
        public decimal TargetPerDay { get; set; }
        public decimal MachineCount { get; set; }
        public decimal WorkerCount { get; set; }
    }
}
