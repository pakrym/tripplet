using System.Collections.Generic;

namespace TrippleT.Web
{
    public class PlayerRepository
    {
        static PlayerRepository()
        {
            Current = new PlayerRepository();

        }
        /// <summary>
        /// Singleton holding PlayerRepository for current application
        /// </summary>
        public static PlayerRepository Current { get; private set; }
        
        private Dictionary<string, string> _names;
        private Dictionary<string, string> _offers;

        public PlayerRepository()
        {
            _names = new Dictionary<string, string>();
            _offers = new Dictionary<string, string>();
        }

        /// <summary>
        ///  Register player in game
        /// </summary>
        /// <param name="playerId">player id</param>
        /// <param name="name">player name</param>
        /// <returns>true if login successed, false if name was already taken</returns>
        public bool Enter(string playerId, string name)
        {
            if (_names.ContainsKey(playerId)) return false;
            _names.Add(playerId, name);
            return true;
        }

        /// <summary>
        /// Unregister user from game
        /// </summary>
        /// <param name="playerId">player id</param>
        public void Exit(string playerId)
        {
            _names.Remove(playerId);
        }

        /// <summary>
        /// Adds offer from playerA to playerB
        /// </summary>
        /// <param name="playerA">from player id</param>
        /// <param name="playerB">to player id</param>
        public void AddOffer(string playerA, string playerB)
        {
            if (!_offers.ContainsKey(playerA))// dont ovewrrite current offer
                _offers.Add(playerA, playerB);
        }

        /// <summary>
        /// Removes offer
        /// </summary>
        /// <param name="playerId">player id of player offer was made to</param>
        public void RemoveOffer(string playerId)
        {
            _offers.Remove(playerId);
        }

        /// <summary>
        /// Gets player id of player who made the offer
        /// </summary>
        /// <param name="playerId">player id of player offer was made to</param>
        public string GetOffer(string playerId)
        {
            string playerB;
            if (_offers.TryGetValue(playerId, out playerB))
                return playerB;
            return null;

        }
        /// <summary>
        /// Return all player ids along with names
        /// </summary>
        public IEnumerable<KeyValuePair<string, string>> AllNames { get { return _names; } }
    }
}