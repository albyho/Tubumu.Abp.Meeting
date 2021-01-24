using System;
using System.Collections.Generic;
using System.Text;
using Tubumu.Abp.Meeting.Sample.Localization;
using Volo.Abp.Application.Services;

namespace Tubumu.Abp.Meeting.Sample
{
    /* Inherit your application services from this class.
     */
    public abstract class SampleAppService : ApplicationService
    {
        protected SampleAppService()
        {
            LocalizationResource = typeof(SampleResource);
        }
    }
}
