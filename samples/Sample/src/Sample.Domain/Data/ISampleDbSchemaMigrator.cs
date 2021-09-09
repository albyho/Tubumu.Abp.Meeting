using System.Threading.Tasks;

namespace Sample.Data
{
    public interface ISampleDbSchemaMigrator
    {
        Task MigrateAsync();
    }
}
