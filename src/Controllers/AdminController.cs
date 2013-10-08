using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Apper.Aws.Services;
using Apper.Aws.ViewModels;
using Orchard;
using Orchard.Localization;
using Orchard.UI.Notify;

namespace Apper.Aws.Controllers
{
    [ValidateInput(false)]
    public class AdminController : Controller
    {
        private readonly IAwsSettingService _settingsService;

        public AdminController(
            IOrchardServices services,
            IAwsSettingService settingsService)
        {
            _settingsService = settingsService;
            Services = services;
            T = NullLocalizer.Instance;
        }

        public IOrchardServices Services { get; set; }
        public Localizer T { get; set; }

        public ActionResult Index()
        {
            var settings = _settingsService.GetAwsSettings();

            var viewModel = new AwsSettingsViewModel
            {
                AccessKey = settings.AccessKey,
                SecretKey = settings.SecretKey,
                FileBucket =settings.FileBucket,
                LoggingBucket = settings.LoggingBucket
            };

            return View(viewModel);
        }

        [HttpPost]
        public ActionResult Index(AwsSettingsViewModel viewModel)
        {
            var settings = _settingsService.GetAwsSettings();

            settings.AccessKey=viewModel.AccessKey;
            settings.SecretKey=viewModel.SecretKey;
            settings.FileBucket=viewModel.FileBucket;
            settings.LoggingBucket=viewModel.LoggingBucket;

            Services.Notifier.Information(T("Your settings have been saved."));

            return View(viewModel);
        }
    }
}
