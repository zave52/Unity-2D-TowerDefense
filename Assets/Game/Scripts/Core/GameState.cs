namespace TowerDefense.Core
{
    public enum GameMode
    {
        PvE = 0,
        PvP = 1
    }

    public enum GameState
    {
        None = 0,
        Menu = 1,
        Preparation = 2,
        AttackerPreparation = 3,
        Battle = 4,
        RoundEnd = 5,
        GameOver = 6,
        GameWon = 7
    }
}
