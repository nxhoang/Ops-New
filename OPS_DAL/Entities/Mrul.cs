namespace OPS_DAL.Entities
{
    /// <summary>
    /// Refer to T_CM_MRUL table
    /// </summary>
    /// Author: Son Nguyen Cao
    public class Mrul
    {
        public string StyleGroup { get; set; }
        public string StyleSubGroup { get; set; }
        public string StyleSubSubGroup { get; set; }
        public string ModuleLevelCode { get; set; }
        public string MachineRangeCode { get; set; }
        public string CreateRule { get; set; }

        public string LevelDesc { get; set; }
    }
}
