using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

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

        public virtual void Configure(EntityTypeBuilder<T> builder)
        {
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id).ValueGeneratedNever();

            foreach (var property in typeof(T).GetProperties())
            {
                var type = property.PropertyType;
                if (type == typeof(DateTime) || type == typeof(DateTime?))
                {
                    builder.Property(property.Name).HasColumnType("Date");
                }
            }

        }
    }
}