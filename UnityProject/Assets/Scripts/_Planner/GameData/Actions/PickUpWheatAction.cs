
using System;
using UnityEngine;

public class PickUpWheatAction : GoapAction
{
    private bool hasWheat = false;
    private InnComponent targetInn; // where we get the wheat from

    public PickUpWheatAction()
    {

        addPrecondition("hasWheat", false); // don't get a wheat if we already have one
        addEffect("hasWheat", true); // we now have a wheat
    }


    public override void reset()
    {
        hasWheat = false;
        targetInn = null;
    }

    public override bool isDone()
    {
        return hasWheat;
    }

    public override bool requiresInRange()
    {
        return true; // yes we need to be near a supply pile so we can pick up the ore
    }

    public override bool checkProceduralPrecondition(GameObject agent)
    {
        // find the nearest supply pile that has spare ores
        InnComponent[] inns = (InnComponent[])UnityEngine.GameObject.FindObjectsOfType(typeof(InnComponent));
        InnComponent closest = null;
        float closestDist = 0;

        foreach (InnComponent inn in inns)
        {
            if (inn.numWheat >= 2)
            { // we need to take 2 wheats
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
        }
        if (closest == null)
            return false;

        targetInn = closest;
        target = targetInn.gameObject;

        return closest != null;
    }

    public override bool perform(GameObject agent)
    {
        if (targetInn.numWheat >= 2)
        {
            targetInn.numWheat -= 2;
            hasWheat = true;
            //TODO play effect, change actor icon
            BackpackComponent backpack = (BackpackComponent)agent.GetComponent(typeof(BackpackComponent));
            backpack.numWheat += 2;

            return true;
        }
        else
        {
            // we got there but there was no ore available! Someone got there first. Cannot perform action
            return false;
        }
    }
}
