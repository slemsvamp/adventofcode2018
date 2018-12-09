using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace day09_marble_mania {
    class Part01 {
        class Player {
            public int Score { get; set; }
        }

        class Marble {
            public int Index { get; set; }
            public int Value { get; set; }
        }

        class MarbleCircle {
            public Marble[] Marbles { get; set; }

            public MarbleCircle() {
                Marbles = new Marble[] {
                    new Marble { Value = 0 }
                };
            }

            public void RecalculateIndexes() {
                for (int m = 0; m < Marbles.Length; m++) {
                    Marbles[m].Index = m;
                }
            }

            public void InsertAfter(int pIndex, Marble pMarble) {
                var newMarbleCircle = new List<Marble>();

                for (int m = 0; m < Marbles.Length; m++) {
                    newMarbleCircle.Add(Marbles[m]);

                    if (pIndex == m) {
                        newMarbleCircle.Add(pMarble);
                    }
                }
                Marbles = newMarbleCircle.ToArray();
                RecalculateIndexes();
            }

            public int IndexesCounterClockwise(int pStartIndex, int pMoves) {
                var returnIndex = pStartIndex - pMoves;
                while (returnIndex < 0) {
                    returnIndex = Marbles.Length + returnIndex;
                }
                return returnIndex;
            }

            public int IndexesClockwise(int pStartIndex, int pMoves) {
                var returnIndex = pStartIndex + pMoves;
                while (returnIndex > Marbles.Length - 1) {
                    returnIndex = returnIndex - Marbles.Length;
                }
                return returnIndex;
            }

            public int RemoveAt(int pIndex) {
                var newMarbleCircle = new List<Marble>();
                int valueTakenAway = -1;

                for (int m = 0; m < Marbles.Length; m++) {
                    if (pIndex != m) {
                        newMarbleCircle.Add(Marbles[m]);
                    } else {
                        valueTakenAway = Marbles[m].Value;
                    }
                }
                Marbles = newMarbleCircle.ToArray();
                RecalculateIndexes();

                return valueTakenAway;
            }

            public Marble GetMarbleAt(int pIndex) {
                while (pIndex > Marbles.Length - 1) {
                    pIndex = pIndex - Marbles.Length;
                }
                while (pIndex < 0) {
                    pIndex = Marbles.Length + pIndex;
                }
                return Marbles[pIndex];
            }

            public override string ToString() {
                string marbleCircle = "";
                for (int m = 0; m < Marbles.Length; m++) {
                    marbleCircle += Marbles[m].Value == current.Value ? "(" : " ";
                    marbleCircle += $"{Marbles[m].Value}";
                    marbleCircle += Marbles[m].Value == current.Value ? ")" : " ";
                }
                return marbleCircle;
            }
        }

        static Marble current;
        static int marbleBag;

        static MarbleCircle circle;
        static List<Player> players;

        static int lastMarbleValue;
        static int numOfPlayers;

        public static void Run() {
            var instructions = File.ReadAllText("input.txt");

            var parts = instructions.Split(new string[] { " players; last marble is worth " }, StringSplitOptions.RemoveEmptyEntries);

            numOfPlayers = int.Parse(parts[0]);
            lastMarbleValue = int.Parse(parts[1].Replace(" points", ""));

            marbleBag = 1;
            players = new List<Player>();
            circle = new MarbleCircle();
            current = circle.GetMarbleAt(0);

            for (int i = 0; i < numOfPlayers; i++) {
                players.Add(new Player());
            }

            while (Round()) {
            }

            Console.WriteLine("Highest Points: " + players.Max(p => p.Score));
        }

        static bool Round() {
            for (int p = 0; p < numOfPlayers; p++) {
                var player = players[p];
                var marble = Pick();
                
                if (marble == null) {
                    return false;
                }

                if (marble.Value % 23 == 0) {
                    // add to score
                    player.Score += marble.Value;

                    // remove 7th marble counter-clockwise from the current one
                    var removeIndex = circle.IndexesCounterClockwise(current.Index, 7);
                    var removedValue = circle.RemoveAt(removeIndex);
                    player.Score += removedValue;

                    current = circle.GetMarbleAt(removeIndex);
                } else {
                    var insertAtIndex = circle.IndexesClockwise(current.Index, 1);
                    circle.InsertAfter(insertAtIndex, marble);
                    current = marble;
                }
            }

            return true;
        }

        static Marble Pick() {
            if (marbleBag > lastMarbleValue) {
                return null;
            }

            return new Marble() {
                Value = marbleBag++
            };
        }

        static void Place(Marble pMarble) {

        }
    }
}
