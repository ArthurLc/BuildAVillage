
using System;
using UnityEngine;

public class ChopWheatAction : GoapAction
{
    private bool chopped = false;
    private FieldComponent targetField; // where we get the wheats from

    private float startTime = 0;
    public float workDuration = 20; // seconds

    public ChopWheatAction()
    {
        addPrecondition ("hasTool", true); // we need a tool to do this
        addPrecondition ("hasWheat", false); // if we have wheats we don't want more
        addEffect ("hasWheat", true);
    }


    public override void reset()
    {
        chopped = false;
        targetField = null;
        startTime = 0;
    }

    public override bool isDone()
    {
        return chopped;
    }

    public override bool requiresInRange()
    {
        return true; // yes we need to be near a tree
    }

    public override bool checkProceduralPrecondition(GameObject agent)
    {
        // find the nearest tree that we can chop
        FieldComponent[] fields = (FieldComponent[])UnityEngine.GameObject.FindObjectsOfType(typeof(FieldComponent));
        FieldComponent closest = null;
        float closestDist = 0;

        foreach (FieldComponent field in fields)
        {
            if (closest == null)
            {
                // first one, so choose it for now
                closest = field;
                closestDist = (field.gameObject.transform.position - agent.transform.position).magnitude;
            }
            else
            {
                // is this one closer than the last?
                float dist = (field.gameObject.transform.position - agent.transform.position).magnitude;
                if (dist < closestDist)
                {
                    // we found a closer one, use it
                    closest = field;
                    closestDist = dist;
                }
            }
        }
        if (closest == null)
            return false;

        targetField = closest;
        target = targetField.gameObject;

        return closest != null;
    }

    public override bool perform(GameObject agent)
    {
        if (startTime == 0)
        {
            startTime = Time.time;

            if (goapAgent.GetAgentType == GoapAgent.AgentType.Agent3D)
                anim.SetTrigger("CutWeat");
        }

        if (Time.time - startTime > workDuration)
        {
            // finished chopping
            target.GetComponent<Block>().ReduceLife(5);
            BackpackComponent backpack = (BackpackComponent)agent.GetComponent(typeof(BackpackComponent));
            backpack.numWheat += 5;
            chopped = true;
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