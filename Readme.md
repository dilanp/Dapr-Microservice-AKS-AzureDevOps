

- Create the solution "DaprWithAks"
- Add ASP.NET Core Web Api called "OrdersApi" (DotNet 5).
- Add a class library called "SharedLib" (DotNet 5).
- Add ASP.NET Core Web Api called "InventoryApi" (DotNet 5).
- Add Console Application called "Supplier" (DotNet 5).

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
  - Run the powershell script  (as admin) at the solution root => ".\launch.ps1"
  - "app-id" is important for service discovery.
  - "app-port" should be used to directly access apis without Dapr sidecars.
  - "dapr-http-port" should be used to access apis through Dapr sidecars.
  - "dapr-grpc-port" is the one Dapr sidecars use internally.
  - Use the Postman collection to send an order and check the state store contents. Use Redis interactive commands too...
	
Local Interactive Commands
==========================
- Run the following command to interactively connect to redis terminal. Make sure dapr_redis is running in Docker Desktop.
  - docker run --rm -it --link dapr_redis redis redis-cli -h dapr_redis
- Redis commands 
  - 'keys *' - Get all keys.
  - 'hget key data' - Get values by the key.
  - 'del key' - Delete orderList by the key (and values).
  - 'flushall' - Delete everything.
	
Azure Resources
===============
- Create the resource group - "ffdapr.rg".
- Create the Core(SQL) CosmosDb account - "daprstatecosmos01".
- Create a new database in it - "csstatedb".
- Create 3 new collections in that database - "orderstate", "inventorystate", "inventoryitemstate".
- Go to Keys section of CosmosDb and get - URI, Primary Key and Primary Connection String.
- Create a Service Bus (Standard Tier) with namespace - "daprservicebus01".
- Go to Shared access policies section of Service Bus and get - Primary Key and Primary Connection String.
- Create a new queue called "daprcoursequeue" in Azure Service Bus.
- Create an Event Hub (Basic Tier) Namespace - "daprbindingeventhub01".
- Create a new Event Hub there - "stockrefill".
- Add a Shared Access Policy with Send & Listen rights - "sendlisten".
- Select newly created policy and get - Primary key and Connection string–primary key.
- Create a Standard, LRS Storage Account - "daprbindingstoreacc01".
- Select Access keys in newly created storage account and get - Key and Connection string for key 1.
- Once the apps are Dockerized create a new Azure Container Registry (Standard) - "daprk8sscrff".

Dapr Components for Azure
=========================
- Create a folder called "components-azure" at the root solution folder.
- Copy over all yaml config files from "components" folder.
- Rename file to indicate that they use cosmos and asb (Azure Service Bus).
- Update all 3 state store related yaml config files to indicate.
  - file names ends with "-cosmos".
  - type: state.azure.cosmosdb
  - metadata section should reflect values obtained from the Keys section of CosmosDb.
- Update the pubsub.yaml file to indicate.
  - file names ends with "-asb".
  - type: pubsub.azure.servicebus.
  - metadata section should reflect the Primary Connection String from Azure.
- Create "binding-eh.yaml" file inside the "components-azure" folder and specify settings.
  - name: stockrefill
  - type: binding.azure.eventhubs
  - metadata section should reflect values obtained from Azure Event Hub and Storage Account.
	
Run Locally with Azure Resources
================================
- Duplicate the launch.ps1 file at the root of the solution folder and name it launch-azure.ps1
- Update the components path to use "components-azure" folder where we point to Azure resources.
- Run the powershell script (as admin) at the solution root => ".\launch-azure.ps1"

Supplier
========
- Click on the project in VisualStudio and change the "TargetFramework" to net6.0 in the .csproj file.
- Add Azure.Messaging.EventHubs NuGet package (v5.7.1).
- Code the Main() method.
- Manually run 'dotnet run' command inside the Supplier project folder and make sure InventoryController.Refill() method works without errors.

Setup Kubernetes Cluster
========================
- Open a Azure CLI session and Login to Azure.
  - az login
- Set the correct subscription.
  - az account set --subscription 6f6895a4-64d7-483f-84e1-e37338558202
- Create the K8s cluster.
  - az aks create --resource-group ffdapr.rg --name daprk8saksff --node-count 2 --node-vm-size Standard_D2s_v3 --enable-addons monitoring --vm-set-type VirtualMachineScaleSets --generate-ssh-keys
