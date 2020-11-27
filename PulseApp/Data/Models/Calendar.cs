using System;

namespace PulseApp.Data
{
    public class Calendar : BaseModel<int>
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class CalendarDay : BaseModel<int>
    {
        public int CalendarId { get; set; }
        public Calendar Calendar { get; set; }
        public DateTime Day { get; set; }
        public DayType Type { get; set; }
    }

    public class DayType : BaseModel<int>
    {
        public string Name { get; set; }
    }

    public class CalendarConfiguration : BaseEntityTypeConfiguration<Calendar> { }

    public class CalendarDayConfiguration : BaseEntityTypeConfiguration<CalendarDay> { }

    public class DayTypeConfiguration : BaseEntityTypeConfiguration<DayType> { }

}
