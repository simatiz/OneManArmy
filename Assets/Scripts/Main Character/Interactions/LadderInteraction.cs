using UnityEngine;
using UnityEngine.Tilemaps;
using TMPro;

public class FollowPlayerUI : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0, 2.5f, 0);
    public float radius = 1.1f;
    public bool faceCamera = true;
    public LayerMask ladderLayer;
    public Tilemap grassLadderTilemap, mountainLadderTilemap;
    public TextMeshProUGUI buttonLabel;

    private Collider2D currentLadder;
    private CanvasGroup canvasGroup;

    void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        // Спочатку невидима
        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    void Update()
    {
        if (target == null) return;

        Vector3 tpos = target.position + new Vector3(0, +0.6f, 0);

        currentLadder = Physics2D.OverlapCircle(tpos, 0.4f, ladderLayer);

        transform.position = target.position + offset;

        if (faceCamera && Camera.main != null)
        {
            transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward);
        }

        if (currentLadder != null)
        {
            // Показуємо кнопку
            canvasGroup.alpha = 1;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
        if (currentLadder != null && buttonLabel != null)
        {
            var movement = target.GetComponent<PlayerMovement>();
            if (movement != null)
            {
                string tag = currentLadder.tag;
                string direction = "";

                if (tag == "GrassLadder")
                {
                    direction = movement.GetCurrentLevel() == EdgeBlocker.Level.Sand ? "UP" : "DOWN";
                }
                else if (tag == "MountainLadder")
                {
                    direction = movement.GetCurrentLevel() == EdgeBlocker.Level.Grass ? "UP" : "DOWN";
                }

                buttonLabel.text = direction;
            }
        }
        else
        {
            // Ховаємо одразу, якщо не біля драбини
            canvasGroup.alpha = 0;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }

    public void TriggerLadderAction()
    {
        if (currentLadder == null || target == null) return;

        var movement = target.GetComponent<PlayerMovement>();
        if (movement == null) return;

        string tag = currentLadder.tag;

        // Отримуємо Tilemap
        Tilemap ladderTilemap = null;
        if (tag == "GrassLadder")
            ladderTilemap = grassLadderTilemap;
        else if (tag == "MountainLadder")
            ladderTilemap = mountainLadderTilemap;

        if (ladderTilemap == null) return;

        // Визначаємо клітинку драбини (в якій знаходиться гравець)
        Vector3Int cellPos = ladderTilemap.WorldToCell(target.position);
        Vector3 ladderPos = ladderTilemap.GetCellCenterWorld(cellPos);

        // Визначаємо нову позицію
        Vector3 newPosition = target.position;

        if (tag == "GrassLadder")
        {
            if (movement.GetCurrentLevel() == EdgeBlocker.Level.Sand)
            {
                newPosition = ladderPos + new Vector3(0, +1f, 0); // на траву
                movement.SetHeightLevelToGrass();
            }
            else if (movement.GetCurrentLevel() == EdgeBlocker.Level.Grass)
            {
                newPosition = ladderPos + new Vector3(0, -1f, 0); // на пісок
                movement.SetHeightLevelToSand();
            }
        }
        else if (tag == "MountainLadder")
        {
            if (movement.GetCurrentLevel() == EdgeBlocker.Level.Grass)
            {
                newPosition = ladderPos + new Vector3(0, +1f, 0); // на гору
                movement.SetHeightLevelToMountain();
            }
            else if (movement.GetCurrentLevel() == EdgeBlocker.Level.Mountain)
            {
                newPosition = ladderPos + new Vector3(0, -1f, 0); // на траву
                movement.SetHeightLevelToGrass();
            }
        }

        // 4. Телепорт
        target.position = newPosition;

        Debug.Log($"TELEPORT TO {newPosition} (ladder {tag}, cell {cellPos})");
    }
}