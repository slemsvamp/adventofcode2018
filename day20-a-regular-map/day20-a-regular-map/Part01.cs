using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace day20_a_regular_map {
    class Part01 {
        interface IWalk { }

        struct Path : IWalk {
            public IWalk[] Walkables { get; set; }
        }

        struct Step : IWalk {
            public char Direction { get; set; }
        }

        struct Range {
            public int Start { get; set; }
            public int End { get; set; }
        }

        class Walker {
            public Point Position { get; set; }

            private Walker Parent { get; set; }
            private List<Walker> children;
            private IWalk[] instructions;

            private ConsoleColor color;

            public Walker(Point pPosition, IWalk[] pInstructions) {
                //color = colors[colorProgression];
                //colorProgression++;
                Position = pPosition;
                instructions = pInstructions;
                children = new List<Walker>();
            }

            public void Excavate() {
                foreach (var walk in instructions) {
                    if (walk is Step step) {
                        var direction = step.Direction == 'N' ? Direction.North : step.Direction == 'W' ? Direction.West : step.Direction == 'E' ? Direction.East : Direction.South;
                        // add direction from this room to the next
                        var coordinateString = Position.ToCoordinateString();
                        var room = rooms[coordinateString];
                        if (!room.Contains(direction)) {
                            room.Add(direction);
                            rooms[coordinateString] = room;
                        }
                        // move
                        Position = Position.Neighbor(direction);
                        //Console.Clear();
                        //Draw();
                        //Console.ReadKey(true);
                        // add destination room and path to last room
                        coordinateString = Position.ToCoordinateString();
                        if (!rooms.ContainsKey(coordinateString)) {
                            rooms.Add(coordinateString, new List<Direction>());
                            if (Position.X < minCoordinate.X) minCoordinate.X = Position.X;
                            if (Position.Y < minCoordinate.Y) minCoordinate.Y = Position.Y;
                            if (Position.X > maxCoordinate.X) maxCoordinate.X = Position.X;
                            if (Position.Y > maxCoordinate.Y) maxCoordinate.Y = Position.Y;
                        }
                        room = rooms[coordinateString];
                        var oppositeDirection = Opposite(direction);
                        if (!room.Contains(oppositeDirection)) {
                            room.Add(oppositeDirection);
                            rooms[coordinateString] = room;
                        }
                    }
                    if (walk is Path path) {
                        var child = new Walker(Position, path.Walkables);
                        child.Parent = this;
                        children.Add(child);
                    }
                }

                foreach (var child in children) {
                    child.Excavate();
                }
            }

            public void Draw() {
                Console.SetCursorPosition(Position.X + 3, Position.Y + 4);
                Console.ForegroundColor = color;
                Console.Write("☺");

                foreach (var child in children) {
                    child.Draw();
                }
            }
        }

        static int colorProgression;
        static ConsoleColor[] colors = new ConsoleColor[] { ConsoleColor.Green, ConsoleColor.Yellow, ConsoleColor.Blue, ConsoleColor.Magenta, ConsoleColor.Cyan, ConsoleColor.White, ConsoleColor.Red, ConsoleColor.Gray, ConsoleColor.DarkGreen, ConsoleColor.DarkYellow, ConsoleColor.Green, ConsoleColor.Yellow, ConsoleColor.Blue, ConsoleColor.Magenta, ConsoleColor.Cyan, ConsoleColor.White, ConsoleColor.Red, ConsoleColor.Gray, ConsoleColor.DarkGreen, ConsoleColor.DarkYellow };

        static Direction Opposite(Direction pDirection) {
            if (pDirection == Direction.North) return Direction.South;
            else if (pDirection == Direction.South) return Direction.North;
            else if (pDirection == Direction.West) return Direction.East;
            return Direction.West;
        }

        static Dictionary<string, List<Direction>> rooms;
        static Point minCoordinate;
        static Point maxCoordinate;
        static int[,] map;

        public static void Run() {
            Console.CursorVisible = false;
            var input = File.ReadAllText("input.txt");
            input = input.Substring(1, input.Length - 2);
            rooms = new Dictionary<string, List<Direction>>();
            map = new int[0, 0];
            minCoordinate = new Point(int.MaxValue, int.MaxValue);
            maxCoordinate = new Point(int.MinValue, int.MinValue);
            var path = Parse(input);
            Excavate(path);

            // 39, 91 - 2413 (too low)
            // 2430 (too low)
        }

        static void Excavate(Path pPath) {
            // go through the nested path trees
            var walker = new Walker(Point.Empty, pPath.Walkables);
            
            // start coordinate
            rooms.Add(Point.Empty.ToCoordinateString(), new List<Direction>());

            walker.Excavate();

            int width = maxCoordinate.X - minCoordinate.X + 1;
            int height = maxCoordinate.Y - minCoordinate.Y + 1;

            map = new int[width, height];

            foreach (var room in rooms) {
                var parts = room.Key.Split(new string[] { "," }, StringSplitOptions.None);
                var roomPoint = new Point(int.Parse(parts[0]) - minCoordinate.X, int.Parse(parts[1]) - minCoordinate.Y);
                int value = 0;
                foreach (var d in room.Value) {
                    value |= (int)d;
                }

                map[roomPoint.X, roomPoint.Y] = value;
            }

            var start = new Point(-minCoordinate.X, -minCoordinate.Y);


            var path = Explore(start);

            //SaveMap(path);
            //DrawMap();
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine((path.Count - 1) + ". Done!");
            Console.ReadKey(true);
        }

        static List<int> Explore(Point pStart) {
            int mapWidth = map.GetLength(0);
            var open = new Queue<int>();
            var closed = new HashSet<int>();
            var deadEnds = new HashSet<int>();

            open.Enqueue(pStart.Index(mapWidth));

            do {
                var roomIndex = open.Dequeue();
                var roomPoint = roomIndex.ToPoint(mapWidth);

                closed.Add(roomIndex);

                var directions = map[roomPoint.X, roomPoint.Y];

                bool deadEnd = true;
                foreach (var direction in Enum.GetValues(typeof(Direction)).Cast<Direction>()) {
                    if ((directions & (int)direction) == 0) continue;
                    var nb = roomPoint.Neighbor(direction);
                    var nbIndex = nb.Index(mapWidth);
                    if (closed.Contains(nbIndex)) continue;
                    open.Enqueue(nbIndex);
                    deadEnd = false;
                }
                if (deadEnd) {
                    deadEnds.Add(roomIndex);
                }
            } while (open.Count > 0);

            int roomsComplete = 0;
            var deadEndDistance = new Dictionary<int, List<int>>();
            deadEnds.Add(8);
            foreach (var deadEnd in deadEnds) {
                deadEndDistance.Add(deadEnd, AStarPath(pStart, deadEnd.ToPoint(mapWidth)));
                roomsComplete++;
                Console.SetCursorPosition(0, 0);
                Console.Write(roomsComplete + " out of " + deadEnds.Count);
            }

            //for (int x = 0; x < map.GetLength(0); x++) {
            //    for (int y = 0; y < map.GetLength(1); y++) {
            //        var point = new Point(x, y);
            //        deadEndDistance.Add(point.Index(map.GetLength(0)), AStarPath(pStart, point));
            //    }
            //}

            return deadEndDistance.OrderByDescending(de => de.Value.Count).First().Value;
        }

        static List<int> AStarPath(Point pOrigin, Point pTarget) {
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

                    return path;
                }

                var directions = map[current.X, current.Y];

                foreach (var direction in Enum.GetValues(typeof(Direction)).Cast<Direction>()) {
                    if ((directions & (int)direction) == 0) continue;
                    var nb = current.Neighbor(direction);
                    var nbIndex = nb.Index(mapWidth);
                    if (closed.Contains(nbIndex)) continue;

                    var tentativeScore = scores[currentIndex] + DistanceBetween(current, nb);

                    if (!open.Contains(nbIndex)) {
                        open.Enqueue(nbIndex);
                    } else if (scores.ContainsKey(nbIndex) && tentativeScore >= scores[nbIndex]) {
                        continue;
                    }

                    if (!cameFrom.ContainsKey(nbIndex)) {
                        cameFrom.Add(nbIndex, 0);
                    }
                    cameFrom[nbIndex] = currentIndex;

                    if (!scores.ContainsKey(nbIndex)) {
                        scores.Add(nbIndex, 0);
                    }
                    scores[nbIndex] = tentativeScore;
                }
            }

            return new List<int>();
        }

        static int DistanceBetween(Point pFrom, Point pTo) {
            return Math.Abs(pFrom.X - pTo.X) + Math.Abs(pFrom.Y - pTo.Y);
        }

        /// <summary>
        /// Mostly for fun!
        /// </summary>
        static void SaveMap(List<int> pPath = null) {
            var writeToCoordinate = new Action<char, Point, string[]>((c, p, l) => {
                var point = new Point(1 + p.X * 2, 1 + p.Y * 2);
                l[point.Y] = l[point.Y].Substring(0, point.X) + c + l[point.Y].Substring(point.X + 1, l[point.Y].Length - point.X - 1);
            });

            var lines = new string[map.GetLength(1) * 2 + 1];
            for (int y = 0; y <= map.GetLength(1) * 2; y++) {
                lines[y] = new string('#', map.GetLength(0) * 2 + 1);
            }

            for (int x = 0; x < map.GetLength(0); x++) {
                for (int y = 0; y < map.GetLength(1); y++) {
                    var point = new Point(1 + x * 2, 1 + y * 2);
                    var drawCharacter = '.';
                    if (x == -minCoordinate.X && y == -minCoordinate.Y) {
                        drawCharacter = '☺';
                    }
                    writeToCoordinate(drawCharacter, new Point(x, y), lines);

                    var value = map[x, y];
                    foreach (var direction in Enum.GetValues(typeof(Direction)).Cast<Direction>()) {
                        if ((value & (int)direction) > 0) {
                            var np = point.Neighbor(direction);
                            char door = direction == Direction.North || direction == Direction.South ? '-' : '|';
                            lines[np.Y] = lines[np.Y].Substring(0, np.X) + door + lines[np.Y].Substring(np.X + 1, lines[np.Y].Length - np.X - 1);
                        }
                    }
                }
            }

            if (pPath != null) {
                for (int p = 1; p < pPath.Count - 1; p++) {
                    var coordinate = pPath[p].ToPoint(map.GetLength(0));
                    writeToCoordinate('X', coordinate, lines);
                }
            }

            writeToCoordinate('?', pPath[0].ToPoint(map.GetLength(0)), lines);

            using (var sw = new StreamWriter("map.txt")) {
                for (int l = 0; l < lines.Length; l++) {
                    sw.Write(lines[l]);
                    if (l != lines.Length - 1) sw.WriteLine();
                }
            }
        }

        /// <summary>
        /// Mostly for fun!
        /// </summary>
        static void DrawMap() {
            for (int y = 0; y <= map.GetLength(1) * 2; y++) {
                Console.SetCursorPosition(0, y);
                Console.Write(new string('#', map.GetLength(0) * 2 + 1));
            }

            for (int x = 0; x < map.GetLength(0); x++) {
                for (int y = 0; y < map.GetLength(1); y++) {
                    var drawCharacter = '.';
                    if (x == -minCoordinate.X && y == -minCoordinate.Y) {
                        drawCharacter = '☺';
                    }
                    var point = new Point(1 + x * 2, 1 + y * 2);
                    Console.SetCursorPosition(point.X, point.Y);
                    Console.Write(drawCharacter);
                    var value = map[x, y];
                    if ((value & (int)Direction.North) > 0) { var np = point.Neighbor(Direction.North); Console.SetCursorPosition(np.X, np.Y); Console.Write("-"); }
                    if ((value & (int)Direction.South) > 0) { var np = point.Neighbor(Direction.South); Console.SetCursorPosition(np.X, np.Y); Console.Write("-"); }
                    if ((value & (int)Direction.West) > 0) { var np = point.Neighbor(Direction.West); Console.SetCursorPosition(np.X, np.Y); Console.Write("|"); }
                    if ((value & (int)Direction.East) > 0) { var np = point.Neighbor(Direction.East); Console.SetCursorPosition(np.X, np.Y); Console.Write("|"); }
                }
            }
        }

        static bool IsDirection(char pStep) {
            return pStep == 'N' || pStep == 'W' || pStep == 'S' || pStep == 'E';
        }

        static Range[] Paths(string pPath) {
            var ranges = new List<Range>();

            var current = new Range { Start = 0 };
            int openPaths = 0;
            for (int i = 0; i < pPath.Length; i++) {
                if (openPaths == 0 && pPath[i] == '|') {
                    current.End = i;
                    ranges.Add(current);
                    if (i + 1 < pPath.Length) {
                        current = new Range { Start = i + 1 };
                    } else {
                        return ranges.ToArray();
                    }
                } else if (pPath[i] == '(') {
                    openPaths++;
                } else if (pPath[i] == ')') {
                    openPaths--;
                }
            }

            current.End = pPath.Length;
            ranges.Add(current);

            return ranges.ToArray();
        }

        static Path Parse(string pPath) {
            return new Path { Walkables = RecursiveParse(pPath) };
        }

        static int progress;

        static IWalk[] RecursiveParse(string pPath) {
            var p = pPath;
            var walkables = new Queue<IWalk>();

            //Console.SetCursorPosition(0, 0);
            //progress++;
            //Console.Write(progress);

            // Split Paths on Pipe
            var paths = Paths(pPath);

            if (paths.Length > 1) {
                // -- Add Directions / Paths
                foreach (var path in paths) {
                    var subpath = pPath.Substring(path.Start, path.End - path.Start);
                    walkables.Enqueue(new Path { Walkables = RecursiveParse(subpath) });
                }
            } else {
                // -- -- On Paranthesis - Recurse
                for (int i = 0; i < p.Length; i++) {
                    if (IsDirection(p[i])) {
                        walkables.Enqueue(new Step { Direction = p[i] });
                    } else {
                        // paranthesis
                        if (p[i] == '(') {
                            // start path
                            int openPaths = 0, subPathIndex = -1;
                            for (int s = i; s < p.Length; s++) {
                                if (p[s] == '(') openPaths++;
                                else if (p[s] == ')') openPaths--;
                                subPathIndex = s;
                                if (openPaths == 0) break;
                            }

                            var sub = p.Substring(i + 1, subPathIndex - i - 1);
                            var subPath = new Path { Walkables = RecursiveParse(sub) };
                            walkables.Enqueue(subPath);
                            i += sub.Length;
                            // move index
                        }
                    }
                }
            }

            return walkables.ToArray();
        }
    }
}
