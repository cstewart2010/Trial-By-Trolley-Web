using MongoDB.Driver;
using TheReplacement.Trolley.Api.Services.Enums;
using TheReplacement.Trolley.Api.Services.Models;
using TheReplacement.Trolley.Api.Services.Abstractions;

namespace TheReplacement.Trolley.Api.Services
{
    public class PlayerService : BaseService
    {
        private readonly IMongoCollection<Player> _players;

        private PlayerService()
        {
            _players = GetMongoCollection<Player>(MongoCollection.Players);
        }

        static PlayerService()
        {
            Singleton = new PlayerService();
        }

        public static PlayerService Singleton { get; private set; }

        public Player GetPlayer(Guid playerId)
        {
            return _players.Find(player =>  player.PlayerId == playerId).SingleOrDefault() ?? throw new Exception($"Invalid player {playerId}");
        }

        public List<Player> GetPlayers(Game game)
        {
            return _players.Find(player => game.PlayerIds.Contains(player.PlayerId)).ToList();
        }

        public List<Player> GetTeammates(Game game, Player player)
        {
            return _players.Find(teammate => teammate.Team == player.Team
                && teammate.PlayerId != player.PlayerId
                && teammate.GameId == game.GameId).ToList();
        }

        public bool CreatePlayer(Player player, out string error)
        {
            error = "";
            try
            {
                _players.InsertOne(player);
                return true;
            }
            catch (MongoWriteException ex)
            {
                error = ex.WriteError.Details.GetValue("details").AsBsonDocument.ToString();
                return false;
            }
        }

        public bool UpdateTeam(Guid playerId, Team team)
        {
            var player = GetPlayer(playerId);
            player.Team = team;
            return UpdatedPlayerIsAcknowledged(player);
        }

        public bool UpdateGameId(Player player, Guid gameId)
        {
            player.GameId = gameId;
            return UpdatedPlayerIsAcknowledged(player);
        }

        public bool UpdateHand(Player player, PlayerHand updatedHand)
        {
            player.Hand = updatedHand;
            return UpdatedPlayerIsAcknowledged(player);
        }

        public bool UpdateRoundsWon(Player player)
        {
            player.RoundsWon++;
            return UpdatedPlayerIsAcknowledged(player);
        }

        public bool DeletePlayer(Guid playerId)
        {
            return _players.DeleteOne(player => player.PlayerId == playerId).IsAcknowledged;
        }

        private bool UpdatedPlayerIsAcknowledged(Player updatedPlayer)
        {
            var result = _players.ReplaceOne
            (
                player => player.PlayerId == updatedPlayer.PlayerId,
                options: new ReplaceOptions { IsUpsert = true },
                replacement: updatedPlayer
            );

            return result.IsAcknowledged;
        }
    }
}
