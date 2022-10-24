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

    function PopulateOptions(view) {

        ApiClient.getNamedConfiguration("ep_meta_refresh").then(function (config) {
            console.log("Config Options : " + JSON.stringify(config));

            view.querySelector("#lookback_days").value = config.LookbackDays;
        });
    }

    function UpdateLookbackDays(control, view) {

        ApiClient.getNamedConfiguration("ep_meta_refresh").then(function (config) {
            console.log("Config Options : " + JSON.stringify(config));
            let new_value = control.value;
            console.log("New Data URL : " + new_value);
            config.LookbackDays = new_value;

            ApiClient.updateNamedConfiguration("ep_meta_refresh", config).then(function () {
                PopulateOptions(view);
            });
        });
    }

    return function (view, params) {

        view.addEventListener('viewshow', function (e) {

            PopulateOptions(view);

            view.querySelector('#lookback_days').addEventListener("change", function () {
                UpdateLookbackDays(this, view);
            });

        });

        view.addEventListener('viewhide', function (e) {

        });

        view.addEventListener('viewdestroy', function (e) {

        });
    };
});

