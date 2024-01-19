To run the project:

- There is a docker-compose.yml file in /src, running this will stand up the database
  - `docker compose up -d`
- Then either run the project in Visual Studio or run the following commands in the /src/Chat/Chat.Web/Chat.Web folder:
  - `dotnet run`
 
### Telemetry
- To start up the telemetry stack in dev
  - `cd ops/dev/Telemetry`
  - `docker compose up -d`
  - Grafana is hosted at [localhost:3000](http://localhost:3000)
