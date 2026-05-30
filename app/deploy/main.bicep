// Azure infrastructure for the Rigol web stream:
//   - Storage account (required by the Functions runtime)
//   - Linux consumption plan + .NET 9 isolated Function App (the API)
//   - Static Web App (the React SPA)
//
// Deploy:  az deployment group create -g <rg> -f main.bicep -p namePrefix=rigol

@description('Prefix for all resource names (3-11 lowercase chars).')
@minLength(3)
@maxLength(11)
param namePrefix string = 'rigol'

@description('Location for the API + storage.')
param location string = resourceGroup().location

@description('Oscilloscope mode: Simulated or Scpi.')
@allowed(['Simulated', 'Scpi'])
param oscilloscopeMode string = 'Simulated'

@description('VISA resource string when mode is Scpi (e.g. TCPIP::host::INSTR).')
param oscilloscopeResource string = ''

var suffix = uniqueString(resourceGroup().id)
var storageName = toLower('${namePrefix}st${substring(suffix, 0, 6)}')
var planName = '${namePrefix}-plan'
var functionAppName = '${namePrefix}-api-${substring(suffix, 0, 6)}'
var swaName = '${namePrefix}-web'

resource storage 'Microsoft.Storage/storageAccounts@2023-01-01' = {
  name: storageName
  location: location
  sku: { name: 'Standard_LRS' }
  kind: 'StorageV2'
  properties: {
    minimumTlsVersion: 'TLS1_2'
    allowBlobPublicAccess: false
  }
}

resource plan 'Microsoft.Web/serverfarms@2023-12-01' = {
  name: planName
  location: location
  sku: { name: 'Y1', tier: 'Dynamic' }
  kind: 'functionapp'
  properties: { reserved: true } // Linux
}

resource functionApp 'Microsoft.Web/sites@2023-12-01' = {
  name: functionAppName
  location: location
  kind: 'functionapp,linux'
  identity: { type: 'SystemAssigned' }
  properties: {
    serverFarmId: plan.id
    httpsOnly: true
    siteConfig: {
      linuxFxVersion: 'DOTNET-ISOLATED|9.0'
      ftpsState: 'Disabled'
      minTlsVersion: '1.2'
      cors: {
        allowedOrigins: ['https://${swaName}.azurestaticapps.net']
      }
      appSettings: [
        { name: 'AzureWebJobsStorage', value: 'DefaultEndpointsProtocol=https;AccountName=${storage.name};EndpointSuffix=${environment().suffixes.storage};AccountKey=${storage.listKeys().keys[0].value}' }
        { name: 'FUNCTIONS_EXTENSION_VERSION', value: '~4' }
        { name: 'FUNCTIONS_WORKER_RUNTIME', value: 'dotnet-isolated' }
        { name: 'Oscilloscope__Mode', value: oscilloscopeMode }
        { name: 'Oscilloscope__Resource', value: oscilloscopeResource }
      ]
    }
  }
}

resource swa 'Microsoft.Web/staticSites@2023-12-01' = {
  name: swaName
  location: location
  sku: { name: 'Free', tier: 'Free' }
  properties: {}
}

output apiHostName string = functionApp.properties.defaultHostName
output webHostName string = swa.properties.defaultHostname
