using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace day06_chronal_coordinates {
    public class Part01 {
        public static Rectangle BoardRectangle => new Rectangle(0, 0, 400, 400);

        public enum Direction {
            North, South, East, West
        }

        public static Dictionary<int, List<Point>> ownedAreas;

        public class Virus {
            public int Id { get; set; }
            public Point Origin { get; set; }
            public Stack<Point> Open { get; set; }
            public HashSet<Point> Closed { get; set; }
            public bool IsInfinite { get; set; }

            public Virus() {
                Open = new Stack<Point>();
                Closed = new HashSet<Point>();
            }

            public void SetOrigin(Area[,] pBoard, Point pOrigin) {
                Origin = pOrigin;

                pBoard[Origin.X, Origin.Y] = new Area {
                    ClaimedBy = Id,
                    State = Area.AreaState.Start
                };

                var pointsToCheck = Neighbours(Origin);

                foreach (var pointToCheck in pointsToCheck) {
                    if (pointToCheck.X < BoardRectangle.Left || pointToCheck.X >= BoardRectangle.Width || pointToCheck.Y < BoardRectangle.Top || pointToCheck.Y >= BoardRectangle.Height) {
                        IsInfinite = true;
                    } else {
                        var area = pBoard[pointToCheck.X, pointToCheck.Y];
                        if (area.State == Area.AreaState.Open) {
                            Open.Push(new Point(pointToCheck.X, pointToCheck.Y));
                            area.ClaimedBy = Id;
                        }
                    }
                }
            }

            public bool Claim(Area[,] pBoard) {
                bool boardChanged = false;

                var nextOpenList = new List<Point>();

                while (Open.Count > 0) {
                    var point = Open.Pop();

                    if (point.X < BoardRectangle.Left || point.X >= BoardRectangle.Width || point.Y < BoardRectangle.Top || point.Y >= BoardRectangle.Height) {
                        IsInfinite = true;
                    } else {
                        var area = pBoard[point.X, point.Y];
                        if (area.State == Area.AreaState.Open) {
                            area.ClaimedBy = Id;
                            area.State = Area.AreaState.Claimed;
                            boardChanged = true;
                        } else if (area.State == Area.AreaState.Claimed && area.ClaimedBy.Value != Id) {
                            area.State = Area.AreaState.Equidistant;
                            area.ClaimedBy = null;
                            boardChanged = true;
                        }
                    }

                    var pointsToCheckNext = Neighbours(point);

                    foreach (var pointToCheckNext in pointsToCheckNext) {
                        if (pBoard[pointToCheckNext.X, pointToCheckNext.Y].State == Area.AreaState.Open) {
                            nextOpenList.Add(pointToCheckNext);
                        }
                    }

                    Closed.Add(point);
                }

                foreach (var pointToCheckNext in nextOpenList) {
                    Open.Push(pointToCheckNext);
                }

                return boardChanged;
            }
        }

        public class Area {
            public enum AreaState {
                Open, Equidistant, Claimed, Owned, Start
            }

            public int? ClaimedBy { get; set; }
            public AreaState State { get; set; }
        }

        public static Point[] Neighbours(Point pPoint) {
            var pointNorth = new Point(pPoint.X, pPoint.Y - 1);
            var pointEast = new Point(pPoint.X + 1, pPoint.Y);
            var pointSouth = new Point(pPoint.X, pPoint.Y + 1);
            var pointWest = new Point(pPoint.X - 1, pPoint.Y);

            return new Point[] {
                pointNorth, pointEast, pointSouth, pointWest
            };
        }

        public static void Run() {
            Console.WindowHeight = 50;
            Console.WindowWidth = 50;
            Console.BufferHeight = 50;
            Console.BufferWidth = 50;

            ownedAreas = new Dictionary<int, List<Point>>();
            var lines = File.ReadLines("input.txt");
            var viruses = new Dictionary<int, Virus>();
            var board = new Area[400, 400];
            int virusId = 1;

            for (int x = 0; x < 400; x++) {
                for (int y = 0; y < 400; y++) {
                    board[x, y] = new Area { ClaimedBy = null, State = Area.AreaState.Open };
                }
            }

            foreach (var line in lines) {
                var parts = line.Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
                var point = new Point(int.Parse(parts[0]), int.Parse(parts[1]));

                var virus = new Virus {
                    Id = virusId++
                };

                virus.SetOrigin(board, point);

                viruses.Add(virus.Id, virus);

                board[point.X, point.Y] = new Area {
                    ClaimedBy = virus.Id,
                    State = Area.AreaState.Claimed
                };
            }

            bool boardChanged = true;

            int maxChanges = 100000;
            int changes = 0;

            do {
                bool claimedRound = ClaimRound(viruses, board);
                bool administeredOwnership = AdministerOwnership(board);
                boardChanged = claimedRound || administeredOwnership;
                changes++;

                for (int x = 0; x < 400; x += 10) {
                    for (int y = 0; y < 400; y += 10) {
                        Console.SetCursorPosition(x / 10, y / 10);

                        int n = 0;
                        for (int sx = 0; sx < 10; sx++) {
                            for (int sy = 0; sy < 10; sy++) {
                                if (n < 4 && board[x+sx, y+sy].State == Area.AreaState.Start) {
                                    n = 4;
                                }
                                if (n < 3 && board[x + sx, y + sy].State == Area.AreaState.Owned) {
                                    n = 3;
                                }
                                if (n < 2 && board[x + sx, y + sy].State == Area.AreaState.Claimed) {
                                    n = 2;
                                }
                                if (n < 1 && board[x + sx, y + sy].State == Area.AreaState.Equidistant) {
                                    n = 1;
                                }
                            }

                            Console.Write(n == 0 ? " " : n == 1 ? "." : n == 2 ? "C" : n == 3 ? "O" : "S");
                        }
                    }
                }

                if (OwnedAreasCount > 0 && OwnedAreasCount % 100 == 0) {
                    Console.WriteLine("OwnedAreas: " + ownedAreas.Count.ToString());
                }
            } while (boardChanged && changes < maxChanges);

            if (changes == maxChanges) {
                Console.WriteLine("Max Changes met.");
            }
        }

        public static bool ClaimRound(Dictionary<int, Virus> pViruses, Area[,] pBoard) {
            bool boardChanged = false;

            foreach (var virus in pViruses.Values) {
                if (virus.Claim(pBoard)) {
                    boardChanged = true;
                }
            }

            return boardChanged;
        }

        public static int OwnedAreasCount = 0;

        public static bool AdministerOwnership(Area[,] pBoard) {
            bool boardChanged = false;

            for (int x = 0; x < 400; x++) {
                for (int y = 0; y < 400; y++) {
                    var area = pBoard[x, y];
                    if (area.State == Area.AreaState.Claimed) {
                        area.State = Area.AreaState.Owned;
                        if (!ownedAreas.ContainsKey(area.ClaimedBy.Value)) {
                            ownedAreas.Add(area.ClaimedBy.Value, new List<Point>());
                        }
                        ownedAreas[area.ClaimedBy.Value].Add(new Point(x, y));
                        OwnedAreasCount++;
                        boardChanged = true;
                    }
                }
            }

            return boardChanged;
        }
    }
}
