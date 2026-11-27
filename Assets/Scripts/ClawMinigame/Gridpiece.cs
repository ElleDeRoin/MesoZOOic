using UnityEngine;

namespace DinoDig
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class GridPiece : MonoBehaviour
    {
        public bool IsEgg;
        public BlockType BlockType;
        public SpriteRenderer SpriteRenderer { get; private set; }

        [HideInInspector] public int Column;
        [HideInInspector] public int RowIndex;

        public bool IsSliding { get; private set; }

        private Vector3 _slideTarget;
        private float _slideSpeed;

        void Awake()
        {
            SpriteRenderer = GetComponent<SpriteRenderer>();
        }

        void Update()
        {
            if (!IsSliding) return;

            transform.position = Vector3.MoveTowards(
                transform.position,
                _slideTarget,
                _slideSpeed * Time.deltaTime
            );

            if ((transform.position - _slideTarget).sqrMagnitude < 0.0001f)
            {
                transform.position = _slideTarget;
                IsSliding = false;
            }
        }

        public void SlideTo(Vector3 target, float speed)
        {
            _slideTarget = target;
            _slideSpeed = speed;
            IsSliding = true;
        }

        public void SnapTo(Vector3 position)
        {
            IsSliding = false;
            transform.position = position;
        }

        public void StopSliding() => IsSliding = false;

        public void SetSortingOrder(int order)
        {
            if (SpriteRenderer != null)
                SpriteRenderer.sortingOrder = order;
        }
    }
}