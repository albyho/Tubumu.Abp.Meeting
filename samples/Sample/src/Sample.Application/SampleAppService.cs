using System;
using System.Collections.Generic;
using System.Text;
using Sample.Localization;
using Volo.Abp.Application.Services;

namespace Sample
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
