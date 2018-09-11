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

            var routes = <%=routesJson %>;
            var mapX = <%=mapXCoordinate %>;
            var mapY = <%=mapYCoordinate %>;
            var routeCount = <%=routeCount %>;
            var locationNames = locationNames;
            var locationMarkers = [];
            for (i = 0; i < routes.length; i++) {
                for (j = 0; j < routes[i].allLocations.length; j++) {
                    var marker = L.marker([routes[i].allLocations[j].coordinates.lat, routes[i].allLocations[j].coordinates.lng]).bindTooltip(routes[i].allLocations[j].locationName);
                    locationMarkers.push(marker);
                }
            };

            //http://www.liedman.net/leaflet-routing-machine/api/

            var LocationsLayerGroup = L.layerGroup(locationMarkers);
            var RoutesLayerGroup = L.layerGroup();
            map = new L.Map('map',
                {
                    center: [mapX, mapY],
                    layers: LocationsLayerGroup, RoutesLayerGroup,
                    zoom: 10
                });

            var osm = new L.TileLayer(osmUrl,
                {
                    minZoom: 1,
                    maxZoom: 30,
                    attribution: osmAttrib
                }).addTo(map);

            map.addLayer(osm);
            $("#map").height($(window).height() * .9);
            map.invalidateSize();

            var layerControlLocations = L.control.layers();
            var layerControlRoutes = L.control.layers();
            layerControlLocations.addOverlay(LocationsLayerGroup, "Locations").addTo(map);

            var RoutesLayerGroup = L.layerGroup();
            var previousRouteId = null;
            var currentRouteId = null;
            var polyLines = [];
            for (i = 0; i < routes.length; i++) {
                var points = [[]];
                for (j = 0; j < routes[i].allLocations.length - 1; j++) {
                    var pointA = new L.LatLng(routes[i].allLocations[j].coordinates.lat, routes[i].allLocations[j].coordinates.lng);
                    var pointB = new L.LatLng(routes[i].allLocations[j + 1].coordinates.lat, routes[i].allLocations[j + 1].coordinates.lng);
                    points.push([pointA, pointB]);
                }
                var color = "rgb(" + routes[i].color.R + " ," + routes[i].color.G + "," + routes[i].color.B + ")";
                var multiPolyLine = new L.polygon(points, {
                    color: color,
                    weight: 4,
                    opacity: 1,
                    smoothFactor: 50
                });

                var overlayName = routes[i].id;
                var overlayRoutes = {
                };

                overlayRoutes[overlayName] = multiPolyLine;
                overlayRoutes[overlayName].addTo(map);
                layerControlRoutes.addOverlay(overlayRoutes[overlayName], overlayName).addTo(map);

            }

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

    </script>

</asp:Content>
