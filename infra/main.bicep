// ──────────────────────────────────────────────────────────────────────
// E-Greetings — Azure deployment template
// Provisions: SQL Server + DB, App Service Plan (Linux B1), Backend Web App,
//             Frontend Static Web App, Key Vault for secrets.
// Run via infra/deploy.sh.
// ──────────────────────────────────────────────────────────────────────

@description('Azure region for all resources.')
param location string = resourceGroup().location

@description('Short app name; resource names are derived from this.')
param appName string = 'egreetings'

@description('Environment suffix: dev / staging / prod.')
@allowed([ 'dev', 'staging', 'prod' ])
param environment string = 'prod'

@description('SQL admin login name.')
param sqlAdminLogin string = 'sqladmin'

@secure()
@description('SQL admin password. Min 12 chars, mixed case + digit + symbol.')
param sqlAdminPassword string

@secure()
@description('JWT signing key (≥ 64 bytes recommended). Stored in Key Vault.')
param jwtSecretKey string

var resourcePrefix = '${appName}-${environment}'
var sqlServerName  = '${resourcePrefix}-sql'
var sqlDbName      = 'EGreetingsDb'
var planName       = '${resourcePrefix}-plan'
var backendName    = '${resourcePrefix}-backend'
var frontendName   = '${resourcePrefix}-frontend'
var keyVaultName   = take(replace('${resourcePrefix}-kv', '-', ''), 24)

// ── Azure SQL ─────────────────────────────────────────────────────────
resource sqlServer 'Microsoft.Sql/servers@2023-05-01-preview' = {
  name: sqlServerName
  location: location
  properties: {
    administratorLogin: sqlAdminLogin
    administratorLoginPassword: sqlAdminPassword
    minimalTlsVersion: '1.2'
    publicNetworkAccess: 'Enabled'
  }
}

resource sqlDb 'Microsoft.Sql/servers/databases@2023-05-01-preview' = {
  parent: sqlServer
  name: sqlDbName
  location: location
  sku: { name: 'Basic', tier: 'Basic' }
}

resource sqlFirewallAzureSvc 'Microsoft.Sql/servers/firewallRules@2023-05-01-preview' = {
  parent: sqlServer
  name: 'AllowAzureServices'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

// ── App Service Plan (Linux, B1) ──────────────────────────────────────
resource appPlan 'Microsoft.Web/serverfarms@2023-12-01' = {
  name: planName
  location: location
  sku: {
    name: 'B1'
    tier: 'Basic'
  }
  kind: 'linux'
  properties: {
    reserved: true
  }
}

// ── Backend Web App (.NET 9) ──────────────────────────────────────────
resource backend 'Microsoft.Web/sites@2023-12-01' = {
  name: backendName
  location: location
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: appPlan.id
    httpsOnly: true
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|9.0'
      alwaysOn: true
      ftpsState: 'Disabled'
      minTlsVersion: '1.2'
      appSettings: [
        {
          name: 'ASPNETCORE_ENVIRONMENT'
          value: 'Production'
        }
        {
          name: 'ConnectionStrings__DefaultConnection'
          value: 'Server=tcp:${sqlServer.properties.fullyQualifiedDomainName},1433;Database=${sqlDbName};User Id=${sqlAdminLogin};Password=${sqlAdminPassword};Encrypt=True;TrustServerCertificate=False;Connection Timeout=30'
        }
        {
          name: 'JwtSettings__SecretKey'
          value: jwtSecretKey
        }
        {
          name: 'KeyVault__Uri'
          value: 'https://${keyVaultName}${az.environment().suffixes.keyvaultDns}'
        }
      ]
    }
  }
}

// ── Frontend Static Web App ───────────────────────────────────────────
resource frontend 'Microsoft.Web/staticSites@2023-12-01' = {
  name: frontendName
  location: location
  sku: {
    name: 'Free'
    tier: 'Free'
  }
  properties: {}
}

// ── Key Vault (secrets store, RBAC-authorized) ────────────────────────
resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' = {
  name: keyVaultName
  location: location
  properties: {
    sku: {
      family: 'A'
      name: 'standard'
    }
    tenantId: subscription().tenantId
    enableRbacAuthorization: true
    enableSoftDelete: true
    softDeleteRetentionInDays: 7
    publicNetworkAccess: 'Enabled'
  }
}

resource jwtSecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: 'JwtSettings--SecretKey'
  properties: {
    value: jwtSecretKey
  }
}

// Grant the backend app's managed identity "Key Vault Secrets User" role.
var kvSecretsUserRoleId = '4633458b-17de-408a-b874-0445c86b69e6'
resource kvRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: keyVault
  name: guid(keyVault.id, backend.id, kvSecretsUserRoleId)
  properties: {
    principalId: backend.identity.principalId
    principalType: 'ServicePrincipal'
    roleDefinitionId: subscriptionResourceId(
      'Microsoft.Authorization/roleDefinitions',
      kvSecretsUserRoleId
    )
  }
}

// ── Outputs ───────────────────────────────────────────────────────────
output backendUrl  string = 'https://${backend.properties.defaultHostName}'
output frontendUrl string = 'https://${frontend.properties.defaultHostname}'
output sqlServerFqdn string = sqlServer.properties.fullyQualifiedDomainName
output keyVaultUri string = 'https://${keyVaultName}${az.environment().suffixes.keyvaultDns}'
