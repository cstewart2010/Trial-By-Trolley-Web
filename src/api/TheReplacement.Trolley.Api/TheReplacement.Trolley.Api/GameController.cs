using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
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
        [OpenApiParameter(name: "id", In = ParameterLocation.Query, Required = true, Type = typeof(Guid), Description = "The **Id** parameter")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Game), Description = "The OK response")]
        public IActionResult GetGame(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "game")] HttpRequest request)
        {
            var id = Guid.Parse(request.Query["id"]);
            var game = GameService.Singleton.GetGame(id);
            return new OkObjectResult(game);
        }

        [FunctionName("CreateNewGame")]
        [OpenApiOperation(operationId: "CreateNewGame", tags: "Game")]
        [OpenApiRequestBody("application/json", typeof(NewGameForm), Required = true, Description = "The Game")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Game), Description = "The OK response")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "text/plain", bodyType: typeof(Game), Description = "The Bad response")]
        public IActionResult CreateNewGame(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "game")] HttpRequest request)
        {
            var reader = new StreamReader(request.Body);
            var json = reader.ReadToEnd();
            var form = JsonConvert.DeserializeObject<NewGameForm>(json);
            var newGame = new Game
            {
                GameId = Guid.NewGuid(),
                Initialization = form.Initialization,
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
    }
}

