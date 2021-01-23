using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PulseApp.Data;
using PulseApp.Helpers;
using PulseApp.Models;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using PulseApp.Protos;
using Grpc.Core;

namespace PulseApp.Services
{
    public class CalendarService : CalendarManager.CalendarManagerBase
    {
        public CalendarService(IServiceProvider provider, IDbContextFactory<ApplicationDbContext> dbFactory)
        {
            this.Provider = provider;
            this.DbFactory = dbFactory;
        }

        public IDbContextFactory<ApplicationDbContext> DbFactory { get; set; }

        public IServiceProvider Provider { get; set; }

        public override async Task<CalendarsResponse> GetCalendars(EmptyRequest request, ServerCallContext context)
        {
            using var db = DbFactory.CreateDbContext();

            var calendars = await db.Calendars
                        .Select(CalendarDto.Selector)
                        .ToArrayAsync();

            var response = new CalendarsResponse();
            response.Calendars.Add(calendars);

            return response;
        }

        public override async Task<CalendarResponse> GetCalendar(IdRequest request, ServerCallContext context)
        {
            using var db = DbFactory.CreateDbContext();
            var calendar = await db.Calendars
                    .Select(CalendarDto.Selector)
                    .SingleOrDefaultAsync(e => e.Id == request.Id);

            var response = new CalendarResponse();
            response.Calendar = calendar;

            return response;
        }

        public override async Task<EmptyResponse> DeleteCalendar(IdRequest request, ServerCallContext context)
        {
            using var db = DbFactory.CreateDbContext();
            db.Calendars.Remove(new Calendar() { Id = request.Id });
            await db.SaveChangesAsync();

            return new EmptyResponse();
        }

        public override async Task<EmptyResponse> UpdateCalendar(CalendarUpdateRequest request, ServerCallContext context)
        {
            using var db = DbFactory.CreateDbContext();
            var existing = await db.Calendars.SingleOrDefaultAsync(e => e.Id == request.Id);
            existing.Name = request.Name;
            existing.Weekends = request.Weekends; 
            db.Calendars.Update(existing);
            await db.SaveChangesAsync();

            return new EmptyResponse();
        }

        public override async Task<IdResponse> AddCalendar(CalendarAddRequest request, ServerCallContext context)
        {
            using var db = DbFactory.CreateDbContext();
            var settings = Provider.GetRequiredService<SettingService>();

            var calendar = new Calendar
            {
                Name = request.Name,
                StartDate = request.StartDate.ToDateTime(),
                Weekends = request.Weekends,
            };
            db.SetId(calendar);

            var calendarDays = calendar.MakeWeekends(calendar.StartDate.Year, calendar.StartDate.Month);
            db.SetId(calendarDays);

            await db.Calendars.AddAsync(calendar);
            await db.CalendarDays.AddRangeAsync(calendarDays);
            await db.SaveChangesAsync();

            return new IdResponse() { Id = calendar.Id };
        }

        public override async Task<DayTypesResponse> GetDayTypes(EmptyRequest request, ServerCallContext context)
        {
            using var db = DbFactory.CreateDbContext();

            var dayTypes = await db.DayTypes
                .Select(DayTypeDto.Selector)
                .ToArrayAsync();

            var response = new DayTypesResponse();
            response.DayTypes.Add(dayTypes);

            return response;
        }

        public override async Task<CalendarDaysResponse> GetCalendarDays(IdDateRequest request, ServerCallContext context)
        {
            using var db = DbFactory.CreateDbContext();

            var calendar = await db.Calendars
                    .SingleOrDefaultAsync(a => a.Id == request.Id);

            var response = new CalendarDaysResponse();
            if(calendar == null)
            {
                return response;
            }

            var yearStartDate = GetYearStartDate(calendar, request.Date.ToDateTime());

            if(yearStartDate > DateTime.Today.AddYears(3)) //TODO: Time Zone Handling
            {
                throw new Exception("Calendar is only available for three future years.");
            }

            var (from, to) = DateTimeHelper.YearRange(yearStartDate.Year, yearStartDate.Month);

            var count = await db.CalendarDays
                    .Where(a => a.CalendarId == request.Id && a.Date >= from && a.Date <= to)
                    .CountAsync();

            if(count == 0 && calendar.Weekends.Contains('1'))
            {
                var days = calendar.MakeWeekends(yearStartDate.Year, yearStartDate.Month);
                db.SetId(days);
                await db.CalendarDays.AddRangeAsync(days);
                await db.SaveChangesAsync();
            }

            var calendarDays = await db.CalendarDays
                .Where(a => a.CalendarId == request.Id && a.Date >= from && a.Date <= to)
                .Select(CalendarDayDto.Selector)
                .ToArrayAsync();

            response.Fill(calendarDays, from, to);
            return response;
        }

