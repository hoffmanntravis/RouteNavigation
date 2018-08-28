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

            var coordinates = <%=jsonCoordinates %>;
            var mapX = <%=mapXCoordinate %>;
            var mapY = <%=mapYCoordinate %>;

            var locations = [];
            for (index = 0; index < coordinates.length; index++) {
                var marker = L.marker([coordinates[index].lat, coordinates[index].lng]);
                locations.push(marker);
            };

            var locationsLayerGroup = L.layerGroup(locations);

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

            for (index = 0; index < coordinates.length - 1; index++) {
                var pointA = new L.LatLng(coordinates[index].lat, coordinates[index].lng);
                var pointB = new L.LatLng(coordinates[index + 1].lat, coordinates[index + 1].lng);
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

        function addMarker(x, y) {
            L.marker([x,y]).addTo(map);
        }

    </script>

</asp:Content>
