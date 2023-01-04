# What's next?

## Authentication with Azure AD 🔐

This repos is setup to use code based Azure AD authentication. Check commented lines 74 onwards in [Startup.cs](GenericODataWebAPI/Startup.cs) to enable it. In addition to that uncomment 'Authorize annotations' in [SflightController.cs](../GenericODataWebAPI/Controllers/SflightController.cs).

Alternatively to the code based approach, consider [outsourcing the config](https://learn.microsoft.com/azure/app-service/configure-authentication-provider-aad) to your App Service. Use standard variable `X-MS-TOKEN-AAD-ACCESS-TOKEN` to retrieve the access token from the request header. [Learn more](https://learn.microsoft.com/azure/app-service/configure-authentication-oauth-tokens#retrieve-tokens-in-app-code)

Consider SAP Principal Propagation for your authentication scenario handled by [Azure API Management](https://learn.microsoft.com/azure/api-management/sap-api#production-considerations).

[Learn more](https://github.com/Azure/api-management-policy-snippets/blob/master/examples/Request%20OAuth2%20access%20token%20from%20SAP%20using%20AAD%20JWT%20token.xml)

## Connectivity to SAP backends and secure virtual network access 🔌

SAP backends on Azure typically run in fully isolated virtual networks. There are multiple ways to connect to them. Most popular ones are:

* Integrate your App Service with an Azure virtual network (VNet). [Learn more](https://learn.microsoft.com/azure/app-service/configure-vnet-integration-enable).
* Private Endpoints for Azure App Service. [Learn more](https://learn.microsoft.com/azure/app-service/networking/private-endpoint?source=recommendations)
* User Azure API Management for OData with SAP Principal Propagation. [Learn more](https://learn.microsoft.com/azure/api-management/sap-api#production-considerations)

VNet integration enables your app to securely access resources in your VNet, such as your SAP Gateway, but doesn't block public access to your App Service. To achieve full private connectivity for the app service too, look into private endpoints.

## Load tests

We performed a simple load test with [Apache JMeter](https://jmeter.apache.org/) on app service (scale out to 10 instance) with [SKU](https://docs.microsoft.com/azure/app-service/overview-hosting-plans) **S1 (100ACU, 1.75GB RAM)** per region. CosmosDB was left on default Throughput (autoscale) with a max RU/s of 4000. We provided JMeter with 32GB RAM to be able to sustain the 10k threads. The process ran on a VM (E8-4ds_v4, 4 vcpus, 64 GiB memory) in Azure West-US. So, the requests go to the US based geodes until FrontDoor decides to re-route to europe.

JMeter was configured to perform the GET `/api/odata/Sflight` with 10k threads in parallel for a duration of 60 seconds. We re-ran the process for 3 times while waiting for 5mins in between. Over the course of this analysis an average of 0.1% of requests failed with http 503 while the second run even completed 100% successful. Meaning we are likely close to the maximum simultaneous load capacity for this setup.

You can check the results dashboard on the output [folder](Test/Output/index.html).

We left the [batch file](Test/JMeter-cli-test.bat) for you so you can replicate the test or easily come up with a more sophisticated load-testing logic.

In order to scale further we would now need to increase the app service SKU.

## DevOps 👩🏾‍💻

* Consider activating GitHub Actions for your Azure project for out-of-the-box integrated CI/CD flows. [Learn more](https://docs.microsoft.com/azure/app-service/deploy-github-actions?tabs=applevel)
* Explore cloud-native zero-downtime deployment styles like "[blue-green](https://learn.microsoft.com/azure/architecture/example-scenario/blue-green-spring/blue-green-spring)" with Azure App Service deployment slots. [Learn more](https://docs.microsoft.com/azure/app-service/deploy-staging-slots)

If you are using the Azure Developer CLI the repository contains all necessary building blocks to setup a preconfigured CI/CD pipeline for:

* GitHub Actions
* Azure DevOps

You find more information about the options [here](AZD-CICD-SETUP.md).
