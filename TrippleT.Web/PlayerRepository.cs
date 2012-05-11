using System.Collections.Generic;

namespace TrippleT.Web
{
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
}