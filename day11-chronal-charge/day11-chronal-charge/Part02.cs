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

        struct FuelCellAggregate {
            public long PowerLevel { get; set; }
        }

        delegate void PerFuelCellDelegate(FuelCell pCell, int pX, int pY);
        delegate void PerFuelCellAggregateDelegate(FuelCellAggregate pCell, int pX, int pY);

        static Size FuelCellGridSize;

        static Dictionary<int, FuelCellAggregate[,]> history;

        public static void Run() {
            long largestResult = long.MinValue;
            int largestSize = 0;
            var point = Point.Empty;
            history = new Dictionary<int, FuelCellAggregate[,]>();

            int input = 8141;

            FuelCellGridSize = new Size(300, 300);

            if (fuelCells == null) {
                fuelCells = new FuelCell[FuelCellGridSize.Width, FuelCellGridSize.Height];
                var fuelCellAggregates = new FuelCellAggregate[FuelCellGridSize.Width, FuelCellGridSize.Height];

                var perFuelCell = new Action<PerFuelCellDelegate>(dlgt => {
                    for (int x = 1; x <= FuelCellGridSize.Width; x++) {
                        for (int y = 1; y <= FuelCellGridSize.Height; y++) {
                            if (fuelCells[x - 1, y - 1] == null) {
                                fuelCells[x - 1, y - 1] = new FuelCell();
                            }
                            dlgt?.Invoke(fuelCells[x - 1, y - 1], x, y);
                        }
                    }
                });

                history.Add(1, new FuelCellAggregate[300, 300]);

                perFuelCell((cell, x, y) => {
                    cell.Position = new Point(x, y);
                    cell.RackId = x + 10;
                    var powerLevel = CalculatePowerLevel(cell.RackId, y, input);
                    cell.PowerLevel = powerLevel;
                    history[1][x-1, y-1] = new FuelCellAggregate { PowerLevel = powerLevel };
                });
            }

            for (int size = 2; size <= 300; size++) {
                var result = RunOnce(size);

                if (result.PowerLevel > largestResult) {
                    largestSize = size;
                    largestResult = result.PowerLevel;
                    point = result.Coordinate;
                }
            }

            Console.WriteLine($"{point.X},{point.Y},{largestSize}");
        }

        internal struct Result {
            public Point Coordinate { get; set; }
            public long PowerLevel { get; set; }
        }

        static FuelCell[,] fuelCells;

        public static Result RunOnce(int pSize) {
            int previousSize = pSize - 1;

            var previousFuelCellAggregates = new FuelCellAggregate[0, 0];

            if (history.ContainsKey(previousSize)) {
                previousFuelCellAggregates = history[previousSize];
            }

            history.Add(pSize, new FuelCellAggregate[300 - pSize + 1, 300 - pSize + 1]);

            long largestPower = long.MinValue;
            var largestPoint = Point.Empty;

            for (int x = 0; x < FuelCellGridSize.Width - pSize + 1; x++) {
                for (int y = 0; y < FuelCellGridSize.Height - pSize + 1; y++) {
                    long powerlevel = 0;
                    for (int sy = 0; sy < pSize - 1; sy++) {
                        var rightCell = fuelCells[x + pSize - 1, y + sy].PowerLevel;
                        powerlevel += rightCell;
                    }

                    for (int sx = 0; sx < pSize - 1; sx++) {
                        var bottomCell = fuelCells[x + sx, y + pSize - 1].PowerLevel;
                        powerlevel += bottomCell;
                    }

                    // corner
                    powerlevel += fuelCells[x + pSize - 1, y + pSize - 1].PowerLevel;

                    if (powerlevel > largestPower) {
                        largestPower = powerlevel;
                        largestPoint = new Point(x, y);
                    }

                    history[pSize][x, y] = new FuelCellAggregate { PowerLevel = history[previousSize][x, y].PowerLevel + powerlevel };
                }
            }

            return new Result {
                Coordinate = new Point { X = largestPoint.X + 1, Y = largestPoint.Y + 1 },
                PowerLevel = largestPower
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
