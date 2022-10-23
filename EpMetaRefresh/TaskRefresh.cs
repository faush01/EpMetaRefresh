﻿using MediaBrowser.Controller.Configuration;
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

namespace EpMetaRefresh
{
    public class TaskRefresh : IScheduledTask
    {
        public string Name => "Refresh Recent Episodes";
        public string Key => "EpMetaRefreshTask";
        public string Description => "Refreshes recently aired episodes with missing or incomplete metadata";
        public string Category => "Episode Metadata Refresh";

        private ILogger _logger;
        private ILibraryManager _libraryManager;
        private IFileSystem _fileSystem;

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

        private string i2s(int? value)
        {
            if(value == null)
            {
                return "--";
            }
            else
            {
                return value.Value.ToString("D2");
            }
        }

        public Task Execute(CancellationToken cancellationToken, IProgress<double> progress)
        {
            _logger.Info("Running Task");

            InternalItemsQuery query = new InternalItemsQuery();
            query.IsVirtualItem = false;
            query.IncludeItemTypes = new string[] { "Episode" };

            BaseItem[] results = _libraryManager.GetItemList(query);

            TimeSpan look_back = TimeSpan.FromDays(8);
            DateTimeOffset look_back_date = DateTimeOffset.Now.Subtract(look_back);

            MetadataRefreshOptions refresh_options = new MetadataRefreshOptions(_fileSystem);
            refresh_options.MetadataRefreshMode = MetadataRefreshMode.FullRefresh;
            refresh_options.ReplaceAllImages = true;
            refresh_options.ReplaceAllMetadata = true;

            int total_episodes = 0;
            int episodes_updated = 0;
            int episodes_no_prem = 0;

            foreach (BaseItem item in results)
            {
                if (item.GetType() == typeof(Episode))
                {
                    Episode episode = (Episode)item;
                    total_episodes++;

                    string episodeName = "(" + episode.InternalId + ")";
                    episodeName += "(" + episode.SeriesName + ")";
                    episodeName += "(s" + i2s(episode.ParentIndexNumber) + "e" + i2s(episode.IndexNumber) + ")";
                    episodeName += "(" + episode.Name + ")";

                    if (episode.PremiereDate == null)
                    {
                        episodes_no_prem++;
                        _logger.Info("No Prem Date : " + episodeName);
                    }
                    else if (episode.PremiereDate > look_back_date)
                    {
                        episodes_updated++;
                        episodeName += "(" + episode.PremiereDate.Value.ToString("yyyy-MM-dd HH:mm:ss") + ")";
                        _logger.Info("Refreshing Metadata : " + episodeName);
                        episode.RefreshMetadata(refresh_options, cancellationToken);
                    }
                }
            }

            _logger.Info("total_episodes   : " + total_episodes);
            _logger.Info("episodes_updated : " + episodes_updated);
            _logger.Info("episodes_no_prem : " + episodes_no_prem);

            return Task.CompletedTask;
        }
    }
}