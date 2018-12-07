using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace day07_the_sum_of_its_parts {
    class Part02 {
        struct Order {
            public string First { get; set; }
            public string Then { get; set; }
        }

        class FlowItem {
            public string Name { get; set; }
            public List<FlowItem> Required { get; set; }
            public bool Complete { get; set; }
            public bool Assigned { get; set; }
        }

        static Dictionary<string, FlowItem> items;

        // YES I AM LAZY
        static string Letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        private static FlowItem CreateOrGet(string pName) {
            if (items.ContainsKey(pName)) return items[pName];
            var flowItem = new FlowItem {
                Name = pName,
                Required = new List<FlowItem>(),
                Complete = false
            };

            items.Add(pName, flowItem);
            return flowItem;
        }

        class Elf {
            public string WorkingOn { get; set; }
            public int WorkLeft { get; set; }
        }

        public static void Run() {
            var lines = File.ReadLines("input.txt");
            var orders = new List<Order>();
            foreach (var line in lines) {
                orders.Add(Parse(line));
            }

            int maxElves = 5;

            items = new Dictionary<string, FlowItem>();

            foreach (var order in orders) {
                var first = CreateOrGet(order.First);
                var then = CreateOrGet(order.Then);
                then.Required.Add(first);
            }

            var start = items.Values.Where(i => i.Complete == false && i.Required.Count() == 0).OrderBy(c => c.Name).FirstOrDefault();

            string result = start.Name;

            var current = start;
            var running = true;

            var elves = new Elf[maxElves];

            for (int e = 0; e < maxElves; e++) {
                elves[e] = new Elf { WorkingOn = "", WorkLeft = 0 };
            }

            int s = 0;

            while (running) {
                var allWithNoRequired = new Func<FlowItem, bool>(i => i.Required.Count == 0);
                var allWithAllCompleteRequired = new Func<FlowItem, bool>(i => i.Required.All(r => r.Complete));

                // assign work
                for (int elfIndex = 0; elfIndex < elves.Length; elfIndex++) {
                    var elf = elves[elfIndex];

                    if (elf.WorkingOn == string.Empty && elf.WorkLeft == 0) {
                        // assign
                        var next = items.Values.Where(i => allWithNoRequired(i) || allWithAllCompleteRequired(i)).Where(i => !i.Complete && !i.Assigned).OrderBy(i => i.Name).FirstOrDefault();
                        if (next == null) {
                            break;
                        }
                        elf.WorkingOn = next.Name;
                        elf.WorkLeft = 60 + Letters.IndexOf(next.Name) + 1;
                        next.Assigned = true;
                    }
                }

                // deduct work
                for (int elfIndex = 0; elfIndex < elves.Length; elfIndex++) {
                    var elf = elves[elfIndex];
                    if (elf.WorkingOn != string.Empty) {
                        elf.WorkLeft--;
                        if (elf.WorkLeft == 0) {
                            result += elf.WorkingOn;
                            //Console.WriteLine("Completed " + elf.WorkingOn + " on " + s);
                            items[elf.WorkingOn].Complete = true;
                            elf.WorkingOn = "";
                        }
                    }
                }

                s++;
                if (items.Values.All(i => i.Complete)) break;
            }

            Console.WriteLine("Seconds to Complete: " + s.ToString());
        }

        static Order Parse(string pLine) {
            return new Order {
                First = pLine.Substring(5, 1),
                Then = pLine.Substring(36, 1)
            };
        }
    }
}