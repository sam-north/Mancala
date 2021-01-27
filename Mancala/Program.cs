using System;
using System.Collections.Generic;
using System.Linq;

namespace Mancala
{
    class Program
    {
        private static bool gameIsPlayable;
        private static bool isPlayer1Turn;
        private static Player player1;
        private static Player player2;

        static void Main(string[] args)
        {
            Print("Mancala");
            Play();
        }

        private static void Play()
        {
            SetupGame();
            while (gameIsPlayable)
                Turn();
            AnnounceWinner();
            var response = Prompt("Start Over?");
            if (response.ToLower() == "y" || response.ToLower() == "yes")
                Play();
        }

        private static void AnnounceWinner()
        {
            var winner = (player1.Board[6] > player2.Board[6]) ? "Player 1" : (player2.Board[6] > player1.Board[6]) ? "Player 2" : "Tie";

            Console.ForegroundColor = ConsoleColor.Green;
            Print(winner + " wins!");
            Print($"{player1.Name} had {player1.Board[6]} and {player2.Name} had {player2.Board[6]}");
            Console.ResetColor();
        }

        private static void SetupGame()
        {
            gameIsPlayable = true;
            isPlayer1Turn = true;
            player1 = CreatePlayer("Player 1");
            player2 = CreatePlayer("Player 2");
        }

        private static Player CreatePlayer(string name)
        {
            var player = new Player();
            player.Name = name;
            player.Board = new int[7] { 4, 4, 4, 4, 4, 4, 0 };
            return player;
        }

        private static void Turn()
        {
            var currentPlayer = (isPlayer1Turn) ? player1 : player2;
            var opponentPlayer = (isPlayer1Turn) ? player2 : player1;
            PrintPlayerBoard(currentPlayer, opponentPlayer);
            var playerPossibleSpots = GetPlayerPossibleSpotsToMove(currentPlayer);
            var indexToMove = Convert.ToInt16(Prompt($"What spot to move? ({string.Join(',', playerPossibleSpots)})", playerPossibleSpots.Select(x => x.ToString()).ToList())) - 1;
            Move(indexToMove, currentPlayer, opponentPlayer);
            gameIsPlayable = CheckForEndOfGame();
        }

        private static bool CheckForEndOfGame()
        {
            var player1PossibleMoves = GetPlayerPossibleSpotsToMove(player1);
            if (!player1PossibleMoves.Any())
            {
                for (int i = 0; i < 6; i++)
                {
                    player2.Board[6] += player2.Board[i];
                    player2.Board[i] = 0;
                }
                return false;
            }

            var player2PossibleMoves = GetPlayerPossibleSpotsToMove(player2);
            if (!player2PossibleMoves.Any())
            {

                for (int i = 0; i < 6; i++)
                {
                    player1.Board[6] += player1.Board[i];
                    player1.Board[i] = 0;
                }
                return false;
            }

            return true;
        }

        private static void Move(int indexToMove, Player currentPlayer, Player opponentPlayer)
        {
            var currentMarblesToMove = currentPlayer.Board[indexToMove];
            currentPlayer.Board[indexToMove] = 0;
            var currentTargetIndex = indexToMove + 1;
            var targetCurrentPlayerBoard = true;
            while (currentMarblesToMove > 0)
            {
                if (currentTargetIndex == 6)
                {
                    if (targetCurrentPlayerBoard)
                    {
                        currentPlayer.Board[currentTargetIndex] += 1;
                        currentMarblesToMove -= 1;
                        if (currentMarblesToMove == 0)
                            return;
                    }

                    targetCurrentPlayerBoard = !targetCurrentPlayerBoard;
                    currentTargetIndex = 0;
                }
                else
                {
                    if (targetCurrentPlayerBoard)
                    {
                        var opponentIndex = 5 - currentTargetIndex;
                        if (currentPlayer.Board[currentTargetIndex] == 0 && currentMarblesToMove == 1 && opponentPlayer.Board[opponentIndex] > 0)
                        {
                            currentPlayer.Board[6] += currentMarblesToMove;
                            currentMarblesToMove -= 1;
                            currentPlayer.Board[6] += opponentPlayer.Board[opponentIndex];
                            opponentPlayer.Board[opponentIndex] = 0;
                        }
                        else
                        {
                            MoveStandardSpot(currentPlayer, ref currentMarblesToMove, ref currentTargetIndex);
                        }
                    }
                    else
                        MoveStandardSpot(opponentPlayer, ref currentMarblesToMove, ref currentTargetIndex);
                }
            }

            isPlayer1Turn = !isPlayer1Turn;
        }

        private static void MoveStandardSpot(Player player, ref int currentMarblesToMove, ref int currentTargetIndex)
        {
            player.Board[currentTargetIndex] += 1;
            currentMarblesToMove -= 1;
            currentTargetIndex++;
        }

        private static List<int> GetPlayerPossibleSpotsToMove(Player currentPlayer)
        {
            var result = new List<int>();
            for (int i = 0; i < currentPlayer.Board.Length - 1; i++)
            {
                if (currentPlayer.Board[i] > 0)
                    result.Add(i + 1);
            }
            return result;
        }

        private static string Prompt(string promptThis)
        {
            string input = "";
            while (input.Trim().Length <= 0)
            {
                Print(promptThis);
                input = Console.ReadLine();
            }
            return input;
        }

        private static string Prompt(string promptThis, List<string> expectedResponses)
        {
            var input = Prompt(promptThis);
            while (!expectedResponses.Contains(input))
                input = Prompt($"Choose from: {string.Join(",", expectedResponses)}");
            return input;
        }

        private static void Print(string line)
        {
            Console.WriteLine(line);
        }

        private static void PrintPlayerBoard(Player currentPlayer, Player opposingPlayer)
        {
            Console.WriteLine($"{currentPlayer.Name} it is your turn.");
            for (int i = 6; i >= 0; i--)
            {
                if (i != 6)
                    Console.Write(",");
                else
                    Console.ForegroundColor = ConsoleColor.Red;
                var valueString = opposingPlayer.Board[i] > 0 ? opposingPlayer.Board[i].ToString() : " ";
                Console.Write($"[{valueString}]");
                Console.ResetColor();
                if (i == 0)
                {
                    var opponentScoreDigits = currentPlayer.Board[6].ToString().Length;
                    Console.Write(",");
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write($"[{string.Empty.PadRight(opponentScoreDigits)}]");
                }
                Console.ResetColor();
            }
            Console.WriteLine();
            for (int i = 0; i < currentPlayer.Board.Length; i++)
            {
                if (i == 0)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    var opponentScoreDigits = opposingPlayer.Board[6].ToString().Length;
                    Console.Write($"[{string.Empty.PadRight(opponentScoreDigits)}]");
                    Console.ResetColor();
                    Console.Write(",");
                }
                var valueString = currentPlayer.Board[i] > 0 ? currentPlayer.Board[i].ToString() : " ";
                if (i == 6)
                    Console.ForegroundColor = ConsoleColor.Green;
                Console.Write($"[{valueString}]");
                if (i != 6)
                    Console.Write(",");
                Console.ResetColor();

            }
            Console.WriteLine();
        }
    }
}
