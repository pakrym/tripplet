using System.Collections.Generic;

namespace TrippleT.Web
{
    public class GameRepository
    {
        static GameRepository()
        {
            Current = new GameRepository();
        }

        private Dictionary<string, Game> _games;

        public GameRepository()
        {
            _games = new Dictionary<string, Game>();
        }

        public static GameRepository Current { get; private set; }

        public void NewGame(string pa, string pb, int size)
        {
            var game = new Game(pa, pb, size);
            if (_games.ContainsKey(pa)) _games.Remove(pa);
            if (_games.ContainsKey(pb)) _games.Remove(pb);

            _games.Add(pa, game);
            _games.Add(pb, game);
        }

        public Game GetGameByPlayer(string player)
        {
            Game g;
            if (_games.TryGetValue(player, out g))
            {
                return g;
            }
            return null;
        }

        public bool Destroy(string player)
        {
            var game = GetGameByPlayer(player);

            var res = false;
            if (game != null)
            {
                foreach (var p in game.Players)
                {
                    res |= _games.Remove(p);
                }
            }
            return res;
        }

    }
}