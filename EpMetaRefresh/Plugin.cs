/*
Copyright(C) 2022

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program. If not, see<http://www.gnu.org/licenses/>.
*/

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
    public class Plugin : BasePlugin<PluginConfiguration>, IHasThumbImage, IHasWebPages
    {
        public override string Name => "Episode Refresh";
        public override Guid Id => new Guid("98445989-0c60-4f07-8c49-920b3fd808fe");
        public override string Description => "Auto refresh recently aired episode metadata. This helps to fill in missing data if the episode was added before titles and rating were available in the metadata sources.";
        public PluginConfiguration PluginConfiguration => Configuration;
        private readonly ILogger _logger;

        public Plugin(ILogManager logger,
            IApplicationPaths applicationPaths,
            IXmlSerializer xmlSerializer) : base(applicationPaths, xmlSerializer)
        {
            _logger = logger.GetLogger("EpMetaRefresh - Plugin");
            _logger.Info("Plugin Loaded");
        }

        public IEnumerable<PluginPageInfo> GetPages()
        {
            return new[]
            {
                new PluginPageInfo
                {
                    Name = "settings",
                    EmbeddedResourcePath = GetType().Namespace + ".Pages.Settings.html",
                    EnableInMainMenu = true
                },
                new PluginPageInfo
                {
                    Name = "settings.js",
                    EmbeddedResourcePath = GetType().Namespace + ".Pages.Settings.js"
                }
            };
        }

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
