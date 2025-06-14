using UnityEngine;

public class CharacterSwitcher : MonoBehaviour
{
    public GameObject[] characters;
    private int currentIndex = 0;
    public CameraFollow cameraFollow;
    public FollowPlayerUI followUI;

    private CharacterSelector selector;
    public static GameObject currentPlayer;

    void Start()
    {
        // Активуємо першого персонажа на початку
        ActivateOnly(currentIndex);
        selector = GetComponent<CharacterSelector>();
        currentPlayer = characters[currentIndex];
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) SwitchTo(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SwitchTo(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SwitchTo(2);
    }

    void SwitchTo(int newIndex)
    {
        if (newIndex == currentIndex || newIndex >= characters.Length)
            return;

        selector.SelectCharacter(newIndex);

        Vector3 currentPos = characters[currentIndex].transform.position;
        Quaternion currentRot = characters[currentIndex].transform.rotation;
        EdgeBlocker.Level currentHeightLevel = characters[currentIndex].GetComponent<PlayerMovement>().GetCurrentLevel();

        characters[currentIndex].SetActive(false);

        GameObject newCharacter = characters[newIndex];
        newCharacter.transform.position = currentPos;
        newCharacter.transform.rotation = currentRot;

        newCharacter.SetActive(true);
        currentPlayer = newCharacter;

        var movement = newCharacter.GetComponent<PlayerMovement>();
        if (movement != null)
        {
            movement.SetNewHeightLevel(currentHeightLevel);
        }

        currentIndex = newIndex;

        if (cameraFollow != null)
            cameraFollow.target = newCharacter.transform;

        if (followUI != null)
            followUI.target = newCharacter.transform;

        InventoryManager.Instance.ReapplyTo(newCharacter.GetComponent<CharacterStatsBase>());
    }

    void ActivateOnly(int index)
    {
        for (int i = 0; i < characters.Length; i++)
            characters[i].SetActive(i == index);
    }
}