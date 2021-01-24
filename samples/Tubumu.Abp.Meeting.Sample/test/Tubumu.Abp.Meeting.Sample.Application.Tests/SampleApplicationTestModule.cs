using Volo.Abp.Modularity;

namespace Tubumu.Abp.Meeting.Sample
{
    [DependsOn(
        typeof(SampleApplicationModule),
        typeof(SampleDomainTestModule)
        )]
    public class SampleApplicationTestModule : AbpModule
    {

    }
}