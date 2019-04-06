using UnityEngine;
using UnityEngine.EventSystems;

/*
* @Arthur Lacour
* @RaycastSelecter.cs
* @15/03/2018
* @Description:
*   - Sélection d'entitée.
*       (L'entitée doit être sur le Layer "Selectable".)
*       (Son tag déterminera les infos affichées.)
* 
* @Condition:
*   - Il faut l'attacher sur une camera.
*/

[RequireComponent(typeof(Camera))]
public class RaycastSelecter : MonoBehaviour {

    Camera camera;
    Ray ray;

    // Use this for initialization
    void Start ()
    {
        camera = GetComponent<Camera>();
        ray = new Ray(camera.transform.position, camera.transform.forward);
    }
	
	// Update is called once per frame
	void Update ()
    {
        ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Debug.DrawRay(ray.origin, ray.direction * 100.0f, Color.red);

        if (EventSystem.current.IsPointerOverGameObject() == false)
        {
            if (Input.GetButtonDown("Fire1"))
                FireRaycast();
            else if (Input.GetButtonDown("Fire2"))
                GiveOrder();
        }
    }

    private void FireRaycast()
    {
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100.0f, LayerMask.GetMask("Selectable")))
        {
            UIManager.Instance.SelectEntity(true, hit.transform);
        }
        else {
            UIManager.Instance.SelectEntity(false, null);
        }
    }
    private void GiveOrder()
    {

    }
}
