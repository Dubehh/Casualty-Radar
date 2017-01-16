﻿using System.Collections.Generic;
using System.Diagnostics;
using Casualty_Radar.Models.DataControl.Graph;
using Casualty_Radar.Utils;


namespace Casualty_Radar.Core.Algorithms {
    /// <summary>
    /// A class which contains all methods needed for calculating a path with the A-Star algorithm
    /// </summary>
    class Pathfinder {
        private Node _startNode; // The beginning point for the route
        private Node _endNode; // The destination for the route
        private List<Node> _closedNodes; // A list with all closed nodes, for the application to compare
        private List<Node> _openNodes; // A list with all open nodes, for the application to compare

        public Pathfinder(Node startNode, Node endNode) {
            _closedNodes = new List<Node>();
            _openNodes = new List<Node>();
            _endNode = endNode;
            _endNode.StarData = new StarData(_endNode, _endNode);
            _startNode = startNode;
            _startNode.StarData = new StarData(_startNode, _endNode) { State = NodeState.Open };
        }

        /// <summary>
        /// Attempts to find a path from the start location to the end location
        /// </summary>
        /// <returns>Returns a List of Points representing the path. If no path was found, the returned list is empty</returns>
        public List<Node> FindPath() {
            // The start node is the first entry in the 'open' list
            List<Node> path = new List<Node>();
            bool success = Search(_startNode);
            if (!success) return path;

            // If a path was found, follow the parents from the end node to build a list of locations
            Node node = _endNode;
            while (node.StarData.Parent != null) {
                path.Add(node);
                node = node.StarData.Parent;
            }

            path.Add(_startNode);
            // Reverse the list so it's in the correct order when returned
            path.Reverse();
            path.Add(_endNode);
            return path;
        }

        /// <summary>
        /// Recursive function that starts with the starting node and keeps repeating until a route was successfully found.
        /// For each node it checks all adjacent nodes and their usefulness for the route
        /// </summary>
        /// <param name="currentNode">The node where the path currently is</param>
        /// <returns></returns>
        private bool Search(Node currentNode)
        {
            // Set the current node to Closed since it cannot be traversed more than once
            currentNode.StarData.State = NodeState.Closed;
            _closedNodes.Add(currentNode);
            List<Node> nextNodes = GetAdjacentStarNodes(currentNode);
            // Sort by F-value so that the shortest possible routes are considered first
            nextNodes.Sort((node1, node2) => node1.StarData.F.CompareTo(node2.StarData.F));
            foreach (Node nextNode in nextNodes) {
                // Check whether the end node has been reached
                if (nextNode.GetPoint() == _endNode.GetPoint())
                    return true;
                // If not, check the next set of nodes
                if (Search(nextNode)) // Note: Recurses back into Search(Node)
                    return true;
            }

            // The method returns false if this path leads to be a dead end
            return false;
        }

        /// <summary>
        /// Searches through all adjacent nodes of a given node and checks their efficiency 
        /// </summary>
        /// <param name="fromNode">The node to get and check all the adjacent nodes for</param>
        /// <returns></returns>
        private List<Node> GetAdjacentStarNodes(Node fromNode) {
            List<Node> nodes = new List<Node>();
            List<Node> adjacentNodes = MapUtil.GetAdjacentNodes(fromNode);

            foreach (Node node in adjacentNodes) {
                node.StarData = _openNodes.Find(n => n.ID == node.ID) == null ? new StarData(node, _endNode) : _openNodes.Find(n => n.ID == node.ID).StarData;

                foreach (Node n in _closedNodes) if (n.ID == node.ID) node.StarData.State = NodeState.Closed;
                foreach (Node n in _openNodes) if (n.ID == node.ID) node.StarData.State = NodeState.Open;

                // Ignore already-closed nodes
                switch (node.StarData.State) {
                    case NodeState.Closed:
                        continue;
                    case NodeState.Open:
                        double traversalCost = MapUtil.GetDistance(node, node.StarData.Parent);
                        double gTemp = fromNode.StarData.G + traversalCost;
                        if (gTemp < node.StarData.G) {
                            node.StarData.Parent = fromNode;
                            nodes.Add(node);
                        }
                        break;
                    default:
                        // If it's untested, set the parent and flag it as 'Open' for consideration
                        node.StarData.Parent = fromNode;
                        node.StarData.State = NodeState.Open;
                        _openNodes.Add(node);
                        nodes.Add(node);
                        break;
                }
            }

            return nodes;
        }
    }
}