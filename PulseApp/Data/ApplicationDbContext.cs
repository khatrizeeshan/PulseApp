using Microsoft.EntityFrameworkCore;
using PulseApp.Helpers;
using System;

namespace PulseApp.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.ApplyConfigurationsFromAssembly(typeof(BaseModel<>).Assembly);
            builder.AddSequencesForEntities();
        }

        public DbSet<Employee> Employees { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<AttendanceType> AttendanceTypes { get; set; }
        public DbSet<LeaveType> LeaveTypes { get; set; }
        public DbSet<Timing> Timings { get; set; }
        public DbSet<User> Users { get; set; }
    }
}
