﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Casualty_Radar.Models.DataControl.Graph;
using Casualty_Radar.Utils;
using GMap.NET;

namespace Casualty_Radar.Models.Navigation {
    class Route {
        public List<Node> RouteNodes { get; set; } // All the nodes in the calculated route
        public List<NavigationStep> RouteSteps { get; set; } // All the calculated steps of the route
        public List<Panel> RouteStepPanels { get; set; } // All the generated panels for the route steps
        public string StartingRoad { get; set; } // The starting road for the route
        public string DestinationRoad { get; set; } // The destination road for the route
        public double TotalDistance { get; set; } // The total distance of the route

        public Route() {
            RouteSteps = new List<NavigationStep>();
            RouteStepPanels = new List<Panel>();
        }

        /// <summary>
        /// Gets all latitude and longitude points of each node in the route
        /// </summary>
        /// <returns>A list with all the retrieved points</returns>
        public List<PointLatLng> GetRoutePoints() => RouteNodes.Select(node => node.GetPoint()).ToList();

        /// <summary>
        /// Calculates all the route steps and sets information about the route
        /// </summary>
        public void CalculateRouteSteps() {
            double prevAngle = -1;
            int height = 0;
            Color color = Color.Gainsboro;
            for (int index = 0; index < RouteNodes.Count; index++) {
                Node node = RouteNodes[index];
                if (index + 1 != RouteNodes.Count && index + 2 != RouteNodes.Count) {
                    Node nextNode = RouteNodes[index + 1];
                    Node nextNextNode = RouteNodes[index + 2];

                    if (index == 0)
                        StartingRoad = MapUtil.GetWay(RouteNodes[0], nextNode).Name; // Set the starting road for the route

                    TotalDistance += MapUtil.GetDistance(node, nextNode); // Add this step's distance to the total distance

                    // Check in which direction the step should point
                    double angle = RouteUtil.AngleFromCoordinate(nextNode, nextNextNode);
                    var type = prevAngle >= 0
                        ? NavigationStep.CalcRouteStepType(RouteUtil.CalcBearing(prevAngle, angle))
                        : RouteStepType.Straight;

                    // Get the distance for the step
                    string distance =
                        NavigationStep.GetFormattedDistance(Math.Round(MapUtil.GetDistance(node, nextNode), 2));

                    NavigationStep step = new NavigationStep(distance, type, MapUtil.GetWay(nextNode, nextNextNode));
                    RouteSteps.Add(step);

                    // Check if the route is finished
                    if (index + 3 == RouteNodes.Count) {
                        step = new NavigationStep(distance, RouteStepType.DestinationReached,
                            MapUtil.GetWay(nextNode, nextNextNode));
                        RouteStepPanels.Add(NavigationStep.CreateRouteStepPanel(step, color, height));
                        DestinationRoad = MapUtil.GetWay(nextNode, nextNextNode).Name;
                    }
                    else RouteStepPanels.Add(NavigationStep.CreateRouteStepPanel(step, color, height));

                    color = color == Color.Gainsboro ? Color.White : Color.Gainsboro;
                    height += 51;
                    prevAngle = angle;
                }
                else break;
            }
            TotalDistance = Math.Round(TotalDistance, 2);
        }
    }
}