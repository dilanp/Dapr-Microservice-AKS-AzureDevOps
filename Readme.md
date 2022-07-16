

- Create the solution "DaprWithAks"
- Add ASP.NET Core Web Api called "OrdersApi" (DotNet 5).
- Add a class library called "SharedLib" (DotNet 5).
- Add ASP.NET Core Web Api called "InventoryApi" (DotNet 5).

OrdersApi
=========
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
- Rename WeatherForecastController to InventoryController, remove route, unwanted code and also delete the WeatherForecast.cs file.
- Click on the project in VisualStudio and change the "TargetFramework" to net6.0 in the .csproj file.
- Add Dapr.AspNetCore NuGet package to the project (v1.8.0).
- Register Dapr in Startup.ConfigureServices() method including JSON serialization settings.
- Add 'app.UseCloudEvents()' call in Startup.Configure() to enable event subscription.
- Add 'endpoints.MapSubscribeHandler()' call in Startup.Configure() to enable event subscription.
- Add required model classes in the "Models" folder.
- Add a reference to SharedLib class library.
- Add required state classes in the "State" folder.