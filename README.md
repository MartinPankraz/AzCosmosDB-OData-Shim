# AzCosmosDB-OData-Shim
Project to connect consumers like SAP Business Technology Platform apps/services via OData with Azure CosmosDB. Furthermore it enables the geode-pattern for global read-access to selected SAP data.

Find the related blog on the SAP community [here]().

Find the related Azure DevOps project for CI/CD [here](https://dev.azure.com/mapankra/CosmosDB%20OData%20SAP%20umbrella).

![geode](images/geode-pattern.png)
_Fig.1 architecture overview_

## High-level Prerequisites to replicate our blue print

Our implementation creates a fully functional solution of the geode pattern tested from ECC and S4. The approach is standardized, so that all components could be replaced.

- Azure account with subscription and rights to deploy Azure CosmosDB and App Service in two regions
- Azure AD authorization to configure app registration and potentially give admin consent initially
- SAP on Azure with private VNet connectivity or routing from private VNet to SAP on-premise
- SAP BTP account with Business Application Studio, Destination configured and Fiori Launchpad service to host HTML5 app
- Access to SE80 on SAP backend to upload Z-Programm for data extraction HTTP Post via ABAP.

## Deployment Guide
All deployments could be done via scripting and CI/CD. For simple reproduction find below the manual steps:

### Azure CosmosDB
<details>
<summary>click to expand</summary>

We need at least two instance of Cosmos to verify global access. We configure global read and primary region write to avoid concurrent locking challenges in our blue print. Going forward you might want to think about global write too. In our case SAP backend will always override what is in Cosmos if there is a race condition.

Choose CosmosDB with Core SQL API
#### Basics
- Provide required fields and pay attention to your primary region choice.
- Choose capacity mode Provisioned Throughput to allow multi-region setup
#### Global Distribution
- Keep Geo-Redundancy disabled (we will add regions later)
- Multi-Region Writed disabled (check first section for reasoning)
#### Networking (private VNet required)
- Configure Private endpoint to make Cosmos only accessible from your private VNet that "knows" SAP
#### Backup and Encryption
- Configure as you wish. 

Wait for provisioning to finish.

#### Configure Cosmos Settings
- Replicate data globally -> add read regions as per your needs
- Default Consistency -> Understand your consistency choice and its impact on global read
- Firewall and virtual networks -> familiarize with settings to understand connectivity issues going forward. Allow access from Azure Portal and possibly from your admin ip to begin with. Ultimately your VPN or ExpressRoute connection should be leveraged over your private Azure VNet. In our case we are communication over a P2S VPN with Azure.
- Private Endpoint Connections -> Add a private endpoint for each private VNet in each region, where you are running Cosmos. Meaning you would need additional VNets to achieve private routing.
- Keys -> note down the primary key and URI for your appsettings.json.
</details>

### Azure App Service (Create at least 2 Web Apps)
<details>
<summary>click to expand</summary>

- Instance Details -> Publish Code
- Runtime Stack -> .Net 5
- OS according to your needs. We ran on Windows during our implementation.
- Region -> match your CosmosDB instances (in our case West Europe and West US)
- App Service Plan (SKU) -> can be anything that supports SSL (currently default B1 for instance)

#### Configure App Service
- Essentials -> Health Check -> Enable and put path /health
##### Settings
- Networking -> Configure VNet integration with the related VNets where Cosmos private endpoints sit. Be aware you will need enough space for an additional empty subnet.
- Configuration -> Add app setting "geode-name" and put the location name where your app service runs (e.g europe or west us). We will use it later on for our geode service to be able to trace-back easily from where our requests were served
- Configuration -> Add app setting "WEBSITE_VNET_ROUTE_ALL" with value 1. This ensures that all traffic leaving app service stays on the private VNet, so that it will use the private endpoint of CosmosDB. Otherwise you will see Firewall hits on Cosmos.
</details>

### FrontDoor
<details>
<summary>click to expand</summary>

- Create a resource in any resource group on Azure.
- Fill your desired front-end domain, Session Affinity disabled, WAF disabled
- Add a backend pool with our two azure app service backends (keep defaults "priority" 1 and "weight" 50), fill /health as health probe, https, Probe method HEAD, keep rest as is
- Add routing rule and keep rule defaults as is (pattern match on /* etc.)

Once provisioned pickup Frontend host URL for SAP BTP Destination setup later on.
</details>

### SAP backend for data up-stream
<details>
<summary>click to expand</summary>

- Create a destination named "AzureFrontDoor" for external https connections on SM59 in your ABAP system
- Fill your FrontDoor address (yourdomain.azurefd.net) and port 443. Alternatively you could fill your private CosmosDB connectivity details and connect directly. The ABAP SDK for AZure could give you head start doing that. We advise against it, because the geode pattern would be bypassed. FrontDoor ensures that you reach the closest App Service and CosmosDB instance that is available.
- Set SSL active and maintain cert-list for Azure SSL certificates. You can do that from transaction STRUST. The certificate chain can be exported from any browser when you try to hit your FrontDoor domain and then inspect the certificates. You need to import the whole chain. While writing this doc that was:

    ![fd-cert-chain](images/fd-cert-chain.png)

- Once finished you should make the connection test from SM59 and see http 404 as response. When the process on STRUST was not successfull you will get an SSL handshake error here.
- Repeat the process for destination "AzureADLogin"
- Fill your AD login endpoint login.microsoftonline.com, port 443 and Path prefix: /<your AAD tenant id>/oauth2/v2.0/token
- Activate SSL and check STRUST once more if connection test fails

- Create an ABAP program on SE80 based on the code in [ZDemoFrontDoorReport.abap](ZDemoFrontDoorReport.abap). It will leverage the popular demo data set SFlight.

I highly recommend checking the API calls through Postman first, because the http log on the SAP app server can be tedious.

</details>

### Azure AD
<details>
<summary>click to expand</summary>

</details>

### SAP BTP Destination (one each region)
<details>
<summary>click to expand</summary>

- Create a destination named "AzureCosmosDB" on subaccount level on your BTP cockpit (in our case one for west europe and for west us)
- URL -> [your FrontDoor domain].azurefd.net
- Proxy Type -> Internet
- Authentication -> OAuth2ClientCredentials
- Client ID -> api://[Your app registration id in AAD]
- Client Secret -> the secret you generated in your app registration
- Token Service URL -> https://login.microsoftonline.com/[your AAD tenant id]/oauth2/v2.0/token

#### Additional Properties
- Add HTML5.DynamicDestination with value true
- Add scope with value "Your app registration id in AAD" (same as Client ID)
- Add WebIDEEnabled with value true
- Add WebIDEUsage with value odata_abap

</details>

### SAP BTP HTML5 App (source in second repos)
<details>
<summary>click to expand</summary>
Find the source for the consuming SAPUI5 app [here](https://github.com/MartinPankraz/SAPUI5-CosmosDB-umbrella).

- clone from GitHub and run in Business Application Studio with npm start or right click the webapp folder -> preview application
- build and deploy to cloud foundry the [usual way](https://developers.sap.com/tutorials/appstudio-sapui5-create.html#294b8b1d-0791-4e31-b9b1-525e533557c0)
- To be able to consume the HTML5 app you need to add a hosting service. We choose the SAP Fiori Launchpad service.

    ![ui5-app-screen](images/ui5-app-screen.png)

</details>

### Postman config to test OData API

### Publish OData API to Azure App service


Feel free to reach out in case of any question over GitHub Issues :-)