        private DateTime GetYearStartDate(Calendar calendar, DateTime date)
        {
            var year = calendar.StartDate.Month >= date.Month ? date.Year : date.Year - 1;
            return new DateTime(year, calendar.StartDate.Month, 1);
        }

        public override async Task<EmptyResponse> MarkDay(MarkDayRequest request, ServerCallContext context)
        {
            var date = new DateTime(request.Year, request.Month, request.Day);
            using var db = DbFactory.CreateDbContext();

            var calendarDay = await db.CalendarDays.SingleOrDefaultAsync(e => e.CalendarId == request.CalendarId && e.Date == date);
            if (calendarDay == null)
            {
                calendarDay = new CalendarDay()
                {
                    CalendarId = request.CalendarId,
                    Date = date,
                    DayTypeId = request.DayTypeId,
                    Comments = request.Comments,
                };

                db.SetId(calendarDay);
                await db.CalendarDays.AddAsync(calendarDay);
            }
            else
            {
                calendarDay.DayTypeId = request.DayTypeId;
                calendarDay.Comments = request.Comments;
                db.CalendarDays.Update(calendarDay);
            }

            await db.SaveChangesAsync();

            return new EmptyResponse();
        }

        public override async Task<DaysResponse> GetOffDays(YearMonthRequest request, ServerCallContext context)
        {
            var (start, end) = DateTimeHelper.MonthRange(request.Year, request.Month);

            using var db = DbFactory.CreateDbContext();
            var offDays = await db.CalendarDays
                .Where(a => a.Date >= start && a.Date <= end)
                .Select(d => d.Date.Day)
                .ToArrayAsync();

            var response = new DaysResponse();
            response.Days.Add(offDays);
            return response;
        }

        internal async Task<DateTime[]> GetOffDates(DateTime start, DateTime end)
        {
            using var db = DbFactory.CreateDbContext();
            return await db.CalendarDays
                .Where(a => a.Date >= start && a.Date <= end)
                .Select(d => d.Date)
                .ToArrayAsync();
        }

        public override async Task<DatesResponse> GetOffDates(DateRangeRequest request, ServerCallContext context)
        {
            var response = new DatesResponse();
            var offDates = await GetOffDates(request.StartDate.ToDateTime(), request.EndDate.ToDateTime());
            response.Dates.Add(offDates.Select(d => d.ToDate()));
            return response;
        }

    }

    public class CalendarDto
    {
        public static Expression<Func<Calendar, CalendarProto>> Selector = e => new CalendarProto
        {
            Id = e.Id,
            Name = e.Name,
            StartDate = e.StartDate.ToDate(),
            Weekends = e.Weekends,
        };
    }

    public class DayTypeDto
    {
        public static Expression<Func<DayType, DayTypeProto>> Selector = e => new DayTypeProto
        {
            Id = e.Id,
            Code = e.Code,
            Name = e.Name,
        };
    }


    public class CalendarDayDto
    {
        public int CalendarId { get; set; }

        public int Day { get; set; }

        public int Month { get; set; }

        public int Year { get; set; }

        public int DayTypeId { get; set; }

        public string DayTypeCode { get; set; }

        public string Comments { get; set; }

        public static Expression<Func<CalendarDay, CalendarDayDto>> Selector = e => new CalendarDayDto
        {
            CalendarId = e.CalendarId,
            Day = e.Date.Day,
            Month = e.Date.Month,
            Year = e.Date.Year,
            DayTypeId = e.DayTypeId,
            DayTypeCode = e.DayType.Code,
            Comments = e.Comments,
        };
    }

    public static class CalendarExtensions
    {
        public static void Fill(this CalendarDaysResponse response, CalendarDayDto[] days, DateTime start, DateTime end)
        {
            var date = start;
            while (date <= end)
            {
                var firstDay = DateTimeHelper.FirstDay(date.Year, date.Month);
                var month = new CalendarMonthProto()
                {
                    Month = date.Month,
                    MonthName = DateTimeHelper.GetMonthName(date.Month),
                    Year = date.Year,
                    Days = DateTime.DaysInMonth(date.Year, date.Month),
                };

                for (int i = 0; i <= 31; i++)
                {
                    var day = days.FirstOrDefault(a => a.Year == date.Year && a.Month == date.Month && a.Day == i);
                    if (day != null)
                    {
                        var proto = new CalendarDayProto()
                        {
                            DayTypeId = day.DayTypeId,
                            DayTypeCode = day.DayTypeCode,
                        };

                        if(day.Comments != null) {
                            proto.Comments = day.Comments;
                        }

                        month.CalendarDays.Add(i, proto);
                    }
                }

                response.Months.Add(month);
                date = date.AddMonths(1);
            }
        }
    }
}
