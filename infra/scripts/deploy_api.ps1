# Params
param (
    [string]$apiAppName,
    [string]$resourceGroupName
)

$pythonAppPath = "..\..\src\ChatAPI"
$tempDir = "artifacts\api\temp"
$publishDir = "artifacts\api\publish"
$zipFilePath = "artifacts\api\app.zip"


#dotnet publish $pythonAppPath\ChatApi.csproj -c Release --no-restore --no-self-contained -o $tempDir
Start-Process "dotnet" -ArgumentList "publish", "$pythonAppPath\ChatApi.csproj", "-c", "Release", "-o", "$publishDir" -NoNewWindow -Wait

#Compress-Archive $tempDir $zipFilePath -Force

# Construct the argument list
$args = "$publishDir $zipFilePath $tempDir --exclude_files appsettings.Development.json"


# Execute the Python script
Start-Process "python" -ArgumentList "directory_zipper.py $args" -NoNewWindow -Wait

# Deploy the zip file to the Azure Web App
az webapp deploy --resource-group $resourceGroupName --name $apiAppName --src-path $zipFilePath --type 'zip'  --timeout 60000 --track-status false

#Set-Location -Path $publishDir

#az webapp up --name $apiAppName --runtime dotnet:8

#Set-Location -Path ../../..

# Clean up temp files
Remove-Item $publishDir -Recurse