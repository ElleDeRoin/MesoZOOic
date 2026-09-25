using UnityEngine;

namespace DinoDig
{
    public class ClickRaycastTest : MonoBehaviour
    {
        [SerializeField] private Camera gameCamera;

        private void Awake()
        {
            if (gameCamera == null)
                gameCamera = Camera.main;
        }

        private void Update()
        {
            if (!Input.GetMouseButtonDown(0))
                return;

            Vector3 screenPosition = Input.mousePosition;

            Vector3 worldPosition =
                gameCamera.ScreenToWorldPoint(
                    new Vector3(
                        screenPosition.x,
                        screenPosition.y,
                        -gameCamera.transform.position.z
                    )
                );

            Vector2 point = new Vector2(
                worldPosition.x,
                worldPosition.y
            );

            Collider2D hit = Physics2D.OverlapPoint(point);

            if (hit == null)
            {
                Debug.Log("Clicked nothing.");
                return;
            }

            Debug.Log("Hit: " + hit.name);

            ColumnMarker marker =
                hit.GetComponent<ColumnMarker>();

            if (marker != null)
            {
                marker.HandleClick();
            }
            else
            {
                Debug.Log("Hit collider has no ColumnMarker.");
            }
        }
    }
}