<%@ Page Title="Map" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" Async="true" CodeBehind="map.aspx.cs" Inherits="RouteNavigation._Map" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <div id="map"></div>

    <link rel="stylesheet" href="/leaflet/leaflet.css" type="text/css">
    <script src="/leaflet/leaflet.js"></script>
    <script type="text/javascript">
        function showMap() {
            // set up the map
            map = new L.Map('map');

            // create the tile layer with correct attribution
            var osmUrl = 'https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png';
            var osmAttrib = 'Map data © <a href="https://openstreetmap.org">OpenStreetMap</a> contributors';
            var osm = new L.TileLayer(osmUrl, { minZoom: 1, maxZoom: 30, attribution: osmAttrib });

            // start the map in South-East England
            map.setView(new L.LatLng(51.3, 0.7), 10);

            map.addLayer(osm);
            $("#map").height($(window).height() * .9);
            map.invalidateSize();


            getJson('/LocationCoordinates.ashx?locationId=15589', function (err, locations) {
                if (err != null) {
                    console.error(err);
                } else {
                    for (index = 0; index < locations.length; x++)
                    {
                        addMarker(locations[index].coordinates_latitude, locations[index].coordinates_longitude);
                    };
                }
            });

        }

        var getJson = function (url, callback) {
            var xhr = new XMLHttpRequest();
            xhr.open('GET', url, true);
            xhr.responseType = 'json';
            xhr.onload = function () {
                var status = xhr.status;
                if (status === 200) {
                    callback(null, xhr.response);
                } else {
                    callback(status, xhr.response);
                }
            };
            xhr.send();
        }

        function addMarker(x, y) {
            L.marker([x,y]).addTo(map);
        }

    </script>

</asp:Content>
