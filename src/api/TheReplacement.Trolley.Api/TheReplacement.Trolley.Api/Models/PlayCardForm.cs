using TheReplacement.Trolley.Api.Services.Enums;

namespace TheReplacement.Trolley.Api.Client.Models
{
    internal class PlayCardForm
    {
        public string ImageId { get; set; }
        public CardType Type { get; set; }
        public bool IsLeftTrack { get; set; }
        public bool IsSuggested { get; set; }
    }
}
