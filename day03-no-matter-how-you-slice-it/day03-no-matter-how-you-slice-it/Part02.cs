using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace day03_no_matter_how_you_slice_it {
    class Part02 {
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

            Claim foundNonCollidingClaim = null;

            foreach (var claim in claims) {
                bool collided = false;
                foreach (var otherClaim in claims) {
                    if (claim.Id == otherClaim.Id) continue;
                    if (claim.Intersects(otherClaim)) {
                        collided = true;
                        break;
                    }
                }
                if (!collided) {
                    foundNonCollidingClaim = claim;
                    break;
                }
            }

            Console.WriteLine($"NonCollidingClaimId: {foundNonCollidingClaim.Id}");
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
