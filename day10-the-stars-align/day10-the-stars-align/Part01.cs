using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace day10_the_stars_align {
    class Part01 {
        class Star {
            public Point Position { get; set; }
            public Point Velocity { get; set; }
        }

        class Camera {
            public Point Position { get; set; }
            public Rectangle Window { get; set; }
        }

        static Regex starParseRegex = new Regex(@"position=<\s*(?<px>-?\d+),\s*(?<py>-?\d+)> velocity=<\s*(?<vx>-?\d+),\s*(?<vy>-?\d+)>");

        static int secondsPassed = 0;
        static List<Star> stars;
        static List<Star> starsBuffer;

        static Camera camera;
        static Rectangle world;

        public static void Run() {
            Console.SetWindowSize(80, 60);
            Console.SetBufferSize(80, 60);
            Console.CursorVisible = false;

            var lines = File.ReadLines("input.txt");
            camera = new Camera();

            stars = new List<Star>();
            starsBuffer = new List<Star>();

            foreach (var line in lines) {
                var star = Parse(line);
                stars.Add(star);
                starsBuffer.Add(star);
            }

            int minX = int.MaxValue, minY = int.MaxValue, maxX = 0, maxY = 0;

            camera.Position = new Point(minX, minY);
            camera.Window = new Rectangle(0, 0, 80, 30);

            Action<Star> starProcessAction = new Action<Star>(star => {
                star.Position = new Point(star.Position.X + star.Velocity.X, star.Position.Y + star.Velocity.Y);
            });

            long area = long.MaxValue;

            while (true) {
                long lastArea = area;
                starsBuffer = ProcessStars(starProcessAction);
                area = (long)world.Width * world.Height;
                if (lastArea < area) {
                    break;
                }
                stars = NewStarList(starsBuffer);
                secondsPassed++;
            }

            Render(camera);
        }

        static List<Star> NewStarList(List<Star> pList) {
            var result = new List<Star>();
            foreach (var star in pList) {
                result.Add(new Star { Position = star.Position, Velocity = star.Velocity });
            }
            return result;
        }

        static List<Star> ProcessStars(Action<Star> pStarAction) {
            int minX = int.MaxValue, minY = int.MaxValue, maxX = 0, maxY = 0;
            var result = new List<Star>();
            foreach (var star in stars) {
                var bufferedStar = new Star { Position = star.Position, Velocity = star.Velocity };
                minX = minX > bufferedStar.Position.X ? bufferedStar.Position.X : minX;
                minY = minY > bufferedStar.Position.Y ? bufferedStar.Position.Y : minY;
                maxX = maxX < bufferedStar.Position.X ? bufferedStar.Position.X : maxX;
                maxY = maxY < bufferedStar.Position.Y ? bufferedStar.Position.Y : maxY;

                pStarAction?.Invoke(bufferedStar);

                result.Add(bufferedStar);
            }
            world = new Rectangle(minX, minY, maxX - minX, maxY - minY);
            camera.Position = new Point(minX, minY);
            return result;
        }

        static void Render(Camera pCamera) {
            Console.ForegroundColor = ConsoleColor.Yellow;
            var rectangle = new Rectangle(pCamera.Position.X, pCamera.Position.Y, pCamera.Window.Width, pCamera.Window.Height);
            foreach (var star in stars) {
                if (rectangle.IntersectsWith(new Rectangle(star.Position.X, star.Position.Y, 1, 1))) {
                    Console.SetCursorPosition(star.Position.X - camera.Position.X, star.Position.Y - camera.Position.Y);
                    Console.Write("*");
                }
            }
        }

        static Star Parse(string pLine) {
            var match = starParseRegex.Match(pLine);
            return new Star {
                Position = new Point(int.Parse(match.Groups["px"].Value), int.Parse(match.Groups["py"].Value)),
                Velocity = new Point(int.Parse(match.Groups["vx"].Value), int.Parse(match.Groups["vy"].Value))
            };
        }
    }
}
