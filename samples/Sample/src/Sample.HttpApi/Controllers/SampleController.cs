﻿using Sample.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace Sample.Controllers
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