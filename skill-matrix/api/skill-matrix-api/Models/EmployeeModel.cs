using System;

namespace skill_matrix_api.Models
{
    public class EmployeeModel
    {
        public int Id { get; set; }
        public string EmployeeId { get; set; }
        public string Name { get; set; }
        public Nullable<System.DateTime> DateOfJoining { get; set; }
        public bool IsActive { get; set; }
        public Nullable<int> DesignationId { get; set; }
        public Nullable<decimal> Experience { get; set; }
    }
}