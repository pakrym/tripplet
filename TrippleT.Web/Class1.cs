using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using SignalR;
using SignalR.Hubs;

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

    public class GameProvider
    {
        static GameProvider()
        {
            Current = new GameProvider();
        }

        private Dictionary<string, Game> _games;

        public GameProvider()
        {
            _games = new Dictionary<string, Game>();
        }

        public static GameProvider Current { get; private set; }

        public void NewGame(string pa, string pb, int size)
        {
            var game = new Game(pa, pb, size);
            if (_games.ContainsKey(pa)) _games.Remove(pa);
            if (_games.ContainsKey(pb)) _games.Remove(pb);

            _games.Add(pa, game);
            _games.Add(pb, game);
        }

        public Game Get(string player)
        {
            Game g;
            if (_games.TryGetValue(player, out g))
            {
                return g;
            }
            return null;
        }

        public void Destroy(string player)
        {
            var game = Get(player);
            if (game != null)
            {
                foreach (var p in game.Players)
                {
                    _games.Remove(p);
                }
            }
        }

    }


    public class Player
    {
        public String Name { get; set; }
        public String PlayerId { get; set; }
    }

    public class PlayerRepository
    {
        static PlayerRepository()
        {
            Current = new PlayerRepository();

        }

        public static PlayerRepository Current { get; private set; }
        private Dictionary<string, string> _names;
        private Dictionary<string, string> _offers;

        public PlayerRepository()
        {
            _names = new Dictionary<string, string>();
            _offers = new Dictionary<string, string>();
        }

        public void Enter(string playerId, string name)
        {
            if (_names.ContainsKey(playerId)) _names.Remove(playerId);
            _names.Add(playerId, name);
        }

        public void Exit(string playerId)
        {
            _names.Remove(playerId);
        }

        public void AddOffer(string playerA, string playerB)
        {
            if (!_offers.ContainsKey(playerA))// dont ovewrrite current offer
                _offers.Add(playerA, playerB);
        }

        public void RemoveOffer(string playerId)
        {
            _offers.Remove(playerId);
        }

        public string GetOffer(string playerId)
        {
            string playerB;
            if (_offers.TryGetValue(playerId, out playerB))
                return playerB;
            return null;

        }

        public IEnumerable<KeyValuePair<string, string>> AllNames { get { return _names; } }
    }

    public class TickTacToe : Hub, IDisconnect
    {
        private Random _random = new Random();
        public void Move(int x, int y, int z)
        {
            var playerId = this.Context.ConnectionId;

            var game = GameProvider.Current.Get(playerId);
            //don't update is move  was not valid
            if (game.Put(playerId, x, y, z))
            {
                foreach (var player in game.Players)
                {
                    Clients[player].UpdateMove(playerId, x, y, z);
                }
            }
        }

        public void MakeOffer(string playerId)
        {

            var player2Id = this.Context.ConnectionId;
            Clients[playerId].Offer(player2Id);
            PlayerRepository.Current.AddOffer(playerId, player2Id);
        }

        public void Accept()
        {

            var playerId = this.Context.ConnectionId;
            var offer = PlayerRepository.Current.GetOffer(playerId);
            if (offer == null) return; // offer was canceled
            PlayerRepository.Current.RemoveOffer(playerId);
            GameProvider.Current.NewGame(playerId, offer, 4);
            if (_random.NextDouble() > 0.5)
            {
                Clients.Game(offer, playerId);
            }
            else
            {
                Clients.Game(playerId, offer);
            }

        }

        public void Decline()
        {
            PlayerRepository.Current.RemoveOffer(this.Context.ConnectionId);
        }

        public void Register(string name)
        {
            var playerId = this.Context.ConnectionId;

            PlayerRepository.Current.Enter(playerId, name);
            Clients.Enter(playerId, name);

            // DIRTY HACK BELOW
            // send all online player names using Enter method
            // skip just loged in player name, we've already sent it
            var c = Clients[playerId];
            foreach (var n in PlayerRepository.Current.AllNames)
            {
                if (n.Key == playerId) continue;
                c.Enter(n.Key, n.Value);
            }


        }


        public Task Disconnect()
        {
            var playerId = Context.ConnectionId;

            var g = GameProvider.Current.Get(playerId);
            Clients.Left(playerId);
            GameProvider.Current.Destroy(playerId);
            PlayerRepository.Current.Exit(playerId);

            return null;
        }
    }
}