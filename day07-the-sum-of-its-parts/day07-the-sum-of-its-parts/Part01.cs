using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace day07_the_sum_of_its_parts {
    class Part01 {
        struct Order {
            public string First { get; set; }
            public string Then { get; set; }
        }

        class FlowItem {
            public string Name { get; set; }
            public List<FlowItem> Required { get; set; }
            public bool Complete { get; set; }
        }

        static Dictionary<string, FlowItem> items;

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

        public static void Run() {
            var lines = File.ReadLines("input.txt");
            var orders = new List<Order>();
            foreach (var line in lines) {
                orders.Add(Parse(line));
            }

            items = new Dictionary<string, FlowItem>();

            foreach (var order in orders) {
                var first = CreateOrGet(order.First);
                var then = CreateOrGet(order.Then);
                then.Required.Add(first);
            }

            var start = items.Values.Where(i => i.Complete == false && i.Required.Count() == 0).OrderBy(c => c.Name).FirstOrDefault();

            string result = start.Name;
            start.Complete = true;

            var current = start;
            var running = true;

            while (running) {
                var allWithNoRequired = new Func<FlowItem, bool>(i => i.Required.Count == 0);
                var allWithAllCompleteRequired = new Func<FlowItem, bool>(i => i.Required.All(r => r.Complete));

                var next = items.Values.Where(i => allWithNoRequired(i) || allWithAllCompleteRequired(i)).Where(i => !i.Complete).OrderBy(i => i.Name).FirstOrDefault();

                if (next == null) {
                    running = false;
                } else {
                    result += next.Name;
                    next.Complete = true;
                    current = next;
                }
            }

            Console.WriteLine("Sequence: " + result);
        }

        static Order Parse(string pLine) {
            return new Order {
                First = pLine.Substring(5, 1),
                Then = pLine.Substring(36, 1)
            };
        }
    }
}
