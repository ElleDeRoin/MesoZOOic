using UnityEngine;

namespace DinoDig
{

    [RequireComponent(typeof(SpriteRenderer))]
    public class GridPiece : MonoBehaviour
    {
        [Tooltip("If true, this piece is a dinosaur egg prize instead of a matchable block.")]
        public bool IsEgg;

        [Tooltip("Ignored if IsEgg is true.")]
        public BlockType BlockType;

        public SpriteRenderer SpriteRenderer { get; private set; }

        [HideInInspector] public int Column;
        [HideInInspector] public int RowIndex;

        void Awake()
        {
            SpriteRenderer = GetComponent<SpriteRenderer>();
        }

        public void SetSortingOrder(int order)
        {
            if (SpriteRenderer != null)
                SpriteRenderer.sortingOrder = order;
        }
    }
}