using System;

namespace TowerDefense.Core
{
    public sealed class GameStateMachine
    {
        public event Action<GameState, GameState> StateChanged;

        public GameState CurrentState { get; private set; } = GameState.None;
        public int CurrentRound { get; private set; }

        public bool TrySetState(GameState next)
        {
            if (next == CurrentState || !CanTransition(CurrentState, next))
            {
                return false;
            }

            var previous = CurrentState;
            CurrentState = next;

            if (next == GameState.Preparation)
            {
                CurrentRound = Math.Max(1, CurrentRound + 1);
            }

            if (next == GameState.Menu)
            {
                CurrentRound = 0;
            }

            StateChanged?.Invoke(previous, next);
            return true;
        }

        public bool CanTransition(GameState from, GameState to)
        {
            return (from, to) switch
            {
                (GameState.None, GameState.Menu) => true,
                (GameState.Menu, GameState.Preparation) => true,
                (GameState.Preparation, GameState.AttackerPreparation) => true,
                (GameState.Preparation, GameState.Battle) => true,
                (GameState.AttackerPreparation, GameState.Battle) => true,
                (GameState.Battle, GameState.RoundEnd) => true,
                (GameState.Battle, GameState.GameOver) => true,
                (GameState.RoundEnd, GameState.Preparation) => true,
                (GameState.RoundEnd, GameState.GameOver) => true,
                (GameState.RoundEnd, GameState.GameWon) => true,
                (GameState.GameOver, GameState.Menu) => true,
                (GameState.GameWon, GameState.Menu) => true,
                _ => false
            };
        }
    }
}
