using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace day09_marble_mania {
    class Part02 {
        class Player {
            public long Score { get; set; }
        }

        static long marbleBag;
        static Marble current;
        static List<Player> players;

        static long lastMarbleValue;
        static int numOfPlayers;

        class Marble {
            public Marble Left { get; set; }
            public Marble Right { get; set; }
            public long Value { get; set; }

            public Marble(long pValue) {
                Value = pValue;
            }
        }

        static Marble marble;

        public static void Run() {
            var instructions = File.ReadAllText("input.txt");

            var parts = instructions.Split(new string[] { " players; last marble is worth " }, StringSplitOptions.RemoveEmptyEntries);

            numOfPlayers = int.Parse(parts[0]);
            lastMarbleValue = int.Parse(parts[1].Replace(" points", ""));

            lastMarbleValue *= 100;

            marbleBag = 1;
            players = new List<Player>();

            for (int i = 0; i < numOfPlayers; i++) {
                players.Add(new Player());
            }

            current = new Marble(0);
            current.Left = current;
            current.Right = current;

            bool run = false;
            do {
                run = Round();
            } while (run);

            Console.WriteLine("Highest Points: " + players.Max(p => p.Score));
        }

        static bool Round() {
            for (int p = 0; p < numOfPlayers; p++) {
                var player = players[p];
                var marble = new Marble(marbleBag++);

                if (marble.Value > lastMarbleValue) {
                    return false;
                }

                if (marble.Value % 23 == 0) {
                    player.Score += marble.Value;

                    var removeMarble = current.Left.Left.Left.Left.Left.Left.Left;

                    var leftNeighbor = removeMarble.Left;
                    var rightNeighbor = removeMarble.Right;

                    leftNeighbor.Right = rightNeighbor;
                    rightNeighbor.Left = leftNeighbor;

                    player.Score += removeMarble.Value;

                    current = rightNeighbor;
                } else {
                    if (current.Value == 0) {
                        current.Left = marble;
                        current.Right = marble;
                        marble.Left = current;
                        marble.Right = current;
                    } else {
                        var picked = current.Right.Right;
                        var currentsLeftNeighbor = picked.Left;
                        currentsLeftNeighbor.Right = marble;
                        picked.Left = marble;

                        marble.Left = currentsLeftNeighbor;
                        marble.Right = picked;
                    }

                    current = marble;
                }
            }

            return true;
        }
    }
}
