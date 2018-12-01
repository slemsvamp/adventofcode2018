using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace day01_chronal_calibration {
    class Part02 {
        public static void Run() {
            Console.WriteLine("-*- Day01 - Part02 -*-");
            int result = 0;
            bool foundDuplicate = false;
            HashSet<int> knownFrequencies = new HashSet<int>();
            string[] frequencyChanges = File.ReadAllLines("input.txt");
            knownFrequencies.Add(result);
            while (!foundDuplicate) {
                foreach (var change in frequencyChanges) {
                    result += ParseInput(change);
                    if (knownFrequencies.Contains(result)) {
                        foundDuplicate = true;
                        break;
                    }
                    knownFrequencies.Add(result);
                }
            }
            Console.WriteLine($"Frequency Met Twice: {result}");
        }

        static int ParseInput(string change) {
            switch (change[0]) {
                case '+':
                    return int.Parse(change.Substring(1));
                case '-':
                    return -int.Parse(change.Substring(1));
            }
            throw new Exception($"Input could not be parsed: {change}");
        }
    }
}
