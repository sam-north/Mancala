using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace Mancala
{
    public class MancalaGame
    {
        const string saveFileName = "manacala-save.json";
        MancalaGameState _gameState;
        List<string> _feedbackMessages = new List<string>();

        public List<string> InitializeSession(string username = null, string userInput = null)
        {
            LoadGame();
            Play(username, userInput);
            return _feedbackMessages;
        }

        private void Play(string username, string userInput)
        {
            if (_gameState == null || (_gameState != null && !_gameState.HasGameBeenSetup))
                SetupGame();
            if (_gameState.GameIsPlayable)
            {
                CheckGameState(username, userInput);
                PrintPlayerBoard();
                SaveGame();
            }
        }

        private void LoadGame()
        {
            if (File.Exists(saveFileName))
            {
                var fileText = File.ReadAllText(saveFileName);
                _gameState = JsonSerializer.Deserialize<MancalaGameState>(fileText);
                if (_gameState.HasGameBeenSetup && !_gameState.GameIsPlayable)
                    _gameState = null;
            }
        }

        private void SaveGame()
        {
            string jsonString = JsonSerializer.Serialize(_gameState);
            File.WriteAllText(saveFileName, jsonString);
        }

        private void CheckGameState(string username, string input)
        {
            if (_gameState.GameIsPlayable) Turn(username, input);
            else AnnounceWinner();
        }

        private void AnnounceWinner()
        {
            var winner = (_gameState.Player1.Board[6] > _gameState.Player2.Board[6]) ? "Player 1" : (_gameState.Player2.Board[6] > _gameState.Player1.Board[6]) ? "Player 2" : "Tie";

            _feedbackMessages.Add(winner + " wins!");
            _feedbackMessages.Add($"{_gameState.Player1.Name} had {_gameState.Player1.Board[6]} and {_gameState.Player2.Name} had {_gameState.Player2.Board[6]}");
        }

        private void SetupGame()
        {
            _gameState = new MancalaGameState
            {
                GameIsPlayable = true,
                IsPlayer1Turn = true,
                Player1 = CreatePlayer("Player1"),
                Player2 = CreatePlayer("Player2"),
                HasGameBeenSetup = true
            };
            _feedbackMessages.Add("Welcome to Mancala!");
        }

        private Player CreatePlayer(string name)
        {
            var player = new Player();
            player.Name = name;
            player.Board = new int[7] { 4, 4, 4, 4, 4, 4, 0 };
            return player;
        }

        private void Turn(string username, string input)
        {
            _gameState.GameIsPlayable = CheckForEndOfGame();
            if (!_gameState.GameIsPlayable) return;

            if (string.IsNullOrWhiteSpace(username) && string.IsNullOrWhiteSpace(input)) return;

            var currentPlayer = GetCurrentPlayer();
            var opponentPlayer = GetOpponentPlayer();
            if (username.ToLower() != currentPlayer.Name.ToLower())
            {
                _feedbackMessages.Add("It's not your turn!");
                return;
            }

            var playerPossibleSpots = GetPlayerPossibleSpotsToMove(currentPlayer);
            if (!(short.TryParse(input, out var indexToMove) && playerPossibleSpots.Contains(indexToMove)))
            {
                _feedbackMessages.Add($"Invalid move. Choose from: {string.Join(",", playerPossibleSpots)}");
                return;
            }

            Move(indexToMove - 1, currentPlayer, opponentPlayer);
        }

        private bool CheckForEndOfGame()
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

        private void Move(int indexToMove, Player currentPlayer, Player opponentPlayer)
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

        private void MoveStandardSpot(Player player, ref int currentMarblesToMove, ref int currentTargetIndex)
        {
            player.Board[currentTargetIndex] += 1;
            currentMarblesToMove -= 1;
            currentTargetIndex++;
        }

        private List<int> GetPlayerPossibleSpotsToMove(Player currentPlayer)
        {
            var result = new List<int>();
            for (int i = 0; i < currentPlayer.Board.Length - 1; i++)
            {
                if (currentPlayer.Board[i] > 0)
                    result.Add(i + 1);
            }
            return result;
        }

        private void PrintPlayerBoard()
        {
            var currentPlayer = GetCurrentPlayer();
            var opponentPlayer = GetOpponentPlayer();
            _feedbackMessages.Add($"{currentPlayer.Name} it is your turn.");
            var feedbackMessage = new StringBuilder();
            for (int i = 6; i >= 0; i--)
            {
                if (i != 6)
                    feedbackMessage.Append(",");
                var valueString = opponentPlayer.Board[i] > 0 ? opponentPlayer.Board[i].ToString() : " ";
                feedbackMessage.Append($"[{valueString}]");
                if (i == 0)
                {
                    var opponentScoreDigits = currentPlayer.Board[6].ToString().Length;
                    feedbackMessage.Append(",");
                    feedbackMessage.Append($"[{string.Empty.PadRight(opponentScoreDigits)}]");
                }
            }
            _feedbackMessages.Add(feedbackMessage.ToString());
            feedbackMessage.Clear();
            for (int i = 0; i < currentPlayer.Board.Length; i++)
            {
                if (i == 0)
                {
                    var opponentScoreDigits = opponentPlayer.Board[6].ToString().Length;
                    feedbackMessage.Append($"[{string.Empty.PadRight(opponentScoreDigits)}]");
                    feedbackMessage.Append(",");
                }
                var valueString = currentPlayer.Board[i] > 0 ? currentPlayer.Board[i].ToString() : " ";
                feedbackMessage.Append($"[{valueString}]");
                if (i != 6)
                    feedbackMessage.Append(",");
            }
            _feedbackMessages.Add(feedbackMessage.ToString());
        }

        private Player GetOpponentPlayer()
        {
            return (_gameState.IsPlayer1Turn) ? _gameState.Player2 : _gameState.Player1;
        }

        private Player GetCurrentPlayer()
        {
            return (_gameState.IsPlayer1Turn) ? _gameState.Player1 : _gameState.Player2;
        }

    }
}
