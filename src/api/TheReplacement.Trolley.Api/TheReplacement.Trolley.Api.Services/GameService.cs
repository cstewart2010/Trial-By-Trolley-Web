using MongoDB.Driver;
using System;
using TheReplacement.Trolley.Api.Services.Abstractions;
using TheReplacement.Trolley.Api.Services.Enums;
using TheReplacement.Trolley.Api.Services.Models;

namespace TheReplacement.Trolley.Api.Services
{
    public class GameService : BaseService
    {
        private readonly IMongoCollection<Game> _games;
        private readonly IMongoCollection<Decks> _decks;

        private GameService()
        {
            _games = GetMongoCollection<Game>(MongoCollection.Games);
            _decks = GetMongoCollection<Decks>(MongoCollection.Cards);
        }

        static GameService()
        {
            Singleton = new GameService();
        }

        public static GameService Singleton { get; private set; }

        public Game GetGame(Guid id)
        {
            return _games.Find(game => game.GameId == id).SingleOrDefault() ?? throw new Exception($"Game {id} not found");
        }

        public bool CreateNewGame(Game game, out string error)
        {
            error = "";
            try
            {
                var decks = _decks.Find(x => x._id == "cards").Single();
                game.InnocentDeck = GetShuffledDeck(decks.InnocentDeck);
                game.ModifierDeck = GetShuffledDeck(decks.ModifierDeck);
                game.GuiltyDeck = GetShuffledDeck(decks.GuiltyDeck);
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
            if (!(game.HostId == Guid.Empty && game.PlayerIds.Contains(hostId)))
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

        public bool RemovePlayer(Game game, Guid playerId)
        {
            game.PlayerIds.Remove(playerId);
            return UpdateGameIsAcknowledged(game);
        }

        public bool DiscardTrack(Game game)
        {
            game.DiscardedCards.AddRange(game.Track.LeftTrack);
            game.DiscardedCards.AddRange(game.Track.RightTrack);
            game.Track = new Track();
            return UpdateGameIsAcknowledged(game);
        }

        public bool PlayCard(Game game, BaseCard card, bool isLeft)
        {
            if (isLeft)
            {
                game.Track.LeftTrack.Add(card);
            }
            else
            {
                game.Track.RightTrack.Add(card);
            }

            return UpdateGameIsAcknowledged(game);
        }

        public bool DealToTeam(Game game)
        {
            var players = PlayerService.Singleton.GetPlayers(game);
            var isSuccessful = players.Aggregate(true, (current, player) =>
            {
                var hand = player.Hand;
                hand.InnocentCards.Add(new InnocentCard
                {
                    ImageId = game.InnocentDeck.Pop()
                });
                hand.ModifierCards.Add(new ModifierCard
                {
                    ImageId = game.ModifierDeck.Pop()
                });
                hand.GuiltyCards.Add(new GuiltyCard
                {
                    ImageId = game.GuiltyDeck.Pop()
                });
                return current & PlayerService.Singleton.UpdateHand(player, hand) & UpdateGameIsAcknowledged(game);
            });

            return isSuccessful;
        }

        public bool ShuffleDeck(Game game)
        {
            game.InnocentDeck = GetDiscardedCardsIntoShuffledDeck(game, game.InnocentDeck, CardType.Innocent);
            game.ModifierDeck = GetDiscardedCardsIntoShuffledDeck(game, game.ModifierDeck, CardType.Modifier);
            game.GuiltyDeck = GetDiscardedCardsIntoShuffledDeck(game, game.GuiltyDeck, CardType.Guilty);
            game.DiscardedCards = new List<BaseCard>();

            return UpdateGameIsAcknowledged(game);
        }

        private static Stack<int> GetDiscardedCardsIntoShuffledDeck(
            Game game,
            Stack<int> deckStack,
            CardType deckType)
        {
            var deck = deckStack.ToList();
            deck.AddRange(game.DiscardedCards.Where(card => card.Type == deckType).Select(card => card.ImageId));
            return GetShuffledDeck(deck);
        }

        private static Stack<int> GetShuffledDeck(List<int> deck)
        {
            var random = new Random();
            var shuffledDeck = new Stack<int>();
            while (deck.Any())
            {
                var index = random.Next(deck.Count);
                shuffledDeck.Push(deck[index]);
                deck.RemoveAt(index);
            }

            return shuffledDeck;
        }

        public bool AddToDiscussion(Game game, Guid playerId, string message)
        {
            var player = PlayerService.Singleton.GetPlayer(playerId);
            var item = new DiscussionItem
            {
                Message = message,
                Name = player.Name,
                Timestamp = DateTime.Now
            };
            game.Discussion.Add(item);

            return UpdateGameIsAcknowledged(game);
        }

        public bool DeleteGame(Game gameToDelete)
        {
            var gameDeletionResult = _games.DeleteOne(game => game.GameId == gameToDelete.GameId).IsAcknowledged;
            var trainerDeletionResult = gameToDelete.PlayerIds.Aggregate(true, (current, playerId) =>
            {
                return current & PlayerService.Singleton.DeletePlayer(playerId);
            });

            return gameDeletionResult && trainerDeletionResult;
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
