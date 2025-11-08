using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public void OnPlayButton()
    {
        // Just load the Game scene
        SceneManager.LoadScene("Game");
    }
}
