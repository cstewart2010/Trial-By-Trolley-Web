using MongoDB.Bson;

namespace TheReplacement.Trolley.Api.Services.Models
{
    public class Game
    {
        public ObjectId _id { get; set; }
        public Guid GameId { get; set; }
        public Guid HostId { get; set; }
        public DateTime Initialization { get; set; }
        public List<Guid> PlayerIds { get; set; } = new List<Guid>();
        public List<InnocentCard> InnocentDeck { get; set; } = new List<InnocentCard>();
        public List<GuiltyCard> GuiltyDeck { get; set; } = new List<GuiltyCard>();
        public List<ModifierCard> ModifierDeck { get; set; } = new List<ModifierCard>();
        public DateTime LastAction { get; set; }
    }
}
