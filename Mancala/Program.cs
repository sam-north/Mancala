using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Mancala
{
    class Program
    {
        private static string saveFileName = "manacala-save.json";
        private static MancalaGameState _gameState;

        static void Main(string[] args)
        {
            Print("Mancala");
            LoadGame();
            Play();
        }

        private static void Play()
        {
            if (_gameState == null || (_gameState != null && !_gameState.HasGameBeenSetup))
                SetupGame();
            while (_gameState.GameIsPlayable)
            {
                PrintPlayerBoard();
                var userInput = Prompt(string.Empty);
                CheckGameState(GetCurrentPlayer().Name, userInput);
                SaveGame();
            }
        }

        private static void LoadGame()
        {
            if (File.Exists(saveFileName))
            {
                var fileText = File.ReadAllText(saveFileName);
                _gameState = JsonSerializer.Deserialize<MancalaGameState>(fileText);
                if (_gameState.HasGameBeenSetup && !_gameState.GameIsPlayable)
                    _gameState = null;
            }
        }

        private static void SaveGame()
        {
            string jsonString = JsonSerializer.Serialize(_gameState);
            File.WriteAllText(saveFileName, jsonString);
        }

        private static void CheckGameState(string username, string input)
        {
            if (_gameState.GameIsPlayable) Turn(username, input);
            else AnnounceWinner();
        }

        private static void AnnounceWinner()
        {
            var winner = (_gameState.Player1.Board[6] > _gameState.Player2.Board[6]) ? "Player 1" : (_gameState.Player2.Board[6] > _gameState.Player1.Board[6]) ? "Player 2" : "Tie";

            Console.ForegroundColor = ConsoleColor.Green;
            Print(winner + " wins!");
            Print($"{_gameState.Player1.Name} had {_gameState.Player1.Board[6]} and {_gameState.Player2.Name} had {_gameState.Player2.Board[6]}");
            Console.ResetColor();

            var response = Prompt("Start Over?");
            if (response.ToLower() == "y" || response.ToLower() == "yes")
                Play();
        }

        private static void SetupGame()
        {
            _gameState = new MancalaGameState
            {
                GameIsPlayable = true,
                IsPlayer1Turn = true,
                Player1 = CreatePlayer("Player1"),
                Player2 = CreatePlayer("Player2"),
                HasGameBeenSetup = true
            };
        }

        private static Player CreatePlayer(string name)
        {
            var player = new Player();
            player.Name = name;
            player.Board = new int[7] { 4, 4, 4, 4, 4, 4, 0 };
            return player;
        }

        private static void Turn(string username, string input)
        {
            _gameState.GameIsPlayable = CheckForEndOfGame();
            if (!_gameState.GameIsPlayable) return;

            if (string.IsNullOrWhiteSpace(username) && string.IsNullOrWhiteSpace(input))
            {
                Print("Invalid input");
                return;
            }

            var currentPlayer = GetCurrentPlayer();
            var opponentPlayer = GetOpponentPlayer();
            if (username.ToLower() != currentPlayer.Name.ToLower())
            {
                Print("It's not your turn!");
                return;
            }

            var playerPossibleSpots = GetPlayerPossibleSpotsToMove(currentPlayer);
            if (!(short.TryParse(input, out var indexToMove) && playerPossibleSpots.Contains(indexToMove)))
            {
                Print($"Invalid move. Choose from: {string.Join(",", playerPossibleSpots)}");
                return;
            }

            Move(indexToMove - 1, currentPlayer, opponentPlayer);
        }

        private static bool CheckForEndOfGame()
        {
            var player1PossibleMoves = GetPlayerPossibleSpotsToMove(_gameState.Player1);
            if (!player1PossibleMoves.Any())
            {
                for (int i = 0; i < 6; i++)
                {
                    _gameState.Player2.Board[6] += _gameState.Player2.Board[i];
                    _gameState.Player2.Board[i] = 0;
                }
                return false;
            }

            var player2PossibleMoves = GetPlayerPossibleSpotsToMove(_gameState.Player2);
            if (!player2PossibleMoves.Any())
            {

                for (int i = 0; i < 6; i++)
                {
                    _gameState.Player1.Board[6] += _gameState.Player1.Board[i];
                    _gameState.Player1.Board[i] = 0;
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

            _gameState.IsPlayer1Turn = !_gameState.IsPlayer1Turn;
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

        private static void PrintPlayerBoard()
        {
            var currentPlayer = GetCurrentPlayer();
            var opponentPlayer = GetOpponentPlayer();
            Console.WriteLine($"{currentPlayer.Name} it is your turn.");
            for (int i = 6; i >= 0; i--)
            {
                if (i != 6)
                    Console.Write(",");
                else
                    Console.ForegroundColor = ConsoleColor.Red;
                var valueString = opponentPlayer.Board[i] > 0 ? opponentPlayer.Board[i].ToString() : " ";
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
                    var opponentScoreDigits = opponentPlayer.Board[6].ToString().Length;
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
        }

        private static Player GetOpponentPlayer()
        {
            return (_gameState.IsPlayer1Turn) ? _gameState.Player2 : _gameState.Player1;
        }

        private static Player GetCurrentPlayer()
        {
            return (_gameState.IsPlayer1Turn) ? _gameState.Player1 : _gameState.Player2;
        }
    }
}
