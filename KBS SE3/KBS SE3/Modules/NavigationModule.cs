﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms;
using Casualty_Radar.Core;
using Casualty_Radar.Models;
using Casualty_Radar.Models.Navigation;
using Casualty_Radar.Properties;
using Casualty_Radar.Utils;
using Casualty_Radar.Models.DataControl;
using Casualty_Radar.Core.Algorithms;
using Casualty_Radar.Core.Dialog;
using Casualty_Radar.Models.DataControl.Graph;

namespace Casualty_Radar.Modules {
    /// <summary>
    /// Module that contains a map displaying the starting and ending point for the route and the route between them. Also contains a panel in which the information about the alert is being shown.
    /// </summary>
    partial class NavigationModule : UserControl, IModule {
        private readonly LocationManager _locationManager;
        private GMapOverlay _routeOverlay;
        private Pathfinder _pathfinder;
        private Node _startNode;
        private Node _endNode;

        public NavigationModule() {
            InitializeComponent();
            _locationManager = new LocationManager();
        }

        public Breadcrumb GetBreadcrumb() {
            return new Breadcrumb(this, "Navigation", null,
                ModuleManager.GetInstance().ParseInstance(typeof(HomeModule)));
        }

        /// <summary>
        /// Readies the module for when the user has clicked the navigation button in HomeModule. Fills the alert information panel and calculates and draws the fastest route
        /// </summary>
        /// <param name="alert">Alert which contains all the information about the chosen alert</param>
        /// <param name="start">Point with the user's current latitude and longitude</param>
        public void Init(Alert alert, PointLatLng start) {
            _locationManager.CurrentLatitude = start.Lat;
            _locationManager.CurrentLongitude = start.Lng;

            infoTitleLabel.Text = string.Format("{0}\n{1}", alert.Title, alert.Info);
            alertTypePicturebox.Image = alert.Type == 1 ? Resources.Medic : Resources.Firefighter;
            timeLabel.Text = alert.PubDate.TimeOfDay.ToString();
            InitRouteMap(start.Lat, start.Lng, alert.Lat, alert.Lng);

            //Instantiates a data parser which creates a collection with all nodes and ways of a specific zone
            DataParser parser = new DataParser(@"../../Resources/hattem.xml");
            parser.Deserialize();
            DataCollection collection = parser.GetCollection();
            List<Node> targetCollection = collection.Intersections;

            List<Node> nodes = collection.Nodes;
            //foreach (Node node in nodes) map.Overlays[0].Markers.Add(_locationManager.CreateMarkerWithTooltip(node.Lat, node.Lon, 1, node.ID.ToString()));
            
            foreach (Node n in MapUtil.GetAdjacentNodes(nodes.Find(n => n.ID == 1281347185)))
                map.Overlays[0].Markers.Add(_locationManager.CreateMarkerWithTooltip(n.Lat, n.Lon, 2, n.ID.ToString()));

            //_startNode = MapUtil.GetNearest(start.Lat, start.Lng, targetCollection);
            //_endNode = MapUtil.GetNearest(dest.Lat, dest.Lng, targetCollection);
            Casualty_Radar.Container.GetInstance().DisplayDialog(DialogType.DialogMessageType.SUCCESS, "Aantal nodes", targetCollection.Count.ToString());
            _startNode = targetCollection[160];
            map.Overlays[0].Markers.Add(_locationManager.CreateMarker(_startNode.Lat, _startNode.Lon, 2));
            _endNode = targetCollection[1];
            map.Overlays[0].Markers.Add(_locationManager.CreateMarker(_endNode.Lat, _endNode.Lon, 3));

            _pathfinder = new Pathfinder(_startNode, _endNode);
            List<Node> path = _pathfinder.FindPath();
            List<PointLatLng> points = new List<PointLatLng>();

            int height = 0;
            Color color = Color.Gainsboro;
            for (int index = 0; index < path.Count; index++) {
                Node node = path[index];

                foreach (Way way in node.ConnectedWays) Debug.WriteLine(way.TypeDescription);
                points.Add(node.GetPoint());

                if (index + 1 != path.Count) {
                    map.Overlays[0].Markers.Add(_locationManager.CreateMarkerWithTooltip(node.Lat, node.Lon, 0, node.ID.ToString()));
                    Node nextNode = path[index + 1];
                    RouteStepType type = RouteStepType.Straight;
                    string distance = NavigationStep.GetFormattedDistance(Math.Round(MapUtil.GetDistance(node, nextNode), 2));
                    string instruction = "Ga over " + distance + " naar " + type;
                    NavigationStep step = new NavigationStep(instruction, distance, type);
                    CreateRouteStepPanel(step, color, height);
                } else CreateRouteStepPanel(new NavigationStep(), color, height);

                color = color == Color.Gainsboro ? Color.White : Color.Gainsboro;
                height += 51;
            }
            _locationManager.DrawRoute(points, _routeOverlay);
        }

