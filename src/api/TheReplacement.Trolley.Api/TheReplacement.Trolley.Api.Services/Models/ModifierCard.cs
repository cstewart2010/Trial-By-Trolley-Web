using TheReplacement.Trolley.Api.Services.Abstractions;
using TheReplacement.Trolley.Api.Services.Enums;

namespace TheReplacement.Trolley.Api.Services.Models
{
    public class ModifierCard : BaseCard
    {
        public override CardType Type { get; internal set; } = CardType.Modifier;
    }
}
