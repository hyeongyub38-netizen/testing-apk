using System.Collections;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Desktop Controls")]
    public float moveSpeed = 24f;
    public float boostMultiplier = 3f;
    public float mouseSensitivity = 2.2f;
    public float scrollSpeed = 9f;

    [Header("Mobile Touch Controls")]
    public bool enableTouchControls = true;
    public bool forceMobileControlsInEditor;
    public float touchLookSensitivity = 0.14f;
    public float touchMoveSpeed = 22f;
    public float pinchZoomSpeed = 0.075f;
    public float joystickRadiusPixels = 120f;

    [Header("Follow")]
    public bool followSelected;
    public float followDistance = 9f;

    private float yaw;
    private float pitch;
    private float lastClickTime;
    private Coroutine flyRoutine;

    private bool TouchMode => enableTouchControls && (Application.isMobilePlatform || forceMobileControlsInEditor);

    private void Start()
    {
        Vector3 rotation = transform.eulerAngles;
        yaw = rotation.y;
        pitch = rotation.x;
    }

    private void Update()
    {
        bool handledTouch = false;

        if (TouchMode && Input.touchCount > 0)
        {
            handledTouch = HandleTouchControls();
        }

        if (!handledTouch)
        {
            HandleLook();
            HandleMove();
            HandleSelection();
        }

        HandleFollow();

        if (Input.GetKeyDown(KeyCode.F)) FocusSelected(false);
        if (Input.GetKeyDown(KeyCode.V)) followSelected = !followSelected;
        if (Input.GetKeyDown(KeyCode.Tab) && UniverseSimulation.Instance != null) UniverseSimulation.Instance.SelectNextBody();
    }

    private void HandleLook()
    {
        if (!Input.GetMouseButton(1)) return;

        yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
        pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        ApplyRotation();
    }

    private void HandleMove()
    {
        float speed = moveSpeed * (Input.GetKey(KeyCode.LeftShift) ? boostMultiplier : 1f);
        Vector3 input = Vector3.zero;

        if (Input.GetKey(KeyCode.W)) input += Vector3.forward;
        if (Input.GetKey(KeyCode.S)) input += Vector3.back;
        if (Input.GetKey(KeyCode.A)) input += Vector3.left;
        if (Input.GetKey(KeyCode.D)) input += Vector3.right;
        if (Input.GetKey(KeyCode.E)) input += Vector3.up;
        if (Input.GetKey(KeyCode.Q)) input += Vector3.down;

        transform.position += transform.TransformDirection(input.normalized) * speed * Time.deltaTime;

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.001f)
        {
            transform.position += transform.forward * scroll * scrollSpeed * 10f * Time.deltaTime;
        }
    }

    private bool HandleTouchControls()
    {
        bool consumed = false;

        if (Input.touchCount >= 2)
        {
            Touch first = Input.GetTouch(0);
            Touch second = Input.GetTouch(1);

            Vector2 firstPrevious = first.position - first.deltaPosition;
            Vector2 secondPrevious = second.position - second.deltaPosition;

            float previousDistance = Vector2.Distance(firstPrevious, secondPrevious);
            float currentDistance = Vector2.Distance(first.position, second.position);
            float zoomDelta = currentDistance - previousDistance;

            transform.position += transform.forward * zoomDelta * pinchZoomSpeed * Time.deltaTime * 60f;
            consumed = true;
        }

        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch touch = Input.GetTouch(i);
            Vector2 position = touch.position;

            if (IsTouchOnMobileButtonZone(position))
            {
                consumed = true;
                continue;
            }

            if (IsInMoveZone(position))
            {
                Vector2 center = new Vector2(Screen.width * 0.18f, Screen.height * 0.18f);
                Vector2 raw = (position - center) / Mathf.Max(joystickRadiusPixels, 1f);
                Vector2 stick = Vector2.ClampMagnitude(raw, 1f);

                Vector3 input = new Vector3(stick.x, 0f, stick.y);
                transform.position += transform.TransformDirection(input) * touchMoveSpeed * Time.deltaTime;
                consumed = true;
                continue;
            }

            if (IsTap(touch))
            {
                TrySelectAt(position);
                consumed = true;
                continue;
            }

            if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
            {
                yaw += touch.deltaPosition.x * touchLookSensitivity;
                pitch -= touch.deltaPosition.y * touchLookSensitivity;
                ApplyRotation();
                consumed = true;
            }
        }

        return consumed;
    }

    private bool IsInMoveZone(Vector2 position)
    {
        return position.x < Screen.width * 0.42f && position.y < Screen.height * 0.46f;
    }

    private bool IsTouchOnMobileButtonZone(Vector2 position)
    {
        // Unity touch coordinates start at bottom-left. The HUD buttons are mostly top and right side.
        bool topButtons = position.y > Screen.height * 0.82f;
        bool rightButtons = position.x > Screen.width * 0.72f && position.y > Screen.height * 0.12f;
        return topButtons || rightButtons;
    }

    private bool IsTap(Touch touch)
    {
        if (touch.phase != TouchPhase.Ended) return false;
        if (touch.deltaPosition.sqrMagnitude > 100f) return false;
        if (touch.position.x < Screen.width * 0.34f && touch.position.y < Screen.height * 0.48f) return false;
        return touch.tapCount >= 1;
    }

    private void ApplyRotation()
    {
        pitch = Mathf.Clamp(pitch, -86f, 86f);
        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }

    private void HandleSelection()
    {
        if (!Input.GetMouseButtonDown(0)) return;
        if (Input.mousePosition.x < 330f) return;

        TrySelectAt(Input.mousePosition);
    }

    private void TrySelectAt(Vector2 screenPosition)
    {
        Camera cam = GetComponent<Camera>();
        if (cam == null) return;

        Ray ray = cam.ScreenPointToRay(screenPosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 2000f))
        {
            CelestialBody body = hit.collider.GetComponent<CelestialBody>();
            if (body != null && UniverseSimulation.Instance != null)
            {
                UniverseSimulation.Instance.selectedBody = body;

                if (Time.time - lastClickTime < 0.32f)
                {
                    FocusSelected(true);
                }

                lastClickTime = Time.time;
            }
        }
    }

    private void HandleFollow()
    {
        UniverseSimulation simulation = UniverseSimulation.Instance;
        if (!followSelected || simulation == null || simulation.selectedBody == null) return;

        CelestialBody target = simulation.selectedBody;
        Vector3 desired = target.transform.position - transform.forward * Mathf.Max(followDistance, target.radius * 7f);
        transform.position = Vector3.Lerp(transform.position, desired, Time.deltaTime * 3.5f);
    }

    public void FocusSelectedInstant()
    {
        FocusSelected(false);
    }

    public void ToggleFollowSelected()
    {
        followSelected = !followSelected;
    }

    private void FocusSelected(bool smooth)
    {
        UniverseSimulation simulation = UniverseSimulation.Instance;
        if (simulation == null || simulation.selectedBody == null) return;

        CelestialBody target = simulation.selectedBody;
        Vector3 desiredPosition = target.transform.position - transform.forward * Mathf.Max(followDistance, target.radius * 8f);

        if (flyRoutine != null) StopCoroutine(flyRoutine);

        if (smooth)
        {
            flyRoutine = StartCoroutine(FlyTo(desiredPosition, target.transform.position));
        }
        else
        {
            transform.position = desiredPosition;
            transform.LookAt(target.transform.position);
            Vector3 rotation = transform.eulerAngles;
            yaw = rotation.y;
            pitch = rotation.x;
        }
    }

    private IEnumerator FlyTo(Vector3 destination, Vector3 lookTarget)
    {
        Vector3 startPosition = transform.position;
        Quaternion startRotation = transform.rotation;
        Quaternion endRotation = Quaternion.LookRotation((lookTarget - destination).normalized, Vector3.up);

        float elapsed = 0f;
        const float duration = 0.65f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            transform.position = Vector3.Lerp(startPosition, destination, t);
            transform.rotation = Quaternion.Slerp(startRotation, endRotation, t);
            yield return null;
        }

        transform.position = destination;
        transform.rotation = endRotation;

        Vector3 rotation = transform.eulerAngles;
        yaw = rotation.y;
        pitch = rotation.x;
    }
}
