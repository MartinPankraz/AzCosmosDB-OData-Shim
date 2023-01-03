# AzCosmosDB-OData-Shim

[![Deploy to Azure](https://aka.ms/deploytoazurebutton)](https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2FMartinPankraz%2FAzCosmosDB-OData-Shim%2Fmain%2FTemplates%2Fazuredeploy.json)

Dotnet project to connect consumers like apps/services hosted on **SAP Business Technology Platform** via OData with [Azure CosmosDB](https://learn.microsoft.com/azure/cosmos-db/introduction).

Furthermore, it enables the [geodes-pattern](https://docs.microsoft.com/azure/architecture/patterns/geodes) for scalable global read-access to selected SAP data and SAP Private Link scenarios for SAP Cloud Application Programming Model (CAP).

1. [Geodes pattern for BTP apps powered by Azure CosmosDB](https://blogs.sap.com/2021/06/11/sap-where-can-i-get-toilet-paper-an-implementation-of-the-geodes-pattern-with-s4-btp-and-azure-cosmosdb/). Learn more about the setup [here](documentation/GEODES-GUIDE.md)
2. (Coming soon) Private connectivity for SAP CAP apps powered by Azure CosmosDB via OData. Learn more [here](documentation/SAP-PLS-GUIDE.md)

## Prerequisites to replicate the blue print

Our implementation creates a fully functional solution. The approach is standardized, so that all components could be replaced as long as the runtime environment for the application stays .NET 6. To replicate our particular setup you will need:

- [Azure account with subscription](https://azure.microsoft.com/free/) and rights to deploy Azure CosmosDB and App Service in two regions
- Azure AD authorization to configure app registration and potentially give admin consent initially
- SAP on Azure with private VNet connectivity or routing from private VNet to SAP any-premise
- [SAP BTP account](https://cockpit.eu20.hana.ondemand.com/cockpit) with Business Application Studio, Destination configured and Fiori Launchpad service to host an HTML5 app
- Access to SE80 on SAP backend to upload [Z-Programm](ZDemoFrontDoorReport.abap) for data extraction HTTP Post via ABAP.

## Deployment Guide

This repos offers two flavours of deployment guidance supporting the associated blog posts with the goal in mind to reduce time to reproduce. Not every step is template driven yet.

1. via template deployment, VS Code extension and some manual steps - more information is available [here](documentation/DEPLOYMENT-VSCODE.md)
2. (coming soon) via Azure Developer CLI

## Postman config to test OData API

For developer convenience we provide a [Postman collection](Templates/Cosmos-OData-Shim.postman_collection.json) in the [Templates folder](Templates).

Fill the details you collected from your app registration on AAD on the environment and pay attention to the difference between client_id and scope. They use the same id but have different prefix and suffix. We didn't provide fixed values for the prefix, because they can be altered by you during creation on Azure.

![pm-env](images/pm-env.png)

![pm-collection](images/pm-collection.png)

The **Tests** tab writes the env variable bearerToken, which is used for all calls in the collection, that require authentication.

Public interfaces are:

- /health
- /api/geode
- /api/odata/$metadata

Protected interfaces are:

- /api/odata/*

Consider [tweaking](https://docs.microsoft.com/odata/webapi/batch) the OData batch configuration on the [Startup.cs](GenericODataWebAPI/Startup.cs) depending on your requirements.

## What's next?

You can do a lot more once the app is deployed. Curious? We go you covered with some more information [here](documentation/WHATS-NEXT.md)

## Final words

Feel free to reach out over GitHub Issues in case of any questions :-) Until then happy integrating and enjoy reading your SAP data globally.
