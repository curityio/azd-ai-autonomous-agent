
// See the Microsoft online resources
// - https://github.com/microsoft-foundry/foundry-samples/blob/main/infrastructure/infrastructure-setup-bicep/00-basic/main.bicep

param location string
param name string
param tags object = {}

resource aiFoundry 'Microsoft.CognitiveServices/accounts@2025-06-01' = {
  name: name
  tags: tags
  location: location
  identity: {
    type: 'SystemAssigned'
  }
  sku: {
    name: 'S0'
  }
  kind: 'AIServices'
  properties: {
    allowProjectManagement: true
    customSubDomainName: name
    disableLocalAuth: false
  }
}

// Use a project to provide API endpoints to applications
resource aiProject 'Microsoft.CognitiveServices/accounts/projects@2025-06-01' = {
  name: 'proj-default'
  tags: tags
  parent: aiFoundry
  location: location
  identity: {
    type: 'SystemAssigned'
  }
  properties: {}
}

// Deploy a LLM
resource modelDeployment 'Microsoft.CognitiveServices/accounts/deployments@2025-06-01'= {
  parent: aiFoundry
  name: 'gpt-4.1-mini'
  dependsOn: [aiProject]
  tags: tags
  sku: {
    capacity: 250
    name: 'GlobalStandard'
  }
  properties: {
    model: {
      name: 'gpt-4.1-mini'
      format: 'OpenAI'
      version: '2025-04-14'
    }
  }
}
