using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace day03_no_matter_how_you_slice_it {
    class Part01 {
        class Claim {
            public int Id { get; set; }
            public int Left { get; set; }
            public int Top { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }

            public bool Intersects(Claim claim) {
                return !(this.Left + this.Width - 1 < claim.Left ||
                    this.Top + this.Height - 1 < claim.Top ||
                    this.Left > claim.Left + claim.Width - 1 ||
                    this.Top > claim.Top + claim.Height - 1);
            }
        }

        static Regex regex;
        static int[,] fabric;

        public static void Run() {
            var lines = File.ReadLines("input.txt");
            int inchesPerSide = 1000;
            fabric = new int[inchesPerSide, inchesPerSide];
            regex = new Regex("#(\\d*) @ (\\d*),(\\d*): (\\d*)x(\\d*)");

            List<Claim> claims = new List<Claim>();

            foreach (var line in lines) {
                claims.Add(ParseClaim(line));
            }

            foreach (var claim in claims) {
                for (int y = claim.Top; y < claim.Top + claim.Height; y++) {
                    for (int x = claim.Left; x < claim.Left + claim.Width; x++) {
                        fabric[y, x] = fabric[y, x] + 1;
                    }
                }
            }

            int inchesThatCollide = 0;

            for (int y = 0; y < inchesPerSide; y++) {
                for (int x = 0; x < inchesPerSide; x++) {
                    if (fabric[y, x] > 1) {
                        inchesThatCollide++;
                    }
                }
            }

            Console.WriteLine($"InchesThatCollide: {inchesThatCollide}");
        }

        static Claim ParseClaim(string line) {
            var match = regex.Match(line);

            return new Claim {
                Id = int.Parse(match.Groups[1].Value),
                Left = int.Parse(match.Groups[2].Value),
                Top = int.Parse(match.Groups[3].Value),
                Width = int.Parse(match.Groups[4].Value),
                Height = int.Parse(match.Groups[5].Value)
            };
        }
    }
}
