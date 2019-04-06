
using System;
using UnityEngine;

public class CookBreadAction : GoapAction
{
    private bool cooked = false;
    private InnComponent targetInn; // where we cook breads

    private float startTime = 0;
    public float cookDuration = 10; // seconds

    public CookBreadAction()
    {
        addPrecondition("hasTool", true); // we need a tool to do this
        addPrecondition("hasWheat", true);
        addPrecondition("hasFirewood", true);
        addEffect("hasBreads", true);
    }


    public override void reset()
    {
        cooked = false;
        targetInn = null;
        startTime = 0;
    }

    public override bool isDone()
    {
        return cooked;
    }

    public override bool requiresInRange()
    {
        return true; // yes we need to be near a forge
    }

    public override bool checkProceduralPrecondition(GameObject agent)
    {
        // find the nearest forge
        InnComponent[] inns = (InnComponent[])UnityEngine.GameObject.FindObjectsOfType(typeof(InnComponent));
        InnComponent closest = null;
        float closestDist = 0;

        foreach (InnComponent inn in inns)
        {
            if (closest == null)
            {
                // first one, so choose it for now
                closest = inn;
                closestDist = (inn.gameObject.transform.position - agent.transform.position).magnitude;
            }
            else
            {
                // is this one closer than the last?
                float dist = (inn.gameObject.transform.position - agent.transform.position).magnitude;
                if (dist < closestDist)
                {
                    // we found a closer one, use it
                    closest = inn;
                    closestDist = dist;
                }
            }
        }
        if (closest == null)
            return false;

        targetInn = closest;
        target = targetInn.gameObject;

        return closest != null;
    }

    public override bool perform(GameObject agent)
    {
        if (startTime == 0)
        {
            startTime = Time.time;

            if (goapAgent.GetAgentType == GoapAgent.AgentType.Agent3D) {
                anim.SetTrigger("Cook");
            }
        }

        if (Time.time - startTime > cookDuration)
        {
            // finished forging a tool
            BackpackComponent backpack = (BackpackComponent)agent.GetComponent(typeof(BackpackComponent));
            backpack.numFirewood -= 1;
            backpack.numWheat -= 2;
            backpack.numBreads += 1;
            cooked = true;
            ToolComponent tool = backpack.tool.GetComponent(typeof(ToolComponent)) as ToolComponent;
            tool.use(0.34f);
            if (tool.destroyed())
            {
                Destroy(backpack.tool);
                backpack.tool = null;
            }
        }
        return true;
    }

}
