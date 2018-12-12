using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace day12_subterranean_sustainability {
    class Part01 {
        class Rule {
            public Dictionary<int, bool> RuleItems { get; set; }
            public string Filter { get; set; }
            public bool CreatesAPlant { get; set; }

            public Rule(string pLine) {
                RuleItems = new Dictionary<int, bool>();

                var parts = pLine.Split(" => ");

                Filter = parts[0];

                for (int i = -2; i <= 2; i++) {
                    RuleItems.Add(i, parts[0][i + 2] == '#');
                }

                CreatesAPlant = parts[1] == "#";
            }

            public List<int> Valid(int pMinRange, string pGenerationString) {
                var indexes = new List<int>();

                for (int i = pMinRange; i < pMinRange + pGenerationString.Length - 5; i++) {
                    if (pGenerationString.Substring(i - pMinRange, 5) == Filter) {
                        indexes.Add(i+2);
                    }
                }

                return indexes;
            }
        }

        struct Instruction {
            public int PotId { get; set; }
            public bool CreatesAPlant {get;set; }
        }

        struct Pot {
            public bool HasPlant { get; set; }
        }

        class Generation {
            public Dictionary<int, Pot> Pots { get; set; }
            public int MinRange { get; set; }
            public int MaxRange { get; set; }

            public Generation(int pMinRange, string pGenerationString) {
                Pots = new Dictionary<int, Pot>();
                MinRange = int.MaxValue;
                MaxRange = 0;

                for (int i = pMinRange; i < pMinRange + pGenerationString.Length; i++) {
                    Pots.Add(i, new Pot { HasPlant = pGenerationString[i - pMinRange] == '#' });
                    if (i < MinRange) MinRange = i;
                    if (i > MaxRange) MaxRange = i;
                }
            }

            public string ToString(int pMinRange, int pMaxRange) {
                var potString = string.Empty;
                for (int i = pMinRange; i <= pMaxRange; i++) {
                    if (Pots.ContainsKey(i)) {
                        potString += Pots[i].HasPlant ? "#" : ".";
                    } else {
                        potString += ".";
                    }
                }
                return potString;
            }

            public override string ToString() {
                return ToString(MinRange, MaxRange);
            }
        }

        class Cavern {
            public int CurrentGeneration { get; set; }
            public Dictionary<int, Generation> Generations { get; set; }

            public Cavern(string pInitialState) {
                Generations = new Dictionary<int, Generation>();

                CurrentGeneration = -1;
                NextGeneration(0, pInitialState);
            }

            public void AddPot(int pPotId, Pot pPot) {
                var generation = Generations[CurrentGeneration];
                generation.Pots.Add(pPotId, pPot);
            }

            public void NextGeneration(int pMinRange, string pGeneration) {
                CurrentGeneration++;
                Generations.Add(CurrentGeneration, new Generation(pMinRange, pGeneration));
            }

            public string ToString(int pGeneration, int pMinRange, int pMaxRange) {
                return pGeneration + ": " + Generations[pGeneration].ToString(pMinRange, pMaxRange);
            }

            public string ToString(int pGeneration) {
                return pGeneration + ": " + Generations[pGeneration].ToString();
            }

            public override string ToString() {
                return CurrentGeneration + ": " + Generations[CurrentGeneration].ToString();
            }
        }

        static Cavern cavern;

        public static void Run() {
            var lines = File.ReadAllLines("input.txt");

            var initialState = lines[0].Substring(15);
            var rules = new List<Rule>();

            for (int l = 2; l < lines.Length; l++) {
                var rule = new Rule(lines[l]);
                rules.Add(rule);
            }

            cavern = new Cavern(initialState);

            //Console.WriteLine(cavern.ToString(cavern.CurrentGeneration, -3, 35));

            for (int g = 1; g <= 20; g++) {
                var instructions = new List<Instruction>();

                var currentGeneration = cavern.Generations[cavern.CurrentGeneration];

                var currentGenerationString = currentGeneration.ToString();
                var generationString = currentGenerationString;

                int minRange = currentGeneration.MinRange;
                int maxRange = currentGeneration.MaxRange;

                //if (currentGenerationString.StartsWith("#")) {
                    minRange -= 3;
                    generationString = "..." + generationString;
                //}

                //if (currentGenerationString.EndsWith("#")) {
                    maxRange += 3;
                    generationString = generationString + "...";
                //}

                foreach (var rule in rules) {
                    var result = rule.Valid(minRange, generationString);
                    if (result.Count > 0) {
                        foreach (int potId in result) {
                            instructions.Add(new Instruction { PotId = potId, CreatesAPlant = rule.CreatesAPlant });
                        }
                    }
                }

                var generation = new Generation(minRange, new string('.', maxRange - minRange));

                foreach (var instruction in instructions) {
                    var pot = generation.Pots[instruction.PotId];
                    pot.HasPlant = instruction.CreatesAPlant;
                    generation.Pots[instruction.PotId] = pot;
                }

                cavern.NextGeneration(minRange, generation.ToString());
                //Console.WriteLine(cavern.ToString(cavern.CurrentGeneration, -3, 35));
            }

            var lastGeneration = cavern.Generations[20];

            int sum = 0;

            foreach (var kvp in lastGeneration.Pots) {
                if (kvp.Value.HasPlant) {
                    sum += kvp.Key;
                }
            }

            //Console.WriteLine(lastGeneration.ToString());
            Console.WriteLine(sum);
            //AssertMockInput();
        }

        static void AssertMockInput() {
            var mockInputs = new Dictionary<int, string> {
                { 0, "...#..#.#..##......###...###..........." },
                { 1, "...#...#....#.....#..#..#..#..........." },
                { 2, "...##..##...##....#..#..#..##.........." },
                { 3, "..#.#...#..#.#....#..#..#...#.........." },
                { 4, "...#.#..#...#.#...#..#..##..##........." },
                { 5, "....#...##...#.#..#..#...#...#........." },
                { 6, "....##.#.#....#...#..##..##..##........" },
                { 7, "...#..###.#...##..#...#...#...#........" },
                { 8, "...#....##.#.#.#..##..##..##..##......." },
                { 9, "...##..#..#####....#...#...#...#......." },
                { 10, "..#.#..#...#.##....##..##..##..##......" },
                { 11, "...#...##...#.#...#.#...#...#...#......" },
                { 12, "...##.#.#....#.#...#.#..##..##..##....." },
                { 13, "..#..###.#....#.#...#....#...#...#....." },
                { 14, "..#....##.#....#.#..##...##..##..##...." },
                { 15, "..##..#..#.#....#....#..#.#...#...#...." },
                { 16, ".#.#..#...#.#...##...#...#.#..##..##..." },
                { 17, "..#...##...#.#.#.#...##...#....#...#..." },
                { 18, "..##.#.#....#####.#.#.#...##...##..##.." },
                { 19, ".#..###.#..#.#.#######.#.#.#..#.#...#.." },
                { 20, ".#....##....#####...#######....#.#..##." }
            };
            foreach (var kvp in mockInputs) {
                bool assertResult = cavern.ToString(kvp.Key, -3, 35) == kvp.Key + ": " + kvp.Value;

                if (!assertResult) {
                    throw new Exception($"Assert failed! Expected: {kvp.Value}, Actual: {cavern}");

                }
            }
        }
    }
}
