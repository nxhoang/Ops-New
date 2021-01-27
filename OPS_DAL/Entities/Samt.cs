namespace OPS_DAL.Entities
{
    /// <summary>
    /// Refer to T_00_SAMT table
    /// </summary>
    /// Author: Son Nguyen Cao
    public class Samt
    {
        public string StyleCode { get; set; }
        public string ModuleId { get; set; }
        public string ModuleName { get; set; }
        public string Registrar { get; set; }
        public string RegistryDate { get; set; }
        public string FinalAssembly { get; set; }
        public string Confirmed { get; set; }
        public string ItemName { get; set; }
        public string PartId { get; set; }
        public string PartComment { get; set; }
        //public string SubGroup { get; set; }//ADD - SON) 8/Sep/2020
        public string MaxPartId { get; set; }
        public string HasItem { get; set; } //ADD) SON - 12 December 2019
        public string IsNew { get; set; } //ADD - SON)18/Jul/2020
        public int ItemCount { get; set; } //ADD - SON) 27/Jan/2021

    }
}
