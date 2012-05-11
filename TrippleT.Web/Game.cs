using System;

namespace TrippleT.Web
{
    public class Game
    {
        private readonly int _fieldSize;

        public Game(string playera, string playerb, int fieldSize)
        {
            _fieldSize = fieldSize;
            Players = new[] { playera, playerb };
            Field = new byte[fieldSize, fieldSize, fieldSize];
        }

        public string[] Players { get; set; }
        public byte[, ,] Field { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="playerId"></param>
        /// <param name="x">tile x coordinate</param>
        /// <param name="y">tile y coordinate</param>
        /// <param name="z">tile z coordinate</param>
        /// <returns></returns>
        public bool Put(string playerId, int x, int y, int z)
        {
            if ((x < 0) || (x >= _fieldSize) ||
                (y < 0) || (y >= _fieldSize) ||
                (z < 0) || (z >= _fieldSize))
            {
                return false;
            }
            if (Field[x, y, z] != 0) return false; // tile already taken

            var pi = Array.IndexOf(Players, playerId);
            if (pi == -1) // player is not from this game, hm...
            {
                return false;
            }

            Field[x, y, z] = (byte)(pi + 1); // zero is nothing
            return true;
        }



        private class Point
        {
            public int X { get; set; }
            public int Y { get; set; }
            public int Z { get; set; }

            public Point(int x, int y, int z)
            {
                X = x;
                Y = y;
                Z = z;
            }

            public Point Move(int dx, int dy, int dz, int s)
            {
                if ((X + dx >= 0 && X + dx < s)
                    && (Y + dy >= 0 && Y + dy < s)
                    && (Z + dz >= 0 && Z + dz < s))
                {

                    return new Point(X + dx, Y + dy, Z + dz);
                }
                return null;
            }

            // only works if points are on one line
            public int Distance(Point other)
            {
                return 1 + Math.Max(
                    other.X - this.X, 
                    Math.Max(
                        other.Y - this.Y, 
                        other.Z - this.Z));
            }
        }
        /// <summary>
        /// Checks if one of the players won
        /// </summary>
        /// <returns>playerId if someone won, -1 if no one did</returns>
        public string CheckVictory(int x, int y, int z)
        {
            var who = Field[x, y, z];
            if (who == 0) return null;
            var ways = new[] { -1, 0, 1 };
            foreach (var wx in ways)
            {
                foreach (var wy in ways)
                {
                    foreach (var wz in ways)
                    {
                        if (wx == 0 && wy == 0 && wz == 0)
                            continue;

                        Point pa = new Point(x, y, z);

                        Point pb = new Point(x, y, z);


                        Point newA;
                        while ((newA = pa.Move(wx, wy, wz, _fieldSize)) != null
                               && Field[newA.X, newA.Y, newA.Z] == who)
                        {
                            pa = newA;
                        }

                        Point newB;
                        while ((newB = pb.Move(-wx, -wy, -wz, _fieldSize)) != null
                               && Field[newB.X, newB.Y, newB.Z] == who)
                        {
                            pb = newB;
                        }

                        if (pa.Distance(pb) == _fieldSize)
                        {
                            return Players[who - 1];
                        }


                    }
                }
            }
            return null;
        }


    }
}