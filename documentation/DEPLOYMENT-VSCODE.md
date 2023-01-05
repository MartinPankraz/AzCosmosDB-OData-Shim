# Deployment via VS Code Extension

In this example we use the [Azure App Service extension](https://marketplace.visualstudio.com/items?itemName=ms-azuretools.vscode-azureappservice) for VS Code to deploy the project. Learn more about [this process on Microsoft learn](https://learn.microsoft.com/training/modules/create-publish-webapp-app-service-vs-code/5-exercise-publish-app-azure-app-service-vs-code?pivots=nodeexpress).

1. Create an Azure App Service with Dotnet 6 and Windows using the [VS Code extension for Azure](https://code.visualstudio.com/docs/azure/extensions) or the portal.
2. Maintain or upload environment variables in the [Azure App Service configuration](https://learn.microsoft.com/azure/app-service/configure-common?tabs=portal#configure-app-settings) - just like you did for the `appsettings.json` file for local execution in the previous section.
3. Deploy to Web App from VS Code or GitHub Codespaces

For developer convenience we added a [publish.bat](publish.bat) file that builds your project and uploads the content to your Azure App Service.

```cmd
.\publish.bat [your resource group] [name of app service]
```

4. Browse your new OData shim for CosmosDB (it takes a while the first time).

## Azure CosmosDB

<details>
<summary>click to expand</summary>

We need at least two instance of Cosmos to verify global access. We configure global read and primary region write to avoid concurrent locking challenges in our blue print. Going forward you might want to think about global write too. In our case SAP backend will always override what is in Cosmos if there is a race condition.

Choose CosmosDB with Core SQL API

### Basics

- Provide required fields and pay attention to your primary region choice.
- Choose capacity mode Provisioned Throughput to allow multi-region setup

### Global Distribution

- Keep Geo-Redundancy disabled (we will add regions later)
- Multi-Region Writed disabled (check first section for reasoning)

### Networking (private VNet required)

- Configure Private endpoint to make Cosmos only accessible from your private VNet that "knows" SAP

### Backup and Encryption

- Configure as you wish.

Wait for provisioning to finish.

### Configure Cosmos Settings

- Replicate data globally -> add read regions as per your needs
- Default Consistency -> Understand your consistency choice and its impact on global read
- Firewall and virtual networks -> familiarize with settings to understand connectivity issues going forward. Allow access from Azure Portal and possibly from your admin ip to begin with. Ultimately your VPN or ExpressRoute connection should be leveraged over your private Azure VNet. In our case we are communication over a P2S VPN with Azure.
- Private Endpoint Connections -> Add a private endpoint for each private VNet in each region, where you are running Cosmos. Meaning you would need additional VNets to achieve private routing.
- Keys -> note down the primary key and URI for your `appsettings.json`.

### Hosts file settings for local development

Since you protected your CosmosDB via its built-in firewall, private VNet and potentially a VPN, you need to make sure that you can reach it from your dev environment. In my case I added two entries to my hosts file (C:\Windows\System32\drivers\etc\hosts) to resolve the private endpoints on Azure from my P2S VPN connection.

```cmd
10.---.--.14 sap-cosmos-sql.privatelink.documents.azure.com sap-cosmos-sql.documents.azure.com
10.---.--.15 sap-cosmos-sql-westeurope.privatelink.documents.azure.com sap-cosmos-sql-westeurope.documents.azure.com
```

You can collect your specific values from the generated Azure Private DNS Zone, that was created when you configured your private endpoints.

![dns](images/dns.png)
</details>

## Azure App Service (Create at least 2 Web Apps)

<details>
<summary>click to expand</summary>

- Instance Details -> Publish Code
- Runtime Stack -> .Net 6
- OS according to your needs. We ran on Windows during our implementation.
- Region -> match your CosmosDB instances (in our case West Europe and West US)
- App Service Plan (SKU) -> can be anything that supports SSL (currently default B1 for instance)

### Configure App Service

- Essentials -> Health Check -> Enable and put path `/health`

#### Settings

- Networking -> Configure VNet integration with the related VNets where Cosmos private endpoints sit. Be aware you will need enough space for an additional empty subnet.

For the additional app settings you might want to consider to apply [ARM templates](https://docs.microsoft.com/de-de/azure/templates/microsoft.web/sites/config-appsettings?tabs=json) and [CI/CD](https://docs.microsoft.com/en-us/azure/azure-resource-manager/templates/add-template-to-azure-pipelines) for transparent and consisten rollout. To get started with only a few instances doing it manually will suffice.

App Setting | Value
--- | ---
geode-name | your location
WEBSITE_VNET_ROUTE_ALL | 1
Modules:CosmosConfig:CollectionId | sflight
Modules:CosmosConfig:DatabaseId | saps4
Modules:CosmosConfig:Endpoint | https://[your domain].documents.azure.com:443/
Modules:CosmosConfig:Key | your cosmos primary key
RewriteModule:NewRoute | rewrite route to map odata context from app service to front door
AzureAd:Audience | your AAD app registration client id
AzureAd:ClientId | your AAD app registration client id
AzureAd:Instance | https://login.microsoftonline.com/
AzureAd:TenantId | your AAD tenant id

The **geode-name** will be used to be able to trace-back easily from where our requests were served. The routing param ensures that all traffic leaving app service stays on the private VNet, so that it will use the private endpoint of CosmosDB. Otherwise you will see Firewall hits on Cosmos.

</details>

## FrontDoor (global routing based on geo and availability)

<details>
<summary>click to expand</summary>

- Create a resource in any resource group on Azure.
- Fill your desired front-end domain, Session Affinity disabled, WAF disabled
- Add a backend pool with our two azure app service backends (keep defaults "priority" 1 and "weight" 50), fill /health as health probe, https, Probe method HEAD, keep rest as is
- Add routing rule and keep rule defaults as is (pattern match on /* etc.)

Once provisioned pickup Frontend host URL for SAP BTP Destination setup later on.
</details>

## SAP backend for data up-stream

<details>
<summary>click to expand</summary>

- Create a destination named "AzureFrontDoor" for external https connections on **SM59** in your ABAP system
- Fill your FrontDoor address (yourdomain.azurefd.net) and port 443. Alternatively you could fill your private CosmosDB connectivity details and connect directly. The [ABAP SDK for Azure](https://github.com/microsoft/ABAP-SDK-for-Azure) could give you head start doing that. We advise **against** it, because the geode pattern would be bypassed. FrontDoor ensures that you reach the closest App Service and CosmosDB instance that is available.
- Set SSL active and maintain cert-list for Azure SSL certificates. You can do that from transaction **STRUST**. The certificate chain can be exported from any browser when you try to hit your FrontDoor domain and then inspect the certificates. You need to import the whole chain. While writing this doc that was:

    ![fd-cert-chain](images/fd-cert-chain.png)

- Once finished you should make the connection test from SM59 and see http 404 as response. When the process on STRUST was not successfull you will get an SSL handshake error here.
- Repeat the process for destination "AzureADLogin"
- Fill your AD login endpoint login.microsoftonline.com, port 443 and Path prefix: /[your AAD tenant id]/oauth2/v2.0/token
- Activate SSL and check **STRUST** once more if connection test fails

- Create an ABAP program on **SE80** based on the code in [ZDemoFrontDoorReport.abap](ZDemoFrontDoorReport.abap). It will leverage the popular demo data set SFlight.

>**Note**
> I highly recommend checking the API calls through a [REST client](https://marketplace.visualstudio.com/items?itemName=humao.rest-client) or Postman first, because the http log on the SAP app server can be tedious. If you need to troubleshoot on SAP you would need to activate http trace info on SMICM, lock your work process on SAPGUI through SE38 (RSTRC000), navigate within that same session to SE80, trigger your progamm, go back to RSTRC000 and release your workpress and finally check the trace file on ST11 for your previously locked work process number.

</details>

## Azure AD (app registration for secure authentication)

<details>
<summary>click to expand</summary>

For simplicity we are configuring the OAuth2 Client Credentials Grant flow. Of course, you could adapt this to any other SAP CloudFoundry Destination supported flow, or replace the need for destination through SAP Cloud SDK or even other apps that integrate with Azure AD like SAP Identity Authentication Service etc.

- Create a new app registration to secure the Cosmos OData shim API exposed by Azure App Service.
- Overview -> note down the application (client) id, AAD tenant id, application ID URI for your appsettings.json locally, Postman requests and App Service environment variables
- Manage -> Certificate & Secrets -> Generate a secret and note it down (visible only once)
- Manage -> App roles -> Add Sflight (Allows access to Sfligh objects), add Reader (Allow app to read from Cosmos) and add Writer (Allow access to write to Cosmos). Those roles are refrenced on the [code](GenericODataWebAPI/Controllers/SflightController.cs)
- Manage -> API permissions -> Add permissions for just created roles and give admin consent. In case admin consent is hard to get and you are in a trial or PoC scenario, you could use a [free Azure subscription](https://azure.microsoft.com/free/) and register your app with that AAD even though the resources actually run in another subscription. Delegated permissions might get you around admin consent too, but require a more complex setup.

</details>

## SAP BTP Destination (one each region)

<details>
<summary>click to expand</summary>

Create a destination named "AzureCosmosDB" on subaccount level on your BTP cockpit (in our case one for west europe and for west us)

Property | Value
--- | ---
`URL` | [your FrontDoor domain].azurefd.net
`Proxy Type` | Internet
`Authentication` | OAuth2ClientCredentials
`Client ID` | api://[Your app registration id in AAD]
`Client Secret` | the secret you generated in your app registration
`Token Service URL` | https://login.microsoftonline.com/[your AAD tenant id]/oauth2/v2.0/token

### Additional Properties

Property | Value
--- | --- 
`HTML5.DynamicDestination` | value true
`scope` | "Your app registration id in AAD" (same as Client ID) **without** "api://" at the beginning **and** with suffix `/.default` at the end.
`WebIDEEnabled` | true
`WebIDEUsage` | odata_abap

</details>

## SAP BTP HTML5 App ([source in second repos](https://github.com/MartinPankraz/SAPUI5-CosmosDB-umbrella))

<details>
<summary>click to expand</summary>

Find the source for the consuming SAPUI5 app [here](https://github.com/MartinPankraz/SAPUI5-CosmosDB-umbrella).

- clone from GitHub and run in Business Application Studio with npm start or right click the webapp folder -> preview application
- build and deploy to cloud foundry the [usual way](https://developers.sap.com/tutorials/appstudio-sapui5-create.html#294b8b1d-0791-4e31-b9b1-525e533557c0)
- To be able to consume the HTML5 app you need to add a hosting service. We choose the SAP Fiori Launchpad service.

    ![ui5-app-screen](images/ui5-app-screen.png)

</details>

## Publish OData API to Azure App service manually

For developer convenience we added a [publish.bat](publish.bat) file that builds your project and uploads the content to your app service.

```cmd
.\publish.bat [your resource group] [name of app service]
```

You need Azure CLI setup for Powershell and an open session (az login) or install/configure Azure extension for Visual Studio Code for integrated experience. I would recommend the latter ;-)
