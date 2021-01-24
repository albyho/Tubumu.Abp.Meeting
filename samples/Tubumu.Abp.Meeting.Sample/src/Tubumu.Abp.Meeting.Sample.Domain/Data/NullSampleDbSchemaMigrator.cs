using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace Tubumu.Abp.Meeting.Sample.Data
{
    /* This is used if database provider does't define
     * ISampleDbSchemaMigrator implementation.
     */
    public class NullSampleDbSchemaMigrator : ISampleDbSchemaMigrator, ITransientDependency
    {
        public Task MigrateAsync()
        {
            return Task.CompletedTask;
        }
    }
}