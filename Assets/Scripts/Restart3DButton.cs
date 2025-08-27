using UnityEngine;
using UnityEngine.SceneManagement;

public class RestartButton3D : MonoBehaviour
{
    void OnMouseDown()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
