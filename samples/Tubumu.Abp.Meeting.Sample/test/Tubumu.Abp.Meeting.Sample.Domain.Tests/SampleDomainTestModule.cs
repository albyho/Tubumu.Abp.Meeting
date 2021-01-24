using Tubumu.Abp.Meeting.Sample.EntityFrameworkCore;
using Volo.Abp.Modularity;

namespace Tubumu.Abp.Meeting.Sample
{
    [DependsOn(
        typeof(SampleEntityFrameworkCoreTestModule)
        )]
    public class SampleDomainTestModule : AbpModule
    {

    }
}