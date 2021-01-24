using System.Threading.Tasks;

namespace Tubumu.Abp.Meeting.Sample.Data
{
    public interface ISampleDbSchemaMigrator
    {
        Task MigrateAsync();
    }
}