- Install kubectl CLI.
  - sudo az aks install-cli
- Get AKS credentials and merge it to the local config.
  - az aks get-credentials --name daprk8saksff --resource-group ffdapr.rg
- Check the nodes in the cluster.
  - kubectl get nodes

Setup Dapr on K8s cluster
==========================
- Setup Dapr on K8s.
  - dapr init -k
- Get the status of Dapr on K8s.
  - dapr status -k
  - kubectl get pods -n dapr-system -w
- Get all Dapr services on K8s.
  - kubectl get svc -n dapr-system -w
- See the Dapr Dashboard on K8s.
  - dapr dashboard -k

Dockerize the Apps
==================
- Right-click on OrdersApi project "Add > Docker Support" to add the Dockerfile (use Linux containers).
- Right-click on InventoryApi project "Add > Docker Support" to add the Dockerfile (use Linux containers).
- Move to the root solution folder in PowerShell and run the following commands to create Docker images.
  - docker build . -t daprorderservice:latest -f OrdersApi/Dockerfile
  - docker build . -t daprinventoryservice:latest -f InventoryApi/Dockerfile
- Make sure both images are created by running.
  - docker images

Push Images to ACR
==================
- Once the Azure Container Registry (ACR) is created get its URL. Then tag the both images with it.
  - docker tag daprorderservice:latest daprk8sscrff.azurecr.io/daprorderservice:latest
  - docker tag daprinventoryservice:latest daprk8sscrff.azurecr.io/daprinventoryservice:latest
- Next login to the ACR.
  - az acr login --name daprk8sscrff
- Now push the images to ACR.
  - docker push daprk8sscrff.azurecr.io/daprorderservice:latest
  - docker push daprk8sscrff.azurecr.io/daprinventoryservice:latest
- Make sure both images are available in Azure ACR Repository.

Deploy to K8s
=============
- Connect K8s cluster with the ACR by running the following command.
  - az aks update --name daprk8saksff --resource-group ffdapr.rg --attach-acr daprk8sscrff
- Create a folder called "deploy" at the root solution folder.
- Add the "microservice-order.yaml" file there and compose K8s Deployment and Service specs for order service.
- Add the "microservice-inventory.yaml" file there and compose K8s Deployment and Service specs for inventory service.
- Create a file named "component-pubsub-asb.yaml" there and copy content from "components-azure/pubsub-asb.yaml" file.
- Create a file named "component-binding-eh.yaml" there and copy content from "components-azure/binding-eh.yaml" file.
- Create a file named "component-orderstore-cosmos.yaml" there and copy content from "components-azure/orderstore-cosmos.yaml" file.
- Create a file named "component-inventorystore-cosmos.yaml" there and copy content from "components-azure/inventorystore-cosmos.yaml" file.
- Create a file named "component-inventoryitemstore-cosmos.yaml" there and copy content from "components-azure/inventoryitemstore-cosmos.yaml" file.
- Now, add a deploy-solution.ps1 PowerShell file and compose all 7 "kubectl apply" commands there.
- Move into the root solution folder and run the PowerShell file to deploy all components to AKS.
  - Windows => ".\deploy\deploy-solution.ps1"
  - Mac/Linux => "pwsh ./deploy/deploy-solution.ps1"
- Make sure deployment succeeds. Verify components by launching the Dapr Dashboard on K8s.
  - dapr dashboard -k

Setup K8s Ingress Controller
============================
- Install Helm locally.
- Add a new PowerShell script called "deploy-nginx.ps1" into the "deploy" folder.
- Move into the root solution folder and run the PowerShell file to deploy NGINX ingress controller.
  - Windows => ".\deploy\deploy-nginx.ps1"
  - Mac/Linux => "pwsh ./deploy/deploy-nginx.ps1"
- Next draft the "ingress-nginx.yaml" file in the "deploy" folder and specify access to our 2 services.
- Apply this ingress controller yaml file too.
  - Mac/Linux => kubectl apply -f ./deploy/ingress-nginx.yaml
  - Windows => kubectl apply -f .\deploy\ingress-nginx.yaml
- Find out the public IP of the ingress controller to access the services.
  - kubectl get ingress