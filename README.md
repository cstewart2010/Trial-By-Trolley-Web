# Trial By Trolley Web

## Tech Stack
* [NodeJs](https://nodejs.org/en/download/current) and [Vue3](https://vuejs.org/api/) for the frontend
* [ASP.NET](https://visualstudio.microsoft.com/vs/community/) and [Mongo](https://www.mongodb.com/docs/manual/tutorial/install-mongodb-on-windows/) for the backend
* Cloudflare Pages for frontend deployment
* Azure Functions for backend deployment


## Running locally

### Backend

* Install the proper version of MongoDB for your local env
 * MongoDB 6.x for Windows 10
 * MongoDB 7+ for everything else
* add the `tbt-local` database to the the local connection
* set the following environment variables
  * set the `MongoDBConnectionString` environment variable to `mongodb://localhost:<PORT>/`
  * set the `TrialByTrolleyDatabase` environment variable to `tbt-local`
* run the `./scripts/mongo-setup.js` in the Mongo shell to set up local databases
* start the `TheReplacement.Trolley.Api.Client` to start the server

### Frontend

* See the [Client ReadME](./src/client/README.md)
