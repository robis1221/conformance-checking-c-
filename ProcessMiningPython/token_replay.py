class Node():
    def __init__(self, name, transition_id= None):
      self.parents = []
      self.name = name
      self.id = transition_id
      self.is_activated = False
      self.parent_activation_request = {}
      self.children = []
      self.token = 0
    
    def try_to_activate(self,parent_obj):
        if parent_obj not in self.parent_activation_request:
            self.parent_activation_request[parent_obj] = 1
        
        if len(self.parents) == len(self.parent_activation_request):
            self.is_activated = True
            self.parent_activation_request = {}

    def deactivate(self):
        self.parent_activation_request = {}
        self.is_activated = False



class PetriNet():

    def __init__(self):
        self.places= {}
        self.transitions = {}
        self.head = None

    def add_place(self, name):
        node = Node(name)   
        self.places[name] = node

    def add_transition(self, name, id):
        node = Node(name,id)
        self.transitions[id] = node
    
    def transition_name_to_id(self,name):
        #ToDo: will be better to keep dictionary the other way around
        for id_key, item in self.transitions.items(): 
            if item.name == name:
                return id_key
        return None

    def initialize_tokens(self):
        for id,place in self.places.items():
            place.token = 0
        # add marking in the start place
        self.add_marking(1)

    def add_edge(self, source, target):
        if source in self.places:
          self.places[source].children.append(self.transitions[target])
          self.transitions[target].parents.append(self.places[source])
          return self
        else:
          self.transitions[source].children.append(self.places[target])
          self.places[target].parents.append(self.transitions[source])
          return self

    def get_tokens(self, place):
        return self.places[place].token

    def is_enabled(self, transition):
        return self.transitions[transition].is_activated

    def add_marking(self, place, obj = None):
        if not obj:
            self.places[place].token += 1
            for kid in self.places[place].children:
                kid.try_to_activate(self.places[place])
        else:
            obj.token +=1
            for kid in obj.children:
                kid.try_to_activate(obj)

    def fire_transition(self, transition):
        if self.transitions[transition].is_activated:
          element = self.transitions[transition]
          # remove the token from the previous place
          for parent in element.parents:
              if parent.token > 0:
                  parent.token -= 1
          # deactivate all transitions connected to the previous place
          for kid in element.parents[0].children:
              kid.is_activated = False
          for child in element.children:
            self.add_marking(0,child)

    # used for the token replay
    def get_number_of_parents(self,transition_id):
        return len(self.transitions[transition_id].parents)
    
    def get_number_of_children(self,transition_id):
        return len(self.transitions[transition_id].children)

    def get_ids_of_parents_places(self,transition_id):
        places_ids = [x.name for x in self.transitions[transition_id].parents]
        return places_ids

    def get_ids_of_children_places(self,transition_id):
        places_ids = [x.name for x in self.transitions[transition_id].children]
        return places_ids

def read_from_file(file_path):

    log_dict = {}
    from xml.dom import minidom
    from datetime import datetime

    xmldoc = minidom.parse(file_path)
    itemlist = xmldoc.getElementsByTagName('trace')
    for item in itemlist:
        case = ""
        for child in item.childNodes:
            
            if 'string' == child.nodeName:
                case = str(child._attrs['value'].nodeValue)
                if case not in log_dict:
                    log_dict[case] = []
            if 'event' == child.nodeName:
                event_data = {}
                for event_kid in child.childNodes:
                    if hasattr(event_kid,'_attrs'):
                        key_value = event_kid._attrs['key'].nodeValue
                        if key_value == 'time:timestamp':
                            event_data[key_value] = datetime.strptime(event_kid._attrs['value'].nodeValue[:-6], '%Y-%m-%dT%H:%M:%S')
                        elif key_value == 'cost':
                            event_data[key_value] = int(event_kid._attrs['value'].nodeValue)
                        else:
                            event_data[key_value] = str(event_kid._attrs['value'].nodeValue)
                value = log_dict[case]
                value.append(event_data)
                log_dict[case]=value
    return log_dict

