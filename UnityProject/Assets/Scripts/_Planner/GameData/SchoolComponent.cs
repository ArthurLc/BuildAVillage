using System.Collections.Generic;
using UnityEngine;

public class SchoolComponent : Block
{
    [SerializeField] private List<GameObject> labourerList;

    public bool hasOrderLabourer;
    public int idLabourerOrder;

    public void InstantiateLabourer()
    {
        if (idLabourerOrder < labourerList.Count)
        {
            if (labourerList[idLabourerOrder] != null)
            {
                GameObject newLabourer = Instantiate(labourerList[idLabourerOrder], transform.position, transform.rotation) as GameObject;
                newLabourer.name = newLabourer.name.Remove(newLabourer.name.Length - 7);
            }
            else
                Debug.LogError("Il manque le GameObject dans la liste des Labourers.");
        }
        else
            Debug.LogError("L'ID du Labourer est trop élevée. Il faut agrandir la liste.");
    }
}
