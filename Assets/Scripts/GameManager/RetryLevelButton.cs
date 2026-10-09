using UnityEngine;
using UnityEngine.SceneManagement;

public class RetryLevelButton : MonoBehaviour
{
    public void RetryLevel()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
