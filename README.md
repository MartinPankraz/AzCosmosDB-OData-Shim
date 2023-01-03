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

For developer convenience we provide a Postman collection in the [Templates folder](Templates).

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

## Load tests

We performed a simple load test with [Apache JMeter](https://jmeter.apache.org/) on app service (scale out to 10 instance) with [SKU](https://docs.microsoft.com/azure/app-service/overview-hosting-plans) **S1 (100ACU, 1.75GB RAM)** per region. CosmosDB was left on default Throughput (autoscale) with a max RU/s of 4000. We provided JMeter with 32GB RAM to be able to sustain the 10k threads. The process ran on a VM (E8-4ds_v4, 4 vcpus, 64 GiB memory) in Azure West-US. So, the requests go to the US based geodes until FrontDoor decides to re-route to europe.

JMeter was configured to perform the GET `/api/odata/Sflight` with 10k threads in parallel for a duration of 60 seconds. We re-ran the process for 3 times while waiting for 5mins in between. Over the course of this analysis an average of 0.1% of requests failed with http 503 while the second run even completed 100% successful. Meaning we are likely close to the maximum simultaneous load capacity for this setup.

You can check the results dashboard on the output [folder](Test/Output/index.html).

We left the [batch file](Test/JMeter-cli-test.bat) for you so you can replicate the test or easily come up with a more sophisticated load-testing logic.

In order to scale further we would now need to increase the app service SKU.

## Final words

Feel free to reach out over GitHub Issues in case of any questions :-) Until then happy integrating and enjoy reading your SAP data globally.
