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
using System;
using System.Collections.Generic;
using System.Text;

namespace EpMetaRefresh.Options
{
    public static class ConfigurationExtension
    {
        public static PluginOptions GetPluginOptions(this IConfigurationManager manager)
        {
            return manager.GetConfiguration<PluginOptions>("ep_meta_refresh");
        }
        public static void SavePluginOptions(this IConfigurationManager manager, PluginOptions options)
        {
            manager.SaveConfiguration("ep_meta_refresh", options);
        }
    }
}
