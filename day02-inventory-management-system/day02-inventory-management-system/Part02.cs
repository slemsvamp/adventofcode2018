using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace day02_inventory_management_system {
    class Part02 {
        class IDPair {
            public string Line { get; set; }
            public string Check { get; set; }
            public int ErrorAt { get; set; }
        }

        public static void Run() {
            string[] lines = File.ReadAllLines("input.txt");
            var almostDuplicateLines = new List<IDPair>();

            for (var lineIndex = 0; lineIndex < lines.Length; lineIndex++) {
                for (var checkIndex = 0; checkIndex < lines.Length; checkIndex++) {
                    int errors = 0;
                    if (checkIndex == lineIndex) continue;
                    string line = lines[lineIndex];
                    string check = lines[checkIndex];

                    int maxLength = line.Length >= check.Length ? line.Length : check.Length;
                    int errorAt = 0;

                    for (int letterCheck = 0; letterCheck < maxLength; letterCheck++) {
                        if (line[letterCheck] != check[letterCheck]) {
                            errors++;
                            errorAt = letterCheck;
                        }
                        if (errors > 1) break;
                    }

                    if (errors == 1) {
                        almostDuplicateLines.Add(new IDPair { Line = line, Check = check, ErrorAt = errorAt });
                    }
                }
            }

            var winner = new IDPair {
                Line = almostDuplicateLines[0].Line,
                Check = almostDuplicateLines[0].Check,
                ErrorAt = almostDuplicateLines[0].ErrorAt
            };

            using (var stream = File.Open("result.txt", FileMode.Create)) {
                string fixedId = winner.Check.Substring(0, winner.ErrorAt) + winner.Check.Substring(winner.ErrorAt + 1);
                Console.WriteLine($"Check: {winner.Check}");
                Console.WriteLine($"Line: {winner.Line}");
                Console.WriteLine($"ErrorAt: {winner.ErrorAt}");
                Console.WriteLine($"FixedId: {fixedId}");

                stream.Write(UTF8Encoding.UTF8.GetBytes(fixedId), 0, fixedId.Length);
                stream.Flush();
            }

            Console.WriteLine("Output result.txt");
        }
    }
}
