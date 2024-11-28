using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeScene : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public void ChangeSceneToOther()
    {
        SceneManager.LoadScene("BuilderScene");
    }

}
