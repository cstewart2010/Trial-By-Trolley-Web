using MongoDB.Bson;

namespace TheReplacement.Trolley.Api.Services.Models
{
    internal class Decks
    {
        public string _id { get; set; } = "cards";
        public List<int> InnocentDeck { get; set; } = new List<int>();
        public List<int> GuiltyDeck { get; set; } = new List<int>();
        public List<int> ModifierDeck { get; set; } = new List<int>();
    }
}
