namespace OPS_DAL.MesEntities
{
    /// <summary>
    /// Attendance employee
    /// Mapping to t_hr_attemp table
    /// </summary>
    public class AttendEmp
    {
        public int Id { get; set; }
        public string Factory { get; set; }
        public string DeptCode { get; set; }
        public string Department { get; set; }
        public string EmployeeCode { get; set; }
        public string Name { get; set; }
        public string AttDate { get; set; }
        public string CheckIn { get; set; }
        public string CheckOut { get; set; }
        public string WorkKind { get; set; }
        public string WorkShift { get; set; }
        public string WorkShiftCode { get; set; }
        public string Position { get; set; }
        public string ImageName { get; set; }
    }
}
