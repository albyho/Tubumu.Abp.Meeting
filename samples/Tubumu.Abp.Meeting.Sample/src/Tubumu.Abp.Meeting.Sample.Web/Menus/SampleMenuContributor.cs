using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Tubumu.Abp.Meeting.Sample.Localization;
using Tubumu.Abp.Meeting.Sample.MultiTenancy;
using Volo.Abp.TenantManagement.Web.Navigation;
using Volo.Abp.UI.Navigation;

namespace Tubumu.Abp.Meeting.Sample.Web.Menus
{
    public class SampleMenuContributor : IMenuContributor
    {
        public async Task ConfigureMenuAsync(MenuConfigurationContext context)
        {
            if (context.Menu.Name == StandardMenus.Main)
            {
                await ConfigureMainMenuAsync(context);
            }
        }

        private async Task ConfigureMainMenuAsync(MenuConfigurationContext context)
        {
            if (!MultiTenancyConsts.IsEnabled)
            {
                var administration = context.Menu.GetAdministration();
                administration.TryRemoveMenuItem(TenantManagementMenuNames.GroupName);
            }

            var l = context.GetLocalizer<SampleResource>();

            context.Menu.Items.Insert(0, new ApplicationMenuItem(SampleMenus.Home, l["Menu:Home"], "~/"));
            context.Menu.Items.Insert(1, new ApplicationMenuItem(SampleMenus.Home, "Meeting", "~/meeting/index.html"));
        }
    }
}