# alpha algorithm 

def take_sets(log_dict):
    transition_set = set()
    start_transitions = set()
    end_transitions = set()
    for key in log_dict:
        first = 0
        for i,item in enumerate(log_dict[key]):
            name = item['concept:name']
            if 0 == i:
                start_transitions.add(name)
            elif len(log_dict[key]) - 1 == i:
                end_transitions.add(name)
            transition_set.add(name)

    return transition_set, start_transitions, end_transitions

def take_direct_successions(log_dict):
    direct_successions= set()
    for case in log_dict:
        for i in range(len(log_dict[case])-1):
            x = log_dict[case][i]['concept:name']
            y = log_dict[case][i+1]['concept:name']
            direct_successions.add((x,y))
    return direct_successions

def take_casualities(log_dict, direct_successions):
    import copy
    casualities = copy.deepcopy(direct_successions)
    for case in log_dict:
        for i in range(len(log_dict[case])-1):
            x = log_dict[case][i]['concept:name']
            y = log_dict[case][i+1]['concept:name']
            if (y,x) in casualities:
                casualities.remove((y,x))
    return casualities


def take_not_connected(transition_set,direct_successions):
    choices = set()
    for x in transition_set:
        for y in transition_set:
            if((x,y) not in direct_successions and (y,x) not in direct_successions):
                    choices.add((x,y))
    return choices

def check_set(A, ncs):
    if None != ncs:
        for event in A:
            for event2 in A:
                if (event, event2) not in ncs:
                    return False
        return True
    else:
        return False


def check_outsets(A, B, cs):
    for event in A:
        for event2 in B:
            if (event, event2) not in cs:
                return False
    return True

def make_xl_set(all_tasks, direct_followers, causalities, no_causalities):
    import itertools
    xl = set()
    subsets = set()
    for i in range(1, len(all_tasks)):
        for s in itertools.combinations(all_tasks, i):
            subsets.add(s)
    for a in subsets:
        reta = check_set(a, no_causalities)
        for b in subsets:
            retb = check_set(b, no_causalities)
            if reta and retb and \
               check_outsets(a, b, causalities):
                xl.add((a, b))
    return xl

def make_yl_set(xl):
    import copy
    yl = copy.deepcopy(xl)
    for a in xl:
        A = a[0]
        B = a[1]
        for b in xl:

            if set(A).issubset(b[0]) and set(B).issubset(b[1]):
                if a != b:
                    yl.discard(a)
    return yl


def alpha(log_dict):
    # 1. Take all transitions (array of unique values)
    transition_set, start_transitions, end_transitions = take_sets(log_dict)
    # 2. Take all start activities
    # 3. Take all end activities
    
    # Utilize the creation of the bipartite graph by
    direct_successions = take_direct_successions(log_dict)
    casualities = take_casualities(log_dict,direct_successions)
    not_connected = take_not_connected(transition_set,direct_successions)

    xl = make_xl_set(transition_set,direct_successions,casualities,not_connected)
    yl = make_yl_set(xl)

    # build petri net
    p = PetriNet()
    place_id = 1
    transition_id = -1
    place_dict = {}

    # add first place
    p.add_place(place_id)
    place_dict['init'] = place_id
    place_id+=1

    # add all transitions
    for transition in transition_set:
        p.add_transition(transition,transition_id)
        transition_id-=1
    
    # add all other places
    for place in yl:
        p.add_place(place_id)
        place_dict[place] = place_id
        place_id+=1
    
    # add the output place
    p.add_place(place_id)
    place_dict['out'] = place_id

    # build edges

    # first edge is from init place to start_transitions
    for start in start_transitions:
        p.add_edge(1,p.transition_name_to_id(start))
    
    # other edge which is sure is from the end_transitions to the out place
    for end in end_transitions:
        p.add_edge(p.transition_name_to_id(end),place_id)
    
    # add all other edges according to Y_l
    for place in yl:
        place_id = place_dict[place]
        for trans in place[0]:
            p.add_edge(p.transition_name_to_id(trans),place_id)
        for trans in place[1]:
            p.add_edge(place_id,p.transition_name_to_id(trans))

    p.add_marking(1)

    return p


