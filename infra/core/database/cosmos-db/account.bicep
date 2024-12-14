metadata description = 'Create an Azure Cosmos DB account.'

param name string
param location string = resourceGroup().location
param tags object = {}

@allowed(['GlobalDocumentDB', 'MongoDB', 'Parse'])
@description('Sets the kind of account.')
param kind string

@description('Enables serverless for this account. Defaults to false.')
param enableServerless bool = false

@description('Enables NoSQL vector search for this account. Defaults to false.')
param enableNoSQLVectorSearch bool = false

@description('Disables key-based authentication. Defaults to false.')
param disableKeyBasedAuth bool = false

resource account 'Microsoft.DocumentDB/databaseAccounts@2023-04-15' = {
  name: name
  location: location
  tags: tags
  kind: kind
  properties: {
    consistencyPolicy: {
      defaultConsistencyLevel: 'Session'
    }
    databaseAccountOfferType: 'Standard'
    locations: [
      {
        locationName: location
        failoverPriority: 0
        isZoneRedundant: false
      }
    ]
    enableAutomaticFailover: false
    enableMultipleWriteLocations: false
    apiProperties: (kind == 'MongoDB')
      ? {
          serverVersion: '4.2'
        }
      : {}
    disableLocalAuth: false
    capabilities: union(
      (enableServerless)
        ? [
            {
              name: 'EnableServerless'
            }
          ]
        : [],
      (enableNoSQLVectorSearch)
        ? [
            {
              name: 'EnableNoSQLVectorSearch'
            }
          ]
        : []
    )
  }
}

var roleDefinitionId = guid('sql-role-definition-', account.id)

resource cosmosDbRoleDefinition 'Microsoft.DocumentDB/databaseAccounts/sqlRoleDefinitions@2021-07-01-preview' = {
  name: roleDefinitionId
  parent: account  
  properties: {
    roleName: 'My Read Write Role'
    type: 'CustomRole'
    assignableScopes: [
      account.id
    ]
    permissions:[
      {
    dataActions: [
      
      'Microsoft.DocumentDB/databaseAccounts/readMetadata'
      'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers/items/read'
      'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers/executeQuery'
      'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers/readChangeFeed'
    ]
    
  }  
  ]
  }
}

output endpoint string = account.properties.documentEndpoint
output name string = account.name
