param appServicePlanName string
param location string
param appServiceNameWeb string
param appServiceNameAPI string

resource appServicePlan 'Microsoft.Web/serverfarms@2022-03-01' existing = {
  name: appServicePlanName
}



resource appServiceNode 'Microsoft.Web/sites@2022-03-01' = {
  name: appServiceNameWeb
  location: location
  properties: {
    serverFarmId: appServicePlan.id
    siteConfig: {
      
      linuxFxVersion: 'NODE|18-lts'
      appCommandLine: 'pm2 serve /home/site/wwwroot --spa --no-daemon'
      appSettings: [
        {
          name: 'SCM_DO_BUILD_DURING_DEPLOYMENT'
          value: '0'
        }
        {
          name: 'REACT_APP_API_HOST'
          value: 'https://${appServiceNameAPI}.azurewebsites.net'
        }
      ]
    }
    publicNetworkAccess: 'Enabled'
    
  }
}


output appServiceURL string = 'https://${appServiceNameWeb}.azurewebsites.net'
