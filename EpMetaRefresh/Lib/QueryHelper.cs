using EpMetaRefresh.Options;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace EpMetaRefresh.Lib
{

    public static class QueryHelper
    {
        public static string i2s(int? value)
        {
            if (value == null)
            {
                return "--";
            }
            else
            {
                return value.Value.ToString("D2");
            }
        }

        public static int GetEpisodes(
            ILibraryManager _libraryManager,
            PluginOptions plugin_options,
            ILogger _logger,
            List<Episode> episodes)
        {
            InternalItemsQuery query = new InternalItemsQuery();
            query.IsVirtualItem = false;
            query.IncludeItemTypes = new string[] { "Episode" };

            BaseItem[] results = _libraryManager.GetItemList(query);

            _logger.Info("LookbackDays  : " + plugin_options.LookbackDays);
            _logger.Info("IncludeNoPrem : " + plugin_options.IncludeNoPrem);

            TimeSpan look_back = TimeSpan.FromDays(plugin_options.LookbackDays);
            DateTime look_back_date = DateTime.Now.Subtract(look_back);

            int total_episodes = 0;

            foreach (BaseItem item in results)
            {
                if (item.GetType() == typeof(Episode))
                {
                    Episode episode = (Episode)item;
                    total_episodes++;

                    if (episode.PremiereDate == null && plugin_options.IncludeNoPrem)
                    {
                        episodes.Add(episode);
                    }
                    
                    if (episode.PremiereDate != null && episode.PremiereDate.Value.LocalDateTime > look_back_date)
                    {
                        episodes.Add(episode);
                    }
                }
            }

            return total_episodes;
        }
    }
}
