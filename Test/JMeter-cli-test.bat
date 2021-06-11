cd C:\YOUR_PATH\apache-jmeter-5.4.1\bin
set JVM_ARGS=-Xms32756m -Xmx32756m -Dpropname=value
jmeter -n -t "C:\YOUR_PATH\AzCosmosDB-OData-Shim\Test\OData-web-api-test-plan.jmx" -l "C:\YOUR_PATH\AzCosmosDB-OData-Shim\Test\test_results.csv" -e -o "C:\YOUR_PATH\AzCosmosDB-OData-Shim\Test\Output" -f