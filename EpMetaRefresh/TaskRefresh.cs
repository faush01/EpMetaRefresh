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

using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller;
using MediaBrowser.Model.Activity;
using MediaBrowser.Model.IO;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Tasks;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.Services;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Controller.Plugins;
using EpMetaRefresh.Options;
using EpMetaRefresh.Lib;

namespace EpMetaRefresh
{
    public class TaskRefresh : IScheduledTask
    {
        public string Name => "Refresh Recently Aired Episodes";
        public string Key => "EpMetaRefreshTask";
        public string Description => "Refreshes recently aired episodes to fill in missing or incomplete metadata.";
        public string Category => "Episode Refresh";

        private ILogger _logger;
        private ILibraryManager _libraryManager;
        private IFileSystem _fileSystem;
        private IServerConfigurationManager _config;

        public TaskRefresh(IActivityManager activity, 
            ILogManager logger, 
            IServerConfigurationManager config, 
            IFileSystem fileSystem, 
            IServerApplicationHost appHost,
            ILibraryManager libraryManager)
        {
            _logger = logger.GetLogger("EpMetaRefresh - TaskRefresh");
            _libraryManager = libraryManager;
            _fileSystem = fileSystem;
            _config = config;
        }

        public IEnumerable<TaskTriggerInfo> GetDefaultTriggers()
        {
            var trigger = new TaskTriggerInfo
            {
                Type = TaskTriggerInfo.TriggerDaily,
                TimeOfDayTicks = TimeSpan.FromHours(3).Ticks
            };
            return new[] { trigger };
        }

        public Task Execute(CancellationToken cancellationToken, IProgress<double> progress)
        {
            _logger.Info("Running Task");

            PluginOptions plugin_options = _config.GetPluginOptions();

            MetadataRefreshOptions refresh_options = new MetadataRefreshOptions(_fileSystem);
            refresh_options.MetadataRefreshMode = MetadataRefreshMode.FullRefresh;
            refresh_options.ReplaceAllImages = true;
            refresh_options.ReplaceAllMetadata = true;

            List<Episode> episodes_result = new List<Episode>();
            int total_episodes = QueryHelper.GetEpisodes(_libraryManager, plugin_options, _logger, episodes_result);

            int episodes_no_prem = 0;

            foreach (Episode episode in episodes_result)
            {
                string episodeName = "(" + episode.InternalId + ")";
                episodeName += "(" + episode.SeriesName + ")";
                episodeName += "(s" + QueryHelper.i2s(episode.ParentIndexNumber) + "e" + QueryHelper.i2s(episode.IndexNumber) + ")";
                episodeName += "(" + episode.Name + ")";

                if (episode.PremiereDate != null)
                {
                    episodeName += "(" + episode.PremiereDate.Value.ToString("yyyy-MM-dd HH:mm:ss") + ")";
                }
                else
                {
                    episodes_no_prem++;
                    episodeName += "(No Prem Date)";
                }
                _logger.Info("Refreshing Metadata : " + episodeName);
                episode.RefreshMetadata(refresh_options, cancellationToken);
            }

            _logger.Info("total_episodes   : " + total_episodes);
            _logger.Info("episodes_updated : " + episodes_result.Count);
            _logger.Info("episodes_no_prem : " + episodes_no_prem);

            return Task.CompletedTask;
        }
    }
}
