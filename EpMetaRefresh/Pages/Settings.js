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

define([], function () {
    'use strict';

    ApiClient.getApiData = function (url_to_get) {
        console.log("getApiData Url = " + url_to_get);
        return this.ajax({
            type: "GET",
            url: url_to_get,
            dataType: "json"
        });
    };

    function PopulateEpisodeTable(view) {

        var cell_padding = "2px 5px 2px 5px";

        var url = "ep_meta_refresh/get_episodes?stamp=" + new Date().getTime();
        url = ApiClient.getUrl(url);

        ApiClient.getApiData(url).then(function (episode_data) {
            //console.log("Loaded Chapter Data: " + JSON.stringify(episode_data));

            // clean episode table
            var display_episode_list = view.querySelector("#episode_summary");
            while (display_episode_list.firstChild) {
                display_episode_list.removeChild(display_episode_list.firstChild);
            }

            var episode_items = episode_data.Episodes;

            var row_count = 0;
            for (const episode of episode_items) {
                var tr = document.createElement("tr");
                var td = null;

                td = document.createElement("td");
                td.style.overflow = "hidden";
                td.style.whiteSpace = "nowrap";
                td.appendChild(document.createTextNode(episode.Series));
                td.style.padding = cell_padding;
                tr.appendChild(td);

                td = document.createElement("td");
                td.style.overflow = "hidden";
                td.style.whiteSpace = "nowrap";
                td.appendChild(document.createTextNode(episode.Episode));
                td.style.padding = cell_padding;
                tr.appendChild(td);

                td = document.createElement("td");
                td.style.overflow = "hidden";
                td.style.whiteSpace = "nowrap";
                td.appendChild(document.createTextNode(episode.Name));
                td.style.padding = cell_padding;
                tr.appendChild(td);

                td = document.createElement("td");
                td.style.overflow = "hidden";
                td.style.whiteSpace = "nowrap";
                td.appendChild(document.createTextNode(episode.HasOverview));
                td.style.padding = cell_padding;
                tr.appendChild(td);

                td = document.createElement("td");
                td.style.overflow = "hidden";
                td.style.whiteSpace = "nowrap";
                if (episode.CriticRating) {
                    td.appendChild(document.createTextNode(episode.CriticRating));
                }
                td.style.padding = cell_padding;
                tr.appendChild(td);

                td = document.createElement("td");
                td.style.overflow = "hidden";
                td.style.whiteSpace = "nowrap";
                if (episode.CommunityRating) {
                    td.appendChild(document.createTextNode(episode.CommunityRating));
                }
                td.style.padding = cell_padding;
                tr.appendChild(td);

                td = document.createElement("td");
                td.style.overflow = "hidden";
                td.style.whiteSpace = "nowrap";
                td.appendChild(document.createTextNode(episode.PremiereDate));
                td.style.padding = cell_padding;
                tr.appendChild(td);

                td = document.createElement("td");
                td.style.overflow = "hidden";
                td.style.whiteSpace = "nowrap";
                td.appendChild(document.createTextNode(episode.LastRefreshed));
                td.style.padding = cell_padding;
                tr.appendChild(td);

                if (row_count % 2 === 0) {
                    tr.style.backgroundColor = "#77FF7730";
                }
                else {
                    tr.style.backgroundColor = "#7777FF30";
                }

                display_episode_list.appendChild(tr);

                row_count++;
            }

        });
    }

    function PopulateOptions(view) {

        ApiClient.getNamedConfiguration("ep_meta_refresh").then(function (config) {
            //console.log("Config Options : " + JSON.stringify(config));

            view.querySelector("#lookback_days").value = config.LookbackDays;
            view.querySelector("#include_no_prem").checked = config.IncludeNoPrem;

        });
    }

    function UpdateLookbackDays(control, view) {

        ApiClient.getNamedConfiguration("ep_meta_refresh").then(function (config) {
            //console.log("Config Options : " + JSON.stringify(config));
            let new_value = control.value;
            console.log("New Data URL : " + new_value);
            config.LookbackDays = new_value;

            ApiClient.updateNamedConfiguration("ep_meta_refresh", config).then(function () {
                PopulateOptions(view);
                PopulateEpisodeTable(view);
            });
        });
    }

    function UpdateIgnoreNoPrem(control, view) {

        ApiClient.getNamedConfiguration("ep_meta_refresh").then(function (config) {
            //console.log("Config Options : " + JSON.stringify(config));
            let new_value = control.checked;
            console.log("New Data URL : " + new_value);
            config.IncludeNoPrem = new_value;

            ApiClient.updateNamedConfiguration("ep_meta_refresh", config).then(function () {
                PopulateOptions(view);
                PopulateEpisodeTable(view);
            });
        });
    }

    return function (view, params) {

        view.addEventListener('viewshow', function (e) {

            PopulateOptions(view);
            PopulateEpisodeTable(view);

            view.querySelector('#lookback_days').addEventListener("change", function () {
                UpdateLookbackDays(this, view);
            });

            view.querySelector('#include_no_prem').addEventListener("change", function () {
                UpdateIgnoreNoPrem(this, view);
            });

        });

        view.addEventListener('viewhide', function (e) {

        });

        view.addEventListener('viewdestroy', function (e) {

        });
    };
});

