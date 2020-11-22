using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using PulseApp.Data;
using System;
using System.Collections.Generic;

namespace PulseApp.Helpers
{
    public static class SequenceHelper
    {
        public static int GetSequence<T>(this ApplicationDbContext context)
        {
            var sequence = $"{typeof(T).Name}Id";
            SqlParameter result = new SqlParameter("@result", System.Data.SqlDbType.Int)
            {
                Direction = System.Data.ParameterDirection.Output
            };

            context.Database.ExecuteSqlRaw($"SELECT @result = (NEXT VALUE FOR {sequence})", result);

            return (int)result.Value;
        }

        public static void AddSequencesForEntities(this ModelBuilder builder)
        {
            var entities = builder.Model.GetEntityTypes();
            foreach (var entity in entities)
            {
                var type = Type.GetType(entity.Name);
                builder.HasSequence<int>($"{type.Name}Id").StartsAt(1000);
            }
        }

        public static IEnumerable<T> SetId<T>(this ApplicationDbContext context, IEnumerable<T> list) where T : BaseModel<int>
        {
            foreach(var item in list)
            {
                item.Id = context.GetSequence<T>();
            }

            return list;
        }

        public static T SetId<T>(this ApplicationDbContext context, T item) where T : BaseModel<int>
        {
            item.Id = context.GetSequence<T>();
            return item;
        }
    }
}
