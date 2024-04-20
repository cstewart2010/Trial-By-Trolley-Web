using TheReplacement.Trolley.Api.Services.Abstractions;

namespace TheReplacement.Trolley.Api.Services.Models
{
    public class Track
    {
        public List<BaseCard> LeftTrack { get; set; } = new List<BaseCard>();
        public List<BaseCard> RightTrack { get; set; } = new List<BaseCard>();
    }
}
