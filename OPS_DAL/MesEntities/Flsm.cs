using OPS_DAL.Entities;
using System;

namespace OPS_DAL.MesEntities
{
    /// <inheritdoc />
    /// <summary>
    /// Factory Line summary
    /// </summary>
    /// Author: Nguyen Xuan Hoang
    /// <seealso cref="T:OPS_DAL.Entities.FactoryEntity" />
    public class Flsm : FactoryEntity
    {
        public decimal? NoOfLines { get; set; }
        public decimal? TotalMachines { get; set; }
        public decimal? TotalWorkers { get; set; }
        public decimal? TotalTables { get; set; }
        public decimal? Width { get; set; }
        public decimal? Length { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
