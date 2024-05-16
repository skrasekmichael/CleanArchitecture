# Clean Architecture Backend
The **Clean Architecture** Backend developed with **Domain-Driven Design** and **CQRS**. 
This is a backend for a demonstration application *"TeamUp"* (team managment application) as part of my **Master's Thesis** at [BUT FIT](https://www.fit.vut.cz/.en).

### Run

```bash
# with dotnet 8 sdk
dotnet run --project src/TeamUp.Api
```
By default, the application launches at *https://localhost:7089* by default, to change the port, change the `applicationUrl` value in `src/TeamUp.Api/Properties/launchSettings.json`.

#### Database
By default, the application expects postgres database instance running at localhost (port 5432), and logs in using `postgres` username and `devpass` password. To change that, change connection string in `Database` section in `src/TeamUp.Api/appsettings.json`.

Such instance can be launched using the [scripts/rundb.ps1](scripts/rundb.ps1) script.

#### Client
By default, the application expects the client app ([frontend](https://github.com/skrasekmichael/ModularInformationSystemFrontend)) at *https://localhost:7229*, to change that, change the `Url` value in `Client` section in `src/TeamUp.Api/appsettings.json`.
