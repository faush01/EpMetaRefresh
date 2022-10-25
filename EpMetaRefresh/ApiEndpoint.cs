using EpMetaRefresh.Lib;
using EpMetaRefresh.Options;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.MediaEncoding;
using MediaBrowser.Controller.Net;
using MediaBrowser.Controller.Persistence;
using MediaBrowser.Controller.Session;
using MediaBrowser.Model.IO;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Serialization;
using MediaBrowser.Model.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace EpMetaRefresh
{
    public class ApiEndpoint
    {

        // http://localhost:8096/emby/ep_meta_refresh/get_episodes
        [Route("/ep_meta_refresh/get_episodes", "GET", Summary = "Gets a list of episodes that would be refreshed in the schedule task")]
        //[Authenticated]
        public class GetEpisodes : IReturn<Object>
        {
        }

        public class ApiEndpointService : IService
        {
            private readonly ISessionManager _sessionManager;
            private readonly ILogger _logger;
            private readonly IJsonSerializer _jsonSerializer;
            private readonly IFileSystem _fileSystem;
            private readonly IServerConfigurationManager _config;
            private readonly IUserManager _userManager;
            private readonly IUserDataManager _userDataManager;
            private readonly ILibraryManager _libraryManager;
            private readonly IAuthorizationContext _ac;
            private readonly IItemRepository _ir;
            private readonly IFfmpegManager _ffmpeg;
            private readonly IHttpResultFactory _hrf;

            public ApiEndpointService(ILogManager logger,
                IFileSystem fileSystem,
                IServerConfigurationManager config,
                IJsonSerializer jsonSerializer,
                IUserManager userManager,
                ILibraryManager libraryManager,
                ISessionManager sessionManager,
                IAuthorizationContext authContext,
                IUserDataManager userDataManager,
                IItemRepository itemRepository,
                IFfmpegManager ffmpegManager,
                IHttpResultFactory httpResultFactory)
            {
                _logger = logger.GetLogger("EpMetaRefresh - ApiEndpointService");
                _jsonSerializer = jsonSerializer;
                _fileSystem = fileSystem;
                _config = config;
                _userManager = userManager;
                _libraryManager = libraryManager;
                _sessionManager = sessionManager;
                _ac = authContext;
                _userDataManager = userDataManager;
                _ir = itemRepository;
                _ffmpeg = ffmpegManager;
                _hrf = httpResultFactory;

                _logger.Info("ApiEndpointService Loaded");
            }

            public object Get(GetEpisodes request)
            {
                Dictionary<string, object> episode_data = new Dictionary<string, object>();

                PluginOptions plugin_options = _config.GetPluginOptions();
                List<Episode> episodes_result = new List<Episode>();
                int total_episodes = QueryHelper.GetEpisodes(_libraryManager, plugin_options, _logger, episodes_result);

                int episodes_no_prem = 0;

                List<Dictionary<string, object>> episodes = new List<Dictionary<string, object>>();
                foreach (Episode episode in episodes_result)
                {
                    Dictionary<string, object> ep = new Dictionary<string, object>();

                    ep.Add("Series", episode.SeriesName);
                    ep.Add("Episode", "s" + QueryHelper.i2s(episode.ParentIndexNumber) + "e" + QueryHelper.i2s(episode.IndexNumber));
                    ep.Add("Name", episode.Name);
                    ep.Add("CommunityRating", episode.CommunityRating);
                    ep.Add("CriticRating", episode.CriticRating);
                    ep.Add("LastRefreshed", episode.DateLastRefreshed.LocalDateTime.ToString("yyyy-MM-dd HH:mm:ss"));

                    if(!string.IsNullOrEmpty(episode.Overview) && episode.Overview.Length > 20)
                    {
                        ep.Add("HasOverview", true);
                    }
                    else
                    {
                        ep.Add("HasOverview", false);
                    }

                    if (episode.PremiereDate != null)
                    {
                        TimeSpan age_days = DateTime.Now - episode.PremiereDate.Value.LocalDateTime;
                        ep.Add("PremiereDate", episode.PremiereDate.Value.LocalDateTime.ToString("yyyy-MM-dd") + " (" + (int)age_days.TotalDays + ")");
                    }
                    else
                    {
                        episodes_no_prem++;
                        ep.Add("PremiereDate", null);
                    }
                        
                    episodes.Add(ep);
                }

                episodes.Sort(delegate (Dictionary<string, object> c1, Dictionary<string, object> c2)
                {
                    string c1_s = (string)c1["PremiereDate"] ?? "";
                    string c2_s = (string)c2["PremiereDate"] ?? "";
                    return c1_s.CompareTo(c2_s);
                });

                /*
                episodes.Sort(delegate (Dictionary<string, object> c1, Dictionary<string, object> c2)
                {
                    string c1_s = (string)c1["Series"];
                    string c2_s = (string)c2["Series"];
                    int comp = c1_s.CompareTo(c2_s);
                    if (comp != 0) return comp;

                    c1_s = (string)c1["Episode"];
                    c1_s = (string)c2["Episode"];
                    return c1_s.CompareTo(c2_s);
                });
                */

                episode_data.Add("Episodes", episodes);
                episode_data.Add("TotalCount", total_episodes);
                episode_data.Add("UpdatedCount", episodes_result.Count);
                episode_data.Add("NoPremCount", episodes_no_prem);
                episode_data.Add("Result", "ok");

                return episode_data;
            }

        }
    }
}
