using UnityEngine;
using UnityEngine.VFX;


public class DinoPetting : MonoBehaviour
{
    public DinoStats dinoStats;

    [Header("Petting Settings")]
    public float happinessPerRub = 10f;
    public float movementRequired = 30f;
    public float petCooldown = 0.1f;

    [Header("Animation")]
    public Animator animator;
    public float cuddleLingerTime = 0.5f;


    [Header("Hand")]
    public RectTransform hand;
    public Vector2 handOffset = Vector2.zero;
    private Canvas handCanvas;

    private static readonly int IsPettingHash = Animator.StringToHash("IsPetting");

    private Vector2 lastTouchPosition;
    private float cooldownTimer;
    private float cuddleTimer;
    private bool isPetting;
    [SerializeField] ParticleSystem heartVFX = null;

    void Awake()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (hand != null)
        {
            Canvas c = hand.GetComponentInParent<Canvas>();
            if (c != null) handCanvas = c.rootCanvas;
            hand.gameObject.SetActive(false);
        }
    }
    void Update()
    {
        cooldownTimer -= Time.deltaTime;

        if (cuddleTimer > 0f)
        {
            cuddleTimer -= Time.deltaTime;
            if (cuddleTimer <= 0f)
                SetCuddleAnimation(false);
        }
        if (DinoWashing.IsWashing || DinoBrush.IsBrushing)
        {
            isPetting = false;
            HideHand();
            return;
        }

        if (Input.touchCount == 0)
        {
            isPetting = false;
            HideHand();
            return;
        }

        Touch touch = Input.GetTouch(0);

        if (touch.phase == TouchPhase.Began)
        {
            if (IsTouchingDino(touch.position))
            {
                isPetting = true;
                lastTouchPosition = touch.position;
            }
        }

        if (isPetting && touch.phase == TouchPhase.Moved)
        {
            float distance = Vector2.Distance(
                touch.position,
                lastTouchPosition
            );

            if (distance >= movementRequired && cooldownTimer <= 0f)
            {
                PetDino();

                lastTouchPosition = touch.position;
                cooldownTimer = petCooldown;
            }
        }

        if (touch.phase == TouchPhase.Ended ||
            touch.phase == TouchPhase.Canceled)
        {
            isPetting = false;
        }

        if (isPetting)
            MoveHand(touch.position);
        else
            HideHand();
    }

    private void MoveHand(Vector2 screenPosition)
    {
        if (hand == null || handCanvas == null) return;

        if (!hand.gameObject.activeSelf)
            hand.gameObject.SetActive(true);

        Camera uiCam = handCanvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : handCanvas.worldCamera;

        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(
                (RectTransform)handCanvas.transform,
                screenPosition + handOffset,
                uiCam,
                out Vector3 world))
        {
            hand.position = world;
        }
    }

    private void HideHand()
    {
        if (hand != null && hand.gameObject.activeSelf)
            hand.gameObject.SetActive(false);
    }

    public bool IsTouchingDino(Vector2 screenPosition)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            return hit.transform == transform ||
                   hit.transform.IsChildOf(transform);
        }

        return false;
    }

    private void PetDino()
    {
        PetDino(happinessPerRub);
    }
    public void PetDino(float happiness)
    {
        Debug.Log("Petting");

        cuddleTimer = cuddleLingerTime;
        SetCuddleAnimation(true);

        if (heartVFX != null)
            heartVFX.Play();

        if (dinoStats == null)
            return;

        dinoStats.Play(happiness);
    }

    private void SetCuddleAnimation(bool value)
    {
        if (animator != null)
            animator.SetBool(IsPettingHash, value);

        if (dinoStats != null)
        {
            if (value)
                dinoStats.StartPetting();
            else
                dinoStats.StopPetting();
        }
    }
}