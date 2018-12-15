using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace day15_beverage_bandits {
    class Part02 {
        enum Race {
            Goblin, Elf
        }
        class Entity {
            public Race Race { get; set; }
            public int HitPoints { get; set; }
            public Point Position { get; set; }
            public int? NextMove { get; set; }
            public int AttackPower { get; set; }
            public int Initiative { get { return Position.Y * map.GetLength(0) + Position.X; } }
            public Entity Target { get; set; }
            public void TakeDamage(int pDamage) {
                HitPoints -= pDamage;
            }

            public bool IsDead {
                get {
                    return HitPoints <= 0;
                }
            }

            public override string ToString() {
                return $"[{Race}] X={Position.X},Y={Position.Y}, Target={{X={Target.Position.X},Y={Target.Position.Y}}}";
            }
        }
        static bool[,] map;
        static SortedDictionary<int, Entity> entities;
        static string[] lines;
        static Direction[] order;
        static int rounds;
        static bool elvesWon;
        static bool elvesFailed;

        public static void Run() {
            elvesWon = false;
            int elfAttackPower = 16;

            while (!elvesWon) {
                elfAttackPower++;
                elvesFailed = false;
                RunNth(false, elfAttackPower, "input.txt");
            }
        }

        public static void RunNth(bool pDebug, int pElfAttackPower, string pFile) {
            RunOnce(pDebug, pElfAttackPower, pFile, 0, 0, 0);
        }

        public static void RunOnce(bool pDebug, int pElfAttackPower, string pFile, int pExpectedRounds, int pExpectedHitpoints, int pExpectedTotal) {
            Console.CursorVisible = false;

            lines = File.ReadAllLines(pFile);
            order = new Direction[] { Direction.Up, Direction.Left, Direction.Right, Direction.Down };
            map = new bool[lines[0].Length, lines.Length];
            rounds = 0;
            entities = new SortedDictionary<int, Entity>();
            for (int y = 0; y < lines.Length; y++) {
                for (int x = 0; x < lines[y].Length; x++) {
                    map[x, y] = lines[y][x] == '#';
                    if (lines[y][x] == 'G') {
                        var goblin = new Entity { Race = Race.Goblin, HitPoints = 200, Position = new Point(x, y), AttackPower = 3 };
                        entities.Add(goblin.Initiative, goblin);
                    } else if (lines[y][x] == 'E') {
                        var elf = new Entity { Race = Race.Elf, HitPoints = 200, Position = new Point(x, y), AttackPower = pElfAttackPower };
                        entities.Add(elf.Initiative, elf);
                    }
                }
            }

            var cleanLines = new List<string>();
            foreach (var line in lines) {
                cleanLines.Add(line.Replace("E", ".").Replace("G", "."));
            }
            lines = cleanLines.ToArray();

            if (pDebug) DrawBattlefield();
            bool fastForward = false;
            //if (pDebug) Console.ReadKey(true);

            while (Round()) {
                rounds++;
                if (pDebug) {
                    DrawBattlefield();
                    Console.WriteLine("Round: " + rounds);
                }

                if (!fastForward && pDebug) {
                    var key = Console.ReadKey(true);
                    if (key.Key == ConsoleKey.Escape) return;
                    if (key.Key == ConsoleKey.F) fastForward = true;
                }
            };

            if (pDebug) {
                DrawBattlefield();
                Console.ReadKey(true);
            }

            int sum = 0;
            foreach (var kvp in entities) {
                sum += kvp.Value.IsDead ? 0 : kvp.Value.HitPoints;
            }
            var total = sum * rounds;

            if (pExpectedTotal == 0) {
                if (elvesWon) Console.WriteLine(pFile + $": Rounds={rounds}, Hitpoints={sum}, Total={total}, Attack Power: {pElfAttackPower}");
            } else {
                Console.ForegroundColor = ConsoleColor.Red;
                if (pExpectedTotal == total && pExpectedRounds == rounds && pExpectedHitpoints == sum) Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(pFile + $": Expected/Actual (Rnd={pExpectedRounds}/{rounds}, Hit={pExpectedHitpoints}/{sum}, Tot={pExpectedTotal}/{total})");
            }
        }

        static bool Round() {
            SortInitiativesAndCullTheDead();
            var combatEnds = EntitiesTakeActions();
            if (combatEnds) return false;
            return true;
        }

        static bool CombatEndCheck() {
            foreach (var entity in entities) {
                if (entity.Value.Target != null && entity.Value.Target.IsDead) {
                    entity.Value.Target = null;
                }
            }
            return entities.Values.All(e => e.Target == null || e.IsDead);
        }

        static void SortInitiativesAndCullTheDead() {
            var tempEntities = new SortedDictionary<int, Entity>();
            foreach (var kvp in entities) {
                var entity = kvp.Value;
                if (!entity.IsDead) {
                    tempEntities.Add(kvp.Value.Initiative, kvp.Value);
                }
            }
            entities = tempEntities;
        }

        static bool EntitiesTakeActions() {
            var entitiesByInitiative = new SortedSet<int>();
            foreach (var kvp in entities) {
                entitiesByInitiative.Add(kvp.Key);
            }

            foreach (var key in entitiesByInitiative) {
                if (!entities.ContainsKey(key)) continue;

                var entity = entities[key];
                if (entity.HitPoints <= 0) continue; // Dead don't attack, unless zombies!"#¤%½

                SelectTargetIfYouHaveNone(entity);

                if (entity.Target != null) {
                    if (IsInRangeOfAttackingATarget(entity)) {
                        LaunchAttack(entity);
                    } else {
                        MoveTowardsTargetIfAble(entity);
                        if (IsInRangeOfAttackingATarget(entity)) {
                            LaunchAttack(entity);
                        }
                    }
                }

                if (elvesFailed) {
                    return true;
                }
            }

            var allElvesDead = entities.Where(e => e.Value.Race == Race.Elf && !e.Value.IsDead).Count() == 0;
            var allGoblinsDead = entities.Where(e => e.Value.Race == Race.Goblin && !e.Value.IsDead).Count() == 0;

            if (allGoblinsDead) elvesWon = true;
            if (allElvesDead || allGoblinsDead) return true;

            return false;
        }

        static bool SelectTargetIfYouHaveNone(Entity pEntity) {
            //if (pEntity.Target == null) {
            var mapWidth = map.GetLength(0);
            var mapHeight = map.GetLength(1);
            var targetRace = pEntity.Race == Race.Elf ? Race.Goblin : Race.Elf;
            var possibleTargets = entities.Values.Where(e => e.Race == targetRace && e.HitPoints > 0).ToList();
            if (possibleTargets.Count == 0) return false;
            var choices = new List<(Entity, int, int)>();
            foreach (var possibleTarget in possibleTargets) {
                int nearest = 0;
                int score = int.MaxValue;

                foreach (var direction in order) {
                    var target = possibleTarget.Position.Neighbor(direction);

                    if (target.X < 1 || target.X > mapWidth - 2 || target.Y < 1 || target.Y > mapHeight - 2)
                        continue;

                    if (AStarPath(target, pEntity.Position, ref nearest, ref score)) {
                        var targetPoint = nearest.ToPoint(map.GetLength(0));
                        choices.Add((possibleTarget, nearest, score));
                    }
                }
            }

            if (choices.Count > 0) {
                var choice = choices.OrderBy(c => c.Item3).FirstOrDefault();
                pEntity.Target = choice.Item1;
                pEntity.NextMove = choice.Item2;
            }
            //}
            return true;
        }


        static void MoveTowardsTargetIfAble(Entity pEntity) {
            var mapWidth = map.GetLength(0);
            if (!pEntity.NextMove.HasValue) {
                int nearest = 0;
                int score = int.MaxValue;
                if (AStarPath(pEntity.Target.Position, pEntity.Position, ref nearest, ref score)) {
                    pEntity.NextMove = nearest;
                }
            }

            if (pEntity.NextMove.HasValue) {
                entities.Remove(pEntity.Initiative);
                pEntity.Position = pEntity.NextMove.Value.ToPoint(mapWidth);
                pEntity.NextMove = null;
                entities.Add(pEntity.Initiative, pEntity);
            }
        }

        static bool AStarPath(Point pOrigin, Point pTarget, ref int pNearest, ref int pScore) {
            var mapWidth = map.GetLength(0);
            var targetIndex = pTarget.Index(mapWidth);
            var originIndex = pOrigin.Index(mapWidth);
            var scores = new Dictionary<int, int>() { { originIndex, 0 } };
            var cameFrom = new Dictionary<int, int>();
            var open = new Queue<int>();
            var closed = new HashSet<int>();

            open.Enqueue(originIndex);

            while (open.Count > 0) {
                var currentIndex = open.Dequeue();
                var current = currentIndex.ToPoint(mapWidth);

                closed.Add(currentIndex);

                //if (currentIndex != originIndex) {
                // is it the target?
                if (currentIndex == targetIndex) {
                    // reconstruct path and return data we want
                    var path = new List<int>() { currentIndex };
                    while (cameFrom.ContainsKey(currentIndex)) {
                        currentIndex = cameFrom[currentIndex];
                        path.Add(currentIndex);
                    }

                    //if (rounds == 1 && targetIndex == 60 && originIndex == 19) {
                    //    foreach (var p in path) {
                    //        var point = p.ToPoint(mapWidth);
                    //        Console.SetCursorPosition(point.X, point.Y);
                    //        Console.Write("X");
                    //    }
                    //}

                    if (path.Count == 1) {
                        // closest path (same tile)
                        pNearest = path[0];
                        pScore = 0;
                    } else {
                        pNearest = path[1];
                        pScore = path.Count();
                    }
                    return true;
                }

                // is it an entity?
                if (entities.ContainsKey(currentIndex)) {
                    continue;
                }

                // is it a wall?
                if (map[current.X, current.Y]) {
                    continue;
                }
                //}

                foreach (var direction in order) {
                    var neighbor = current.Neighbor(direction);
                    var neighborIndex = neighbor.Index(mapWidth);

                    if (closed.Contains(neighborIndex)) {
                        continue;
                    }

                    var tentativeScore = scores[currentIndex] + DistanceBetween(current, neighbor);

                    var otherEntity = entities.ContainsKey(neighborIndex) && neighborIndex != targetIndex;
                    var wall = map[neighbor.X, neighbor.Y];

                    if (otherEntity || wall) {
                        tentativeScore = int.MaxValue;
                    }

                    if (!open.Contains(neighborIndex) && tentativeScore != int.MaxValue) {
                        open.Enqueue(neighborIndex);
                    } else if (scores.ContainsKey(neighborIndex) && tentativeScore >= scores[neighborIndex]) {
                        continue;
                    }

                    if (!cameFrom.ContainsKey(neighborIndex)) {
                        cameFrom.Add(neighborIndex, 0);
                    }
                    cameFrom[neighborIndex] = currentIndex;

                    if (!scores.ContainsKey(neighborIndex)) {
                        scores.Add(neighborIndex, 0);
                    }
                    scores[neighborIndex] = tentativeScore;
                }
            }

            return false;
        }

        static bool IsInRangeOfAttackingATarget(Entity pEntity) {
            var mapWidth = map.GetLength(0);
            foreach (var direction in order) {
                var targetSquare = pEntity.Position.Neighbor(direction);
                var enemies = entities.Where(e => e.Value.Race != pEntity.Race && e.Key == targetSquare.Index(mapWidth)).ToList();

                if (enemies.Count > 0) {
                    return true;
                }
            }
            return false;
            //return DistanceBetween(pEntity.Position, pEntity.Target.Position) <= 1;
        }

        static int DistanceBetween(Point pFrom, Point pTo) {
            return Math.Abs(pFrom.X - pTo.X) + Math.Abs(pFrom.Y - pTo.Y);
        }

        static void LaunchAttack(Entity pEntity) {
            var mapWidth = map.GetLength(0);

            // if target is close attack it
            //if (pEntity.Target != null) {
            //    pEntity.Target.TakeDamage(pEntity.AttackPower);
            //    return;
            //}

            // else
            var enemies = new List<Entity>();
            foreach (var direction in order) {
                var targetSquare = pEntity.Position.Neighbor(direction);
                enemies.AddRange(entities.Where(e => e.Value.Race != pEntity.Race && e.Value.HitPoints > 0 && e.Key == targetSquare.Index(mapWidth)).Select(k => k.Value).ToList());
            }

            if (enemies.Count > 0) {
                var minHp = enemies.Min(e => e.HitPoints);
                var enemiesWithLowHp = enemies.Where(e => e.HitPoints == minHp).ToList();

                Entity chosenEnemy = null;

                if (enemiesWithLowHp.Count > 1) {
                    chosenEnemy = enemiesWithLowHp.OrderBy(e => e.Initiative).First();
                } else if (enemiesWithLowHp.Count == 1) {
                    chosenEnemy = enemiesWithLowHp[0];
                }

                chosenEnemy.TakeDamage(pEntity.AttackPower);

                if (chosenEnemy.IsDead) {
                    if (chosenEnemy.Race == Race.Elf) {
                        elvesFailed = true;
                    }
                    entities.Remove(chosenEnemy.Initiative);

                    foreach (var kvp in entities) {
                        if (kvp.Value.Target != null && kvp.Value.Target.Initiative == chosenEnemy.Initiative) {
                            kvp.Value.Target = null;
                        }
                    }
                }
            }
        }

        static void DrawBattlefield() {
            Console.Clear();
            Console.SetCursorPosition(0, 0);
            Console.ForegroundColor = ConsoleColor.DarkGray;
            foreach (var line in lines) {
                Console.WriteLine(line);
            }
            var d = new Dictionary<int, int>();
            foreach (var kvp in entities) {
                var entity = kvp.Value;
                Console.SetCursorPosition(entity.Position.X, entity.Position.Y);
                Console.ForegroundColor = entity.Race == Race.Goblin ? ConsoleColor.Red : ConsoleColor.Green;
                Console.Write(entity.Race == Race.Goblin ? "G" : "E");

                if (!d.ContainsKey(entity.Position.Y)) {
                    d.Add(entity.Position.Y, 0);
                }
                d[entity.Position.Y] += 1;
                Console.SetCursorPosition(25 + (d[entity.Position.Y] * 8), entity.Position.Y);
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write((entity.Race == Race.Elf ? "E" : "G") + $"({entity.HitPoints})");
            }
            Console.ForegroundColor = ConsoleColor.White;
            Console.SetCursorPosition(0, lines.Length);
        }
    }
}
