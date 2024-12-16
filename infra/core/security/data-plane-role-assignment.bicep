metadata description = 'Assign RBAC role for data plane access to Azure Cosmos DB for NoSQL.'

@description('Name of the Azure Cosmos DB for NoSQL account.')
param accountName string

@description('Id of the role definition to assign to the targeted principal in the context of the account.')
param roleDefinitionId string

@description('Id of the identity/principal to assign this role in the context of the account.')
param identityId string

resource account 'Microsoft.DocumentDB/databaseAccounts@2024-05-15' existing = {
  name: accountName
}

resource assignment 'Microsoft.DocumentDB/databaseAccounts/sqlRoleAssignments@2024-05-15' = {
  name: guid(roleDefinitionId, identityId, account.id)
  parent: account
  properties: {
    principalId: identityId
    roleDefinitionId: roleDefinitionId
    scope: account.id
  }
}

output assignmentId string = assignment.id
