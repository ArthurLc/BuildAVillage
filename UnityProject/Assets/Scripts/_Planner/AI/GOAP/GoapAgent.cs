using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;


public sealed class GoapAgent : MonoBehaviour {

	private FSM stateMachine;

	private FSM.FSMState idleState; // finds something to do
	private FSM.FSMState moveToState; // moves to a target
	private FSM.FSMState performActionState; // performs an action

    private Animator anim;
    private bool IsStateActualized;

    private HashSet<GoapAction> availableActions;
	private Queue<GoapAction> currentActions;

	private IGoap dataProvider; // this is the implementing class that provides our world data and listens to feedback on planning

	private GoapPlanner planner;

    private HashSet<KeyValuePair<string, object>> currentGoal;

    public enum AgentType
    {
        Agent2D,
        Agent3D
    }

    [SerializeField] private AgentType agentType = AgentType.Agent2D;
    public AgentType GetAgentType
    {
        get { return agentType; }
    }

    public Transform rightHand;

    void Start () {
		stateMachine = new FSM ();
		availableActions = new HashSet<GoapAction> ();
		currentActions = new Queue<GoapAction> ();
		planner = new GoapPlanner ();
		findDataProvider ();
		createIdleState ();
		createMoveToState ();
		createPerformActionState ();
		stateMachine.pushState (idleState);
		loadActions ();

        if (agentType == AgentType.Agent3D)
        {
            anim = GetComponent<Animator>();
            IsStateActualized = false;
        }
    }
	

	void Update () {
		stateMachine.Update (this.gameObject);
	}


	public void addAction(GoapAction a) {
		availableActions.Add (a);
	}

	public GoapAction getAction(Type action) {
		foreach (GoapAction g in availableActions) {
			if (g.GetType().Equals(action) )
			    return g;
		}
		return null;
	}

	public void removeAction(GoapAction action) {
		availableActions.Remove (action);
	}

	private bool hasActionPlan() {
		return currentActions.Count > 0;
	}

	private void createIdleState() {
		idleState = (fsm, gameObj) => {
            // GOAP planning
            if (agentType == AgentType.Agent3D)
            {
                if (IsStateActualized == false)
                {
                    anim.SetTrigger("Idle");
                    IsStateActualized = true;
                }
            }

            // get the world state and the goal we want to plan for
            HashSet<KeyValuePair<string,object>> worldState = dataProvider.getWorldState();
			HashSet<KeyValuePair<string,object>> goal = dataProvider.createGoalState();

			// Plan
			Queue<GoapAction> plan = planner.plan(gameObject, availableActions, worldState, goal);
			if (plan != null)
            {
				// we have a plan, hooray!
				currentActions = plan;
				dataProvider.planFound(goal, plan);

				fsm.popState(); // move to PerformAction state
				fsm.pushState(performActionState);

                if (agentType == AgentType.Agent3D)
                {
                    IsStateActualized = false;
                }

                currentGoal = goal;
            }
            else
            {
				// ugh, we couldn't get a plan
				Debug.Log("<color=orange>Failed Plan:</color>"+prettyPrint(goal));
				dataProvider.planFailed(goal);
				fsm.popState (); // move back to IdleAction state
				fsm.pushState (idleState);
                currentGoal = null;

            }

		};
	}
	
	private void createMoveToState() {
		moveToState = (fsm, gameObj) => {
            // move the game object
            if (agentType == AgentType.Agent3D)
            {
                if (IsStateActualized == false)
                {
                    anim.SetTrigger("Walk");
                    IsStateActualized = true;
                }
            }

            GoapAction action = currentActions.Peek();
			if (action.requiresInRange() && action.target == null) {
				Debug.Log("<color=red>Fatal error:</color> Action requires a target but has none. Planning failed. You did not assign the target in your Action.checkProceduralPrecondition()");
				fsm.popState(); // move
				fsm.popState(); // perform
				fsm.pushState(idleState);
                return;
			}
            
			// get the agent to move itself
			if ( dataProvider.moveAgent(action) )
            {
                if (agentType == AgentType.Agent3D) {
                    IsStateActualized = false;
                }
                fsm.popState();
			}

			/*MovableComponent movable = (MovableComponent) gameObj.GetComponent(typeof(MovableComponent));
			if (movable == null) {
				Debug.Log("<color=red>Fatal error:</color> Trying to move an Agent that doesn't have a MovableComponent. Please give it one.");
				fsm.popState(); // move
				fsm.popState(); // perform
				fsm.pushState(idleState);
				return;
			}

			float step = movable.moveSpeed * Time.deltaTime;
			gameObj.transform.position = Vector3.MoveTowards(gameObj.transform.position, action.target.transform.position, step);

			if (gameObj.transform.position.Equals(action.target.transform.position) ) {
				// we are at the target location, we are done
				action.setInRange(true);
				fsm.popState();
			}*/
		};
	}
	
