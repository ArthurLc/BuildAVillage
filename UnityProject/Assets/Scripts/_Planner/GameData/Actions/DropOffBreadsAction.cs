
using System;
using UnityEngine;

public class DropOffBreadsAction : GoapAction
{
    private bool droppedOffBread = false;
    private InnComponent targetInn; // where we drop off the bread

    public DropOffBreadsAction()
    {
        addPrecondition("hasBreads", true); // can't drop off breads if we don't already have some
        addEffect("hasBreads", false); // we now have no breads
        addEffect("collectBreads", true); // we collected breads
    }


    public override void reset()
    {
        droppedOffBread = false;
        targetInn = null;
    }

    public override bool isDone()
    {
        return droppedOffBread;
    }

    public override bool requiresInRange()
    {
        return true; // yes we need to be near a supply pile so we can drop off the firewood
    }

    public override bool checkProceduralPrecondition(GameObject agent)
    {
        // find the nearest supply pile that has spare firewood
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
        BackpackComponent backpack = (BackpackComponent)agent.GetComponent(typeof(BackpackComponent));
        targetInn.numBreads += backpack.numBreads;
        droppedOffBread = true;
        backpack.numBreads = 0;

        return true;
    }
}
