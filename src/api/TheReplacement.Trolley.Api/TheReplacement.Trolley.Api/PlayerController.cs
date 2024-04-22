using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.OpenApi.Models;
using System.Net;
using TheReplacement.Trolley.Api.Services.Models;
using TheReplacement.Trolley.Api.Services;
using TheReplacement.Trolley.Api.Client.Models;
using TheReplacement.Trolley.Api.Client.Extensions;
using TheReplacement.Trolley.Api.Services.Enums;
using System.IO;

namespace TheReplacement.Trolley.Api.Client
{
    public class PlayerController
    {
        private readonly ILogger<PlayerController> _logger;

        public PlayerController(ILogger<PlayerController> log)
        {
            _logger = log;
        }

        [FunctionName(nameof(GetPlayer))]
        [OpenApiOperation(operationId: nameof(GetPlayer), tags: nameof(Player))]
        [OpenApiParameter(name: "playerId", In = ParameterLocation.Path, Required = true, Type = typeof(Guid), Description = "The **GameId** parameter")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Player), Description = "The OK response")]
        public IActionResult GetPlayer(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "player/{playerId}")] HttpRequest request,
            Guid playerId)
        {
            var player = PlayerService.Singleton.GetPlayer(playerId);
            return new OkObjectResult(player);
        }

        [FunctionName(nameof(GetPlayers))]
        [OpenApiOperation(operationId: nameof(GetPlayers), tags: nameof(Player))]
        [OpenApiParameter(name: "gameId", In = ParameterLocation.Path, Required = true, Type = typeof(Guid), Description = "The **GameId** parameter")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Player[]), Description = "The OK response")]
        public IActionResult GetPlayers(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "player/all/{gameId}")] HttpRequest request,
            Guid gameId)
        {
            var game = GameService.Singleton.GetGame(gameId);
            var players = PlayerService.Singleton.GetPlayers(game);
            return new OkObjectResult(players);
        }

        [FunctionName(nameof(CreatePlayer))]
        [OpenApiOperation(operationId: nameof(CreatePlayer), tags: nameof(Player))]
        [OpenApiRequestBody("application/json", typeof(CreatePlayerForm), Required = true)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Player), Description = "The OK response")]
        public IActionResult CreatePlayer(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "player")] HttpRequest request)
        {
            var form = request.DeserializeBody<CreatePlayerForm>();
            var player = new Player
            {
                PlayerId = Guid.NewGuid(),
                IsHost = form.IsHost,
                Name = form.Name
            };

            var result = PlayerService.Singleton.CreatePlayer(player, out var error);
            if (!result)
            {
                _logger.LogInformation(error);
                return new BadRequestObjectResult(error);
            }

            return new OkObjectResult(player);
        }

        [FunctionName(nameof(SetGameId))]
        [OpenApiOperation(operationId: nameof(SetGameId), tags: nameof(Player))]
        [OpenApiRequestBody("application/json", typeof(Guid), Required = true)]
        [OpenApiParameter(name: "playerId", In = ParameterLocation.Path, Required = true, Type = typeof(Guid), Description = "The **PlayerId** parameter")]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public IActionResult SetGameId(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "player/join/{playerId}")] HttpRequest request,
            Guid playerId)
        {
            var player = PlayerService.Singleton.GetPlayer(playerId);
            var reader = new StreamReader(request.Body);
            var json = reader.ReadToEnd();
            var gameId = Guid.Parse(json);

            var result = PlayerService.Singleton.UpdateGameId(player, gameId);
            if (!result)
            {
                var error = "Failed to set gameId";
                _logger.LogInformation(error);
                return new BadRequestObjectResult(error);
            }

            return new OkResult();
        }

        [FunctionName(nameof(SuggestCard))]
        [OpenApiOperation(operationId: nameof(SuggestCard), tags: nameof(Player))]
        [OpenApiRequestBody("application/json", typeof(PlayCardForm), Required = true)]
        [OpenApiParameter(name: "playerId", In = ParameterLocation.Path, Required = true, Type = typeof(Guid), Description = "The **PlayerId** parameter")]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public IActionResult SuggestCard(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "player/suggest/{playerId}")] HttpRequest request,
            Guid playerId)
        {
            var player = PlayerService.Singleton.GetPlayer(playerId);
            var form = request.DeserializeBody<PlayCardForm>();
            var hand = GetSuggestedPlayerHand(player, form);
            var result = PlayerService.Singleton.UpdateHand(player, hand);
            if (!result)
            {
                var error = "Failed to update hand";
                _logger.LogInformation(error);
                return new BadRequestObjectResult(error);
            }

            return new OkResult();
        }

        [FunctionName(nameof(LeaveGame))]
        [OpenApiOperation(operationId: nameof(LeaveGame), tags: nameof(Player))]
        [OpenApiParameter(name: "playerId", In = ParameterLocation.Path, Required = true, Type = typeof(Guid), Description = "The **PlayerId** parameter")]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public IActionResult LeaveGame(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "player/leave/{playerId}")] HttpRequest request,
            Guid playerId)
        {
            var result = PlayerService.Singleton.DeletePlayer(playerId);
            if (!result)
            {
                var error = "Failed to delete player";
                _logger.LogInformation(error);
                return new BadRequestObjectResult(error);
            }

            return new OkResult();
        }

        private static PlayerHand GetSuggestedPlayerHand(Player player, PlayCardForm form)
        {
            int index;
            switch (form.Type)
            {
                case CardType.Innocent:
                    index = player.Hand.InnocentCards.FindIndex(card => card.ImageId == form.ImageId);
                    player.Hand.InnocentCards[index].IsSuggested = form.IsSuggested;
                    break;
                case CardType.Guilty:
                    index = player.Hand.GuiltyCards.FindIndex(card => card.ImageId == form.ImageId);
                    player.Hand.GuiltyCards[index].IsSuggested = form.IsSuggested;
                    break;
                case CardType.Modifier:
                    index = player.Hand.ModifierCards.FindIndex(card => card.ImageId == form.ImageId);
                    player.Hand.ModifierCards[index].IsSuggested = form.IsSuggested;
                    break;
            }

            return player.Hand;
        }
    }
}
