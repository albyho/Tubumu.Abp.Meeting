using Sample.EntityFrameworkCore;
using Volo.Abp.Modularity;

namespace Sample
{
    [DependsOn(
        typeof(SampleEntityFrameworkCoreTestModule)
        )]
    public class SampleDomainTestModule : AbpModule
    {

    }
}