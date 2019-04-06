using System;
using UnityEngine;

public class PickUpGoldChestAction : GoapAction
{
	private bool hasGoldChest = false;
	private MetallurgyComponent targetMetal; // where we get the GoldChest from

	public PickUpGoldChestAction()
    {
        
        addPrecondition ("hasGoldChest", false); // don't get a GoldChest if we already have one
        addEffect ("hasGoldChest", true); // we now have a GoldChest
    }

	
	public override void reset ()
	{
		hasGoldChest = false;
		targetMetal = null;
	}
	
	public override bool isDone ()
	{
		return hasGoldChest;
	}

	public override bool requiresInRange ()
	{
		return true; // yes we need to be near a supply pile so we can pick up the tool
	}

	public override bool checkProceduralPrecondition (GameObject agent)
	{
		// find the nearest supply pile that has spare tools
		MetallurgyComponent[] metals = (MetallurgyComponent[]) UnityEngine.GameObject.FindObjectsOfType ( typeof(MetallurgyComponent) );
        MetallurgyComponent closest = null;
		float closestDist = 0;

		foreach (MetallurgyComponent metal in metals) {
			if (metal.numGoldChest > 0) {
				if (closest == null) {
					// first one, so choose it for now
					closest = metal;
					closestDist = (metal.gameObject.transform.position - agent.transform.position).magnitude;
				} else {
					// is this one closer than the last?
					float dist = (metal.gameObject.transform.position - agent.transform.position).magnitude;
					if (dist < closestDist) {
						// we found a closer one, use it
						closest = metal;
						closestDist = dist;
					}
				}
			}
		}
		if (closest == null)
			return false;

		targetMetal = closest;
		target = targetMetal.gameObject;

		return closest != null;
	}

	public override bool perform (GameObject agent)
	{
		if (targetMetal.numGoldChest > 0) {
			targetMetal.numGoldChest -= 1;
			hasGoldChest = true;

			// create the tool and add it to the agent

			BackpackComponent backpack = (BackpackComponent)agent.GetComponent(typeof(BackpackComponent));
            backpack.HasGoldChest(true);

            return true;
		} else {
			// we got there but there was no tool available! Someone got there first. Cannot perform action
			return false;
		}
	}

}


