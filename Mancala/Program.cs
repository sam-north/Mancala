using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Mancala
{
    class Program
    {

        static void Main(string[] args)
        {
            var hasSkippedIntroPrompt = false;
            while (true)
            {
                var input = string.Empty;
                if (hasSkippedIntroPrompt)
                    input = Console.ReadLine();
                var splitInput = input.Split(" ").ToList();
                if (splitInput.Count == 1)
                    splitInput.Add(string.Empty);
                hasSkippedIntroPrompt = true;
                var mancalaGame = new MancalaGame();
                var feedbackMessages = mancalaGame.InitializeSession(splitInput[0] ?? string.Empty, splitInput[1] ?? string.Empty);
                foreach (var message in feedbackMessages)
                    Console.WriteLine(message);
            }
        }
    }
}