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

        /// <summary>
        /// Checks if one of the players won
        /// </summary>
        /// <returns>playerId if someone won, -1 if no one did</returns>
        public int CheckVictory()
        {
            return -1;
        }

    }
}