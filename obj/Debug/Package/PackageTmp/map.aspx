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
            var accounts = accounts;
            var locationMarkers = [];

            var iconImage = L.icon({
                iconUrl: '/leaflet/images/marker-icon.png',
                //shadowUrl: '/leaflet/images/marker-shadow.png',

                iconSize: [18.75, 30], // size of the icon
                //shadowSize: [21, 21], // size of the shadow
                iconAnchor: [9.375, 30], // point of the icon which will correspond to marker's location
                //shadowAnchor: [0, 0],  // the same for the shadow
                //popupAnchor: [-3, -76] // point from which the popup should open reLative to the iconAnchor
            });



            for (i = 0; i < routes.length; i++) {
                for (j = 0; j < routes[i].AllLocations.length; j++) {
                    var location = routes[i].AllLocations[j];
                    var accountText = "Name: " + location.Account
                    var locationAddressText = "Address: " + location.Address
                    var locationCoordinates = "Coordinates: " + "(Lat: " + location.Coordinates.Lat + ",Lng: " + location.Coordinates.Lng + ")";
                    //If you want to add these, need to postback to database location properties with updated info / add to route details
                    var intendedVisitDate = "Intended Visit Date: " + location.IntededPickupDate;
                    var locationOilPickupDaysUntilDue = "Oil Days Until Due: " + location.OilPickupDaysUntilDue;
                    var locationGreaseTrapDaysUntilDue = "G.T. Days Until Due: " + location.GreaseTrapDaysUntilDue;
                    var locationDistanceFromDepot = "Distance From Depot: " + location.DistanceFromDepot + " miles";
                    var locationOil = "Oil: " + location.OilPickupCustomer;
                    var locationGrease = "Grease: " + location.GreaseTrapCustomer;
                    var popup = L.popup().setContent(accountText + "<br>" + locationAddressText + "<br>" + locationOil + "<br>" + intendedVisitDate + "<br>" + locationGrease + "<br>" + locationCoordinates + "<br>" + locationOilPickupDaysUntilDue + "<br>" + locationGreaseTrapDaysUntilDue + "<br>" + locationDistanceFromDepot);

                    var marker = L.marker([routes[i].AllLocations[j].Coordinates.Lat, routes[i].AllLocations[j].Coordinates.Lng], { icon: iconImage })
                    marker.bindTooltip(routes[i].AllLocations[j].Account);
                    marker.bindPopup(popup);
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
            var routeUrls = [];
            for (i = 0; i < routes.length; i++) {
                var points = [[]];
                for (j = 0; j < routes[i].AllLocations.length - 1; j++) {
                    var pointA = new L.LatLng(routes[i].AllLocations[j].Coordinates.Lat, routes[i].AllLocations[j].Coordinates.Lng);
                    var pointB = new L.LatLng(routes[i].AllLocations[j + 1].Coordinates.Lat, routes[i].AllLocations[j + 1].Coordinates.Lng);
                    points.push([pointA, pointB]);
                }
                var color = "rgb(" + routes[i].Color.R + " ," + routes[i].Color.G + "," + routes[i].Color.B + ")";
                var routeUrl = "/routeDetails?routeId=" + routes[i].Id;
                var multiPolyLine = new L.polygon(points, {
                    color: color,
                    weight: 4,
                    opacity: 1,
                    smoothFactor: 50,
                    url: routeUrl
                });

                var toolTip = L.tooltip({ sticky: true }).setContent("Route ID: " + routes[i].id);
                multiPolyLine.bindTooltip(toolTip);


                multiPolyLine.on("click", function (event) { window.open(event.target.options.url); });
                var overlayName = routes[i].id;
                var overlayRoutes = {
                };

                overlayRoutes[overlayName] = multiPolyLine;
                overlayRoutes[overlayName].addTo(map);
                layerControlRoutes.addOverlay(overlayRoutes[overlayName], overlayName).addTo(map);

            }

        };

        function parseJsonDate(jsonDateString) {
            return new Date(parseInt(jsonDateString.replace('/Date(', '')));
        }

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
