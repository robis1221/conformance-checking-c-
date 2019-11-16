using System.Collections.Generic;
using System.Linq;

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

        public Dictionary<string, int> TransitionsNameToId { get => _transitionsNameToId;}
        public Dictionary<int, Node> Transitions { get => _transitions; }

        public PetriNet()
        {
            _places = new Dictionary<int, Node>(){};
            _transitions = new Dictionary<int, Node>(){};
            _transitionsNameToId = new Dictionary<string, int>(){};
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

        public void InitializeTokens()
        {
            foreach (var place in _places.Values)
            {
                place.token = 0;
            }
            AddMarking(1);
        }
    }
}
