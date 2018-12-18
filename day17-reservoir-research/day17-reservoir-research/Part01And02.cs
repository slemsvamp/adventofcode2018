using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace day17_reservoir_research {
    class Part01And02 {
        enum EntityEnum {
            Spring, Sand, WetSand, Clay, Water
        }

        enum Axis {
            Horizontal, Vertical
        }

        enum Direction {
            Left, Right, Down
        }

        enum WaterAction {
            Fall, SeekHorizontals, FlowLeft, FlowRight, Settle
        }

        enum SeekAction {
            Fall, Horizontals, Revisit
        }

        enum SeekResultType {
            Infinite, Blocked, Quantum
        }

        enum WaterlingState {
            Falling, Idle, RoamingLeft, RoamingRight, EnteredInfinity, WalkedIntoAWall, FillAndClimb, SpawnUnderlings, Ghosted
        }

        class SeekResult {
            public SeekResultType Type { get; set; }
            public Point Location { get; set; }
        }

        struct Entity {
            public byte Id { get; set; }
            public char Display { get; set; }
            public ConsoleColor Foreground { get; set; }
            public ConsoleColor Background { get; set; }
        }

        struct Range {
            public int Start { get; set; }
            public int End { get; set; }
        }

        struct Instruction {
            public Axis Type { get; set; }
            public int Slice { get; set; }
            public Range Range { get; set; }
        }

        static Point display;
        static Rectangle camera;
        static Dictionary<EntityEnum, Entity> entitiesByEnum;
        static Dictionary<byte, Entity> entitiesById;
        static List<Instruction> instructions;
        static Regex inputParseRegex;

        
        static List<Waterling> waterlings;
        static byte[] map;
        static Rectangle area;

        static int round;
        static bool running = true;

        static Waterling rootWaterling;

        public static void Run() {
            var lines = File.ReadAllLines("input.txt");
            display = new Point(1, 1);
            camera = new Rectangle(400, 1880, 120, 40);
            round = 0;
            instructions = new List<Instruction>();
            inputParseRegex = new Regex(@"[x|y]=(?<slice>\d+), [x|y]=(?<range_start>\d+)\.\.(?<range_end>\d+)");

            LoadResources();
            Parse(lines);
            LoadInstructions();

            bool play = false;
            bool render = false;
            bool fastforward = true;
            int? fastforwardTo = null;

            rootWaterling = new Waterling() {
                Position = new Point(500, 6)
            };

            waterlings = new List<Waterling>() {
                rootWaterling
            };

            // LoadMap();
            // Console.ReadKey(true);

            while (running) {
                round++;

                if (fastforwardTo.HasValue && round < fastforwardTo.Value) {

                } else {
                    if (render) {
                        RenderView();
                        Console.SetCursorPosition(display.X + 2, display.Y + camera.Height - 2);
                        Console.Write("Round: " + round);
                        Console.Write(", IsInfinite: " + rootWaterling.IsInfinite);
                    }
                    if (play) {
                        Thread.Sleep(200);
                    } else if (!fastforward) {
                        bool letGo = false;
                        while (!letGo) {
                            var key = Console.ReadKey(true);
                            if (key.Key == ConsoleKey.Escape) { return; }
                            if (key.Key == ConsoleKey.RightArrow) camera.X+=camera.Width;
                            if (key.Key == ConsoleKey.LeftArrow) camera.X-=camera.Width;
                            if (key.Key == ConsoleKey.UpArrow) camera.Y-=camera.Height;
                            if (key.Key == ConsoleKey.DownArrow) camera.Y+=camera.Height;
                            if (key.Key == ConsoleKey.Enter) letGo = true;
                            if (render) {
                                RenderView();
                            }
                        }
                    }
                }

                RunWaterlings();

                if (waterlings.All(w => w.Done)) {
                    running = false;
                    RenderView();
                    var sumWater = 0;
                    var sumWetSand = 0;
                    for (int i = 0; i < map.Length; i++) {
                        sumWater += map[i] == entitiesByEnum[EntityEnum.Water].Id ? 1 : 0;
                        sumWetSand += map[i] == entitiesByEnum[EntityEnum.WetSand].Id ? 1 : 0;
                    }
                    Console.ReadKey(true);
                    Console.Clear();
                    Console.WriteLine("Part01: " + (sumWater + sumWetSand));
                    Console.WriteLine("Part02: " + sumWater);
                }
            }

            Console.SetCursorPosition(0, 20);
        }

        static void RenderView() {
            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.Black;

            for (int y = camera.Top; y < camera.Top + camera.Height; y++) {
                int ay = y - area.Top;
                int cy = y - camera.Top;
                for (int x = camera.Left; x < camera.Left + camera.Width; x++) {
                    int ax = x - area.Left;
                    int cx = x - camera.Left;

                    Console.SetCursorPosition(display.X + x - camera.Left, display.Y + y - camera.Top);

                    if (ay < 0 || ax < 0 || ay >= area.Height || ax >= area.Width) {
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        Random rnd = new Random();
                        switch (rnd.Next(0, 4)) {
                            case 0: Console.Write("¸"); break;
                            case 1: Console.Write("´"); break;
                            case 2: Console.Write("'"); break;
                            case 3: Console.Write("·"); break;
                        }
                    } else {
                        var index = ay * area.Width + ax;
                        var entity = entitiesById[map[index]];
                        Console.ForegroundColor = entity.Foreground;
                        Console.BackgroundColor = entity.Background;
                        Console.Write(entity.Display);
                    }

                    Console.ForegroundColor = ConsoleColor.White;
                    Console.BackgroundColor = ConsoleColor.Black;
                }
            }

            //foreach (var waterling in waterlings) {
            //    waterling.Draw();
            //}
        }

        class Waterling {
            public WaterlingState State { get; set; }
            public Point Position { get; set; }
            public Waterling Parent { get; set; }
            public List<Waterling> Underlings { get; set; }
            public List<Waterling> SleepingUnderlings { get; set; }
            public bool Done { get; private set; }
            public bool IsInfinite { get; set; }

            public Waterling() {
                State = WaterlingState.Falling;
                Underlings = new List<Waterling>();
                SleepingUnderlings = new List<Waterling>();
            }

            public void SpawnUnderling(Point pAt, WaterlingState pJob) {
                var underling = new Waterling() {
                    Position = pAt,
                    State = pJob,
                    Parent = this
                };
                Underlings.Add(underling);
                waterlings.Add(underling);
            }

            public void ChildUpdatedItsState(Waterling pChild, WaterlingState pState) {
                if (Underlings.All(u => u.State == WaterlingState.WalkedIntoAWall)) {
                    State = WaterlingState.FillAndClimb;
                }
            }

            public void Update() {
                if (map[CalculateMapPosition(Position)] == entitiesByEnum[EntityEnum.Sand].Id) {
                    MarkAs(EntityEnum.WetSand, Position);
                }

                if (map[CalculateMapPosition(Position)] == entitiesByEnum[EntityEnum.Water].Id) {
                    Done = true;
                    State = WaterlingState.Ghosted;
                }

                HandleState();

                var underlingsCopy = Underlings.ToList();
                foreach (var underling in underlingsCopy) {
                    underling.Update();
                }

                if (Underlings.Any(u => u.IsInfinite)) {
                    IsInfinite = true;
                }

                if (State == WaterlingState.Idle && (Underlings.Count == 0 || Underlings.All(u => u.Done))) {
                    Done = true;
                }
            }

            void HandleState() {
                if (State == WaterlingState.Falling) {
                    var floor = Position.Down();
                    if (IsVoid(floor)) {
                        State = WaterlingState.EnteredInfinity;
                        Parent.ChildUpdatedItsState(this, WaterlingState.EnteredInfinity);
                        IsInfinite = true;
                        Done = true;
                        return;
                    }
                    if (IsObstacle(floor)) {
                        State = WaterlingState.Idle;
                        bool spawnedUnderlings = false;
                        var left = Position.Left();
                        if (!IsObstacle(left) && !IsVoid(left)) {
                            SpawnUnderling(left, WaterlingState.RoamingLeft);
                            spawnedUnderlings = true;
                        }

                        var right = Position.Right();
                        if (!IsObstacle(right) && !IsVoid(right)) {
                            SpawnUnderling(right, WaterlingState.RoamingRight);
                            spawnedUnderlings = true;
                        }

                        if (!spawnedUnderlings) {
                            State = WaterlingState.FillAndClimb;
                        }
                    } else {
                        Position = Position.Down();
                    }
                } else if (State == WaterlingState.RoamingLeft) {
                    // do we have something to stand on?
                    var floor = Position.Down();
                    if (IsVoid(floor)) {
                        State = WaterlingState.EnteredInfinity;
                        Parent.ChildUpdatedItsState(this, WaterlingState.EnteredInfinity);
                        IsInfinite = true;
                        Done = true;
                        return;
                    } else if (!IsObstacle(floor)) {
                        // no floor! we gonna fall!
                        State = WaterlingState.Falling;
                        Parent.ChildUpdatedItsState(this, WaterlingState.Falling);
                        return;
                    }

                    var left = Position.Left();
                    if (IsVoid(left)) {
                        State = WaterlingState.EnteredInfinity;
                        Parent.ChildUpdatedItsState(this, WaterlingState.EnteredInfinity);
                        IsInfinite = true;
                        Done = true;
                        return;
                    }
                    if (IsObstacle(left)) {
                        // report back to papa/mama that we hit an obstacle
                        State = WaterlingState.WalkedIntoAWall;
                        Parent.ChildUpdatedItsState(this, WaterlingState.WalkedIntoAWall);
                        Done = true;
                        return;
                    } else {
                        Position = Position.Left();
                    }
                } else if (State == WaterlingState.RoamingRight) {
                    // do we have something to stand on?
                    var floor = Position.Down();
                    if (IsVoid(floor)) {
                        State = WaterlingState.EnteredInfinity;
                        Parent.ChildUpdatedItsState(this, WaterlingState.EnteredInfinity);
                        IsInfinite = true;
                        Done = true;
                        return;
                    } else if (!IsObstacle(floor)) {
                        // no floor! we gonna fall!
                        State = WaterlingState.Falling;
                        Parent.ChildUpdatedItsState(this, WaterlingState.Falling);
                        return;
                    }

                    var right = Position.Right();
                    if (IsVoid(right)) {
                        State = WaterlingState.EnteredInfinity;
                        Parent.ChildUpdatedItsState(this, WaterlingState.EnteredInfinity);
                        IsInfinite = true;
                        Done = true;
                        return;
                    }
                    if (IsObstacle(right)) {
                        // report back to papa/mama that we hit an obstacle
                        State = WaterlingState.WalkedIntoAWall;
                        Parent.ChildUpdatedItsState(this, WaterlingState.WalkedIntoAWall);
                        Done = true;
                        return;
                    } else {
                        Position = Position.Right();
                    }
                } else if (State == WaterlingState.FillAndClimb) {
                    int minX = int.MaxValue, maxX = int.MinValue;
                    foreach (var underling in Underlings) {
                        if (underling.Position.X < minX) minX = underling.Position.X;
                        if (underling.Position.X > maxX) maxX = underling.Position.X;
                    }
                    if (Position.X < minX) minX = Position.X;
                    if (Position.X > maxX) maxX = Position.X;
                    for (int x = minX; x <= maxX; x++) {
                        MarkAs(EntityEnum.Water, new Point(x, Position.Y));
                    }
                    foreach (var underling in Underlings) {
                        underling.Done = true;
                        waterlings.Remove(underling);
                    }
                    SleepingUnderlings.AddRange(Underlings);
                    Underlings.Clear();
                    Position = Position.Up();
                    State = WaterlingState.Falling;
                }
            }

            public void Draw() {
                if (Done) return;
                var drawnPosition = new Point(display.X + Position.X - camera.X, display.Y + Position.Y - camera.Y);
                Console.SetCursorPosition(drawnPosition.X, drawnPosition.Y);
                Console.ForegroundColor = ConsoleColor.Black;
                Console.BackgroundColor = State == WaterlingState.Ghosted ? ConsoleColor.Yellow : (Done ? ConsoleColor.DarkGray : ConsoleColor.Green);
                Console.Write("☺");

                foreach (var underling in Underlings) {
                    underling.Draw();
                }
            }
        }

        static void RunWaterlings() {
            rootWaterling.Update();
        }

        static int CalculateMapPosition(Point pPoint) {
            return (pPoint.Y - area.Top) * area.Width + (pPoint.X - area.Left);
        }

        static bool IsObstacle(Point pPoint) {
            var id = map[CalculateMapPosition(pPoint)];
            return id == entitiesByEnum[EntityEnum.Clay].Id || id == entitiesByEnum[EntityEnum.Water].Id;
        }

        static bool IsVoid(Point pPoint) {
            return pPoint.X < area.X || pPoint.Y < area.Y || pPoint.X >= area.X + area.Width || pPoint.Y >= area.Y + area.Height;
        }

        static void MarkAs(EntityEnum pType, Point pPosition) {
            var mapIndex = CalculateMapPosition(pPosition);
            if (mapIndex >= 0 && mapIndex < map.Length) {
                map[mapIndex] = entitiesByEnum[pType].Id;
            }
        }

        static void LoadInstructions() {
            map = new byte[area.Width * area.Height];
            var clayId = entitiesByEnum[EntityEnum.Clay].Id;
            foreach (var instruction in instructions) {
                if (instruction.Type == Axis.Horizontal) {
                    var y = instruction.Slice - area.Top;
                    var rangeStart = instruction.Range.Start - area.Left;
                    var rangeEnd = instruction.Range.End - area.Left;
                    for (var x = rangeStart; x <= rangeEnd; x++) {
                        map[y * area.Width + x] = clayId;
                    }
                } else {
                    var x = instruction.Slice - area.Left;
                    var rangeStart = instruction.Range.Start - area.Top;
                    var rangeEnd = instruction.Range.End - area.Top;
                    for (var y = rangeStart; y <= rangeEnd; y++) {
                        map[y * area.Width + x] = clayId;
                    }
                }
            }
        }

        static void Parse(string[] pLines) {
            int minX = 500, minY = int.MaxValue, maxX = 500, maxY = int.MinValue;
            foreach (var line in pLines) {
                var regex = inputParseRegex.Match(line);
                var instruction = new Instruction {
                    Type = line[0] == 'y' ? Axis.Horizontal : Axis.Vertical,
                    Slice = int.Parse(regex.Groups["slice"].Value),
                    Range = new Range { Start = int.Parse(regex.Groups["range_start"].Value), End = int.Parse(regex.Groups["range_end"].Value) }
                };
                instructions.Add(instruction);
                if (instruction.Type == Axis.Horizontal) {
                    if (instruction.Slice < minY) minY = instruction.Slice;
                    if (instruction.Slice > maxY) maxY = instruction.Slice;
                    if (instruction.Range.Start < minX) minX = instruction.Range.Start;
                    if (instruction.Range.End > maxX) maxX = instruction.Range.End;
                } else {
                    if (instruction.Slice < minX) minX = instruction.Slice;
                    if (instruction.Slice > maxX) maxX = instruction.Slice;
                    if (instruction.Range.Start < minY) minY = instruction.Range.Start;
                    if (instruction.Range.End > maxY) maxY = instruction.Range.End;
                }
            }
            area = new Rectangle(minX - 1, minY, maxX - minX + 1 + 1, maxY - minY + 1);
        }

        static void LoadResources() {
            entitiesById = new Dictionary<byte, Entity>();
            entitiesByEnum = new Dictionary<EntityEnum, Entity> {
                { EntityEnum.Sand, new Entity { Id = 0, Display = '·', Foreground = ConsoleColor.Yellow } },
                { EntityEnum.WetSand, new Entity { Id = 1, Display = '|', Foreground = ConsoleColor.DarkYellow } },
                { EntityEnum.Spring, new Entity { Id = 2, Display = '+', Foreground = ConsoleColor.White, Background = ConsoleColor.DarkBlue } },
                { EntityEnum.Clay, new Entity { Id = 3, Display = '#', Foreground = ConsoleColor.DarkGray } },
                { EntityEnum.Water, new Entity { Id = 4, Display = '~', Foreground = ConsoleColor.Blue } }
            };

            foreach (var g in entitiesByEnum) {
                entitiesById.Add(g.Value.Id, g.Value);
            }
        }
    }
}
