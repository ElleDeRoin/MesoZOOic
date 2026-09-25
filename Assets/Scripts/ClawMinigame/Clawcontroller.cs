using System.Collections;
using UnityEngine;

namespace DinoDig
{
    public class ClawController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform holdPoint;

        [Header("Speeds")]
        [SerializeField] private float horizontalSpeed = 8f;
        [SerializeField] private float verticalSpeed = 6f;

        [Header("Positioning")]
        [SerializeField] private float restY = 5f;
        [SerializeField] private float grabOffsetY = 0.5f;

        public bool IsBusy { get; private set; }
        public GridPiece HeldPiece { get; private set; }

        public IEnumerator PickUp(GridManager grid, int column, System.Action<GridPiece> onComplete)
        {
            IsBusy = true;

            GridPiece target = grid.PeekTop(column);
            if (target == null)
            {
                IsBusy = false;
                onComplete?.Invoke(null);
                yield break;
            }

            float targetX = grid.GetWorldPosition(column, 0).x;
            yield return MoveHorizontal(targetX);

            float grabY = target.transform.position.y + grabOffsetY;
            yield return MoveVertical(grabY);

            grid.PopTop(column);
            target.transform.SetParent(holdPoint);
            target.transform.localPosition = Vector3.zero;
            HeldPiece = target;

            yield return MoveVertical(restY);

            IsBusy = false;
            onComplete?.Invoke(target);
        }

        public IEnumerator PlaceDown(GridManager grid, int column, System.Action<bool> onComplete)
        {
            if (HeldPiece == null)
            {
                onComplete?.Invoke(false);
                yield break;
            }

            IsBusy = true;

            float targetX = grid.GetWorldPosition(column, 0).x;
            yield return MoveHorizontal(targetX);

            int destRow = grid.GetColumnHeight(column);
            if (destRow >= grid.RowCount)
            {
                yield return MoveVertical(restY);
                IsBusy = false;
                onComplete?.Invoke(false);
                yield break;
            }

            float placeY = grid.GetWorldPosition(column, destRow).y + grabOffsetY;
            yield return MoveVertical(placeY);

            HeldPiece.transform.SetParent(null);
            bool success = grid.PushTop(column, HeldPiece);
            HeldPiece = null;

            yield return MoveVertical(restY);

            IsBusy = false;
            onComplete?.Invoke(success);
        }

        private IEnumerator MoveHorizontal(float targetX)
        {
            while (Mathf.Abs(transform.position.x - targetX) > 0.01f)
            {
                Vector3 pos = transform.position;
                pos.x = Mathf.MoveTowards(pos.x, targetX, horizontalSpeed * Time.deltaTime);
                transform.position = pos;
                yield return null;
            }
        }

        private IEnumerator MoveVertical(float targetY)
        {
            while (Mathf.Abs(transform.position.y - targetY) > 0.01f)
            {
                Vector3 pos = transform.position;
                pos.y = Mathf.MoveTowards(pos.y, targetY, verticalSpeed * Time.deltaTime);
                transform.position = pos;
                yield return null;
            }
        }
    }
}