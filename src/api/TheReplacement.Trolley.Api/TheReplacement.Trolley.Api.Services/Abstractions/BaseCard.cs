using TheReplacement.Trolley.Api.Services.Enums;

namespace TheReplacement.Trolley.Api.Services.Abstractions
{
    public abstract class BaseCard
    {
        public string Text { get; set; }
        public string ImageId { get; set; }
        public bool IsSuggested { get; set; }
        public abstract CardType Type { get; internal set; }
    }
}
