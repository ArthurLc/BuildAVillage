using UnityEngine;
using System.Collections;

/**
 * Holds resources for the Agent.
 */
public class BackpackComponent : MonoBehaviour
{
	public GameObject tool;
	public int numLogs;
	public int numFirewood;
	public int numOre;
	public int numWheat;
	public int numBreads;
    public string toolType = "ToolAxe";
    private bool hasGoldChest;
    [SerializeField] private GameObject GoldChest;

    public bool GetHasGoldChest
    {
        get { return hasGoldChest; }
    }
    public void HasGoldChest(bool _hasAnymore)
    {
        GoldChest.SetActive(_hasAnymore);
        hasGoldChest = _hasAnymore;
    }
}

