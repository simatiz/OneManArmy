using UnityEngine;

public class PauseManager : MonoBehaviour
{
    [Header("Panels / Buttons")]
    public GameObject[] pauseMenus;
    public GameObject[] pauseButtons;

    GameObject currentMenu;

    void Awake()
    {
        if (pauseMenus != null && pauseMenus.Length > 0)
            currentMenu = pauseMenus[0];
    }

    public void Pause()
    {
        Time.timeScale = 0f;
        currentMenu?.SetActive(true);

        if (currentMenu != null && currentMenu.name == "InventoryPanel")
            InventoryManager.Instance?.UpdateInventoryUI(InventoryManager.Instance.items);

        foreach (var b in pauseButtons) if (b) b.SetActive(false);
    }

    public void Resume()
    {
        Time.timeScale = 1f;
        currentMenu?.SetActive(false);
        foreach (var b in pauseButtons) if (b) b.SetActive(true);
    }

    public void ChangeMenu(int index)
    {
        if (index < 0 || index >= pauseMenus.Length) return;
        currentMenu = pauseMenus[index];
    }

    public void ChangeToOtherMenu(int index)
    {
        if (index < 0 || index >= pauseMenus.Length) return;
        currentMenu.SetActive(false);
        currentMenu = pauseMenus[index];
        currentMenu.SetActive(true);
    }

    public void ExitGame() => Application.Quit();
}