targetScope = 'subscription'

@minLength(1)
@maxLength(64)
@description('Name which is used to generate a short unique hash for each resource')
param environmentName string

@minLength(1)
@maxLength(64)
@description('Name which is used to generate a short unique hash for each resource')
param projectName string

@minLength(1)
@description('Primary location for all resources')
param location string


var resourceToken = uniqueString(environmentName,projectName,location,az.subscription().subscriptionId)
var abbreviations = loadJsonContent('abbreviations.json')


var apiAppName = 'api-${projectName}-${environmentName}-${resourceToken}'
var webAppName = 'web-${projectName}-${environmentName}-${resourceToken}'

resource resourceGroup 'Microsoft.Resources/resourceGroups@2024-03-01' = {
  name: 'rg-${projectName}-${environmentName}-${location}-${resourceToken}'
  location: location
}


module managedIdentity 'core/security/managed-identity.bicep' = {
  name: 'managed-identity'
  scope: resourceGroup
  params: {
    name: 'id-${projectName}-${environmentName}'
    location: location
  }
}


module storage 'core/storage/blob-storage-account.bicep' = {
  name: 'storage'
  scope: resourceGroup
  params: {
    accountName: 'sa${projectName}${environmentName}'
    location: location
    identityName: managedIdentity.outputs.managedIdentityName
  }
  dependsOn:[managedIdentity]
}

module openAIService 'core/ai/openai/openai-account.bicep' = {
  name: 'openAIService'
  scope: resourceGroup
  params: {
    name: '${abbreviations.openAI}-${projectName}-${environmentName}-${resourceToken}'
    location: location
    identityName: managedIdentity.outputs.managedIdentityName
    customSubdomain: 'openai-app-${resourceToken}'
  }
  dependsOn: [managedIdentity]
}

module search 'core/search/search-services.bicep' = {
  name: 'search'
  scope: resourceGroup
  params: {
    name: '${abbreviations.aiSearch}-${projectName}-${environmentName}-${resourceToken}'
    location: location
    semanticSearch: 'standard'
    disableLocalAuth: true
    identityName: managedIdentity.outputs.managedIdentityName
  }
  dependsOn:[storage]
}

module database 'app/database.bicep' = {
  name: 'database'
  scope: resourceGroup
  params: {
    accountName: '${abbreviations.cosmosDbAccount}-${projectName}-${environmentName}-${resourceToken}'
    location: 'centralus'
  }
}



module monitoring 'core/monitor/monitoring.bicep' = {
  name: 'monitoring'
  scope: resourceGroup
  params: {
    location: location
    logAnalyticsName: 'log-${projectName}-${environmentName}'
    applicationInsightsName: 'appi-${projectName}-${environmentName}'
    applicationInsightsDashboardName: 'appid-${projectName}-${environmentName}'
  }
}


module appServicePlanLinux 'core/host/app-service.bicep' = {
  name: 'appServicePlanLinux'
  scope: resourceGroup
  params: {
    location:location
    name:  'asp-lnx-${projectName}-${environmentName}-${resourceToken}'
    linuxMachine: true
  }
}

module loaderFunction 'app/loader-function.bicep' = {
  name: 'loaderFunction'
  scope: resourceGroup
  params: {
    appServicePlanName: appServicePlanLinux.outputs.appServicePlanName
    functionAppName: 'func-loader-${resourceToken}'
    location: location
    StorageBlobURL:storage.outputs.storageBlobURL
    StorageAccountName: storage.outputs.StorageAccountName
    logAnalyticsWorkspaceName: monitoring.outputs.logAnalyticsWorkspaceName
    appInsightsName: monitoring.outputs.applicationInsightsName
    OpenAIEndPoint: openAIService.outputs.endpoint
    identityName: managedIdentity.outputs.managedIdentityName
    AZURE_AI_SEARCH_ENDPOINT: search.outputs.endpoint
  }
  dependsOn:[appServicePlanLinux, monitoring,openAIService]
}

module appServicePlanWindows 'core/host/app-service.bicep' = {
  name: 'appServicePlanWindows'
  scope: resourceGroup
  params: {
    location:location
    name:  'asp-win-${projectName}-${environmentName}-${resourceToken}'
    linuxMachine: false
  }
}


module apiWebApp 'app/api-app.bicep' = {
  name: 'apiWebApp'
  scope: resourceGroup
  params: {
    appServicePlanName: appServicePlanWindows.outputs.appServicePlanName
    appServiceNameAPI: apiAppName
    location: location
    logAnalyticsWorkspaceName: monitoring.outputs.logAnalyticsWorkspaceName
    appInsightsName: monitoring.outputs.applicationInsightsName
    OpenAIEndPoint: openAIService.outputs.endpoint
    identityName: managedIdentity.outputs.managedIdentityName
    AZURE_AI_SEARCH_ENDPOINT: search.outputs.endpoint
  }
  dependsOn:[appServicePlanWindows, monitoring,openAIService]
}


module reactWebApp 'app/web-app.bicep' = {
  name: 'reactWebApp'
  scope: resourceGroup
  params: {
    appServicePlanName: appServicePlanLinux.outputs.appServicePlanName
    appServiceNameWeb: webAppName
    location: location
    appServiceNameAPI: apiAppName
  }
  dependsOn:[appServicePlanLinux, monitoring,openAIService]
}

output resourceGroupName string = resourceGroup.name
output functionAppName string = loaderFunction.outputs.functionAppName
output apiAppName string = apiAppName
output webAppName string = webAppName
output appServiceURL string = apiWebApp.outputs.appServiceURL
