

- Create the solution "DaprWithAks"
- Add ASP.NET Core Web Api called "OrdersApi" (DotNet 5).
- Add a class library called "SharedLib" (DotNet 5).
- Add ASP.NET Core Web Api called "InventoryApi" (DotNet 5).

OrdersApi
=========
- Change the launchSettings.json file to run it as a Kestrel (console app) at port 5001.
- Rename WeatherForecastController to OrdersController, remove route, unwanted code and also delete the WeatherForecast.cs file.
- Click on the project in VisualStudio and change the "TargetFramework" to net6.0 in the .csproj file.
- Add Dapr.AspNetCore NuGet package to the project (v1.8.0).
- Register Dapr in Startup.ConfigureServices() method including JSON serialization settings.
- Add 'app.UseCloudEvents()' call in Startup.Configure() to enable event subscription.
- Add 'endpoints.MapSubscribeHandler()' call in Startup.Configure() to enable event subscription.
- Add required model classes in the "Models" folder.
- Once SharedLib class library is created add a reference to it and use shared topic name constants.
- Complete code in CreateOrder() action method in OrdersController.

SharedLib
=========
- Click on the project in VisualStudio and change the "TargetFramework" to net6.0 in the .csproj file.
- Remove Class1.
- Add a class called "CommonPubSubTopics" and add pubsub topic names as constants.

InventoryApi
============
- Change the launchSettings.json file to run it as a Kestrel (console app) at port 5002.
- Rename WeatherForecastController to InventoryController, remove route, unwanted code and also delete the WeatherForecast.cs file.
- Click on the project in VisualStudio and change the "TargetFramework" to net6.0 in the .csproj file.
- Add Dapr.AspNetCore NuGet package to the project (v1.8.0).
- Register Dapr in Startup.ConfigureServices() method including JSON serialization settings.
- Add 'app.UseCloudEvents()' call in Startup.Configure() to enable event subscription.
- Add 'endpoints.MapSubscribeHandler()' call in Startup.Configure() to enable event subscription.
- Add required model classes in the "Models" folder.
- Add a reference to SharedLib class library.
- Add required state classes in the "State" folder.
- Complete action methods in InventoryController.

Dapr Components
===============
- Create a folder called "components" at the root solution folder.
- Add pubsub.yaml file and specify redis configuration for message queue.
- Add orderstore-redis.yaml file and specify redis state store configuration for orders.
- Add inventorystore-redis.yaml file and specify redis state store configuration for inventory.
- Add inventoryitemstore-redis.yaml file and specify redis state store configuration for inventory.

Running Locally
===============
- Compose the launch.ps1 file at the root of the solution folder to start 2 services by adding following 2 commands.
	- Start-Process powershell.exe -argument '-command dapr run --app-id "order-service" --app-port "5001" --dapr-grpc-port "50010" --dapr-http-port "5010" --components-path "./components" -- dotnet run --project "./OrdersApi/OrdersApi.csproj" --urls="http://+:5001"'
	- Start-Process powershell.exe -argument '-command dapr run --app-id "order-service" --app-port "5002" --dapr-grpc-port "50020" --dapr-http-port "5020" --components-path "./components" -- dotnet run --project "./InventoryApi/InventoryApi.csproj" --urls="http://+:5002"'
	- Run the powershell script at the solution root => ".\launch.ps1"
	- "app-id" is important for service discovery.
	- "app-port" should be used to directly access apis without Dapr sidecars.
	- "dapr-http-port" should be used to access apis through Dapr sidecars.
	- "dapr-grpc-port" is the one Dapr sidecars use internally.
	
Interactive Commands
====================
- Run the following command to interactively connect to redis terminal. Make sure dapr_redis is running in Docker Desktop.
	- docker run --rm -it --link dapr_redis redis redis-cli -h dapr_redis
- Redis commands 
	- 'keys *' - Get all keys.
	- 'hget key data' - Get values by the key.
	- 'del key' - Delete orderList by the key (and values).
	- 'flushall' - Delete everything.