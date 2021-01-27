using System;

namespace OPS_DAL.Entities
{
    /// <summary>
    /// table t_Sd_dorm
    /// </summary>
    /// Author: VitHV
    /// <seealso cref="Buyer" />
    public class Dorm : Buyer
    {
        public decimal Total { get; set; }
        public string StyleColorWays { get; set; }
        public string RegistryDate{ get; set; }
        public string Register { get; set; }
        public string AdConfirm { get; set; }
        public string AdDevSale { get; set; }
        public string StaTus { get; set; }
        public string BuyerStyleCode { get; set; }
        public string BuyerStyleName { get; set; }
        public string RegisterName { get; set; }
        public string Have { get; set; }

        public string Buyer { get; set; }
        public string StyleGroup { get; set; }
        public string SubGroup { get; set; }
        public string SubSubGroup { get; set; }

        //public string STYLECODE { get; set; }
        //public string STYLESIZE { get; set; }
        //public string STYLECOLORSERIAL { get; set; }
        //public string REVNO { get; set; }
        //public string REGISTER { get; set; }
        //public string REGISTRYDATE { get; set; }
        public DateTime? PD_BOM_SALES { get; set; }
        public DateTime? PD_BOM_DESIGN { get; set; }
        public DateTime? PD_BOM_IM { get; set; }
        public DateTime? PD_DESIGN { get; set; }
        public DateTime? PD_QTYASS { get; set; }
        public DateTime? PD_OPLAN_LEAN { get; set; }
        public DateTime? PD_OPLAN_PROD { get; set; }
        public DateTime? PD_TORDER { get; set; }
        public DateTime? PD_PRECOST { get; set; }
        public DateTime? AD_BOM_SALES { get; set; }
        public DateTime? AD_BOM_DESIGN { get; set; }
        public DateTime? AD_BOM_IM { get; set; }
        public DateTime? AD_DESIGN { get; set; }
        public DateTime? AD_QTYASS { get; set; }
        public DateTime? AD_OPLAN_LEAN { get; set; }
        public DateTime? AD_OPLAN_PROD { get; set; }
        public DateTime? AD_TORDER { get; set; }
        public DateTime? AD_PRECOST { get; set; }
        public string FACTORY { get; set; }
        public string REMARKS { get; set; }
        public DateTime? PD_DEV_MD { get; set; }
        public DateTime? PD_DEV_SALES { get; set; }
        public DateTime? PD_BOM_QTYASS { get; set; }
        public DateTime? PD_PT_QTYASS { get; set; }
        public DateTime? PD_CONFIRM { get; set; }
        public DateTime? AD_DEV_MD { get; set; }
        public DateTime? AD_DEV_SALES { get; set; }
        public DateTime? AD_BOM_QTYASS { get; set; }
        public DateTime? AD_PT_QTYASS { get; set; }
        public DateTime? AD_CONFIRM { get; set; }
        public int? LABORPROC { get; set; }
        public decimal? LABORCOST { get; set; }
        public string FINALREVCHK { get; set; }
        public string USE { get; set; }
        public string FRONTVIEW { get; set; }
        public string SIDEVIEW { get; set; }
        public string DESCRIPTION1 { get; set; }
        public string DESCRIPTION2 { get; set; }
        public string DESCRIPTION3 { get; set; }
        public string BOMFILE { get; set; }
        public string FILEDESCRIPTION1 { get; set; }
        public string FILEDESCRIPTION2 { get; set; }
        public string FILEDESCRIPTION3 { get; set; }
        public string SEASONCODE { get; set; }
        public string COLLECNAME { get; set; }
        public string SAMPLESTAGE { get; set; }
        public string MODEL { get; set; }
        public string PATTERN { get; set; }
        public string FUNCTIONS { get; set; }
        public string FUN_ATCH { get; set; }
        public string PICTURE { get; set; }
        public string PDMFILE { get; set; }
        public string DWG_FILE { get; set; }
        public string MDL_FILE { get; set; }
        public string PLX_FILE { get; set; }
        public string PDF_LAYOUT { get; set; }
        public string IGX_FILE { get; set; }
        public string PDF_SOP { get; set; }
        public string CDR_FILE { get; set; }
        public string DXF_FILE { get; set; }
        public string GBR_FILE { get; set; }
        public string MINI_MARKER { get; set; }
        public string CUTTING_FILE { get; set; }
        public string SPEC_FILE { get; set; }
        public string FILE_PLAN { get; set; }
        public string TRIM_FILE { get; set; }
        public decimal? DIFFICULTY { get; set; }
        public decimal? UNITPRODUCTIVITY { get; set; }
        public decimal? LEARNINGCURVE { get; set; }
        public decimal? OFFEROPPRICE { get; set; }
        public decimal? TARGETOPPRICE { get; set; }
        
    }
}
