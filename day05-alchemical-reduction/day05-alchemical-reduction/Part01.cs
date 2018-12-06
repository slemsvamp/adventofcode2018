using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace day05_alchemical_reduction {
    class Part01 {
        public static void Run() {
            string polymer = File.ReadAllText("input_without_p.txt");
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

                    string previousPlayheadUnit = polymer[playhead].ToString().ToUpper();
                    while (playhead > 0 && polymer[playhead-1].ToString().ToUpper() == previousPlayheadUnit) {
                        playhead--;
                        previousPlayheadUnit = polymer[playhead].ToString().ToUpper();
                    }

                    playhead--;
                }

                totalRemoved += numberOfUnitsToRemove;
            }

            Console.WriteLine("");
            Console.WriteLine($"Polymer Length: {polymer.Length}");
        }
    }
}
