using System;
using System.Collections.Generic;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using TheReplacement.Trolley.Api.Client.Extensions;
using TheReplacement.Trolley.Api.Client.Models;
using TheReplacement.Trolley.Api.Services;
using TheReplacement.Trolley.Api.Services.Models;

namespace TheReplacement.Trolley.Api
{
    public class GameController
    {
        private readonly ILogger<GameController> _logger;

        public GameController(ILogger<GameController> log)
        {
            _logger = log;
        }

        [FunctionName("GetGame")]
        [OpenApiOperation(operationId: "GetGame", tags: "Game")]
        [OpenApiParameter(name: "gameId", In = ParameterLocation.Path, Required = true, Type = typeof(Guid), Description = "The **GameId** parameter")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Game), Description = "The OK response")]
        public IActionResult GetGame(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "game/{gameId}")] HttpRequest request,
            Guid gameId)
        {
            var game = GameService.Singleton.GetGame(gameId);
            return new OkObjectResult(game);
        }

        [FunctionName("GetDiscussion")]
        [OpenApiOperation(operationId: "GetDiscussion", tags: "Game")]
        [OpenApiParameter(name: "gameId", In = ParameterLocation.Path, Required = true, Type = typeof(Guid), Description = "The **GameId** parameter")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(List<DiscussionItem>), Description = "The OK response")]
        public IActionResult GetDiscussion(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "game/discussion/{gameId}")] HttpRequest request,
            Guid gameId)
        {
            var game = GameService.Singleton.GetGame(gameId);
            return new OkObjectResult(game.Discussion);
        }

        [FunctionName("CreateNewGame")]
        [OpenApiOperation(operationId: "CreateNewGame", tags: "Game")]
        [OpenApiRequestBody("application/json", typeof(NewGameForm), Required = true, Description = "The Game")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Game), Description = "The OK response")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "text/plain", bodyType: typeof(string), Description = "The Bad response")]
        public IActionResult CreateNewGame(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "game")] HttpRequest request)
        {
            var form = request.DeserializeBody<NewGameForm>();
            var newGame = new Game
            {
                GameId = Guid.NewGuid(),
                Initialization = form.Initialization,
                HostId = form.HostId
            };

            var result = GameService.Singleton.CreateNewGame(newGame, out var error);
            if (!result)
            {
                _logger.LogError(error);
                return new BadRequestObjectResult(error);
            }
            var game = GameService.Singleton.GetGame(newGame.GameId);
            return new OkObjectResult(game);
        }

        [FunctionName("SetHost")]
        [OpenApiOperation(operationId: "SetHost", tags: "Game")]
        [OpenApiParameter(name: "gameId", In = ParameterLocation.Path, Required = true, Type = typeof(Guid), Description = "The **GameId** parameter")]
        [OpenApiRequestBody("application/json", typeof(HostRequestForm), Required = true, Description = "The Game")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.OK, Description = "The OK response")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "text/plain", bodyType: typeof(string), Description = "The Bad response")]
        public IActionResult SetHost(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "game/setHost/{gameId}")] HttpRequest request,
            Guid gameId)
        {
            var form = request.DeserializeBody<HostRequestForm>();
            if (form == null)
            {
                var error = "Failed to deserialize body";
                _logger.LogError(error);
                return new BadRequestObjectResult(error);
            }

            var result = GameService.Singleton.SetHost(gameId, form.HostId);
            if (!result)
            {
                var error = "Failed to set host";
                _logger.LogError(error);
                return new BadRequestObjectResult(error);
            }
            return new OkResult();
        }

        [FunctionName("JoinGame")]
        [OpenApiOperation(operationId: "JoinGame", tags: "Game")]
        [OpenApiParameter(name: "gameId", In = ParameterLocation.Path, Required = true, Type = typeof(Guid), Description = "The **GameId** parameter")]
        [OpenApiRequestBody("application/json", typeof(JoinGameForm), Required = true, Description = "The Game")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.OK, Description = "The OK response")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "text/plain", bodyType: typeof(string), Description = "The Bad response")]
        public IActionResult JoinGame(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "game/join/{gameId}")] HttpRequest request,
            Guid gameId)
        {
            var form = request.DeserializeBody<JoinGameForm>();
            if (form == null || form.PlayerId == Guid.Empty)
            {
                var error = "Failed to deserialize body";
                _logger.LogError(error);
                return new BadRequestObjectResult(error);
            }

            var result = GameService.Singleton.AddPlayer(gameId, form.PlayerId);
            if (!result)
            {
                var error = "Failed to add player to game";
                _logger.LogError(error);
                return new BadRequestObjectResult(error);
            }
            return new OkResult();
        }

        [FunctionName("RemovePlayer")]
        [OpenApiOperation(operationId: "RemovePlayer", tags: "Game")]
        [OpenApiParameter(name: "gameId", In = ParameterLocation.Path, Required = true, Type = typeof(Guid), Description = "The **GameId** parameter")]
        [OpenApiRequestBody("application/json", typeof(RemoveGameForm), Required = true, Description = "The Game")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.OK, Description = "The OK response")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "text/plain", bodyType: typeof(string), Description = "The Bad response")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "text/plain", bodyType: typeof(string), Description = "The Unauthorized response")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.NotFound, contentType: "text/plain", bodyType: typeof(string), Description = "The NotFound response")]
        public IActionResult RemovePlayer(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "game/remove/{gameId}")] HttpRequest request,
            Guid gameId)
        {
            var form = request.DeserializeBody<RemoveGameForm>();
            if (form == null)
            {
                var error = "Failed to deserialize body";
                _logger.LogError(error);
                return new BadRequestObjectResult(error);
            }

            var game = GameService.Singleton.GetGame(gameId);
            if (game.HostId != form.HostId)
            {
                var error = "User is not authorized";
                _logger.LogError(error);
                return new UnauthorizedObjectResult(error);
            }

            if (!game.PlayerIds.Contains(form.PlayerId))
            {
                var error = $"Player was not found: {form.PlayerId}";
                _logger.LogError(error);
                return new NotFoundObjectResult(error);
            }

            var result = GameService.Singleton.RemovePlayer(game, form.PlayerId);
            if (!result)
            {
                var error = "Failed to add player to game";
                _logger.LogError(error);
                return new BadRequestObjectResult(error);
            }
            return new OkResult();
        }

        [FunctionName("DiscardTrack")]
        [OpenApiOperation(operationId: "DiscardTrack", tags: "Game")]
        [OpenApiParameter(name: "gameId", In = ParameterLocation.Path, Required = true, Type = typeof(Guid), Description = "The **GameId** parameter")]
        [OpenApiRequestBody("application/json", typeof(HostRequestForm), Required = true, Description = "The Game")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.OK, Description = "The OK response")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "text/plain", bodyType: typeof(string), Description = "The Bad response")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "text/plain", bodyType: typeof(string), Description = "The Unauthorized response")]
        public IActionResult DiscardTrack(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "game/discardTrack/{gameId}")] HttpRequest request,
            Guid gameId)
        {
            var form = request.DeserializeBody<HostRequestForm>();
            if (form == null)
            {
                var error = "Failed to deserialize body";
                _logger.LogError(error);
                return new BadRequestObjectResult(error);
            }

            var game = GameService.Singleton.GetGame(gameId);
            if (game.HostId != form.HostId)
            {
                var error = "User is not authorized";
                _logger.LogError(error);
                return new UnauthorizedObjectResult(error);
            }

            var result = GameService.Singleton.DiscardTrack(game);
            if (!result)
            {
                var error = "Failed to discard the track";
                _logger.LogError(error);
                return new BadRequestObjectResult(error);
            }
            return new OkResult();
        }

        [FunctionName("ReshuffleDecks")]
        [OpenApiOperation(operationId: "ReshuffleDecks", tags: "Game")]
        [OpenApiParameter(name: "gameId", In = ParameterLocation.Path, Required = true, Type = typeof(Guid), Description = "The **GameId** parameter")]
        [OpenApiRequestBody("application/json", typeof(HostRequestForm), Required = true, Description = "The Game")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Game), Description = "The OK response")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "text/plain", bodyType: typeof(string), Description = "The Bad response")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "text/plain", bodyType: typeof(string), Description = "The Unauthorized response")]
        public IActionResult ReshuffleDecks(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "game/shuffle/{gameId}")] HttpRequest request,
            Guid gameId)
        {
            var form = request.DeserializeBody<HostRequestForm>();
            if (form == null)
            {
                var error = "Failed to deserialize body";
                _logger.LogError(error);
                return new BadRequestObjectResult(error);
            }

            var game = GameService.Singleton.GetGame(gameId);
            if (game.HostId != form.HostId)
            {
                var error = "User is not authorized";
                _logger.LogError(error);
                return new UnauthorizedObjectResult(error);
            }

            var result = GameService.Singleton.ShuffleDeck(game);
            if (!result)
            {
                var error = "Failed to shuffle deck";
                _logger.LogError(error);
                return new BadRequestObjectResult(error);
            }

            game = GameService.Singleton.GetGame(gameId);
            return new OkObjectResult(game);
        }

        [FunctionName("DealToTeam")]
        [OpenApiOperation(operationId: "DiscardTrack", tags: "Game")]
        [OpenApiParameter(name: "gameId", In = ParameterLocation.Path, Required = true, Type = typeof(Guid), Description = "The **GameId** parameter")]
        [OpenApiRequestBody("application/json", typeof(HostRequestForm), Required = true, Description = "The Game")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.OK, Description = "The OK response")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "text/plain", bodyType: typeof(string), Description = "The Bad response")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "text/plain", bodyType: typeof(string), Description = "The Unauthorized response")]
        public IActionResult DealToTeam(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "game/deal/{gameId}")] HttpRequest request,
            Guid gameId)
        {
            var form = request.DeserializeBody<HostRequestForm>();
            if (form == null)
            {
                var error = "Failed to deserialize body";
                _logger.LogError(error);
                return new BadRequestObjectResult(error);
            }

            var game = GameService.Singleton.GetGame(gameId);
            if (game.HostId != form.HostId)
            {
                var error = "User is not authorized";
                _logger.LogError(error);
                return new UnauthorizedObjectResult(error);
            }

            var result = GameService.Singleton.DiscardTrack(game);
            if (!result)
            {
                var error = "Failed to deal to team";
                _logger.LogError(error);
                return new BadRequestObjectResult(error);
            }
            return new OkResult();
        }

        [FunctionName("AddToDiscussion")]
        [OpenApiOperation(operationId: "AddToDiscussion", tags: "Game")]
        [OpenApiRequestBody("application/json", typeof(DiscussionForm), Required = true, Description = "The Game")]
        [OpenApiParameter(name: "gameId", In = ParameterLocation.Path, Required = true, Type = typeof(Guid), Description = "The **GameId** parameter")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.OK, Description = "The OK response")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "text/plain", bodyType: typeof(string), Description = "The Bad response")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "text/plain", bodyType: typeof(string), Description = "The Unauthorized response")]
        public IActionResult AddToDiscussion(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "game/discussion/{gameId}")] HttpRequest request,
            Guid gameId)
        {
            var form = request.DeserializeBody<DiscussionForm>();
            if (form == null)
            {
                var error = "Failed to deserialize body";
                _logger.LogError(error);
                return new BadRequestObjectResult(error);
            }

            var game = GameService.Singleton.GetGame(gameId);
            if (!game.PlayerIds.Contains(form.PlayerId))
            {
                var error = "User is not authorized";
                _logger.LogError(error);
                return new UnauthorizedObjectResult(error);
            }

            var result = GameService.Singleton.AddToDiscussion(game, form.PlayerId, form.Message);
            if (!result)
            {
                var error = "Failed to add to discussion";
                _logger.LogError(error);
                return new BadRequestObjectResult(error);
            }
            return new OkResult();
        }

        [FunctionName("DeleteGame")]
        [OpenApiOperation(operationId: "DeleteGame", tags: "Game")]
        [OpenApiParameter(name: "gameId", In = ParameterLocation.Path, Required = true, Type = typeof(Guid), Description = "The **GameId** parameter")]
        [OpenApiRequestBody("application/json", typeof(HostRequestForm), Required = true, Description = "The Game")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.OK, Description = "The OK response")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "text/plain", bodyType: typeof(string), Description = "The Bad response")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "text/plain", bodyType: typeof(string), Description = "The Unauthorized response")]
        public IActionResult DeleteGame(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "game/{gameId}")] HttpRequest request,
            Guid gameId)
        {
            var form = request.DeserializeBody<HostRequestForm>();
            if (form == null)
            {
                var error = "Failed to deserialize body";
                _logger.LogError(error);
                return new BadRequestObjectResult(error);
            }

            var game = GameService.Singleton.GetGame(gameId);
            if (game.HostId != form.HostId)
            {
                var error = "User is not authorized";
                _logger.LogError(error);
                return new UnauthorizedObjectResult(error);
            }
            var result = GameService.Singleton.DeleteGame(game);
            if (!result)
            {
                var error = "Failed to delete the game";
                _logger.LogError(error);
                return new BadRequestObjectResult(error);
            }

            return new OkResult();
        }
    }
}

