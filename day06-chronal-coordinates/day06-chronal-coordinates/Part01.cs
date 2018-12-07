using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;

namespace day06_chronal_coordinates {
    public class Part01 {
        #region Enums
        public enum Direction {
            North, South, East, West
        }
        #endregion

        #region Classes
        public class Board {
            public Area[,] Areas { get; set; }

            public Board() {
                Areas = new Area[BoardRectangle.Width, BoardRectangle.Height];

                for (int x = 0; x < BoardRectangle.Width; x++) {
                    for (int y = 0; y < BoardRectangle.Height; y++) {
                        Areas[x, y] = Area.Empty;
                    }
                }
            }

            public Area GetArea(Point pPoint) {
                return Areas[pPoint.X, pPoint.Y];
            }

            public bool Claim(int pVirusId, Point pPoint) {
                var area = GetArea(pPoint);

                if (area.State == Area.AreaState.Open) {
                    area.State = Area.AreaState.Claimed;
                    area.ClaimedBy = pVirusId;
                } else if (area.State == Area.AreaState.Claimed) {
                    area.State = Area.AreaState.Equidistant;
                    area.ClaimedBy = null; // no one owns this
                } else {
                    return false;
                }

                return true;
            }

            public bool Origin(int pVirusId, Point pPoint) {
                var area = GetArea(pPoint);

                if (area.State == Area.AreaState.Open) {
                    area = new Area {
                        ClaimedBy = pVirusId,
                        State = Area.AreaState.Start
                    };
                    return true;
                }

                return false;
            }
        }

        public class Virus {
            public int Id { get; set; }
            public Point Origin { get; set; }
            public Stack<Point> Open { get; set; }
            public bool IsInfinite { get; set; }
            public Direction[] SpreadDirections { get; set; }

            public Virus() {
                Open = new Stack<Point>();
            }

            public void SetOrigin(Board pBoard, Point pOrigin) {
                Origin = pOrigin;
                pBoard.Origin(Id, pOrigin);

                //var pointsToCheck = Neighbours(Origin);

                //foreach (var pointToCheck in pointsToCheck) {
                //    if (pointToCheck.X < BoardRectangle.Left || pointToCheck.X >= BoardRectangle.Width || pointToCheck.Y < BoardRectangle.Top || pointToCheck.Y >= BoardRectangle.Height) {
                //        IsInfinite = true;
                //    } else {
                //        var area = pBoard[pointToCheck.X, pointToCheck.Y];
                //        if (area.State == Area.AreaState.Open) {
                //            Open.Push(new Point(pointToCheck.X, pointToCheck.Y));
                //            area.ClaimedBy = Id;
                //        }
                //    }
                //}
            }

            public bool Claim(Board pBoard) {
                return false;

                //bool boardChanged = false;

                //var nextOpenList = new List<Point>();

                //while (Open.Count > 0) {
                //    var point = Open.Pop();

                //    if (point.X < BoardRectangle.Left || point.X >= BoardRectangle.Width || point.Y < BoardRectangle.Top || point.Y >= BoardRectangle.Height) {
                //        IsInfinite = true;
                //    } else {
                //        var area = pBoard[point.X, point.Y];
                //        if (area.State == Area.AreaState.Open) {
                //            area.ClaimedBy = Id;
                //            area.State = Area.AreaState.Claimed;
                //            boardChanged = true;
                //        } else if (area.State == Area.AreaState.Claimed && area.ClaimedBy.Value != Id) {
                //            area.State = Area.AreaState.Equidistant;
                //            area.ClaimedBy = null;
                //            boardChanged = true;
                //        }
                //    }

                //    var pointsToCheckNext = Neighbours(point);

                //    foreach (var pointToCheckNext in pointsToCheckNext) {
                //        bool outside = pointToCheckNext.X < BoardRectangle.Left || pointToCheckNext.X >= BoardRectangle.Width || pointToCheckNext.Y < BoardRectangle.Top || pointToCheckNext.Y >= BoardRectangle.Height;
                //        if (!outside && pBoard[pointToCheckNext.X, pointToCheckNext.Y].State == Area.AreaState.Open) {
                //            nextOpenList.Add(pointToCheckNext);
                //        }
                //    }

                //    Closed.Add(point);
                //}

                //foreach (var pointToCheckNext in nextOpenList) {
                //    Open.Push(pointToCheckNext);
                //}

                //return boardChanged;
            }
        }

        public class Area {
            public enum AreaState {
                Open, Equidistant, Claimed, Owned, Start
            }

            public int? ClaimedBy { get; set; }
            public AreaState State { get; set; }

            public static Area Empty {
                get {
                    return new Area {
                        ClaimedBy = null,
                        State = AreaState.Open
                    };
                }
            }
        }
        #endregion

        public static Point[] Neighbours(Point pPoint) {
            var points = new Point[] {
                new Point(pPoint.X, pPoint.Y - 1),
                new Point(pPoint.X + 1, pPoint.Y),
                new Point(pPoint.X, pPoint.Y + 1),
                new Point(pPoint.X - 1, pPoint.Y)
            };

            List<Point> result = new List<Point>();

            foreach (var point in points) {
                if (point.X < 0 || point.Y < 0 || point.X >= BoardRectangle.Width || point.Y >= BoardRectangle.Height)
                    continue;
                result.Add(point);
            }

            return result.ToArray();
        }

