using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using SignalR;
using SignalR.Hubs;

namespace TrippleT.Web
{
    public class TickTacToe : Hub, IDisconnect
    {
        private Random _random = new Random();

        /// <summary>
        /// Called by JS when player sets mark on field
        /// </summary>
        /// <param name="x"> x coordinate</param>
        /// <param name="y">x coordinate</param>
        /// <param name="z">x coordinate</param>
        public void Move(int x, int y, int z)
        {
            var playerId = this.Context.ConnectionId;

            var game = GameRepository.Current.GetGameByPlayer(playerId);
        
            //don't update if move  was not valid
            if (game.Put(playerId, x, y, z))
            {
                foreach (var player in game.Players)
                {
                    Clients[player].UpdateMove(playerId, x, y, z);
                }
            }
            var win = game.CheckVictory(x, y, z);
            if (win != null)
            {
                foreach (var player in game.Players)
                {
                    Clients[player].Win(win);
                }
            }
        }

        /// <summary>
        /// Called when one player offers a game to other
        /// </summary>
        /// <param name="playerId">whom to offer the game</param>
        public void MakeOffer(string playerId)
        {

            var player2Id = this.Context.ConnectionId;
            Clients[playerId].Offer(player2Id);
            PlayerRepository.Current.AddOffer(playerId, player2Id);
        }

        /// <summary>
        /// Called when player accepts game offer
        /// </summary>
        public void Accept()
        {

            var playerId = this.Context.ConnectionId;
            var offer = PlayerRepository.Current.GetOffer(playerId);
            if (offer == null) return; // offer was canceled
            PlayerRepository.Current.RemoveOffer(playerId);
            GameRepository.Current.NewGame(playerId, offer, 4);
            if (_random.NextDouble() > 0.5)
            {
                Clients.Game(offer, playerId);
            }
            else
            {
                Clients.Game(playerId, offer);
            }

        }
        /// <summary>
        /// Called when player declines game offer
        /// </summary>
        public void Decline()
        {
            PlayerRepository.Current.RemoveOffer(this.Context.ConnectionId);
        }

        /// <summary>
        /// Called when player pushes "let me in" button
        /// </summary>
        /// <param name="name">player display name</param>
        public void Register(string name)
        {
            var playerId = this.Context.ConnectionId;

            if (!PlayerRepository.Current.Enter(playerId, name)) return;
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
        /// <summary>
        /// Called when player want's to leave current game
        /// </summary>
        public void LeaveGame()
        {
            
            var playerId = this.Context.ConnectionId;
            
            if (GameRepository.Current.Destroy(playerId))
                Clients.LeftGame(playerId);
        }

        /// <summary>
        /// Called when client disconnect detected
        /// </summary>
        /// <returns></returns>
        public Task Disconnect()
        {
            var playerId = Context.ConnectionId;

            var g = GameRepository.Current.GetGameByPlayer(playerId);
            Clients.Left(playerId);
            GameRepository.Current.Destroy(playerId);
            PlayerRepository.Current.Exit(playerId);

            return null;
        }
    }
}