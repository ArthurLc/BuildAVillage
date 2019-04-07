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

    [SerializeField] private GameObject gameModes;

    public void OpenOrClose_GameModes()
    {
        gameModes.SetActive(!gameModes.activeInHierarchy);
    }
    public void LaunchGame_Solo() {
        SceneManager.LoadScene(2);
    }
    public void LaunchLobby()
    {
        SceneManager.LoadScene(1);
    }
    public void QuitGame() {
        Application.Quit();
    }
}
