using UnityEngine;

/*
* @Arthur Lacour
* @Block.cs
* @15/03/2018
* @Description:
*   - Classe mère de tous les Blocks statics du jeux. (Bâtiments, ressources...)
*   
* @Condition:
*/

public class Block : MonoBehaviour {

    public int currentLife = 40;
    public int maxLife = 40;
    [TextArea]
    public string description = "\"Need a description\"";

    // Use this for initialization
    void Start () {
        currentLife = maxLife;
    }

    public void ReduceLife(int _extractedValue)
    {
        currentLife -= _extractedValue;

        if(currentLife <= 0) {
            if (GetComponentInChildren<Camera>() != null)
                UIManager.Instance.SelectEntity(false, null);
            Destroy(this.gameObject);
        }
    }
}
