namespace Mancala
{
    public class Player
    {
        public Player()
        {
            Board = new int[7];
        }

        public int[] Board { get; set; }
        public string Name { get; set; }
    }
}
