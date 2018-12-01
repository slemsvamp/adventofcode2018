using System;
using System.IO;

namespace day01_chronal_calibration {
    class Part01 {
        public static void Run() {
            Console.WriteLine("-*- Day01 - Part01 -*-");
            string[] frequencyChanges = File.ReadAllLines("input.txt");
            int result = 0;
            foreach (var change in frequencyChanges) {
                result += ParseInput(change);
            }
            Console.WriteLine($"Ending Frequency: {result}");
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
