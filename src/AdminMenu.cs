using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.Localization;
using Orchard.Security;
using Orchard.UI.Navigation;

namespace Apper.Aws
{
    public class AdminMenu : INavigationProvider
    {
        public Localizer T { get; set; }
        public string MenuName { get { return "admin"; } }

        public void GetNavigation(NavigationBuilder builder)
        {
            builder.Add(T("Aws"), "98",
                    menu => menu.Add(T("Aws"), "1", item => item.Action("Index", "Admin", new { area = "Apper.Aws" })
                            .Permission(StandardPermissions.SiteOwner)));
        }
    }
}