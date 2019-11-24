using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace ProcessMining
{
    public class Node
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

    public class PetriNet
    {

        private Dictionary<int, Node> _places;
        private Dictionary<int, Node> _transitions;        
        private Dictionary<string, int> _transitionsNameToId;
        private int _sinkPlaceId;
        private int _entrancePlaceId;

        public Dictionary<string, int> TransitionsNameToId { get => _transitionsNameToId;}
        public Dictionary<int, Node> Transitions { get => _transitions; }

        public PetriNet()
        {
            _places = new Dictionary<int, Node>(){};
            _transitions = new Dictionary<int, Node>(){};
            _transitionsNameToId = new Dictionary<string, int>(){};
            _sinkPlaceId = 1;
            _entrancePlaceId = 1;
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

        public void AddEdge(int source, int target)
        {
            if (_places.ContainsKey(source))
            {
                _places[source].children.Add(_transitions[target]);
                _transitions[target].parents.Add(_places[source]);
            }
            else
            {
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

        public List<int> GetIdsOfChildPlaces(int transitionId)
        {
            return _transitions[transitionId].children.Select(node => node.id).ToList();
        }


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

        public List<int> GetIdsOfChildTransitions(int placeId)
        {
            return _places[placeId].children.Select(node => node.id).ToList();
        }

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
            AddMarking(1);
        }

        class CostPath
        {
            public int cost;
            public List<int> path;

            public CostPath()
            {
                cost = 0;
                path = new List<int>();
            }
        }

        public int GetCost(int from, int to)
        {
            return 1;
        }

        // Not tested
        public int GetShortestPath(int from, int to, Dictionary<int, List<int>> cost_dict)
        {
            var queue = new PriorityQueue<Int32,Int32>();

            var minimum = this.GetLowestTransitionId();
            var maximum = this.GetHighestPlaceId();

            Dictionary<int,CostPath> dist = new Dictionary<int,CostPath>();

            for (int i = minimum; i < maximum; i++)
            {
                if (0 == i)
                {
                    continue;
                }

                dist[i] = (new CostPath(){cost = Int32.MaxValue});
            }

            var minPath = dist[from].path;
            minPath.Add(from);
            dist[from] = new CostPath(){cost = 0, path = minPath};

            HashSet<int> visited = new HashSet<int>();


            queue.Enqueue(0,from);
            while (!queue.IsEmpty)
            {
                var element = queue.Dequeue();
                var id = element.Value;

                visited.Add(id);

                List<int> elements = null;
                if (id > 0)
                {
                    //transitions are with negative ids
                    elements = GetIdsOfChildTransitions(id);
                }
                else
                {
                    elements = GetIdsOfChildPlaces(id);
                }

                foreach (var elementId in elements)
                {
                    if (!visited.Contains(elementId))
                    {
                        var costSoFar = element.Key + GetCost(id, elementId);

                        var pathSoFar = dist[id].path;
                        pathSoFar.Add(elementId);

                        dist[elementId] = new CostPath() { cost = costSoFar, path = pathSoFar };

                        queue.Enqueue(costSoFar, elementId);

                    }
                }

            }

            return dist[to].cost;
        }
    }
}