	private void createPerformActionState() {

		performActionState = (fsm, gameObj) => {
			// perform the action

			if (!hasActionPlan()) {
				// no actions to perform
				Debug.Log("<color=red>Done actions</color>");
				fsm.popState();
				fsm.pushState(idleState);
				dataProvider.actionsFinished();
				return;
			}

			GoapAction action = currentActions.Peek();
			if ( action.isDone() ) {
				// the action is done. Remove it so we can perform the next one
                HashSet<KeyValuePair<string, object>> goal = dataProvider.createGoalState();
                if (currentGoal != null && goal != null &&
                currentGoal.Count > 0 && goal.Count > 0
                && inState(currentGoal, goal))
                    currentActions.Dequeue();
                else
                {
                    // no actions left, move to Plan state
                    fsm.popState();
                    fsm.pushState(idleState);
                    dataProvider.actionsFinished();
                    return;
                }
            }

			if (hasActionPlan()) {
				// perform the next action
				action = currentActions.Peek();
				bool inRange = action.requiresInRange() ? action.isInRange() : true;

				if ( inRange ) {
					// we are in range, so perform the action
					bool success = action.perform(gameObj);

					if (!success) {
						// action failed, we need to plan again
						fsm.popState();
						fsm.pushState(idleState);
						dataProvider.planAborted(action);
					}
				} else {
					// we need to move there first
					// push moveTo state
					fsm.pushState(moveToState);
				}

			} else {
				// no actions left, move to Plan state
				fsm.popState();
				fsm.pushState(idleState);
				dataProvider.actionsFinished();
			}

		};
	}

	private void findDataProvider() {
		foreach (Component comp in gameObject.GetComponents(typeof(Component)) ) {
			if ( typeof(IGoap).IsAssignableFrom(comp.GetType()) ) {
				dataProvider = (IGoap)comp;
				return;
			}
		}
	}

	private void loadActions ()
	{
		GoapAction[] actions = gameObject.GetComponents<GoapAction>();
		foreach (GoapAction a in actions) {
			availableActions.Add (a);
		}
		Debug.Log("Found actions: "+prettyPrint(actions));
	}

	public static string prettyPrint(HashSet<KeyValuePair<string,object>> state) {
		String s = "";
		foreach (KeyValuePair<string,object> kvp in state) {
			s += kvp.Key + ":" + kvp.Value.ToString();
			s += ", ";
		}
		return s;
	}

	public static string prettyPrint(Queue<GoapAction> actions) {
		String s = "";
		foreach (GoapAction a in actions) {
			s += a.GetType().Name;
			s += "-> ";
		}
		s += "GOAL";
		return s;
	}

	public static string prettyPrint(GoapAction[] actions) {
		String s = "";
		foreach (GoapAction a in actions) {
			s += a.GetType().Name;
			s += ", ";
		}
		return s;
	}

	public static string prettyPrint(GoapAction action) {
		String s = ""+action.GetType().Name;
		return s;
	}

    /**
    * Check that all items in 'test' are in 'state'. If just one does not match or is not there
    * then this returns false.
    */
    private bool inState(HashSet<KeyValuePair<string, object>> test, HashSet<KeyValuePair<string, object>> state)
    {
        bool allMatch = true;
        foreach (KeyValuePair<string, object> t in test)
        {
            bool match = false;
            foreach (KeyValuePair<string, object> s in state)
            {
                if (s.Equals(t))
                {
                    match = true;
                    break;
                }
            }
            if (!match)
                allMatch = false;
        }
        return allMatch;
    }
}
