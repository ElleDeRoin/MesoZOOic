using UnityEngine;

public class DinoPetting : MonoBehaviour
{
    public DinoStats dinoStats;

    [Header("Petting Settings")]
    public float happinessPerRub = 10f;
    public float movementRequired = 30f;
    public float petCooldown = 0.1f;

    private Vector2 lastTouchPosition;
    private float cooldownTimer;
    private bool isPetting;

    void Update()
    {
        cooldownTimer -= Time.deltaTime;

        if (Input.touchCount == 0)
        {
            isPetting = false;
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
    }

    private bool IsTouchingDino(Vector2 screenPosition)
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
        Debug.Log("Petting");
        if (dinoStats == null)

            return;

        dinoStats.Play(happinessPerRub);
    }
}