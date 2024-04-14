using MongoDB.Driver;
using TheReplacement.Trolley.Api.Services.Abstractions;
using TheReplacement.Trolley.Api.Services.Enums;
using TheReplacement.Trolley.Api.Services.Models;

namespace TheReplacement.Trolley.Api.Services
{
    public class GameService : BaseService
    {
        private readonly IMongoCollection<Game> _games;

        private GameService()
        {
            _games = GetMongoCollection<Game>(MongoCollection.Games);
        }

        static GameService()
        {
            Singleton = new GameService();
        }

        public static GameService Singleton { get; private set; }

        public Game GetGame(Guid id)
        {
            return _games.Find(game => game.GameId == id).SingleOrDefault()?? throw new Exception($"Game {id} not found");
        }

        public bool CreateNewGame(Game game, out string error)
        {
            error = "";
            try
            {
                game.LastAction = DateTime.Now;
                _games.InsertOne(game);
                return true;
            }
            catch (MongoWriteException ex)
            {
                error = ex.WriteError.Details.GetValue("details").AsBsonDocument.ToString();
                return false;
            }
        }

        public bool SetHost(Guid gameId, Guid hostId)
        {
            var game = GetGame(gameId);
            if (game.HostId != Guid.Empty)
            {
                return false;
            }

            game.HostId = hostId;
            return UpdateGameIsAcknowledged(game);
        }

        public bool AddPlayer(Guid gameId, Guid playerId)
        {
            var game = GetGame(gameId);
            game.PlayerIds.Add(playerId);
            return UpdateGameIsAcknowledged(game);
        }

        public bool RemovePlayer(Guid gameId, Guid playerId)
        {
            var game = GetGame(gameId);
            game.PlayerIds.Remove(playerId);
            return UpdateGameIsAcknowledged(game);
        }

        private bool UpdateGameIsAcknowledged(Game updatedGame)
        {
            updatedGame.LastAction = DateTime.Now;
            var result = _games.ReplaceOne
            (
                game => game.GameId == updatedGame.GameId,
                options: new ReplaceOptions { IsUpsert = true },
                replacement: updatedGame
            );

            return result.IsAcknowledged;
        }
    }
}