def get_frequencies_from_log(log_dict):
    class TraceItem():
        def __init__(self,arr, count):
            self.sequence = arr
            self.occurance = count

    no_duplicates = {}
    for item in log_dict.items():
        trace = item[1]
        trace_arr = []
        for _ in trace:
            trace_arr.append(_['concept:name'])
        if repr(trace_arr) in no_duplicates:
            no_duplicates[repr(trace_arr)].occurance = no_duplicates[repr(trace_arr)].occurance + 1
            # no_duplicates[repr(trace_arr)] = (no_duplicates[repr(trace_arr)][0],no_duplicates[repr(trace_arr)][1] + 1)
        else:
            no_duplicates[repr(trace_arr)] = TraceItem(trace_arr,1)
    
    return no_duplicates


def fitness_token_replay(log_dict, mined_petrinet):
    frequency_log = get_frequencies_from_log(log_dict)
    
    class TokenReplayInfo():
        def __init__(self,produced=0, consumed=0, missed=0, remained=0):
            self.produced = produced
            self.consumed = consumed
            self.missed = missed
            self.remained = remained
        def multiply_by(self,n):
            self.produced *= n
            self.consumed *= n
            self.missed *= n
            self.remained *= n

        def sum_up(self,other:"TokenReplayInfo"):
            self.produced += other.produced
            self.consumed += other.consumed
            self.missed += other.missed
            self.remained += other.remained

        def compute_fitness(self):
            return 0.5*(1-(self.missed/self.consumed)) + 0.5*(1-(self.remained/self.produced))

    
    result = TokenReplayInfo()


    # The mined model is coming with a token in the start place by default
    for key, trace_info in frequency_log.items():
        # clean the petri net tokens
        mined_petrinet.initialize_tokens()
        sequence_token_info = TokenReplayInfo(produced=1)
        for step in trace_info.sequence:
            transition_id = mined_petrinet.transition_name_to_id(step)
            is_enabled = mined_petrinet.is_enabled(transition_id)

            if not is_enabled:
                # produce a correct amount of missing tokens
                places_before = mined_petrinet.get_ids_of_parents_places(transition_id)
                for place in places_before:
                    if(0 == mined_petrinet.get_tokens(place)):
                        mined_petrinet.add_marking(place)
                        sequence_token_info.missed += 1

            sequence_token_info.consumed += mined_petrinet.get_number_of_parents(transition_id)
            sequence_token_info.produced += mined_petrinet.get_number_of_children(transition_id)
            mined_petrinet.fire_transition(transition_id)

                
        last_transition_id = mined_petrinet.transition_name_to_id(trace_info.sequence[len(trace_info.sequence)-1])

        if( 1 != mined_petrinet.get_number_of_children(last_transition_id)):
            print("Error, the last transition should have only out place")
        # consume the tokens left in the out place
        sequence_token_info.consumed += mined_petrinet.get_tokens(mined_petrinet.transitions[last_transition_id].children[0].name)
        
        # check if there are any tokens left behind in the net
        for step in trace_info.sequence:
            transition_id = mined_petrinet.transition_name_to_id(step)

            sequence_token_info.remained +=sum([mined_petrinet.get_tokens(x) for x in mined_petrinet.get_ids_of_parents_places(transition_id)])
        
        sequence_token_info.multiply_by(trace_info.occurance)
        result.sum_up(sequence_token_info)
    
    return result.compute_fitness()

            


    


def test_scenario_1():
    log = read_from_file("extension-log.xes")
    log_noisy = read_from_file("extension-log-noisy.xes")
    

    mined_model = alpha(log)
    print (round(fitness_token_replay(log, mined_model), 5))
    print (round(fitness_token_replay(log_noisy, mined_model), 5))


test_scenario_1()