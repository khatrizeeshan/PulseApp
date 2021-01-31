﻿using Microsoft.EntityFrameworkCore;
using PulseApp.Helpers;
using PulseApp.Models;
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
        public DbSet<EmployeeCalendar> EmployeeCalendars { get; set; }
        public DbSet<EmployeeLeavePolicy> EmployeeLeavePolicies { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<AttendanceType> AttendanceTypes { get; set; }
        public DbSet<Calendar> Calendars { get; set; }
        public DbSet<CalendarDay> CalendarDays { get; set; }
        public DbSet<DayType> DayTypes { get; set; }
        public DbSet<LeavePolicyType> LeavePolicyTypes { get; set; }
        public DbSet<LeavePolicy> LeavePolicies { get; set; }
        public DbSet<LeavePolicyDetail> LeavePolicyDetails { get; set; }
        public DbSet<Leave> Leaves { get; set; }
        public DbSet<LeaveType> LeaveTypes { get; set; }
        public DbSet<Timing> Timings { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Setting> Settings { get; set; }
    }
}
