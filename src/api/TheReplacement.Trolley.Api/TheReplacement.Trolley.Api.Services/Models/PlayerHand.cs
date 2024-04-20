namespace TheReplacement.Trolley.Api.Services.Models
{
    public class PlayerHand
    {
        public List<InnocentCard> InnocentCards { get; set; } = new List<InnocentCard>();
        public List<GuiltyCard> GuiltyCards { get; set; } = new List<GuiltyCard>();
        public List<ModifierCard> ModifierCards { get; set; } = new List<ModifierCard>();
    }
}
