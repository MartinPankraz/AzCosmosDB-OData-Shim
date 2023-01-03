# What's next?

## Authentication with Azure AD 🔐

[Configure](https://learn.microsoft.com/azure/app-service/configure-authentication-provider-aad) your App Service or Azure Functions app to use Azure AD login. Use standard variable `X-MS-TOKEN-AAD-ACCESS-TOKEN` to retrieve the access token from the request header. [Learn more](https://learn.microsoft.com/azure/app-service/configure-authentication-oauth-tokens#retrieve-tokens-in-app-code)

Consider SAP Principal Propagation for your authentication scenario handled by [Azure API Management](https://learn.microsoft.com/azure/api-management/sap-api#production-considerations).

[Learn more](https://github.com/Azure/api-management-policy-snippets/blob/master/examples/Request%20OAuth2%20access%20token%20from%20SAP%20using%20AAD%20JWT%20token.xml)

## Connectivity to SAP backends and secure virtual network access 🔌

SAP backends on Azure typically run in fully isolated virtual networks. There are multiple ways to connect to them. Most popular ones are:

* Integrate your App Service with an Azure virtual network (VNet). [Learn more](https://learn.microsoft.com/azure/app-service/configure-vnet-integration-enable).
* Private Endpoints for Azure App Service. [Learn more](https://learn.microsoft.com/azure/app-service/networking/private-endpoint?source=recommendations)
* User Azure API Management for OData with SAP Principal Propagation. [Learn more](https://learn.microsoft.com/azure/api-management/sap-api#production-considerations)

VNet integration enables your app to securely access resources in your VNet, such as your SAP Gateway, but doesn't block public access to your App Service. To achieve full private connectivity for the app service too, look into private endpoints.

## DevOps 👩🏾‍💻

* Consider activating GitHub Actions for your Azure project for out-of-the-box integrated CI/CD flows. [Learn more](https://docs.microsoft.com/azure/app-service/deploy-github-actions?tabs=applevel)
* Explore cloud-native zero-downtime deployment styles like "[blue-green](https://learn.microsoft.com/azure/architecture/example-scenario/blue-green-spring/blue-green-spring)" with Azure App Service deployment slots. [Learn more](https://docs.microsoft.com/azure/app-service/deploy-staging-slots)

If you are using the Azure Developer CLI the repository contains all necessary building blocks to setup a preconfigured CI/CD pipeline for:

* GitHub Actions
* Azure DevOps

You find more information about the options [here](AZD-CICD-SETUP.md).
