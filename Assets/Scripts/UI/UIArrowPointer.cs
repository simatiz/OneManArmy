using UnityEngine;

public class UIArrowPointerMultiPlayer : MonoBehaviour
{
    public Transform[] players;
    public RectTransform arrowUI;
    public Camera mainCamera;
    public string itemTag = "Item";

    private Transform activePlayer;
    private Transform nearestItem;
    private Canvas canvas;

    private Vector3 targetPosition; // Ціль для плавного руху

    public float followSpeed = 5f; // Чим менше — тим повільніше

    void Start()
    {
        canvas = GetComponentInParent<Canvas>();
    }

    void Update()
    {
        UpdateActivePlayer();
        if (activePlayer == null) return;

        FindNearestItem();

        if (nearestItem == null)
        {
            arrowUI.gameObject.SetActive(false);
            return;
        }

        Vector3 viewportPoint = mainCamera.WorldToViewportPoint(nearestItem.position);

        if (viewportPoint.z > 0 && viewportPoint.x > 0 && viewportPoint.x < 1 && viewportPoint.y > 0 && viewportPoint.y < 1)
        {
            arrowUI.gameObject.SetActive(false);
            return;
        }

        arrowUI.gameObject.SetActive(true);

        Vector3 direction = (nearestItem.position - activePlayer.position).normalized;
        Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0);
        Vector3 screenPos = mainCamera.WorldToScreenPoint(activePlayer.position + direction * 1000f);
        Vector3 clampedScreenPos = ClampToScreenEdge(screenPos);

        // Цільова позиція (екран)
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            clampedScreenPos,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : mainCamera,
            out Vector2 localPoint
        );

        // Плавне наближення до цілі
        arrowUI.localPosition = Vector3.Lerp(arrowUI.localPosition, localPoint, Time.deltaTime * followSpeed);

        Vector3 dirToTarget = (screenPos - screenCenter).normalized;
        float angle = Mathf.Atan2(dirToTarget.y, dirToTarget.x) * Mathf.Rad2Deg;
        arrowUI.rotation = Quaternion.Euler(0, 0, angle);
    }

    void UpdateActivePlayer()
    {
        foreach (Transform player in players)
        {
            if (player.gameObject.activeInHierarchy)
            {
                activePlayer = player;
                return;
            }
        }

        activePlayer = null;
    }

    void FindNearestItem()
    {
        GameObject[] items = GameObject.FindGameObjectsWithTag(itemTag);
        float minDistance = Mathf.Infinity;
        Transform closest = null;

        foreach (GameObject item in items)
        {
            float distance = Vector2.Distance(activePlayer.position, item.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closest = item.transform;
            }
        }

        nearestItem = closest;
    }

    Vector3 ClampToScreenEdge(Vector3 screenPos)
    {
        // Різні відступи: по горизонталі — менше, по вертикалі — більше
        float borderX = 80f; // Відступ зліва/справа
        float borderY = 120f; // Відступ зверху/знизу

        Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0);
        Vector3 direction = (screenPos - screenCenter).normalized;

        // Обчислюємо максимальні межі в кожну сторону
        float maxX = (Screen.width / 2f) - borderX;
        float maxY = (Screen.height / 2f) - borderY;

        // Обмежуємо рух стрілки в межах прямокутного еліпсу (з анізотропією)
        float ratioX = direction.x / Mathf.Abs(direction.x == 0 ? 0.0001f : direction.x);
        float ratioY = direction.y / Mathf.Abs(direction.y == 0 ? 0.0001f : direction.y);
        float scale = 1f / Mathf.Max(
            Mathf.Abs(direction.x) / maxX,
            Mathf.Abs(direction.y) / maxY
        );

        Vector3 edgePos = screenCenter + direction * scale;

        return edgePos;
    }
}