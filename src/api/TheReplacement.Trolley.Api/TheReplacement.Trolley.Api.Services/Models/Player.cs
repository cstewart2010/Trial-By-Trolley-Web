namespace TheReplacement.Trolley.Api.Services.Models
{
    public class Player
    {
        public Guid Id { get; set; }
        public Guid GameId { get; set; }
        public string Name { get; set; }
        public bool IsHost { get; set; }
        public PlayerHand Hand { get; set; }
    }
}
