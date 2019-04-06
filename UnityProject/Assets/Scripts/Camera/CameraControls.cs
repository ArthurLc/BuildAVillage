using UnityEngine;

/*
* @Arthur Lacour
* @CameraControls.cs
* @15/03/2018
* @Description:
*   - Script de contrôle de la camera.
*       (Déplacements : Horizontal/Vertical)
*       (Zoom : Mouse ScrollWheel)
* 
* @Condition:
*   - Il faut l'attacher sur une camera.
*/

[RequireComponent(typeof(Camera))]
public class CameraControls : MonoBehaviour {

    Camera cam;

    [SerializeField] float speedMov = 0.5f;
    [SerializeField] float speedZoom = 2.0f;

    float horizontal;
    float vertical;
    float zoom;

    // Use this for initialization
    void Start () {
        cam = GetComponent<Camera>();
	}
	
	// Update is called once per frame
	void Update () {
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");
        zoom = Input.GetAxis("Mouse ScrollWheel");

        cam.transform.localPosition = new Vector3(cam.transform.localPosition.x + (horizontal * speedMov * (cam.transform.localPosition.y/10.0f)), cam.transform.localPosition.y + (-zoom * speedZoom), cam.transform.localPosition.z + (vertical * speedMov * (cam.transform.localPosition.y / 10.0f)));
    }
}
