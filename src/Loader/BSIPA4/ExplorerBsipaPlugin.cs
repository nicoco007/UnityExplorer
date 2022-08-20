#if BSIPA4
using IPA;
using IPA.Utilities;
using UnityExplorer.Config;

namespace UnityExplorer.Loader.BSIPA4
{
    [Plugin(RuntimeOptions.SingleStartInit)]
    public class ExplorerBsipaPlugin : IExplorerLoader
    {
        private IPA.Logging.Logger _logger;
        private ConfigHandler _configHandler;

        [Init]
        public ExplorerBsipaPlugin(IPA.Logging.Logger logger)
        {
            _logger = logger;
        }

        public ConfigHandler ConfigHandler => _configHandler;

        public Action<object> OnLogMessage => HandleMessage;

        public Action<object> OnLogWarning => HandleWarning;

        public Action<object> OnLogError => HandleError;

        public string ExplorerFolderDestination => UnityGame.UserDataPath;

        public string ExplorerFolderName => ExplorerCore.DEFAULT_EXPLORER_FOLDER_NAME;

        public string UnhollowedModulesFolder => string.Empty;

        private void HandleMessage(object message) => HandleLog(message, _logger.Info);

        private void HandleWarning(object message) => HandleLog(message, _logger.Warn);

        private void HandleError(object message) => HandleLog(message, _logger.Error);

        private void HandleLog(object message, Action<string> method)
        {
            method(message?.ToString());
        }

        [OnEnable]
        public void OnEnable()
        {
            _configHandler = new BsipaConfigHandler();
            ExplorerCore.Init(this);
        }

        [OnDisable]
        public void OnDisable()
        {
        }
    }
}
#endif