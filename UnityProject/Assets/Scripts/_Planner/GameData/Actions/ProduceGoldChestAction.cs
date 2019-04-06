
using System;
using UnityEngine;

public class ProduceGoldChestAction : GoapAction
{
	private bool needToProduce = false;
	private bool produce = false;
    private MetallurgyComponent targetMetal; // where we produce GoldChest
	
	private float startTime = 0;
	public float produceDuration = 15; // seconds
	
	public ProduceGoldChestAction() {
		addPrecondition ("hasOre", true);
		addPrecondition ("hasFirewood", true);
        addEffect ("produceLabourer", true);
	}
	
	
	public override void reset ()
	{
        needToProduce = false;
        produce = false;
		targetMetal = null;
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
		MetallurgyComponent[] metals = (MetallurgyComponent[]) UnityEngine.GameObject.FindObjectsOfType ( typeof(MetallurgyComponent) );
        MetallurgyComponent closest = null;
		float closestDist = 0;
		
		foreach (MetallurgyComponent metal in metals) {
            if (metal.numGoldChest < metal.numOrder) {
                if (closest == null)
                {
                    // first one, so choose it for now
                    closest = metal;
                    closestDist = (metal.gameObject.transform.position - agent.transform.position).magnitude;
                }
                else
                {
                    // is this one closer than the last?
                    float dist = (metal.gameObject.transform.position - agent.transform.position).magnitude;
                    if (dist < closestDist)
                    {
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
        if (startTime == 0)
        {
            startTime = Time.time;

            if (goapAgent.GetAgentType == GoapAgent.AgentType.Agent3D)
                anim.SetTrigger("Idle");
        }

        if (needToProduce == false)
        {
            startTime = Time.time;

            if (targetMetal.numGoldChest < targetMetal.numOrder)
            {
                needToProduce = true;

                if (goapAgent.GetAgentType == GoapAgent.AgentType.Agent3D)
                    anim.SetTrigger("Forge");
            }
        }

        if (Time.time - startTime > produceDuration) {
			// finished forging a tool
			BackpackComponent backpack = (BackpackComponent)agent.GetComponent(typeof(BackpackComponent));
			backpack.numFirewood -= 2;
			backpack.numOre -= 3;
            targetMetal.numGoldChest += 1;

            ToolComponent tool = backpack.tool.GetComponent(typeof(ToolComponent)) as ToolComponent;
            tool.use(0.34f);
            if (tool.destroyed())
            {
                Destroy(backpack.tool);
                backpack.tool = null;
            }
            produce = true;
        }
		return true;
	}
	
}
