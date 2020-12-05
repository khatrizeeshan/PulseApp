using Microsoft.EntityFrameworkCore;
using PulseApp.Data;
using PulseApp.Models;
using System.Linq;

namespace PulseApp.Services
{
    public class SettingService
    {

        public SettingService(IDbContextFactory<ApplicationDbContext> dbFactory)
        {
            this.DbFactory = dbFactory;
        }

        public IDbContextFactory<ApplicationDbContext> DbFactory { get; set; }

        private Setting _Setting;
        private Setting Setting 
        { 
            get 
            {
                if(_Setting == null)
                {
                    using var context = DbFactory.CreateDbContext();
                    _Setting = context.Settings.Single();
                }

                return _Setting;
            } 
        }

        public string Weekends { get { return Setting.Weekends; } }
    }
}
