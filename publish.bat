cd GenericODataWebAPI
dotnet publish
cd bin/Debug/net5.0/publish
PowerShell Compress-Archive -Path * -DestinationPath upload.zip -Force
call az webapp deployment source config-zip --resource-group %1 --name %2 --src .\upload.zip
cd ../../../../..