using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PulseApp.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace PulseApp.Data
{
    public class CalendarService
    {

        public CalendarService(IServiceProvider provider, IDbContextFactory<ApplicationDbContext> dbFactory)
        {
            this.Provider = provider;
            this.DbFactory = dbFactory;
        }

        public IDbContextFactory<ApplicationDbContext> DbFactory { get; set; }

        public IServiceProvider Provider { get; set; }

        public async Task<CalendarDto[]> GetCalendarsAsync()
        {
            using var context = DbFactory.CreateDbContext();
            return await context.Calendars
                    .Select(CalendarDto.Selector)
                    .ToArrayAsync();
        }

        public async Task<CalendarDto> GetCalendarAsync(int id)
        {
            using var context = DbFactory.CreateDbContext();
            return await context.Calendars
                .Select(CalendarDto.Selector)
                .SingleOrDefaultAsync(e => e.Id == id);
        }

        public async Task DeleteAsync(int id)
        {
            using var context = DbFactory.CreateDbContext();
            context.Calendars.Remove(new Calendar() { Id = id });
            await context.SaveChangesAsync();
        }

        public async Task UpdateAsync(int id, CalendarDto dto)
        {
            using var context = DbFactory.CreateDbContext();
            var existing = await context.Calendars.SingleOrDefaultAsync(e => e.Id == id);
            existing.Fill(dto);
            context.Calendars.Update(existing);
            await context.SaveChangesAsync();
        }

        public async Task<int> AddAsync(CalendarDto dto)
        {
            using var context = DbFactory.CreateDbContext();
            var settings = Provider.GetRequiredService<SettingService>();

            var calendar = new Calendar();
            calendar.Fill(dto);
            context.SetId(calendar);

            var calendarDays = calendar.MakeWeekends(settings.Weekends);
            context.SetId(calendarDays);

            await context.Calendars.AddAsync(calendar);
            await context.CalendarDays.AddRangeAsync(calendarDays);
            await context.SaveChangesAsync();

            return calendar.Id;
        }

        public async Task<int> GetLastIdAsync()
        {
            using var context = DbFactory.CreateDbContext();
            return await context.Calendars.OrderByDescending(c => c.StartDate)
                .Select(c => c.Id).FirstOrDefaultAsync();
        }

        public async Task<DayType[]> GetDayTypesAsync()
        {
            using var context = DbFactory.CreateDbContext();

            return await context.DayTypes
                .ToArrayAsync();
        }

        public async Task<CalendayDayMonthDto[]> GetCalendarDaysAsync(int calendarId)
        {
            using var context = DbFactory.CreateDbContext();

            var calendar = await context.Calendars
                    .SingleOrDefaultAsync(a => a.Id == calendarId);

            if(calendar == null)
            {
                return Array.Empty<CalendayDayMonthDto>();
            }

            var calendarDays = await context.CalendarDays
                    .Where(a => a.CalendarId == calendarId)
                    .Select(CalendarDayDto.Selector)
                    .ToArrayAsync();

            var result = new List<CalendayDayMonthDto>();
            result.Fill(calendarDays, calendar.StartDate, calendar.EndDate);
            return result.ToArray();
        }

        public async Task MarkDayAsync(int calendarId, int year, int month, int day, int dayTypeId, string comments = null)
        {
            var date = new DateTime(year, month, day);
            using var context = DbFactory.CreateDbContext();

            var calendarDay = await context.CalendarDays.SingleOrDefaultAsync(e => e.CalendarId == calendarId && e.Date == date);
            if (calendarDay == null)
            {
                calendarDay = new CalendarDay()
                {
                    CalendarId = calendarId,
                    Date = date,
                    DayTypeId = dayTypeId,
                    Comments = comments,
                };

                context.SetId(calendarDay);
                await context.CalendarDays.AddAsync(calendarDay);
            }
            else
            {
                calendarDay.DayTypeId = dayTypeId;
                context.CalendarDays.Update(calendarDay);
            }

            await context.SaveChangesAsync();
        }

        public async Task<int[]> GetOffDays(int year, int month)
        {
            var (start, end) = DateTimeHelper.MonthRange(year, month);

            using var context = DbFactory.CreateDbContext();
            var offDays = await context.CalendarDays
                .Where(a => a.Date >= start && a.Date <= end)
                .Select(d => d.Date.Day)
                .ToArrayAsync();

            return offDays;
        }

        public async Task<DateTime[]> GetOffDates(DateTime start, DateTime end)
        {
            using var context = DbFactory.CreateDbContext();
            var offDates = await context.CalendarDays
                .Where(a => a.Date >= start && a.Date <= end)
                .Select(d => d.Date)
                .ToArrayAsync();

            return offDates;
        }

        public async Task<int> GetCalendarIdAsync(DateTime date)
        {
            using var context = DbFactory.CreateDbContext();
            var calendarId = await context.Calendars
                    .Where(a => date >= a.StartDate && date <= a.EndDate)
                    .Select(c => c.Id)
                    .SingleOrDefaultAsync();

            if (calendarId == 0)
            {
                throw new Exception("No calendar found for selected date.");
            }

            return calendarId;
        }
    }

    public class CalendarDto
    {
        public int Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public static Expression<Func<Calendar, CalendarDto>> Selector = e => new CalendarDto
        {
            Id = e.Id,
            StartDate = e.StartDate,
            EndDate = e.EndDate,
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

    public class CalendayDayMonthDto
    {
        public int Month { get; set; }
        public string MonthName { get; set; }
        public int Year { get; set; }
        public int Days { get; set; }
        public Dictionary<int, CalendarDayDetailDto> CalendarDays { get; set; }
    }

    public class CalendarDayDetailDto
    {
        public int DayTypeId { get; set; }
        public string DayTypeCode { get; set; }
        public string Comments { get; set; }
    }

    public static class CalendarExtensions
    {
        public static void Fill(this Calendar calendar, CalendarDto dto)
        {
            calendar.StartDate = dto.StartDate;
            calendar.EndDate = dto.EndDate;
        }

        public static string ToRangeString(this CalendarDto dto)
        {
            return $"{dto.StartDate.ToShortDateString()} - {dto.EndDate.ToShortDateString()}";
        }

        public static void Fill(this List<CalendayDayMonthDto> destination, CalendarDayDto[] days, DateTime start, DateTime end)
        {
            var date = start;
            while (date <= end)
            {
                var month = date.Month;
                var year = date.Year;
                var firstDay = DateTimeHelper.FirstDay(year, month);
                var dto = new CalendayDayMonthDto()
                {
                    Month = month,
                    MonthName = DateTimeHelper.GetMonthName(month),
                    Year = year,
                    Days = DateTime.DaysInMonth(year, month),
                    CalendarDays = new Dictionary<int, CalendarDayDetailDto>(),
                };

                for (int i = 0; i <= 31; i++)
                {
                    var day = days.FirstOrDefault(a => a.Month == month && a.Day == i);
                    if (day != null)
                    {
                        dto.CalendarDays.Add(i, new CalendarDayDetailDto()
                        {
                            DayTypeId = day.DayTypeId,
                            DayTypeCode = day.DayTypeCode,
                            Comments = day.Comments
                        });
                    }
                }

                destination.Add(dto);
                date = date.AddMonths(1);
            }
        }
    }
}
