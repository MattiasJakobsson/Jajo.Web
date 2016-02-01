using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SuperGlue.UnitOfWork;

namespace SuperGlue.Localization
{
    public class LoadLocalizationsOnStartup : IApplicationTask
    {
        private readonly ILocalizeText _localizeText;

        public LoadLocalizationsOnStartup(ILocalizeText localizeText)
        {
            _localizeText = localizeText;
        }

        public Task Start(IDictionary<string, object> environment)
        {
            return _localizeText.Load();
        }

        public Task ShutDown(IDictionary<string, object> environment)
        {
            return Task.CompletedTask;
        }

        public Task Exception(IDictionary<string, object> environment, Exception exception)
        {
            return Task.CompletedTask;
        }
    }
}