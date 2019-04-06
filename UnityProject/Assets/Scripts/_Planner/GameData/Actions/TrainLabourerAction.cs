
using System;
using UnityEngine;

public class TrainLabourerAction : GoapAction
{
	private bool produce = false;
    private SchoolComponent targetSchool; // where we train Labourer
	
	private float startTime = 0;
	public float produceDuration = 15; // seconds
	
	public TrainLabourerAction() {
        addPrecondition("hasGoldChest", true); // if we doesn't starve
        addEffect ("produceLabourer", true);
	}
	
	
	public override void reset ()
	{
        produce = false;
        targetSchool = null;
		startTime = 0;
	}
	
	public override bool isDone ()
	{
		return produce;
	}
	
	public override bool requiresInRange ()
	{
		return true; // yes we need to be near a forge
	}
	
	public override bool checkProceduralPrecondition (GameObject agent)
	{
        // find the nearest forge
        SchoolComponent[] schools = (SchoolComponent[]) UnityEngine.GameObject.FindObjectsOfType ( typeof(SchoolComponent) );
        SchoolComponent closest = null;
		float closestDist = 0;
		
		foreach (SchoolComponent school in schools) {
            if (school.hasOrderLabourer) {
                if (closest == null)
                {
                    // first one, so choose it for now
                    closest = school;
                    closestDist = (school.gameObject.transform.position - agent.transform.position).magnitude;
                }
                else
                {
                    // is this one closer than the last?
                    float dist = (school.gameObject.transform.position - agent.transform.position).magnitude;
                    if (dist < closestDist)
                    {
                        // we found a closer one, use it
                        closest = school;
                        closestDist = dist;
                    }
                }
            }
		}
		if (closest == null)
			return false;

		targetSchool = closest;
		target = targetSchool.gameObject;
		
		return closest != null;
	}
	
	public override bool perform (GameObject agent)
    {
        BackpackComponent backpack = (BackpackComponent)agent.GetComponent(typeof(BackpackComponent));
        backpack.HasGoldChest(false);
        targetSchool.InstantiateLabourer();
        produce = true;

        return true;
    }
	
}
