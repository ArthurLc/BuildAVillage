
using System;
using UnityEngine;

public class DropOffWheatsAction : GoapAction
{
    private bool droppedOffWheats = false;
    private InnComponent targetInn; // where we drop off the wheats

    public DropOffWheatsAction() {
        addPrecondition ("hasWheat", true); // can't drop off wheats if we don't already have some
        addEffect ("hasWheat", false); // we now have no wheats
        addEffect ("collectWheats", true); // we collected wheats
    }


    public override void reset()
    {
        droppedOffWheats = false;
        targetInn = null;
    }

    public override bool isDone()
    {
        return droppedOffWheats;
    }

    public override bool requiresInRange()
    {
        return true; // yes we need to be near a supply pile so we can drop off the logs
    }

    public override bool checkProceduralPrecondition(GameObject agent)
    {
        // find the nearest supply pile
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
        targetInn.numWheat += backpack.numWheat;
        droppedOffWheats = true;
        backpack.numWheat = 0;

        return true;
    }
}
