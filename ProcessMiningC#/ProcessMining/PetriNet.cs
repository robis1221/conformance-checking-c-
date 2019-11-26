﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace ProcessMining
{
    class Node
    {
        private Dictionary<Node, bool> parentActivationRequest;

        public readonly string name;
        public List<Node> parents { get; set; }
        public List<Node> children { get; set; }
        public int token { get; set; }
        public int id { get; set; }
        public bool isActivated { get; set; }
        

        public Node(int id, string name = "")
        {
            this.parents = new List<Node>(){};
            this.children = new List<Node>(){};
            this.name = name;
            this.id = id;
            this.isActivated = false;
            this.token = 0;
            parentActivationRequest = new Dictionary<Node, bool>(){};

        }

        public void TryToActivate(Node parentObj)
        {
            if (!parentActivationRequest.ContainsKey(parentObj))
            {
                parentActivationRequest.Add(parentObj,true);
            }

            if (this.parents.Count == parentActivationRequest.Count)
            {
                this.isActivated = true;
                this.parentActivationRequest = new Dictionary<Node, bool>(){};
            }
        }

        public void Deactivate()
        {
            this.parentActivationRequest = new Dictionary<Node, bool>(){};
            this.isActivated = false;
        }


    }

    class PetriNet
    {

        private Dictionary<int, Node> _places;
        private Dictionary<int, Node> _transitions;
        private Dictionary<string, int> _transitionsNameToId;
        private int _sinkPlaceId;
        private int _entrancePlaceId;
        private Dictionary<int, CostPath> _dist;
        // first key is always a transition
        private Dictionary<int, Dictionary<int, int>> _costDictionary;



        public PetriNet()
        {
            _places = new Dictionary<int, Node>(){};
            _transitions = new Dictionary<int, Node>(){};
            _transitionsNameToId = new Dictionary<string, int>(){};
            _sinkPlaceId = 1;
            _entrancePlaceId = 1;
            _dist = null;
            _costDictionary = new Dictionary<int, Dictionary<int, int>>();
        }

        public PetriNet(PetriNet other)
        {
            _places = new Dictionary<int, Node>(other._places);
            _transitions = new Dictionary<int, Node>(other._transitions);
            _transitionsNameToId = new Dictionary<string, int>(other._transitionsNameToId);
            _sinkPlaceId = other._sinkPlaceId;
            _entrancePlaceId = other._entrancePlaceId;
            _dist = new Dictionary<int, CostPath>(other._dist);
            _costDictionary = new Dictionary<int, Dictionary<int, int>>(other._costDictionary);
        }

        public int GetSinkPlace()
        {
            if (1 == this._sinkPlaceId)
            {
                foreach (var pair in _places)
                {
                    var node = pair.Value;
                    if (0 == node.children.Count)
                    {
                        _sinkPlaceId = pair.Key;
                        break;
                    }
                }
            }

            return _sinkPlaceId;
        }



        public int GetHighestPlaceId()
        {
            return _places.Keys.Max();
        }

        public int GetLowestTransitionId()
        {
            return _transitions.Keys.Min();
        }

        public void AddPlace(int id)
        {
            _places.Add(id,new Node(id));
        }

        public void AddTransition(int id, string name)
        {
            _transitions.Add(id,new Node(id,name));
            _transitionsNameToId.Add(name,id);
        }

        public int TransitionNameToId(string name)
        {
            return _transitionsNameToId.ContainsKey(name) ? _transitionsNameToId[name] : 0;
        }

        public void AddEdge(int source, int target, int cost = 1)
        {
            //add a cost to this edge
            if (_places.ContainsKey(source))
            {
                if (_costDictionary.ContainsKey(target))
                {
                    _costDictionary[target][source] = cost;
                }
                else
                {
                    _costDictionary[target] = new Dictionary<int, int>(){{ source, cost}};
                }
                _places[source].children.Add(_transitions[target]);
                _transitions[target].parents.Add(_places[source]);
            }
            else
            {
                if (_costDictionary.ContainsKey(source))
                {
                    _costDictionary[source][target] = cost;
                }
                else
                {
                    _costDictionary[source] = new Dictionary<int, int>() { { target, cost } };
                }
                _transitions[source].children.Add(_places[target]);
                _places[target].parents.Add(_transitions[source]);
            }
        }

        public int GetTokens(int placeId)
        {
            return _places[placeId].token;
        }

        public bool IsEnabled(int transitionId)
        {
            return _transitions[transitionId].isActivated;
        }

        public void AddMarking(int placeId, Node obj= null)
        {
            if (null == obj)
            {
                _places[placeId].token += 1;
                foreach (var kid in _places[placeId].children)
                {
                    kid.TryToActivate(_places[placeId]);
                }
            }
            else
            {
                obj.token += 1;
                foreach (var kid in obj.children)
                {
                    kid.TryToActivate(obj);
                }
            }
        }

        public void FireTransition(int transitionId)
        {
            if (_transitions[transitionId].isActivated)
            {
                var element = _transitions[transitionId];

                // Remove the token from the previous place
                foreach (var parent in element.parents)
                {
                    if (parent.token > 0)
                    {
                        parent.token -= 1;
                    }
                }

                // Deactivate all transitions connected to the previous place
                foreach (var parent in element.parents)
                {
                    foreach (var kid in parent.children)
                    {
                        kid.isActivated = false;
                    }
                }
                
                foreach (var kid in element.children)
                {
                    this.AddMarking(0,kid);
                }
            }
        }

        public int GetNumberOfParents(int transitionId)
        {
            return _transitions[transitionId].parents.Count;
        }

        public int GetNumberOfChildren(int transitionId)
        {
            return _transitions[transitionId].children.Count;
        }

        public List<int> GetIdsOfParentPlaces(int transitionId)
        {
            return _transitions[transitionId].parents.Select(node => node.id).ToList();
        }

        public List<int> GetIdsOfChildPlaces(int transitionId)
        {
            return _transitions[transitionId].children.Select(node => node.id).ToList();
        }

        public List<int> GetIdsOfChildTransitions(int placeId)
        {
            return _places[placeId].children.Select(node => node.id).ToList();
        }

        // Test

        public List<int> GetIdsOfChildTransactions(int transactionId)
        {
            var childTransactionIds = new List<int>();

            var childPlacesIds = GetIdsOfChildPlaces(transactionId);
            foreach (var parentPlaceId in childPlacesIds)
            {
                var childIds = _places[parentPlaceId].children.Select(child => child.id).ToList();
                childTransactionIds.AddRange(childIds);
            }

            return childTransactionIds;
        }

        public List<int> GetIdsOfParentTransactions(int transactionId)
        {
            var parentTransactionIds = new List<int>();

            var parentPlacesIds = GetIdsOfParentPlaces(transactionId);
            foreach (var parentPlaceId in parentPlacesIds)
            {
                var parentIds = _places[parentPlaceId].parents.Select(parent => parent.id).ToList();
                parentTransactionIds.AddRange(parentIds);
            }

            return parentTransactionIds;
        }

        // End of test

        public List<int> GetIdsOfParentTransitions(int placeId)
        {
            return _places[placeId].parents.Select(node => node.id).ToList();
        }

        public void InitializeTokens()
        {
            foreach (var place in _places.Values)
            {
                place.token = 0;
            }
        }
        public class CostPath
        {
            public int cost;
            public List<int> path;

            public CostPath()
            {
                cost = 0;
                path = new List<int>();
            }
        }

        private int GetCost(int from, int to)
        {
            int firstKey = 0 > from ? from : to;
            int secondKey = 0 < from ? from : to;
            return _costDictionary[firstKey][secondKey];
        }

        // Simple Dijkstra
        private Dictionary<int, CostPath> GetShortestPath(int from, int to)
        {
            var queue = new PriorityQueue<Int32,Int32>();

            var minimum = this.GetLowestTransitionId();
            var maximum = this.GetHighestPlaceId();

            Dictionary<int,CostPath> dist = new Dictionary<int,CostPath>();

            for (int i = minimum; i <= maximum; i++)
            {
                if (0 == i)
                {
                    continue;
                }

                dist[i] = (new CostPath(){cost = Int32.MaxValue});
            }

            var minPath = dist[to].path;
            minPath.Add(to);
            dist[to] = new CostPath(){cost = 0, path = minPath};

            HashSet<int> visited = new HashSet<int>();


            queue.Enqueue(0,to);
            while (!queue.IsEmpty)
            {
                var element = queue.Dequeue();
                var id = element.Value;

                visited.Add(id);

                List<int> adjacentNodes = null;
                if (id > 0)
                {
                    //transitions are with negative ids
                    adjacentNodes = GetIdsOfParentTransitions(id);
                }
                else
                {
                    adjacentNodes = GetIdsOfParentPlaces(id);
                }

                foreach (var elementId in adjacentNodes)
                {
                    if (!visited.Contains(elementId))
                    {
                        var costSoFar = element.Key + GetCost(id, elementId);

                        List<int> pathSoFar = new List<int>(dist[id].path);
                        pathSoFar.Add(elementId);

                        dist[elementId] = new CostPath() { cost = costSoFar, path = pathSoFar };

                        queue.Enqueue(costSoFar, elementId);

                    }
                }
            }

            return dist;
        }

        public List<int> GetValidShortestPath(int from, int to)
        {
            if (null == _dist)
            {
                _dist = GetShortestPath(from, to);
            }

            var copyOfPetriNet = new PetriNet(this);
            // take the shortest path from this transition to the end
            var path = _dist[from].path;
            path.Reverse();
            // remove positive nodes (places)
            path = path.Where(a => a < 0).ToList();

            List<int> finalPath = new List<int>();

            // check if it can be executed
            // since we are calculating the path we should have the first transition activated
            foreach (var transitionId in path)
            {
                bool isEnabled = copyOfPetriNet.IsEnabled(transitionId);
                // find the token backwards
                if (!isEnabled)
                {
                    var startingTransition = transitionId;
                    var parentPlaces = copyOfPetriNet.GetIdsOfParentPlaces(startingTransition);
                    foreach (var placeId in parentPlaces)
                    {
                        if (0 == copyOfPetriNet.GetTokens(placeId))
                        {
                            var pathsFromEntranceToThisPlace =
                                copyOfPetriNet.GetAllPathsBetweenTwoVertices(1, placeId);
                            var helpPaths = new List<int>();

                            foreach (var pathFromEntranceToThisTransition in pathsFromEntranceToThisPlace)
                            {
                                // if there is a token in the last place of the path then we don't need to look for other one
                                for (int index = pathFromEntranceToThisTransition.Count - 1; index >= 0; index--)
                                {
                                    if (pathFromEntranceToThisTransition[index] > 0)
                                    {
                                        if (0 != copyOfPetriNet.GetTokens(pathFromEntranceToThisTransition[index]))
                                        {
                                            if (index == pathFromEntranceToThisTransition.Count - 1)
                                            {
                                                // if there is a token in the last place of the path then we don't need to look for other one
                                                break;
                                            }
                                            var count = pathFromEntranceToThisTransition.Count - index;
                                            var portionToBeAddedToPath = pathFromEntranceToThisTransition.GetRange(index, count);
                                            portionToBeAddedToPath = portionToBeAddedToPath.Where(a => a < 0).ToList();
                                            foreach (var helpTransition in portionToBeAddedToPath)
                                            {
                                                copyOfPetriNet.FireTransition(helpTransition);
                                            }
                                            helpPaths.AddRange(portionToBeAddedToPath);
                                            break;
                                        }
                                    }
                                }
                            }
                            finalPath.AddRange(helpPaths);
                        }
                    }
                }
                copyOfPetriNet.FireTransition(transitionId);
            }
            finalPath.AddRange(path);
            return finalPath;
        }

        /// <summary>
        /// Finds all the paths between the two vertices
        /// </summary>
        /// <param name="id1"></param>
        /// <param name="Id2"></param>
        /// <returns></returns>
        private List<List<int>> GetAllPathsBetweenTwoVertices(int id1,int Id2)
        {
            List<List<int>> paths = new List<List<int>>();
            

            HashSet<int> visited = new HashSet<int>();

            Stack<int> currentPath = new Stack<int>();

            GetAllPathsBetweenTwoVerticesHelper(id1, Id2, visited, currentPath, paths);

            return paths;
        }

        private void GetAllPathsBetweenTwoVerticesHelper(int id1, int id2, HashSet<int> visited, Stack<int> currentPath, List<List<int>> paths)
        {
            if (id1 == id2)
            {
                currentPath.Push(id1);
                var listPath = currentPath.ToList();
                currentPath.Pop();
                listPath.Reverse();
                paths.Add(listPath);
                return;
            }
            else
            {
                currentPath.Push(id1);
                visited.Add(id1);
                List<int> adjacentNodes = id1 > 0 ? GetIdsOfChildTransitions(id1) : GetIdsOfChildPlaces(id1);
                foreach (int adjacentNodeIds in adjacentNodes)
                {
                    if (!visited.Contains(adjacentNodeIds))
                    {
                        GetAllPathsBetweenTwoVerticesHelper(adjacentNodeIds, id2,visited,currentPath,paths);
                    }
                }
            }
            
            currentPath.Pop();
            visited.Remove(id1);            
        }
    }
}
