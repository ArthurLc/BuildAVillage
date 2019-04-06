using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Metallurgist : Labourer
{
	/**
	 * Our only goal will ever be to make tools.
	 * The ForgeTooldAction will be able to fulfill this goal.
	 */
	public override HashSet<KeyValuePair<string,object>> createGoalState () {
        HashSet<KeyValuePair<string, object>> goal = base.createGoalState();
        if (goal != null && goal.Count > 0)
            return goal;

        goal = new HashSet<KeyValuePair<string, object>>();

        goal.Add(new KeyValuePair<string, object>("produceLabourer", true ));
        return goal;
	}
}

