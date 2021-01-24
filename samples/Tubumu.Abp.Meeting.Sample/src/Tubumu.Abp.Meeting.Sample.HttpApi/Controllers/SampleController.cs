using Tubumu.Abp.Meeting.Sample.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace Tubumu.Abp.Meeting.Sample.Controllers
{
    /* Inherit your controllers from this class.
     */
    public abstract class SampleController : AbpController
    {
        protected SampleController()
        {
            LocalizationResource = typeof(SampleResource);
        }
    }
}