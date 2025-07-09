using UnityEngine;

public class QuestMarker : MonoBehaviour
{
    public GameObject player; // Assign your player GameObject here
    public float verticalOffset = 2f; // How high above the target the marker appears
    public float rotationSpeed = 100f; // Speed of rotation

    // For off-screen indicator
    public RectTransform offScreenArrowPrefab; // UGUI Image with an arrow sprite
    private RectTransform _currentOffScreenArrow;
    private Camera _mainCamera;
    private Canvas _uiCanvas; // Reference to your main UI Canvas (Screen Space - Overlay)

    void Start()
    {
        _mainCamera = Camera.main;
        if (_mainCamera == null)
        {
            Debug.LogError("Main Camera not found! Please tag your camera as 'MainCamera'.");
            enabled = false;
            return;
        }

        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                Debug.LogError("Player GameObject not assigned or tagged as 'Player'!");
                enabled = false;
                return;
            }
        }

        _uiCanvas = FindObjectOfType<Canvas>(); // Find the main UI Canvas
        if (_uiCanvas == null)
        {
            Debug.LogWarning("No UI Canvas found in scene for off-screen markers. Please ensure you have a Screen Space - Overlay Canvas.");
        }

        // Hide marker initially
        gameObject.SetActive(false);
    }

    void Update()
    {
        Quest trackedQuest = QuestManager.Instance.TrackedQuest;

        if (trackedQuest != null)
        {
            QuestObjective currentObjective = trackedQuest.GetCurrentActiveObjective();

            if (currentObjective != null && currentObjective.targetLocation != null)
            {
                Vector3 targetPos = currentObjective.targetLocation.position + Vector3.up * verticalOffset;
                transform.position = targetPos;
                transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

                // Check if target is on screen
                Vector3 screenPoint = _mainCamera.WorldToViewportPoint(targetPos);
                bool onScreen = screenPoint.z > 0 && screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y > 0 && screenPoint.y < 1;

                gameObject.SetActive(onScreen); // Show/hide 3D marker

                UpdateOffScreenIndicator(onScreen, targetPos);
            }
            else
            {
                gameObject.SetActive(false);
                ClearOffScreenIndicator();
            }
        }
        else
        {
            gameObject.SetActive(false);
            ClearOffScreenIndicator();
        }
    }

    void UpdateOffScreenIndicator(bool onScreen, Vector3 targetWorldPos)
    {
        if (offScreenArrowPrefab == null || _uiCanvas == null) return;

        if (onScreen)
        {
            ClearOffScreenIndicator();
            return;
        }

        if (_currentOffScreenArrow == null)
        {
            _currentOffScreenArrow = Instantiate(offScreenArrowPrefab, _uiCanvas.transform);
        }

        Vector3 screenPos = _mainCamera.WorldToScreenPoint(targetWorldPos);

        // Calculate position on screen edge
        Vector3 clampedScreenPos = screenPos;
        float arrowRotation = 0f;

        // Clamp to screen edges and calculate rotation
        if (screenPos.z < 0) // Behind camera
        {
            screenPos *= -1; // Invert to simulate being in front for calculation
        }

        Vector3 screenCenter = new Vector3(Screen.width / 2, Screen.height / 2, 0);
        screenPos -= screenCenter; // Vector from center to target

        float angle = Mathf.Atan2(screenPos.y, screenPos.x) * Mathf.Rad2Deg;
        angle -= 90; // Adjust for arrow pointing up by default
        arrowRotation = angle;

        float border = 50f; // Offset from edge of screen
        clampedScreenPos.x = Mathf.Clamp(screenPos.x, -Screen.width / 2 + border, Screen.width / 2 - border);
        clampedScreenPos.y = Mathf.Clamp(screenPos.y, -Screen.height / 2 + border, Screen.height / 2 - border);

        // If target is beyond the border, it's off-screen
        if (clampedScreenPos.x == screenPos.x && clampedScreenPos.y == screenPos.y)
        {
            // If target is exactly on screen, but outside a "safe zone", still point to it
            // This logic might need refinement based on desired visual behavior.
        }

        _currentOffScreenArrow.anchoredPosition = clampedScreenPos;
        _currentOffScreenArrow.localEulerAngles = new Vector3(0, 0, arrowRotation);
        _currentOffScreenArrow.gameObject.SetActive(true);
    }

    void ClearOffScreenIndicator()
    {
        if (_currentOffScreenArrow != null)
        {
            Destroy(_currentOffScreenArrow.gameObject);
            _currentOffScreenArrow = null;
        }
    }
}
