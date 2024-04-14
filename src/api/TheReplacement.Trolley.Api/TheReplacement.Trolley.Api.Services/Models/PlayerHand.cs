namespace TheReplacement.Trolley.Api.Services.Models
{
    public class PlayerHand
    {
        public List<InnocentCard> InnocentCards { get; set; }
        public List<GuiltyCard> GuiltyCards { get; set; }
        public List<ModifierCard> ModifierCards { get; set; }
    }
}
