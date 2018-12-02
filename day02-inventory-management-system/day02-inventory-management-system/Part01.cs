using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace day02_inventory_management_system {
    class Part01 {
        class LetterStructure : Dictionary<string, int> { }

        public static void Run() {
            Dictionary<string, LetterStructure> letterStructures = new Dictionary<string, LetterStructure>();
            
            string[] lines = File.ReadAllLines("input.txt");
            int max = 0;

            foreach (string line in lines) {
                var letterStructure = new LetterStructure();

                foreach (char character in line) {
                    string stringCharacter = character.ToString();
                    if (!letterStructure.ContainsKey(stringCharacter)) {
                        letterStructure.Add(stringCharacter, 0);
                    }
                    int newNumber = letterStructure[stringCharacter] + 1;
                    letterStructure[stringCharacter] = newNumber;
                    if (max < newNumber) {
                        max = newNumber;
                    }
                }

                letterStructures.Add(line, letterStructure);
            }

            Dictionary<int, int> identicalLetteredOnes = new Dictionary<int, int>();

            for (int t = 2; t <= max; t++) {
                foreach (var letterStructure in letterStructures.Values) {
                    foreach (var kvp in letterStructure) {
                        if (kvp.Value == t) {
                            if (!identicalLetteredOnes.ContainsKey(t)) {
                                identicalLetteredOnes.Add(t, 0);
                            }
                            int newNumber = identicalLetteredOnes[t] + 1;
                            identicalLetteredOnes[t] = newNumber;
                            break;
                        }
                    }
                }
            }
            
            Console.WriteLine($"Number of LetterStructures: {letterStructures.Count}");

            int result = 0;

            foreach (var iLO in identicalLetteredOnes) {
                Console.WriteLine($"Key: {iLO.Key}, Value: {iLO.Value}");
                if (result == 0) {
                    result = iLO.Value;
                } else {
                    result *= iLO.Value;
                }
            }

            Console.WriteLine($"Result: {result}");
        }
    }
}
