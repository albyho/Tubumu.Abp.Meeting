using Volo.Abp.Modularity;

namespace Sample
{
    [DependsOn(
        typeof(SampleApplicationModule),
        typeof(SampleDomainTestModule)
        )]
    public class SampleApplicationTestModule : AbpModule
    {

    }
}