        public static Rectangle BoardRectangle;
        public static Dictionary<int, List<Point>> ownedAreas;
        public static int OwnedAreasCount = 0;

        public static void Run() {
            BoardRectangle = new Rectangle(0, 0, 400, 400);

            Console.WindowWidth = 85;
            Console.WindowHeight = 50;
            Console.BufferWidth = 85;
            Console.BufferHeight = 50;

            ownedAreas = new Dictionary<int, List<Point>>();
            var lines = File.ReadLines("input.txt");
            var viruses = new Dictionary<int, Virus>();
            int virusId = 1;

            Board board = new Board();

            foreach (var line in lines) {
                var parts = line.Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
                var point = new Point(int.Parse(parts[0]), int.Parse(parts[1]));

                var virus = new Virus {
                    Id = virusId++
                };

                viruses.Add(virus.Id, virus);
                virus.SetOrigin(board, point);
                virus.Open.Push(point);
            }

            bool boardChanged = true;

            int maxChanges = 100000;
            int changes = 0;

            do {
                bool claimedRound = ClaimRound(viruses, board);
                bool administeredOwnership = AdministerOwnership(board);
                boardChanged = claimedRound || administeredOwnership;
                changes++;

                //Console.Clear();

                //int visualizeWidth = 80;
                //int visualizeHeight = 50;

                //int fromTop = 171;
                //int fromLeft = 222;

                //for (int x = 0 + fromLeft; x < visualizeWidth + fromLeft; x++) {
                //    for (int y = 0 + fromTop; y < visualizeHeight + fromTop; y++) {
                //        Console.SetCursorPosition(x - fromLeft, y - fromTop);
                //        var area = board.GetArea(new Point(x, y));

                //        int n = 0;
                //        if (n < 4 && area.State == Area.AreaState.Start) {
                //            n = 4;
                //        } else if (n < 3 && area.State == Area.AreaState.Owned) {
                //            n = 3;
                //        } else if (n < 2 && area.State == Area.AreaState.Claimed) {
                //            n = 2;
                //        } else if (n < 1 && area.State == Area.AreaState.Equidistant) {
                //            n = 1;
                //        }

                //        Console.Write(n == 0 ? " " : n == 1 ? "." : n == 2 ? "C" : n == 3 ? "o" : "S");
                //    }
                //}


                //Console.SetCursorPosition(0, 0);
                //Console.WriteLine("OwnedAreasCount: " + OwnedAreasCount);

                //var virus = viruses[1];
                //Console.WriteLine("");
                //Console.WriteLine("Virus 1 - IsInfinite: " + virus.IsInfinite.ToString());
                //Console.WriteLine("Virus 1 - Open #: " + virus.Open.Count);
                //Console.WriteLine("Virus 1 - Owned: " + ownedAreas[1].Count);

                //Console.ReadKey(true);
            } while (boardChanged && changes < maxChanges);

            var maxAreaCountObject = ownedAreas.Join(viruses, x => x.Key, x => x.Key, (oa, v) => new { Virus = v.Value, IsInfinite = v.Value.IsInfinite, AreasCount = oa.Value.Count })
                .Where(x => x.IsInfinite == false)
                .OrderByDescending(x => x.AreasCount).First();

            Console.WriteLine("Max Areas Count: " + maxAreaCountObject.AreasCount);

            if (changes == maxChanges) {
                Console.WriteLine("Max Changes met.");
            }
        }

        public static bool ClaimRound(Dictionary<int, Virus> pViruses, Board pBoard) {
            bool boardChanged = false;

            foreach (var virus in pViruses.Values) {
                var pointsToProcess = new List<Point>();
                while (virus.Open.Count > 0) {
                    pointsToProcess.Add(virus.Open.Pop());
                }

                foreach (var point in pointsToProcess) {
                    var area = pBoard.GetArea(point);
                    if (area.State == Area.AreaState.Open) {
                        if (pBoard.Claim(virus.Id, point)) {
                            boardChanged = true;
                        }
                    }

                    var neighbours = Neighbours(point);

                    if (neighbours.Length != 4) {
                        virus.IsInfinite = true;
                    }

                    foreach (var neighbourPoint in neighbours) {
                        var neighbourArea = pBoard.GetArea(neighbourPoint);
                        if (neighbourArea.State == Area.AreaState.Open) {
                            if (!virus.Open.Contains(neighbourPoint)) {
                                virus.Open.Push(neighbourPoint);
                            }
                        }
                    }
                }
            }

            return boardChanged;
        }

        public static bool AdministerOwnership(Board pBoard) {
            bool boardChanged = false;

            for (int x = 0; x < BoardRectangle.Width; x++) {
                for (int y = 0; y < BoardRectangle.Height; y++) {
                    var area = pBoard.GetArea(new Point(x, y));

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
