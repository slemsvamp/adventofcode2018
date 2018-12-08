using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace day08_memory_maneuver {
    class Part01 {
        class Node {
            public int ChildNodes { get; set; }
            public List<Node> Children { get; set; }
            public List<int> MetadataEntries { get; set; }
        }

        public static void Run() {
            var numbersText = File.ReadAllText("mockInput.txt");
            var parts = numbersText.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            List<int> numbers = new List<int>();
            foreach (var part in parts) {
                numbers.Add(int.Parse(part));
            }

            var nodes = new Dictionary<int, Node>();

            for (int i = 0; i < numbers.Count; i++) {
                var node = CreateNode(i, numbers);
                nodes.Add(i, node);
            }

            Console.WriteLine(numbers.Count);
        }

        static Node CreateNode(Node pParent, List<int> pNumbers) {
            // read header
            var quantityChildNodes = pNumbers[pIndex];
            var quantityMetadataEntries = pNumbers[pIndex+1];

            var reverse = new List<int>(pNumbers);
            reverse.Reverse();

            var inbetween = new List<int>();
            

            var node = new Node(null, inbetween);

        }
    }
}
