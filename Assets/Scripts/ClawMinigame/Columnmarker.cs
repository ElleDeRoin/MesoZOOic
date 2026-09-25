using UnityEngine;

namespace DinoDig
{
    [RequireComponent(typeof(Collider2D))]
    public class ColumnMarker : MonoBehaviour
    {
        [SerializeField] private int columnIndex;

        private void OnMouseDown()
        {
            HandleClick();
        }

        public void HandleClick()
        {
            Debug.Log($"CLICKED COLUMN: {columnIndex}");

            if (GameManager.Instance == null)
            {
                Debug.LogError("GameManager.Instance is NULL!");
                return;
            }

            GameManager.Instance.OnColumnClicked(columnIndex);
        }
    }
}