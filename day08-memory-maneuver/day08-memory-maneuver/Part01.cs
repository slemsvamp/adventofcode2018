using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace day08_memory_maneuver {
    class Part01 {
        class Node {
            public List<Node> Children { get; set; }
            public int MetadataSum { get; set; }
        }

        public static int metadataSum = 0;

        public static void Run() {
            var numbersText = File.ReadAllText("input.txt");
            var parts = numbersText.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);

            var data = new Stack<int>();

            foreach (var part in parts.Reverse()) {
                data.Push(int.Parse(part));
            }

            var node = CreateNode(data, 1);

            Console.WriteLine(metadataSum);
        }

        static Node CreateNode(Stack<int> pStack, int pLevel) {
            var childrenCount = pStack.Pop();
            var metadataCount = pStack.Pop();

            var node = new Node {
                Children = new List<Node>(),
                MetadataSum = 0
            };

            for (int i = 0; i < childrenCount; i++) {
                var child = CreateNode(pStack, pLevel + 1);
                node.Children.Add(child);
            }
            
            for (int p = 0; p < metadataCount; p++) {
                node.MetadataSum += pStack.Pop();
            }

            metadataSum += node.MetadataSum;

            return node;
        }
    }
}
