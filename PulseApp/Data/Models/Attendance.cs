using System;

namespace PulseApp.Data
{
    public class Attendance : BaseModel<int>
    {
        public int EmployeeId { get; set; }
        public Employee Employee { get; set; }
        public DateTime Date { get; set; }
        public int AttendanceTypeId { get; set; }
        public AttendanceType AttendanceType { get; set; }
        public int? LeaveTypeId { get; set; }
        public LeaveType LeaveType { get; set; }
        public int Hours { get; set; }
        public string Comments { get; set; }
    }

    public class AttendanceType : BaseModel<int>
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public bool IsDefault { get; set; }
    }

    public class AttendanceConfiguration : BaseEntityTypeConfiguration<Attendance> { }
    public class AttendanceTypeConfiguration : BaseEntityTypeConfiguration<AttendanceType> { }

}
