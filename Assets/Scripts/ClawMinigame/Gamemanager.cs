using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DinoDig
{
    public enum GameState
    {
        WaitingForSource,
        ClawMovingToSource,
        WaitingForDestination,
        ClawMovingToDestination,
        ResolvingMatches,
        GameOver
    }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField] private GridManager grid;
        [SerializeField] private ClawController claw;
        [SerializeField] private float cascadeStepDelay = 0.15f;

        public GameState State { get; private set; } = GameState.WaitingForSource;

        public event System.Action<int> OnScoreChanged;
        public event System.Action OnGameOver;
        public event System.Action<int> OnEggCollected;

        private int _sourceColumn = -1;
        private int _score;

        void Awake()
        {
            Instance = this;
        }

        public void OnColumnClicked(int column)
        {
            if (State == GameState.GameOver) return;

            if (State == GameState.WaitingForSource)
            {
                GridPiece top = grid.PeekTop(column);
                if (top == null) return;

                if (top.IsEgg)
                {
                    CollectEgg(column, top);
                    return;
                }

                _sourceColumn = column;
                State = GameState.ClawMovingToSource;
                StartCoroutine(claw.PickUp(grid, column, OnPickedUp));
            }
            else if (State == GameState.WaitingForDestination)
            {
                if (column == _sourceColumn) return;

                State = GameState.ClawMovingToDestination;
                StartCoroutine(claw.PlaceDown(grid, column, OnPlaced));
            }
        }

        private void OnPickedUp(GridPiece piece)
        {
            State = piece == null ? GameState.WaitingForSource : GameState.WaitingForDestination;
        }

        private void OnPlaced(bool success)
        {
            if (!success)
            {
                State = GameState.WaitingForDestination;
                return;
            }

            State = GameState.ResolvingMatches;
            StartCoroutine(ResolveAndContinue());
        }

        private IEnumerator ResolveAndContinue()
        {
            while (true)
            {
                List<GridPiece> matches = MatchResolver.FindAllMatches(grid);
                if (matches.Count == 0) break;

                _score += matches.Count * 10;
                OnScoreChanged?.Invoke(_score);

                var affectedColumns = new HashSet<int>();
                foreach (GridPiece piece in matches)
                {
                    affectedColumns.Add(piece.Column);
                    grid.RemovePiece(piece);
                }
                foreach (int col in affectedColumns)
                    grid.CollapseColumn(col);

                yield return new WaitForSeconds(cascadeStepDelay);
            }

            if (CheckGameOver())
            {
                State = GameState.GameOver;
                OnGameOver?.Invoke();
            }
            else
            {
                _sourceColumn = -1;
                State = GameState.WaitingForSource;
            }
        }

        private bool CheckGameOver()
        {
            int dangerRow = grid.RowCount - 1;
            for (int col = 0; col < grid.ColumnCount; col++)
            {
                if (grid.GetColumnHeight(col) - 1 >= dangerRow)
                    return true;
            }
            return false;
        }

        private void CollectEgg(int column, GridPiece egg)
        {
            grid.RemovePiece(egg);
            OnEggCollected?.Invoke(column);
        }
    }
}