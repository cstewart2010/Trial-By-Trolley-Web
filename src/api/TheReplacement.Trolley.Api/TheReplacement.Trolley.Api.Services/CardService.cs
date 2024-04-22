using MongoDB.Driver;
using TheReplacement.Trolley.Api.Services.Abstractions;
using TheReplacement.Trolley.Api.Services.Enums;
using TheReplacement.Trolley.Api.Services.Models;

namespace TheReplacement.Trolley.Api.Services
{
    public class CardService : BaseService
    {
        private readonly IMongoCollection<GuiltyCard> _guiltyCards;
        private readonly IMongoCollection<InnocentCard> _innocentCards;
        private readonly IMongoCollection<ModifierCard> _modifierCards;

        private CardService()
        {
            _guiltyCards = GetMongoCollection<GuiltyCard>(MongoCollection.Cards);
            _innocentCards = GetMongoCollection<InnocentCard>(MongoCollection.Cards);
            _modifierCards = GetMongoCollection<ModifierCard>(MongoCollection.Cards);
        }

        static CardService()
        {
            Singleton = new CardService();
        }

        public static CardService Singleton { get; private set; }

        public BaseCard GetCard(string imageId, CardType cardType)
        {
            return cardType switch
            {
                CardType.Innocent => _innocentCards.Find(card => card.ImageId == imageId).SingleOrDefault(),
                CardType.Guilty => _guiltyCards.Find(card => card.ImageId == imageId).SingleOrDefault(),
                CardType.Modifier => _modifierCards.Find(card => card.ImageId == imageId).SingleOrDefault(),
                _ => throw new ArgumentOutOfRangeException(nameof(cardType)),
            };
        }
    }
}
