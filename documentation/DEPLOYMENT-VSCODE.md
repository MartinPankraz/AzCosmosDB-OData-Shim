# Deployment of App Service

In this example we use the [Azure App Service extension](https://marketplace.visualstudio.com/items?itemName=ms-azuretools.vscode-azureappservice) for VS Code to deploy the project. Learn more about [this process on Microsoft learn](https://learn.microsoft.com/training/modules/create-publish-webapp-app-service-vs-code/5-exercise-publish-app-azure-app-service-vs-code?pivots=nodeexpress).

1. Create an Azure App Service with **Dotnet 6** and Windows using the [VS Code extension for Azure](https://code.visualstudio.com/docs/azure/extensions) or the portal.
2. Maintain or upload environment variables in the [Azure App Service configuration](https://learn.microsoft.com/azure/app-service/configure-common?tabs=portal#configure-app-settings) - just like you did for the `appsettings.json` file for local execution in the previous section.
3. Deploy to Web App from VS Code or GitHub Codespaces

For developer convenience we added a [publish.bat](publish.bat) file that builds your project and uploads the content to your Azure App Service.

```cmd
.\publish.bat [your resource group] [name of app service]
```

4. Browse your new OData shim for CosmosDB (it takes a while the first time).

> **Note** - You need Azure CLI setup for Powershell and an open session (az login) or install/configure Azure extension for Visual Studio Code for integrated experience. I would recommend the latter ;-)
