param appServicePlanName string
param appServiceNameAPI string
param location string
param identityName string
param OpenAIEndPoint string
param logAnalyticsWorkspaceName string
param appInsightsName string
param AZURE_AI_SEARCH_ENDPOINT string


resource appServicePlan 'Microsoft.Web/serverfarms@2022-03-01' existing = {
  name: appServicePlanName
}

resource appInsights 'Microsoft.Insights/components@2020-02-02' existing = {
  name: appInsightsName
}

resource managedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' existing = {
  name: identityName
}

resource appServiceAPI 'Microsoft.Web/sites@2022-03-01' = {
  name: appServiceNameAPI
  location: location
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${managedIdentity.id}': {}
    }
  }
  properties: {
    serverFarmId: appServicePlan.id
    siteConfig: {
      // Set the runtime stack to .NET 8
      netFrameworkVersion: 'v8.0'  // Use .NET 8.0
      use32BitWorkerProcess: false  // Optional: Use 64-bit process

      // Optional: specify the startup command for custom deployment scenarios
      appCommandLine: ''  

      appSettings: [
        {
          name: 'SCM_DO_BUILD_DURING_DEPLOYMENT'
          value: '1'
        }        
        {
          name: 'AZURE_OPENAI_ENDPOINT'
          value: OpenAIEndPoint
        }         
        {
          name: 'AZURE_OPENAI_EMBEDDING'
          value: 'text-embedding'
        } 
        {
          name: 'AZURE_OPENAI_DEPLOYMENT'
          value: 'gpt-chat'
        }    
        {
          name: 'AZURE_OPENAI_API_VERSION'
          value: '2023-05-15'
        }
        {
          name: 'CosmosDb_ConnectionString'
          value: ''
        }
        {
          name: 'CosmosDb_Database'
          value: 'chatdatabase'
        } 
        {
          name: 'CosmosDb_ProductContainer'
          value: 'products'
        } 
        {
          name: 'CosmosDb_ChatContainer'
          value: 'chathistory'
        }        
        {
          name: 'AZURE_AI_SEARCH_ENDPOINT'
          value: AZURE_AI_SEARCH_ENDPOINT
        }      
        {
          name: 'AZURE_AI_SEARCH_INDEX'
          value: 'azure-support'
        } 
        {
          name: 'AZURE_CLIENT_ID'
          value: managedIdentity.properties.clientId
        } 
        {
          name: 'APPINSIGHTS_INSTRUMENTATIONKEY'
          value: appInsights.properties.InstrumentationKey
        }
        {
          name: 'ApplicationInsightsAgent_EXTENSION_VERSION'
          value: '~3'
        }
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: appInsights.properties.ConnectionString
        }
      ]
      alwaysOn: true
    }
    publicNetworkAccess: 'Enabled'
  }
}


resource logAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2021-06-01'  existing =  {
  name: logAnalyticsWorkspaceName
}

resource diagnosticSettingsAPI 'Microsoft.Insights/diagnosticSettings@2021-05-01-preview' = {
  name: '${appServiceNameAPI}-diagnostic'
  scope: appServiceAPI
  properties: {
    workspaceId: logAnalyticsWorkspace.id
    logs: [
      {
        category: 'AppServiceHTTPLogs'
        enabled: true
        retentionPolicy: {
          enabled: false
          days: 0
        }
      }
      {
        category: 'AppServiceConsoleLogs'
        enabled: true
        retentionPolicy: {
          enabled: false
          days: 0
        }
      }
      {
        category: 'AppServiceAppLogs'
        enabled: true
        retentionPolicy: {
          enabled: false
          days: 0
        }
      }
    ]
    metrics: [
      {
        category: 'AllMetrics'
        enabled: true
        retentionPolicy: {
          enabled: false
          days: 0
        }
      }
    ]
  }
}

output appServiceURL string = 'https://${appServiceNameAPI}.azurewebsites.net'
