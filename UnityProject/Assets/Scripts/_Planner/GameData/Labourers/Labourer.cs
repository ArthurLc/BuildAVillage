using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;


/**
 * A general labourer class.
 * You should subclass this for specific Labourer classes and implement
 * the createGoalState() method that will populate the goal for the GOAP
 * planner.
 */
public abstract class Labourer : MonoBehaviour, IGoap
{
	public GoapAgent agent;
	public BackpackComponent backpack;
    [SerializeField] private NavMeshAgent navMeshAgent;
    public float moveSpeed = 1;
	public float currentHunger = 500;
	public float maxHunger = 500;
    public int minHungerBeforeStarving = 50;
    public int currentLife = 100;
    public int maxLife = 100;
    [TextArea]
    public string description = "\"Need a description\"";

    void Start ()
	{
		if (backpack == null)
			backpack = gameObject.AddComponent <BackpackComponent>( ) as BackpackComponent;
		if (backpack.toolType != "" && backpack.tool == null) {
			GameObject prefab = Resources.Load<GameObject> (backpack.toolType);
			GameObject tool = Instantiate (prefab, transform.position, transform.rotation) as GameObject;
			backpack.tool = tool;
            switch (agent.GetAgentType)
            {
                case GoapAgent.AgentType.Agent2D:
                    tool.transform.parent = transform; // attach the tool
                    break;
                case GoapAgent.AgentType.Agent3D:
                    tool.transform.parent = agent.rightHand; // attach the tool
                    tool.transform.localPosition = prefab.transform.localPosition;
                    tool.transform.localRotation = prefab.transform.localRotation;
                    break;
                default:
                    break;
            }
		}

        if (agent == null)
            agent = gameObject.GetComponent<GoapAgent>();

        if (navMeshAgent == null)
            navMeshAgent = GetComponent<NavMeshAgent>();

        navMeshAgent.speed = moveSpeed;
        currentHunger = maxHunger;
        currentLife = maxLife;
    }


	void FixedUpdate ()
	{
        currentHunger -= Time.deltaTime;
    }

	/**
	 * Key-Value data that will feed the GOAP actions and system while planning.
	 */
	public HashSet<KeyValuePair<string,object>> getWorldState () {
		HashSet<KeyValuePair<string,object>> worldData = new HashSet<KeyValuePair<string,object>> ();

		worldData.Add(new KeyValuePair<string, object>("hasOre", (backpack.numOre > 0) ));
		worldData.Add(new KeyValuePair<string, object>("hasLogs", (backpack.numLogs > 0) ));
		worldData.Add(new KeyValuePair<string, object>("hasFirewood", (backpack.numFirewood > 0) ));
        worldData.Add(new KeyValuePair<string, object>("hasWheat", (backpack.numWheat > 0)));
        worldData.Add(new KeyValuePair<string, object>("hasBreads", (backpack.numBreads > 0)));
        worldData.Add(new KeyValuePair<string, object>("hasTool", (backpack.tool != null) ));
        worldData.Add(new KeyValuePair<string, object>("hasGoldChest", (backpack.GetHasGoldChest)));
        worldData.Add(new KeyValuePair<string, object>("isStarving", (currentHunger <= minHungerBeforeStarving)));

        return worldData;
	}

    /**
	 * Implement in subclasses
	 */
    public virtual HashSet<KeyValuePair<string, object>> createGoalState()
    {
        HashSet<KeyValuePair<string, object>> goal = null;

        if (currentHunger < minHungerBeforeStarving && FindFood())
        {
            goal = new HashSet<KeyValuePair<string, object>>();
            goal.Add(new KeyValuePair<string, object>("isStarving", false));
        }
        return goal;
    }


    public void planFailed (HashSet<KeyValuePair<string, object>> failedGoal)
	{
		// Not handling this here since we are making sure our goals will always succeed.
		// But normally you want to make sure the world state has changed before running
		// the same goal again, or else it will just fail.
	}

	public void planFound (HashSet<KeyValuePair<string, object>> goal, Queue<GoapAction> actions)
	{
		// Yay we found a plan for our goal
		Debug.Log ("<color=green>Plan found</color> "+GoapAgent.prettyPrint(actions));
	}

	public void actionsFinished ()
	{
		// Everything is done, we completed our actions for this gool. Hooray!
		Debug.Log ("<color=blue>Actions completed</color>");
	}

	public void planAborted (GoapAction aborter)
	{
		// An action bailed out of the plan. State has been reset to plan again.
		// Take note of what happened and make sure if you run the same goal again
		// that it can succeed.
		Debug.Log ("<color=red>Plan Aborted</color> "+GoapAgent.prettyPrint(aborter));
	}

	public bool moveAgent(GoapAction nextAction) {
		// move towards the NextAction's target
		float step = moveSpeed * Time.deltaTime;

        switch (agent.GetAgentType)
        {
            case GoapAgent.AgentType.Agent2D:
                gameObject.transform.position = Vector3.MoveTowards(gameObject.transform.position, nextAction.target.transform.position, step);
                break;
            case GoapAgent.AgentType.Agent3D:
                navMeshAgent.SetDestination(nextAction.target.transform.position);
                break;
            default:
                break;
        }

		if (Vector3.Distance(gameObject.transform.position, nextAction.target.transform.position) < 1.0f ) {
			// we are at the target location, we are done
			nextAction.setInRange(true);
            if(agent.GetAgentType == GoapAgent.AgentType.Agent3D)
                navMeshAgent.SetDestination(nextAction.target.transform.position);
            return true;
		} else
			return false;
	}

    public bool FindFood()
    {
        // find the nearest tree that we can chop
        InnComponent[] inns = (InnComponent[])UnityEngine.GameObject.FindObjectsOfType(typeof(InnComponent));
        InnComponent closest = null;
        float closestDist = 0;

        foreach (InnComponent inn in inns)
        {
            if (closest == null && inn.numBreads > 0)
            {
                // first one, so choose it for now
                closest = inn;
                closestDist = (inn.gameObject.transform.position - transform.position).magnitude;
            }
            else
            {
                // is this one closer than the last?
                float dist = (inn.gameObject.transform.position - transform.position).magnitude;
                if (dist < closestDist && inn.numBreads > 0)
                {
                    // we found a closer one, use it
                    closest = inn;
                    closestDist = dist;
                }
            }
        }
        if (closest == null)
            return false;

        EatBreadAction eatAct = GetComponent<EatBreadAction>();

        if (eatAct != null)
        {
            eatAct.targetInn = closest;
            eatAct.target = eatAct.targetInn.gameObject;
        }
        return closest != null;
    }
}

