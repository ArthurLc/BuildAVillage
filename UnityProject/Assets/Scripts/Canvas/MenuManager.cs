using UnityEngine;
using UnityEngine.SceneManagement;

/*
* @Arthur Lacour
* @MenuManager.cs
* @15/03/2018
* @Description:
*   - Script de management des Scenes depuis le menu.
*   
* @Condition:
*/

public class MenuManager : MonoBehaviour {
    
    public void LaunchGame() {
        SceneManager.LoadScene(1);
    }
    public void QuitGame() {
        Application.Quit();
    }
}
