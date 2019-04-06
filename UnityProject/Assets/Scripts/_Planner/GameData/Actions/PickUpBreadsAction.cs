
using System;
using UnityEngine;

public class PickUpBreadsAction : GoapAction
{
    private bool hasBread = false;
    private InnComponent targetInn; // where we get the bread from

    public PickUpBreadsAction()
    {
       
        addPrecondition("hasBreads", false); // don't get a bread if we already have one
        addEffect("hasBreads", true); // we now have a bread
    }


    public override void reset()
    {
        hasBread = false;
        targetInn = null;
    }

    public override bool isDone()
    {
        return hasBread;
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

        foreach (InnComponent inn in inns) {
            if (inn.numBreads > 0) {
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
        if (targetInn.numBreads > 0) {
            targetInn.numBreads -= 1;
            hasBread = true;
            //TODO play effect, change actor icon
            BackpackComponent backpack = (BackpackComponent)agent.GetComponent(typeof(BackpackComponent));
            backpack.numBreads += 1;

            return true;
        }
        else
        {
            // we got there but there was no ore available! Someone got there first. Cannot perform action
            return false;
        }
    }
}
