using MongoDB.Bson;
using TheReplacement.Trolley.Api.Services.Abstractions;

namespace TheReplacement.Trolley.Api.Services.Models
{
    public class Game
    {
        public ObjectId _id { get; set; }
        public Guid GameId { get; set; }
        public Guid HostId { get; set; }
        public DateTime Initialization { get; set; }
        public List<Guid> PlayerIds { get; set; } = new List<Guid>();
        public Stack<int> InnocentDeck { get; set; } = new Stack<int>();
        public Stack<int> GuiltyDeck { get; set; } = new Stack<int>();
        public Stack<int> ModifierDeck { get; set; } = new Stack<int>();
        public int RoundNumber { get; set; }
        public List<BaseCard> DiscardedCards { get; set; } = new List<BaseCard>();
        public List<DiscussionItem> Discussion { get; set; } = new List<DiscussionItem>();
        public Track Track { get; set; } = new Track();
        public DateTime LastAction { get; set; }
    }
}
