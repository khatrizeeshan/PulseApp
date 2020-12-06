using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PulseApp.Data;
using PulseApp.Helpers;
using PulseApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using PulseApp.Protos;
using Grpc.Core;
using Google.Protobuf.WellKnownTypes;

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
            existing.StartDate = request.StartDate.ToDateTime();
            existing.EndDate = request.EndDate.ToDateTime();
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
                StartDate = request.StartDate.ToDateTime(),
                EndDate = request.EndDate.ToDateTime()
            };
            db.SetId(calendar);

            var calendarDays = calendar.MakeWeekends(settings.Weekends);
            db.SetId(calendarDays);

            await db.Calendars.AddAsync(calendar);
            await db.CalendarDays.AddRangeAsync(calendarDays);
            await db.SaveChangesAsync();

            return new IdResponse() { Id = calendar.Id };
        }

        public override async Task<CalendarResponse> LastCalendar(EmptyRequest request, ServerCallContext context)
        {
            using var db = DbFactory.CreateDbContext();
            var calendar = await db.Calendars
                                .OrderByDescending(c => c.StartDate)
                                .Select(CalendarDto.Selector)
                                .FirstOrDefaultAsync();

            var response = new CalendarResponse();
            response.Calendar = calendar;

            return response;
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

        public override async Task<CalendarDaysResponse> GetCalendarDays(IdRequest request, ServerCallContext context)
        {
            using var db = DbFactory.CreateDbContext();

            var calendar = await db.Calendars
                    .SingleOrDefaultAsync(a => a.Id == request.Id);

            var response = new CalendarDaysResponse();
            if(calendar == null)
            {
                return response;
            }

            var calendarDays = await db.CalendarDays
                    .Where(a => a.CalendarId == request.Id)
                    .Select(CalendarDayDto.Selector)
                    .ToArrayAsync();

            response.Fill(calendarDays, calendar.StartDate, calendar.EndDate);
            return response;
        }

        public async Task MarkDayAsync(int calendarId, int year, int month, int day, int dayTypeId, string comments = null)
        {
            var date = new DateTime(year, month, day);
            using var db = DbFactory.CreateDbContext();

            var calendarDay = await db.CalendarDays.SingleOrDefaultAsync(e => e.CalendarId == calendarId && e.Date == date);
            if (calendarDay == null)
            {
                calendarDay = new CalendarDay()
                {
                    CalendarId = calendarId,
                    Date = date,
                    DayTypeId = dayTypeId,
                    Comments = comments,
                };

                db.SetId(calendarDay);
                await db.CalendarDays.AddAsync(calendarDay);
            }
            else
            {
                calendarDay.DayTypeId = dayTypeId;
                db.CalendarDays.Update(calendarDay);
            }

            await db.SaveChangesAsync();
        }

        public async Task<int[]> GetOffDays(int year, int month)
        {
            var (start, end) = DateTimeHelper.MonthRange(year, month);

            using var db = DbFactory.CreateDbContext();
            var offDays = await db.CalendarDays
                .Where(a => a.Date >= start && a.Date <= end)
                .Select(d => d.Date.Day)
                .ToArrayAsync();

            return offDays;
        }

        public async Task<DateTime[]> GetOffDates(DateTime start, DateTime end)
        {
            using var db = DbFactory.CreateDbContext();
            var offDates = await db.CalendarDays
                .Where(a => a.Date >= start && a.Date <= end)
                .Select(d => d.Date)
                .ToArrayAsync();

            return offDates;
        }

        public async Task<int> GetCalendarIdAsync(DateTime date)
        {
            using var db = DbFactory.CreateDbContext();
            var calendarId = await db.Calendars
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
        public static Expression<Func<Calendar, CalendarProto>> Selector = e => new CalendarProto
        {
            Id = e.Id,
            StartDate = Timestamp.FromDateTime(e.StartDate),
            EndDate = Timestamp.FromDateTime(e.EndDate),
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
                var month = date.Month;
                var year = date.Year;
                var firstDay = DateTimeHelper.FirstDay(year, month);
                var proto = new CalendarMonthProto()
                {
                    Month = month,
                    MonthName = DateTimeHelper.GetMonthName(month),
                    Year = year,
                    Days = DateTime.DaysInMonth(year, month),
                };

                for (int i = 0; i <= 31; i++)
                {
                    var day = days.FirstOrDefault(a => a.Month == month && a.Day == i);
                    if (day != null)
                    {
                        proto.CalendarDays.Add(i, new CalendarDayProto()
                        {
                            DayTypeId = day.DayTypeId,
                            DayTypeCode = day.DayTypeCode,
                            Comments = day.Comments
                        });
                    }
                }

                response.Months.Add(proto);
                date = date.AddMonths(1);
            }
        }
    }
}
