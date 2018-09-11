<%@ Page Title="Map" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" Async="true" CodeBehind="map.aspx.cs" Inherits="RouteNavigation._Map" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <div id="map"></div>


    <link rel="stylesheet" href="/leaflet/leaflet.css" type="text/css">
    <script src="/leaflet/leaflet.js"></script>
    <script type="text/javascript">
        function showMap() {
            // set up the map


            // create the tile layer with correct attribution
            var osmUrl = 'https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png';
            var osmAttrib = 'Map data © <a href="https://openstreetmap.org">OpenStreetMap</a> contributors';

            var locations = <%=locationsJson %>;
            var mapX = <%=mapXCoordinate %>;
            var mapY = <%=mapYCoordinate %>;
            var locationNames = locationNames;
            var locationMarkers = [];
            for (index = 0; index < locations.length; index++) {
                var marker = L.marker([locations[index].coordinates.lat, locations[index].coordinates.lng]).bindTooltip(locations[index].locationName);
                locationMarkers.push(marker);
            };

            //http://www.liedman.net/leaflet-routing-machine/api/

            var locationsLayerGroup = L.layerGroup(locationMarkers);

            map = new L.Map('map',
                {
                    center: [mapX, mapY],
                    layers: locationsLayerGroup,
                    zoom: 10
                });

            var overlayMaps = {
                "Locations": locationsLayerGroup
            };

            L.control.layers(null, overlayMaps).addTo(map);

            var osm = new L.TileLayer(osmUrl,
                {
                    minZoom: 1,
                    maxZoom: 30,
                    attribution: osmAttrib
                }).addTo(map);

            map.addLayer(osm);
            $("#map").height($(window).height() * .9);
            map.invalidateSize();

            for (index = 0; index < locations.length - 1; index++) {
                var pointA = new L.LatLng(locations[index].coordinates.lat, locations[index].coordinates.lng);
                var pointB = new L.LatLng(locations[index + 1].coordinates.lat, locations[index + 1].coordinates.lng);
                var pointList = [pointA, pointB];

                var firstpolyline = new L.Polyline(pointList, {
                    color: 'blue',
                    weight: 4,
                    opacity: 1,
                    smoothFactor: 50
                });

                firstpolyline.addTo(map);


            };


            function connectDots(data) {
                var features = data.features,
                    feature,
                    c = [],
                    i;

                for (i = 0; i < features.length; i += 1) {
                    feature = features[i];
                    // Make sure this feature is a point.
                    if (feature.geometry === "Point") {
                        c.push(feature.geometry.coordinates);
                    }
                }
                return c;
            }
        }

    </script>

</asp:Content>
