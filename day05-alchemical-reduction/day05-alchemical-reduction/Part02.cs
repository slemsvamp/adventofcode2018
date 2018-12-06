using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace day05_alchemical_reduction {
    class Part02 {
        public static void Run() {
            string polymer = File.ReadAllText("input.txt");

            var upperPolymer = polymer.ToUpper();
            var distinctChars = upperPolymer.Distinct().ToArray();

            Dictionary<char, int> results = new Dictionary<char, int>();

            for (int i = 0; i < distinctChars.Length; i++) {
                string newPolymer = polymer.ToString();
                newPolymer = newPolymer.Replace(distinctChars[i].ToString().ToUpper(), "");
                newPolymer = newPolymer.Replace(distinctChars[i].ToString().ToLower(), "");
                var polymerLength = React(newPolymer);
                results.Add(distinctChars[i], polymerLength);
            }

            var minPolymer = results.Min(kvp => kvp.Value);
            var keyChar = results.Where(kvp => kvp.Value == minPolymer).Select(kvp => kvp.Key.ToString()).First();

            Console.WriteLine("");
            Console.WriteLine($"{keyChar}={minPolymer}");
        }

        static int React(string polymer) {
            int totalRemoved = 0;

            for (int playhead = 0; playhead < polymer.Length; playhead++) {
                int seek = 1;

                string previousUnit = polymer[playhead].ToString();
                bool previousUnitIsLowerPolarity = previousUnit == previousUnit.ToLower();

                int numberOfUnitsToRemove = 0;
                while (playhead + seek < polymer.Length) {
                    string nextUnit = polymer[playhead + seek].ToString();
                    bool nextUnitIsLowerPolarity = nextUnit == nextUnit.ToLower();
                    if (previousUnit.ToUpper() == nextUnit.ToUpper() && previousUnitIsLowerPolarity != nextUnitIsLowerPolarity) {
                        // same type and reverse polarity
                        numberOfUnitsToRemove = 2;
                        break;
                    } else {
                        break;
                    }
                }

                if (numberOfUnitsToRemove > 0) {
                    // remove
                    polymer = polymer.Remove(playhead, numberOfUnitsToRemove);

                    if (playhead == polymer.Length) {
                        continue;
                    }

                    string previousPlayheadUnit = polymer[playhead].ToString().ToUpper();
                    while (playhead > 0 && polymer[playhead - 1].ToString().ToUpper() == previousPlayheadUnit) {
                        playhead--;
                        previousPlayheadUnit = polymer[playhead].ToString().ToUpper();
                    }

                    playhead--;
                }

                totalRemoved += numberOfUnitsToRemove;
            }

            return polymer.Length;
        }
    }
}
