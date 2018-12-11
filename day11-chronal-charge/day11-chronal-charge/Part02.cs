using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Linq;

namespace day11_chronal_charge {
    class Part02 {
        class FuelCell {
            public int RackId { get; set; }
            public long PowerLevel { get; set; }
            public Point Position { get; set; }
        }

        class FuelCellAggregate {
            public long PowerLevel { get; set; }
        }

        delegate void PerFuelCellDelegate(FuelCell pCell, int pX, int pY);
        delegate void PerFuelCellAggregateDelegate(FuelCellAggregate pCell, int pX, int pY);

        static Size FuelCellGridSize;
        static Size CellAreaToCheck;
        static ManualResetEvent[] resetEvents;

        static volatile List<ThreadResult> results;

        public static void Run() {
            ThreadPool.SetMaxThreads(6, 6);
            resetEvents = new ManualResetEvent[60];
            for (int i = 0; i < 60; i++) { resetEvents[i] = new ManualResetEvent(false); }
            results = new List<ThreadResult>();

            for (int size = 1; size <= 60; size++) {
                ThreadPool.QueueUserWorkItem(Task, new Parameters { Input = 8141, Size = size, ResetEvent = size - 1 });
            }

            WaitHandle.WaitAll(resetEvents);
            Console.WriteLine("Completed 60.");
            resetEvents = new ManualResetEvent[60];
            for (int i = 0; i < 60; i++) { resetEvents[i] = new ManualResetEvent(false); }

            for (int size = 61; size <= 120; size++) {
                ThreadPool.QueueUserWorkItem(Task, new Parameters { Input = 8141, Size = size, ResetEvent = size - 61 });
            }

            WaitHandle.WaitAll(resetEvents);
            Console.WriteLine("Completed 120.");
            resetEvents = new ManualResetEvent[60];
            for (int i = 0; i < 60; i++) { resetEvents[i] = new ManualResetEvent(false); }

            for (int size = 121; size <= 180; size++) {
                ThreadPool.QueueUserWorkItem(Task, new Parameters { Input = 8141, Size = size, ResetEvent = size - 121 });
            }

            WaitHandle.WaitAll(resetEvents);
            Console.WriteLine("Completed 180.");
            resetEvents = new ManualResetEvent[60];
            for (int i = 0; i < 60; i++) { resetEvents[i] = new ManualResetEvent(false); }

            for (int size = 181; size <= 240; size++) {
                ThreadPool.QueueUserWorkItem(Task, new Parameters { Input = 8141, Size = size, ResetEvent = size - 181 });
            }

            WaitHandle.WaitAll(resetEvents);
            Console.WriteLine("Completed 240.");
            resetEvents = new ManualResetEvent[60];
            for (int i = 0; i < 60; i++) { resetEvents[i] = new ManualResetEvent(false); }

            for (int size = 241; size <= 300; size++) {
                ThreadPool.QueueUserWorkItem(Task, new Parameters { Input = 8141, Size = size, ResetEvent = size - 241 });
            }

            WaitHandle.WaitAll(resetEvents);
            Console.WriteLine("Completed 300.");

            var largest = results.OrderBy(r => r.PowerLevel).First();

            Console.WriteLine($"{largest.Position.X},{largest.Position.Y},{largest.Size}");
        }

        struct Parameters {
            public int Input { get; set; }
            public int Size { get; set; }
            public int ResetEvent { get; set; }
        }

        struct ThreadResult {
            public Point Position { get; set; }
            public int Size { get; set; }
            public long PowerLevel { get; set; }
        }

        public static void Task(object pState) {
            var parameters = (Parameters)pState;
            var result = RunOnce(parameters.Input, new Size(parameters.Size, parameters.Size));

            long largestResult = long.MinValue;
            int largestSize = 0;
            var point = Point.Empty;

            if (result.PowerLevel > largestResult) {
                largestSize = parameters.Size;
                largestResult = result.PowerLevel;
                point = result.Coordinate;
            }

            results.Add(new ThreadResult {
                Position = point,
                Size = largestSize,
                PowerLevel = largestResult
            });

            resetEvents[parameters.ResetEvent].Set();
        }

        internal struct Result {
            public Point Coordinate { get; set; }
            public long PowerLevel { get; set; }
        }

        public static Result RunOnce(int pInput, Size pSize) {
            CellAreaToCheck = pSize;
            FuelCellGridSize = new Size(300, 300);

            var fuelCells = new FuelCell[FuelCellGridSize.Width, FuelCellGridSize.Height];
            var fuelCellAggregates = new FuelCellAggregate[FuelCellGridSize.Width, FuelCellGridSize.Height];

            var perFuelCell = new Action<PerFuelCellDelegate>(dlgt => {
                for (int x = 1; x <= FuelCellGridSize.Width; x++) {
                    for (int y = 1; y <= FuelCellGridSize.Height; y++) {
                        dlgt?.Invoke(fuelCells[x - 1, y - 1], x, y);
                    }
                }
            });

            var perFuelCellAggregate = new Action<PerFuelCellAggregateDelegate>(dlgt => {
                for (int x = 0; x < FuelCellGridSize.Width; x++) {
                    for (int y = 0; y < FuelCellGridSize.Height; y++) {
                        dlgt?.Invoke(fuelCellAggregates[x, y], x, y);
                    }
                }
            });

            perFuelCell((cell, x, y) => {
                fuelCells[x - 1, y - 1] = new FuelCell();
            });

            perFuelCell((cell, x, y) => {
                cell.Position = new Point(x, y);
                cell.RackId = x + 10;
                var powerLevel = CalculatePowerLevel(cell.RackId, y, pInput);
                cell.PowerLevel = powerLevel;
            });

            perFuelCellAggregate((cell, x, y) => {
                fuelCellAggregates[x, y] = new FuelCellAggregate();
            });

            for (int x = 0; x < FuelCellGridSize.Width - CellAreaToCheck.Width - 1; x++) {
                for (int y = 0; y < FuelCellGridSize.Height - CellAreaToCheck.Height - 1; y++) {
                    for (int sx = 0; sx < CellAreaToCheck.Width; sx++) {
                        for (int sy = 0; sy < CellAreaToCheck.Height; sy++) {
                            fuelCellAggregates[x, y].PowerLevel = fuelCellAggregates[x, y].PowerLevel + fuelCells[x + sx, y + sy].PowerLevel;
                        }
                    }
                }
            }

            long largestPowerCellCenter = long.MinValue;
            var largestPowerCellTopLeft = Point.Empty;

            perFuelCellAggregate((cell, x, y) => {
                if (cell.PowerLevel > largestPowerCellCenter) {
                    largestPowerCellCenter = cell.PowerLevel;
                    largestPowerCellTopLeft = new Point(x, y);
                }
            });

            return new Result {
                Coordinate = new Point { X = largestPowerCellTopLeft.X + 1, Y = largestPowerCellTopLeft.Y + 1 },
                PowerLevel = largestPowerCellCenter
            };
        }

        public static int CalculatePowerLevel(int pRackId, int pY, int pInput) {
            long step1 = (pRackId * pY + pInput) * pRackId;
            int step2 = (int)Math.Floor(step1 / 100f);
            int step3 = (step2 % 10) - 5;
            return step3;
        }
    }
}
