using UnityEngine;
using UnityEngine.SceneManagement;

public class RetryLevelButton : MonoBehaviour
{
    public void RetryLevel()
    {
        Time.timeScale = 1f;
        AbilityManager.ResetAbilities();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
