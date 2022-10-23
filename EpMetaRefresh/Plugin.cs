using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Drawing;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace EpMetaRefresh
{
    public class Plugin : BasePlugin<PluginConfiguration>, IHasThumbImage //,IHasWebPages
    {
        public override string Name => "Recent Episode Metadata Refresh";
        public override Guid Id => new Guid("98445989-0c60-4f07-8c49-920b3fd808fe");
        public override string Description => "Auto refresh recently aired episode metadata";
        public PluginConfiguration PluginConfiguration => Configuration;
        private readonly ILogger _logger;

        public Plugin(ILogManager logger,
            IApplicationPaths applicationPaths,
            IXmlSerializer xmlSerializer) : base(applicationPaths, xmlSerializer)
        {
            _logger = logger.GetLogger("EpMetaRefresh - Plugin");
            _logger.Info("Plugin Loaded");
        }

        /*
        IEnumerable<PluginPageInfo> IHasWebPages.GetPages()
        {
            throw new NotImplementedException();
        }
        */

        public Stream GetThumbImage()
        {
            var type = GetType();
            return type.Assembly.GetManifestResourceStream(type.Namespace + ".Media.logo.png");
        }

        public ImageFormat ThumbImageFormat
        {
            get
            {
                return ImageFormat.Png;
            }
        }
    }
}
