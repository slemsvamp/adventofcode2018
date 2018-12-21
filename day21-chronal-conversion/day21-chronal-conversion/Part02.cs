using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace day21_chronal_conversion {
    class Part02 {
        class Instruction {
            public Opcode OpCode { get; set; }
            public uint A { get; set; }
            public uint B { get; set; }
            public uint C { get; set; }

            public override string ToString() {
                return $"{OpCode} {A} {B} {C}";
            }
        }

        static uint instructionPointer;
        static uint instructionPointerValue;
        static List<Instruction> instructions;
        static uint[] registers;

        enum Opcode {
            addr, addi,
            mulr, muli,
            banr, bani,
            borr, bori,
            setr, seti,
            gtir, gtri, gtrr,
            eqir, eqri, eqrr
        }

        public static void Run() {
            Initialize("input.txt");

            Console.WriteLine("Buckle up, this one will take a few minutes.. I'm lazy..");

            uint lastNumber = 0;
            var uniqueNumbers = new HashSet<uint>();

            while (true) {
                registers[instructionPointer] = instructionPointerValue;
                RunOpcode(instructions[(int)registers[instructionPointer]]);
                instructionPointerValue = registers[instructionPointer];
                instructionPointerValue++;
                if (instructionPointerValue == 28) {
                    if (uniqueNumbers.Contains(registers[3])) {
                        registers[0] = lastNumber;
                        break;
                    } else {
                        uniqueNumbers.Add(registers[3]);
                        lastNumber = registers[3];
                    }
                }
            }

            Console.WriteLine("Part02: " + registers[0]);
        }

        static void Initialize(string pFile) {
            var lines = File.ReadAllLines(pFile);
            registers = new uint[6];
            instructions = new List<Instruction>();
            instructionPointerValue = 0;
            var registerFromFile = new List<Instruction>();
            instructionPointer = uint.Parse(lines[0].Replace("#ip ", ""));
            for (var l = 1; l < lines.Length; l++) {
                var parts = lines[l].Split(new string[] { " " }, StringSplitOptions.None);
                instructions.Add(new Instruction {
                    OpCode = (Opcode)Enum.Parse(typeof(Opcode), parts[0]),
                    A = uint.Parse(parts[1]),
                    B = uint.Parse(parts[2]),
                    C = uint.Parse(parts[3])
                });
            }
        }

        static bool ArrayEq(byte[] pA, byte[] pB) {
            return pA.Length == pB.Length && pA[0] == pB[0] && pA[1] == pB[1] && pA[2] == pB[2] && pA[3] == pB[3];
        }

        static void RunOpcode(Instruction pInstruction) {
            // TIL: There's no byte addition operator for bytes, so I have to explicitly cast them.
            switch (pInstruction.OpCode) {
                case Opcode.addr:
                    registers[pInstruction.C] = registers[pInstruction.A] + registers[pInstruction.B];
                    break;
                case Opcode.addi:
                    registers[pInstruction.C] = registers[pInstruction.A] + pInstruction.B;
                    break;
                case Opcode.mulr:
                    registers[pInstruction.C] = registers[pInstruction.A] * registers[pInstruction.B];
                    break;
                case Opcode.muli:
                    registers[pInstruction.C] = registers[pInstruction.A] * pInstruction.B;
                    break;
                case Opcode.banr:
                    registers[pInstruction.C] = registers[pInstruction.A] & registers[pInstruction.B];
                    break;
                case Opcode.bani:
                    registers[pInstruction.C] = registers[pInstruction.A] & pInstruction.B;
                    break;
                case Opcode.borr:
                    registers[pInstruction.C] = registers[pInstruction.A] | registers[pInstruction.B];
                    break;
                case Opcode.bori:
                    registers[pInstruction.C] = registers[pInstruction.A] | pInstruction.B;
                    break;
                case Opcode.setr:
                    registers[pInstruction.C] = registers[pInstruction.A];
                    break;
                case Opcode.seti:
                    registers[pInstruction.C] = pInstruction.A;
                    break;
                case Opcode.gtir:
                    registers[pInstruction.C] = (uint)(pInstruction.A > registers[pInstruction.B] ? 1 : 0);
                    break;
                case Opcode.gtri:
                    registers[pInstruction.C] = (uint)(registers[pInstruction.A] > pInstruction.B ? 1 : 0);
                    break;
                case Opcode.gtrr:
                    registers[pInstruction.C] = (uint)(registers[pInstruction.A] > registers[pInstruction.B] ? 1 : 0);
                    break;
                case Opcode.eqir:
                    registers[pInstruction.C] = (uint)(pInstruction.A == registers[pInstruction.B] ? 1 : 0);
                    break;
                case Opcode.eqri:
                    registers[pInstruction.C] = (uint)(registers[pInstruction.A] == pInstruction.B ? 1 : 0);
                    break;
                case Opcode.eqrr:
                    registers[pInstruction.C] = (uint)(registers[pInstruction.A] == registers[pInstruction.B] ? 1 : 0);
                    break;
            }
        }
    }
}
