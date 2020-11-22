using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PulseApp.Data
{
    public class BaseModel<T>
    {
        public T Id { get; set; }
    }

    public abstract class BaseEntityTypeConfiguration
    {
        protected void Configure<T>(EntityTypeBuilder<T> builder) where T : BaseModel<int>
        {
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id).ValueGeneratedNever();
        }
    }
}