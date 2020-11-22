using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PulseApp.Data
{
    public class AttendanceType : BaseModel<int>
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public bool IsDefault { get; set; }
    }

    public class AttendanceTypeConfiguration : BaseEntityTypeConfiguration, IEntityTypeConfiguration<AttendanceType>
    {
        public void Configure(EntityTypeBuilder<AttendanceType> builder)
        {
            base.Configure(builder);
        }
    }
}