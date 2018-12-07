using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;

namespace day06_chronal_coordinates {
    public class Part02 {
        public class Board {
            public Point Start { get; set; }
            public Size Size { get; set; }
            public Point[] Coordinates { get; set; }

            public Board(Point[] pCoordinates) {
                Coordinates = pCoordinates;
            }
        }

        public static void Run() {
            var lines = File.ReadLines("input.txt");

            var points = new List<Point>();

            var minPoint = new Point(int.MaxValue, int.MaxValue);
            var maxPoint = new Point(0, 0);

            foreach (var line in lines) {
                var parts = line.Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
                var point = new Point(int.Parse(parts[0]), int.Parse(parts[1]));
                points.Add(point);
                if (point.X < minPoint.X) minPoint.X = point.X;
                if (point.Y < minPoint.Y) minPoint.Y = point.Y;
                if (point.X > maxPoint.X) maxPoint.X = point.X;
                if (point.Y > maxPoint.Y) maxPoint.Y = point.Y;
            }

            var board = new Board(points.ToArray()) {
                Start = minPoint,
                Size = new Size(maxPoint.X - minPoint.X + 1, maxPoint.Y - minPoint.Y + 1)
            };

            Dictionary<Point, int> distances = new Dictionary<Point, int>();

            for (int x = board.Start.X; x < board.Start.X + board.Size.Width; x++) {
                for (int y = board.Start.Y; y < board.Start.Y + board.Size.Height; y++) {
                    var point = new Point(x, y);
                    int totalDistance = 0;
                    bool notInterested = false;

                    foreach (var coordinate in board.Coordinates) {
                        int distance = Distance(point, coordinate);

                        if (distance + totalDistance >= 10000) {
                            notInterested = true;
                            break;
                        }

                        totalDistance += distance;
                    }

                    if (notInterested == false) {
                        distances.Add(point, totalDistance);
                    }
                }
            }

            Console.WriteLine("Distances With Less Than 10000: " + distances.Count);
        }

        public static int Distance(Point pPointA, Point pPointB) {
            return Math.Abs(pPointA.X - pPointB.X) + Math.Abs(pPointA.Y - pPointB.Y);
        }
    }
}