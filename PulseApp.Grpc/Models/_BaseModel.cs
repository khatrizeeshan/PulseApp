using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PulseApp.Models
{
    public class BaseModel<T>
    {
        public T Id { get; set; }
    }

    public abstract class BaseEntityTypeConfiguration<T> : IEntityTypeConfiguration<T>
        where T : BaseModel<int>
    {
        protected BaseEntityTypeConfiguration()
        {

        }

        public void Configure(EntityTypeBuilder<T> builder)
        {
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id).ValueGeneratedNever();
        }
    }
}