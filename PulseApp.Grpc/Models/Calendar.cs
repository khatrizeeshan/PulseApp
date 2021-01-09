using System;

namespace PulseApp.Models
{
    public class Calendar : BaseModel<int>
    {
        public string Name { get; set; }
        public string Weekends { get; set; }
        public DateTime StartDate { get; set; }
    }

    public class CalendarDay : BaseModel<int>
    {
        public int CalendarId { get; set; }
        public Calendar Calendar { get; set; }
        public DateTime Date { get; set; }
        public int DayTypeId { get; set; }
        public DayType DayType { get; set; }
        public string Comments { get; set; }
    }

    public class DayType : BaseModel<int>
    {
        public string Code { get; set; }
        public string Name { get; set; }
    }

    public class CalendarTypeConfiguration : BaseEntityTypeConfiguration<LeavePolicyType> { }

    public class CalendarConfiguration : BaseEntityTypeConfiguration<Calendar> { }

    public class CalendarDayConfiguration : BaseEntityTypeConfiguration<CalendarDay> { }

    public class DayTypeConfiguration : BaseEntityTypeConfiguration<DayType> { }

}
