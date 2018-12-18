using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;

namespace day18_settlers_of_the_north_pole {
    class Part02 {
        static int[] map;
        static int mapWidth;
        public static void Run() {
            mapWidth = 50;
            var input = File.ReadAllText("input.txt");
            //mapWidth = 10;
            //var input = File.ReadAllText("mockInput.txt");

            Console.SetWindowSize(60, 60);
            Console.SetBufferSize(60, 60);

            input = input.Replace(Environment.NewLine, "");
            map = new int[mapWidth * mapWidth];

            for (int i = 0; i < mapWidth * mapWidth; i++) {
                var c = input[i]; map[i] = c == '.' ? 0 : c == '|' ? 1 : 2;
            }

            var recordedMap = new int[mapWidth * mapWidth];
            int moreTurns = -1;
            for (long g = 0; g < 1000000000; g++) {
                if (moreTurns >= 0) {
                    moreTurns--;
                    if (moreTurns <= 0) break;
                }
                //DrawMap();
                //Console.ReadKey(true);

                if (g == 1000) {
                    for (int i = 0; i < mapWidth * mapWidth; i++) {
                        recordedMap[i] = map[i];
                    }
                }
                if (g > 1000) {
                    int e = 0;
                    for (int i = 0; i < mapWidth * mapWidth; i++) {
                        if (recordedMap[i] == map[i]) {
                            e++;
                        }
                    }
                    if (e == mapWidth * mapWidth) {
                        moreTurns = (int)((1000000000 - g) % (g - 1000));
                        if (moreTurns == 0) {
                            break;
                        }
                    }
                }
                var nextMap = new int[mapWidth * mapWidth];
                for (int i = 0; i < mapWidth * mapWidth; i++) {
                    int openAcres = 0, trees = 0, lumberyards = 0;
                    var pt = ToPoint(i);
                    for (int n = 0; n < 8; n++) {
                        var np = Neighbor(n, pt);
                        var v = 0;
                        if (TryGet(np, out v)) {
                            if (v == 0) openAcres++;
                            if (v == 1) trees++;
                            if (v == 2) lumberyards++;
                        }
                    }
                    nextMap[i] = map[i];
                    if (map[i] == 0 && trees >= 3) {
                        nextMap[i] = 1;
                    }
                    if (map[i] == 1 && lumberyards >= 3) {
                        nextMap[i] = 2;
                    }
                    if (map[i] == 2) {
                        if (lumberyards >= 1 && trees >= 1) {
                            nextMap[i] = 2;
                        } else {
                            nextMap[i] = 0;
                        }
                    }
                }

                map = nextMap;
            }

            //DrawMap();

            int sumLumberyards = 0, sumTrees = 0;
            for (int i = 0; i < mapWidth * mapWidth; i++) {
                var v = map[i];
                if (v == 1) sumTrees++;
                if (v == 2) sumLumberyards++;
            }
            Console.WriteLine("Part02: " + (sumLumberyards * sumTrees));
        }

        static void DrawMap() {
            Console.Clear();
            for (int i = 0; i < mapWidth * mapWidth; i++) {
                var pt = ToPoint(i);
                Console.SetCursorPosition(pt.X, pt.Y);
                Console.Write(map[i] == 0 ? '.' : map[i] == 1 ? '|' : '#');
            }
        }

        static Point ToPoint(int pIndex) {
            return new Point(pIndex % mapWidth, pIndex / mapWidth);
        }

        static Point Neighbor(int pIndex, Point pPoint) {
            switch (pIndex) {
                case 0: return new Point(pPoint.X - 1, pPoint.Y - 1);
                case 1: return new Point(pPoint.X, pPoint.Y - 1);
                case 2: return new Point(pPoint.X + 1, pPoint.Y - 1);
                case 3: return new Point(pPoint.X - 1, pPoint.Y);
                case 4: return new Point(pPoint.X + 1, pPoint.Y);
                case 5: return new Point(pPoint.X - 1, pPoint.Y + 1);
                case 6: return new Point(pPoint.X, pPoint.Y + 1);
                case 7: return new Point(pPoint.X + 1, pPoint.Y + 1);
            }
            throw new Exception("Index must be 0-8");
        }

        static int Get(Point pPoint) {
            return map[pPoint.Y * mapWidth + pPoint.X];
        }

        static bool TryGet(Point pPoint, out int pValue) {
            if (pPoint.X < 0 || pPoint.Y < 0 || pPoint.X >= mapWidth || pPoint.Y >= mapWidth) {
                pValue = 0;
                return false;
            }
            pValue = map[pPoint.Y * mapWidth + pPoint.X];
            return true;
        }
    }
}
