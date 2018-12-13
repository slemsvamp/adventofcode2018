using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace day13_mine_cart_madness {
    class Part02 {
        enum Direction {
            Left, Right, Forward,
            North, South, West, East
        }

        class Rail {
            public int X { get; set; }
            public int Y { get; set; }
            public Rail N { get; set; }
            public Rail S { get; set; }
            public Rail W { get; set; }
            public Rail E { get; set; }
            public char Sign { get; set; }
            public bool IsIntersection { get; set; }
            public List<Cart> Carts { get; set; }
        }

        class Map {
            public Rail[,] Rails { get; set; }

            public Map(int pWidth, int pHeight) {
                Rails = new Rail[pWidth, pHeight];
            }

            public Size Size { get { return new Size(Rails.GetLength(0), Rails.GetLength(1)); } }
        }

        class Cart {
            public bool Crashed { get; set; }
            public int Id { get; set; }
            public Rail Location { get; set; }
            public Rail NextLocation { get; set; }
            public Direction Facing { get; set; }
            public int Index { get { return Location.Y * map.Rails.GetLength(1) + Location.X; } }
            public int NextIntersectionDirection { get; set; }
        }

        static Map map;
        static int cartId;

        public static void Run(bool pRender) {
            Console.CursorVisible = false;
            Point camera = new Point(0, 0);
            Size size = new Size(50, 24);
            cartId = 0;
            var lines = File.ReadAllLines("input.txt");
            var cartRule = new List<Direction> { Direction.Left, Direction.Forward, Direction.Right };
            var carts = new Dictionary<int, Cart>();
            int seconds = 0;
            map = new Map(lines[0].Length, lines.Length);
            size = new Size(map.Size.Width, map.Size.Height);
            if (size.Width > 50) size.Width = 50;
            if (size.Height > 24) size.Height = 24;

            for (int y = 0; y < lines.Length; y++) {
                var lineLength = lines[y].Length;
                for (var x = 0; x < lineLength; x++) {
                    var current = lines[y][x];
                    if (@"/\|-+<^>v".Contains(current)) {
                        var sign = current;
                        if (sign == '<' || sign == '>') {
                            sign = '-';
                        } else if (sign == '^' || sign == 'v') {
                            sign = '|';
                        }

                        var rail = new Rail { Sign = sign, X = x, Y = y };

                        var railW = LookForRail(map, x - 1, y);
                        if (railW != null) {
                            if (@"+-".Contains(railW.Sign) || (railW.Sign == '\\' && railW.W == null) || (railW.Sign == '/' && railW.W == null)) {
                                railW.E = rail;
                                rail.W = railW;
                            }
                        }

                        var railN = LookForRail(map, x, y - 1);
                        if (railN != null) {
                            if (@"+|".Contains(railN.Sign) || (railN.Sign == '\\' && railN.N == null) || (railN.Sign == '/' && railN.N == null)) {
                                railN.S = rail;
                                rail.N = railN;
                            }
                        }

                        if (lines[y][x] == '+') {
                            rail.IsIntersection = true;
                        }

                        map.Rails[x, y] = rail;
                        Cart cart = null;
                        switch (lines[y][x]) {
                            case '<':
                                cart = new Cart { Id = cartId++, Location = rail, Facing = Direction.West };
                                break;
                            case '^':
                                cart = new Cart { Id = cartId++, Location = rail, Facing = Direction.North };
                                break;
                            case '>':
                                cart = new Cart { Id = cartId++, Location = rail, Facing = Direction.East };
                                break;
                            case 'v':
                                cart = new Cart { Id = cartId++, Location = rail, Facing = Direction.South };
                                break;
                            default: break;
                        }
                        if (cart != null) carts.Add(cart.Index, cart);
                    }
                }
            }
            
            while (true) {
                seconds++;
                var cartsToReAdd = new List<Cart>();
                var newCarts = new Dictionary<int, Cart>(carts);
                var keys = carts.Keys.OrderBy(k => k).ToList();

                foreach (var key in keys) {
                    var cart = carts[key];
                    if (cart.Crashed) continue;
                    Rail targetRail = null;
                    if (cart.Location.IsIntersection) {
                        // go certain direction
                        var direction = cartRule[cart.NextIntersectionDirection];
                        if (direction == Direction.Forward) {
                            switch (cart.Facing) {
                                case Direction.North: targetRail = cart.Location.N; break;
                                case Direction.East: targetRail = cart.Location.E; break;
                                case Direction.South: targetRail = cart.Location.S; break;
                                case Direction.West: targetRail = cart.Location.W; break;
                            }
                        } else if (direction == Direction.Left) {
                            switch (cart.Facing) {
                                case Direction.North: targetRail = cart.Location.W; cart.Facing = Direction.West; break;
                                case Direction.East: targetRail = cart.Location.N; cart.Facing = Direction.North; break;
                                case Direction.South: targetRail = cart.Location.E; cart.Facing = Direction.East; break;
                                case Direction.West: targetRail = cart.Location.S; cart.Facing = Direction.South; break;
                            }
                        } else if (direction == Direction.Right) {
                            switch (cart.Facing) {
                                case Direction.North: targetRail = cart.Location.E; cart.Facing = Direction.East; break;
                                case Direction.East: targetRail = cart.Location.S; cart.Facing = Direction.South; break;
                                case Direction.South: targetRail = cart.Location.W; cart.Facing = Direction.West; break;
                                case Direction.West: targetRail = cart.Location.N; cart.Facing = Direction.North; break;
                            }
                        }

                        cart.NextIntersectionDirection++;
                        if (cart.NextIntersectionDirection > 2) cart.NextIntersectionDirection = 0;
                    } else {
                        if (cart.Facing == Direction.North) {
                            if (cart.Location.N != null) { targetRail = cart.Location.N; } else if (cart.Location.W != null) { cart.Facing = Direction.West; targetRail = cart.Location.W; } else if (cart.Location.E != null) { cart.Facing = Direction.East; targetRail = cart.Location.E; }
                        } else if (cart.Facing == Direction.East) {
                            if (cart.Location.E != null) { targetRail = cart.Location.E; } else if (cart.Location.N != null) { cart.Facing = Direction.North; targetRail = cart.Location.N; } else if (cart.Location.S != null) { cart.Facing = Direction.South; targetRail = cart.Location.S; }
                        } else if (cart.Facing == Direction.South) {
                            if (cart.Location.S != null) { targetRail = cart.Location.S; } else if (cart.Location.E != null) { cart.Facing = Direction.East; targetRail = cart.Location.E; } else if (cart.Location.W != null) { cart.Facing = Direction.West; targetRail = cart.Location.W; }
                        } else if (cart.Facing == Direction.West) {
                            if (cart.Location.W != null) { targetRail = cart.Location.W; } else if (cart.Location.S != null) { cart.Facing = Direction.South; targetRail = cart.Location.S; } else if (cart.Location.N != null) { cart.Facing = Direction.North; targetRail = cart.Location.N; }
                        }
                    }

                    // move cart to target rail
                    cart.NextLocation = targetRail;
                    cartsToReAdd.Add(cart);

                    newCarts.Remove(cart.Index);
                    var previousLocation = cart.Location;
                    var newCart = new Cart { Location = cart.NextLocation, Facing = cart.Facing, Crashed = cart.Crashed, NextIntersectionDirection = cart.NextIntersectionDirection, Id = cart.Id, NextLocation = null };
                    try {
                        newCarts.Add(newCart.Index, newCart);
                    } catch (ArgumentException ex) {
                        if (ex.Message.StartsWith("An item with the same key has already been added.")) {
                            // Console.WriteLine($"Crash at X={cart.Location.X},Y={cart.Location.Y}");
                            newCarts[newCart.Index].Crashed = true;
                            newCarts.Remove(newCart.Index);
                        }
                    }
                }

                carts = newCarts;

                if (pRender) { Render(carts, null, camera, size, lines); }

                if (carts.Count == 1) {
                    var aloneCart = carts.Single().Value;
                    if (pRender) {
                        Console.SetCursorPosition(0, 0);
                    }
                    Console.WriteLine($"Cart {aloneCart.Id} at X={aloneCart.Location.X},Y={aloneCart.Location.Y}, Facing={aloneCart.Facing.ToString()} on second {seconds}.");
                    return;
                }
            }
        }

        static void Render(Dictionary<int, Cart> pCarts, Cart pFollowCart, Point pCamera, Size pSize, string[] pLines) {
            var carts = pCarts;
            var camera = pCamera;
            var size = pSize;
            var lines = pLines;
            var followCart = pFollowCart;

            foreach (var cart in carts.Values) {
                if ((followCart == null && cart.Id == 6) || (followCart != null && cart.Id == followCart.Id)) {
                    camera.X = cart.Location.X - (size.Width / 2);
                    camera.Y = cart.Location.Y - (size.Height / 2);

                    followCart = cart;

                    if (camera.X < 0) camera.X = 0;
                    if (camera.Y < 0) camera.Y = 0;
                    if (camera.X > map.Size.Width - size.Width) camera.X = map.Size.Width - size.Width;
                    if (camera.Y > map.Size.Height - size.Height) camera.Y = map.Size.Height - size.Height;
                }
            }

            for (int y = camera.Y; y < camera.Y + size.Height; y++) {
                var line = lines[y];
                Console.SetCursorPosition(0, y - camera.Y);
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine(line.Substring(camera.X, size.Width));

                Console.ForegroundColor = ConsoleColor.Red;
                for (int x = camera.X; x < camera.X + size.Width; x++) {
                    foreach (var cart in carts.Values) {
                        if (cart.Location.X == x && cart.Location.Y == y) {
                            Console.SetCursorPosition(x - camera.X, y - camera.Y);
                            switch (cart.Facing) {
                                case Direction.North: Console.Write("^"); break;
                                case Direction.West: Console.Write("<"); break;
                                case Direction.East: Console.Write(">"); break;
                                case Direction.South: Console.Write("v"); break;
                            }
                        }
                    }
                }
            }

            Thread.Sleep(200);
        }

        static Rail LookForRail(Map pMap, int pX, int pY) {
            if (pY >= 0 && pY < pMap.Rails.GetLength(1) && pX >= 0) {
                return pMap.Rails[pX, pY];
            }
            return null;
        }
    }
}
