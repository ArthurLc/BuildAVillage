
using System;
using UnityEngine;

public class EatBreadAction : GoapAction
{
    private bool hasGetABread = false;
    private bool hasEat = false;
    public InnComponent targetInn; // where we eat

    private float startTime = 0;
    public float eatDuration = 6; // seconds

    public EatBreadAction()
    {
        addPrecondition("isStarving", true);
        //addPrecondition("hasBreads", true);
        addEffect("isStarving", false);
    }


    public override void reset()
    {
        hasGetABread = false;
        hasEat = false;
        targetInn = null;
        startTime = 0;
    }

    public override bool isDone()
    {
        return hasEat;
    }

    public override bool requiresInRange()
    {
        return true; // yes we need to be near a forge
    }

    public override bool checkProceduralPrecondition(GameObject agent)
    {
        // find the nearest supply pile that has spare ores
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

            if (targetInn.numBreads <= 0) {
                if (goapAgent.GetAgentType == GoapAgent.AgentType.Agent3D)
                    anim.SetTrigger("IdleEat");
            }
        }

        if (hasGetABread == false)
        {
            startTime = Time.time;

            if (targetInn.numBreads > 0)
            {
                targetInn.numBreads -= 1;
                hasGetABread = true;

                if (goapAgent.GetAgentType == GoapAgent.AgentType.Agent3D)
                    anim.SetTrigger("EatBread");
            }
        }

        if (Time.time - startTime > eatDuration)
        {
            Labourer labourer = (Labourer)agent.GetComponent(typeof(Labourer));
            labourer.currentLife = labourer.maxLife;
            labourer.currentHunger = labourer.maxHunger;
            // finished eating a bread
            hasEat = true;
        }
        return true;
    }

}
