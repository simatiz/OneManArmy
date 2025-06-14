using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour
{
    private PauseManager pm;

    private void Start()
    {
        pm = GetComponent<PauseManager>();
    }

    public void SwitchScene(string sceneName)
    {
        if (!string.IsNullOrEmpty(sceneName))
        {
            if (sceneName == "Level1")
            {
                CharacterStatsBase.maxHealth = 100f;
                CharacterStatsBase.CurrentHealth = CharacterStatsBase.maxHealth;
            }
            pm.Resume();
            SceneManager.LoadScene(sceneName);
            Time.timeScale = 1f;
        }
        else
        {
            Debug.LogWarning("Назва сцени не вказана!");
        }
    }
}