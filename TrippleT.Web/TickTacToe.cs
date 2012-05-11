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
        public void Move(int x, int y, int z)
        {
            var playerId = this.Context.ConnectionId;

            var game = GameRepository.Current.Get(playerId);
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

            var g = GameRepository.Current.Get(playerId);
            Clients.Left(playerId);
            GameRepository.Current.Destroy(playerId);
            PlayerRepository.Current.Exit(playerId);

            return null;
        }
    }
}