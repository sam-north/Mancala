namespace Mancala
{
    public class MancalaGameState
    {
        public bool HasGameBeenSetup { get; set; }
        public bool GameIsPlayable { get; set; }
        public bool IsPlayer1Turn { get; set; }
        public Player Player1 { get; set; }
        public Player Player2 { get; set; }
    }
}