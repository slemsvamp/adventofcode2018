using System;
using System.Collections.Generic;
using System.Text;

namespace day14_chocolate_charts {
    class Part01 {
        struct Recipe {
            public int Score { get; set; }
        }

        struct Elf {
            public int CurrentRecipe { get; set; }
        }

        static List<int> recipes;
        static List<Elf> elves;

        static string finalScore;

        public static void Run() {
            int input = 360781;
            int mockInput = 9;
            //input = mockInput;

            finalScore = "";

            recipes = new List<int>();
            recipes.AddRange(new int[] { 3, 7 });

            elves = new List<Elf>();
            elves.AddRange(new Elf[] {
                new Elf { CurrentRecipe = 0 },
                new Elf { CurrentRecipe = 1 }
            });

            while (!Round(input)) {
            }

            Console.WriteLine(finalScore);
        }

        static bool Round(int pNumberToReach) {
            CombineRecipes(pNumberToReach);
            return ChooseNewRecipes(pNumberToReach);
        }

        static bool ChooseNewRecipes(int pNumberToReach) {
            for (int e = 0; e < elves.Count; e++) {
                var elf = elves[e];
                elf.CurrentRecipe += 1 + recipes[elves[e].CurrentRecipe];
                while (elf.CurrentRecipe >= recipes.Count) {
                    elf.CurrentRecipe -= recipes.Count;
                }
                elves[e] = elf;
            }

            return WeDone(pNumberToReach);
        }

        static bool WeDone(int pNumberToReach) {
            if (recipes.Count > pNumberToReach + 10) {
                return true;
            }
            return false;
        }

        static void CombineRecipes(int pNumberToReach) {
            long score = 0;
            for (int e = 0; e < elves.Count; e++) {
                score += recipes[elves[e].CurrentRecipe];
            }
            string scoreString = score.ToString();
            for (int i = 0; i < scoreString.Length; i++) {
                var recipeScore = int.Parse(scoreString[i].ToString());
                recipes.Add(recipeScore);
                finalScore += recipeScore.ToString();
            }

            int adjustment = recipes.Count - (pNumberToReach + 10);
            if (adjustment < 0) adjustment = 0;

            try {
                var start = (finalScore.Length > 10 ? finalScore.Length - 10 : 0) - adjustment;
                var take = finalScore.Length > 10 ? 10 : finalScore.Length;
                finalScore = finalScore.Substring(start, take);
            } catch {
                Console.WriteLine("Error in FinalScore: " + finalScore);
            }
        }

        // next recipe = steps forward 1 plus the score of their current recipe
    }
}
