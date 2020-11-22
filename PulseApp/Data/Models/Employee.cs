using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;

namespace PulseApp.Data
{
    public class Employee : BaseModel<int>
    {
        public int? UserId { get; set; }
        public User User { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public DateTime Joining { get; set; }
        public ICollection<Timing> Timings { get; set; }
        public ICollection<Attendance> Attendances { get; set; }
    }

    public class EmployeeConfiguration : BaseEntityTypeConfiguration, IEntityTypeConfiguration<Employee>
    {
        public void Configure(EntityTypeBuilder<Employee> builder)
        {
            base.Configure(builder);
        }
    }
}
