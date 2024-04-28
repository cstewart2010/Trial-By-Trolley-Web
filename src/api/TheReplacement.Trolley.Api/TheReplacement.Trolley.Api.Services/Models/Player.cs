using MongoDB.Bson;

namespace TheReplacement.Trolley.Api.Services.Models
{
    public class Player
    {
        public ObjectId _id { get; set; }
        public Guid PlayerId { get; set; }
        public Guid GameId { get; set; }
        public string Name { get; set; }
        public bool IsConductor { get; set; }
        public int RoundsWon { get; set; }
        public bool IsHost { get; set; }
        public PlayerHand Hand { get; set; } = new PlayerHand();
    }
}
