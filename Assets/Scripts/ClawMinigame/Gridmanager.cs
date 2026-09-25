using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DinoDig
{
    public class GridManager : MonoBehaviour
    {
        [Header("Grid Shape")]
        [SerializeField] private int columnCount = 7;
        [SerializeField] private int rowCount = 4;
        [SerializeField] private float cellSize = 1f;
        [SerializeField] private Transform gridOrigin;

        [Header("Prefabs & Visuals")]
        [SerializeField] private GridPiece blockPrefab;
        [SerializeField] private GridPiece eggPrefab;
        [SerializeField] private Sprite[] blockSprites;

        [Header("Initial Fill")]
        [SerializeField] private int startingHeightMin = 1;
        [SerializeField] private int startingHeightMax = 2;

        [Header("Egg Settings")]
        [Range(0f, 1f)]
        [SerializeField] private float eggSpawnChance = 0.05f;

        private List<List<GridPiece>> _stacks;

        public int ColumnCount => columnCount;
        public int RowCount => rowCount;

        private void Awake()
        {
            BuildInitialGrid();
        }

        private void BuildInitialGrid()
        {
            _stacks = new List<List<GridPiece>>(columnCount);

            for (int col = 0; col < columnCount; col++)
            {
                List<GridPiece> list = new List<GridPiece>();

                int height = Random.Range(
                    startingHeightMin,
                    startingHeightMax + 1
                );

                for (int row = 0; row < height; row++)
                {
                    bool isEgg = ShouldSpawnEgg();

                    BlockType type = GetRandomBlockType();

                    GridPiece piece = SpawnPiece(
                        col,
                        row,
                        isEgg,
                        type
                    );

                    list.Add(piece);
                }

                _stacks.Add(list);
            }
        }

        private bool ShouldSpawnEgg()
        {
            return Random.value < eggSpawnChance;
        }

        private BlockType GetRandomBlockType()
        {
            return (BlockType)Random.Range(
                0,
                blockSprites.Length
            );
        }

        private GridPiece SpawnPiece(
            int col,
            int row,
            bool isEgg,
            BlockType type
        )
        {
            GridPiece prefab = isEgg
                ? eggPrefab
                : blockPrefab;

            GridPiece piece = Instantiate(
                prefab,
                GetWorldPosition(col, row),
                Quaternion.identity,
                transform
            );

            piece.IsEgg = isEgg;
            piece.BlockType = type;
            piece.Column = col;
            piece.RowIndex = row;

            if (
                !isEgg &&
                piece.SpriteRenderer != null &&
                blockSprites.Length > (int)type
            )
            {
                piece.SpriteRenderer.sprite =
                    blockSprites[(int)type];
            }

            piece.SetSortingOrder(row);

            return piece;
        }

        public void SpawnNewBottomRow()
        {
            for (int col = 0; col < columnCount; col++)
            {
                List<GridPiece> list = _stacks[col];

                for (int i = 0; i < list.Count; i++)
                {
                    list[i].RowIndex = i + 1;
                }

                bool isEgg = ShouldSpawnEgg();

                BlockType type = GetRandomBlockType();

                GridPiece newPiece = SpawnPiece(
                    col,
                    0,
                    isEgg,
                    type
                );

                list.Insert(0, newPiece);

                for (int row = 0; row < list.Count; row++)
                {
                    GridPiece piece = list[row];

                    piece.Column = col;
                    piece.RowIndex = row;

                    piece.transform.position =
                        GetWorldPosition(col, row);

                    piece.SetSortingOrder(row);
                }
            }
        }

        public Vector3 GetWorldPosition(int col, int row)
        {
            Vector3 origin = gridOrigin != null
                ? gridOrigin.position
                : Vector3.zero;

            return origin + new Vector3(
                col * cellSize,
                row * cellSize,
                0f
            );
        }

        public GridPiece PeekTop(int col)
        {
            List<GridPiece> list = _stacks[col];

            return list.Count > 0
                ? list[list.Count - 1]
                : null;
        }

        public GridPiece PopTop(int col)
        {
            List<GridPiece> list = _stacks[col];

            if (list.Count == 0)
                return null;

            GridPiece piece = list[list.Count - 1];

            list.RemoveAt(list.Count - 1);

            return piece;
        }

        public bool PushTop(int col, GridPiece piece)
        {
            List<GridPiece> list = _stacks[col];

            if (list.Count >= rowCount)
                return false;

            piece.transform.position =
                GetWorldPosition(col, list.Count);

            piece.Column = col;
            piece.RowIndex = list.Count;

            piece.SetSortingOrder(list.Count);

            list.Add(piece);

            return true;
        }

        public int GetColumnHeight(int col)
        {
            return _stacks[col].Count;
        }

        public GridPiece GetPieceAt(int col, int row)
        {
            if (col < 0 || col >= columnCount)
                return null;

            List<GridPiece> list = _stacks[col];

            if (row < 0 || row >= list.Count)
                return null;

            return list[row];
        }

        public void RemovePiece(GridPiece piece)
        {
            _stacks[piece.Column].Remove(piece);

            Destroy(piece.gameObject);
        }

        public void CollapseColumn(int col)
        {
            List<GridPiece> list = _stacks[col];

            for (int i = 0; i < list.Count; i++)
            {
                list[i].RowIndex = i;

                list[i].SetSortingOrder(i);

                list[i].transform.position =
                    GetWorldPosition(col, i);
            }
        }

#if UNITY_EDITOR
        [ContextMenu("Generate Column Click Zones")]
        private void GenerateColumnClickZones()
        {
            Transform existing =
                transform.Find("ColumnClickZones");

            if (existing != null)
                DestroyImmediate(existing.gameObject);

            GameObject container =
                new GameObject("ColumnClickZones");

            Undo.RegisterCreatedObjectUndo(
                container,
                "Generate Column Click Zones"
            );

            container.transform.SetParent(
                transform,
                worldPositionStays: false
            );

            float extraHeadroom = 1f;

            for (int col = 0; col < columnCount; col++)
            {
                GameObject zone =
                    new GameObject($"Column_{col}");

                zone.transform.SetParent(
                    container.transform,
                    worldPositionStays: false
                );

                Vector3 bottomCenter =
                    GetWorldPosition(col, 0);

                float height =
                    rowCount * cellSize + extraHeadroom;

                zone.transform.position =
                    bottomCenter +
                    new Vector3(
                        0f,
                        height / 2f - cellSize / 2f,
                        0f
                    );

                BoxCollider2D collider =
                    zone.AddComponent<BoxCollider2D>();

                collider.isTrigger = true;

                collider.size =
                    new Vector2(cellSize, height);

                ColumnMarker marker =
                    zone.AddComponent<ColumnMarker>();

                SerializedObject serializedMarker =
                    new SerializedObject(marker);

                serializedMarker.FindProperty(
                    "columnIndex"
                ).intValue = col;

                serializedMarker.ApplyModifiedProperties();
            }

            EditorUtility.SetDirty(gameObject);
        }
#endif
    }
}