        /// <summary>
        /// Initializes the GMapControl in the module. Creates markers on the current location and the chosen alert's location
        /// </summary>
        /// <param name="startLat">User's current latitude</param>
        /// <param name="startLng">User's current longitude</param>
        /// <param name="destLat">The alert's latitude</param>
        /// <param name="destLng">The alert's longitude</param>
        public void InitRouteMap(double startLat, double startLng, double destLat, double destLng) {
            map.Overlays.Clear();
            map.ShowCenter = false;
            map.MapProvider = GoogleMapProvider.Instance;
            map.IgnoreMarkerOnMouseWheel = true;
            map.DragButton = MouseButtons.Left;
            GMaps.Instance.Mode = AccessMode.ServerOnly;
            map.Position = new PointLatLng((startLat + destLat) / 2, (startLng + destLng) / 2);
            GMapOverlay markersOverlay = new GMapOverlay("markers");
            _routeOverlay = new GMapOverlay("routes");
            map.Overlays.Add(markersOverlay);
            map.Overlays.Add(_routeOverlay);

            markersOverlay.Markers.Add(_locationManager.CreateMarker(startLat, startLng, 0));
            markersOverlay.Markers.Add(_locationManager.CreateMarker(destLat, destLng, 2));
        }

        /// <summary>
        /// Creates a routestep based on a given NavigationStep
        /// </summary>
        /// <param name="step">The NavigationStep with all the information</param>
        /// <param name="color">Background color for the panel</param>
        /// <param name="Height">Height of the panel</param>
        public void CreateRouteStepPanel(NavigationStep step, Color color, int Height) {
            Image icon;

            switch (step.Type) {
                case RouteStepType.Straight:
                    icon = Resources.straight_icon;
                    break;
                case RouteStepType.Left:
                    icon = Resources.turn_left_icon;
                    break;
                case RouteStepType.Right:
                    icon = Resources.turn_right_icon;
                    break;
                case RouteStepType.DestinationReached:
                    icon = Resources.destination_icon;
                    break;
                default:
                    icon = Resources.straight_icon;
                    break;
            }

            //The panel which will be filled with all of the controls below
            Panel newPanel = new Panel {
                Location = new Point(0, Height),
                Size = new Size(338, 50),
                BackColor = color
            };

            if (step.Distance != null) {
                Label distanceLabel = new Label {
                    Location = new Point(10, 0),
                    Size = new Size(50, 50),
                    TextAlign = ContentAlignment.MiddleCenter,
                    ForeColor = Color.DarkSlateGray,
                    Font = new Font("Microsoft Sans Serif", 9, FontStyle.Bold),
                    Text = step.Distance
                };
                newPanel.Controls.Add(distanceLabel);
            }

            Label instructionLabel = new Label {
                Location = new Point(60, 0),
                Size = new Size(130, 50),
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = Color.DarkSlateGray,
                Font = new Font("Microsoft Sans Serif", 9),
                Text = step.Instruction
            };

            PictureBox instructionIcon = new PictureBox {
                Location = new Point(280, 10),
                Size = new Size(30, 30),
                Image = icon,
                SizeMode = PictureBoxSizeMode.StretchImage
            };

            newPanel.Controls.Add(instructionIcon);
            newPanel.Controls.Add(instructionLabel);

            routeInfoPanel.AutoScroll = true;
            routeInfoPanel.HorizontalScroll.Enabled = false;
            routeInfoPanel.Controls.Add(newPanel);
        }
    }
}