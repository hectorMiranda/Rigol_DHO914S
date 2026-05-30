# Deploying to Azure

The API runs as a .NET 9 isolated **Function App** (Linux consumption) and the
SPA as an **Azure Static Web App**. `deploy/main.bicep` provisions both plus the
storage account the Functions runtime needs.

## 1. Provision infrastructure

```bash
az group create -n rigol-rg -l westus2
az deployment group create -g rigol-rg -f app/deploy/main.bicep \
  -p namePrefix=rigol oscilloscopeMode=Simulated
```

Outputs `apiHostName` and `webHostName`. For real hardware set
`oscilloscopeMode=Scpi oscilloscopeResource='TCPIP::192.168.1.50::INSTR'` — note
the Function App must have network line-of-sight to the instrument (VNet
integration to the bench network).

## 2. Publish the API

```bash
cd app/backend/src/RigolStream.Api
func azure functionapp publish <apiHostName-without-suffix>
# or: dotnet publish -c Release && zip deploy via `az functionapp deployment source config-zip`
```

## 3. Publish the SPA

```bash
cd app/frontend/rigol-web
echo "VITE_API_BASE=https://<apiHostName>" > .env.production
npm ci && npm run build
npx @azure/static-web-apps-cli deploy ./dist --deployment-token <swa-token>
```

`VITE_API_BASE` points the SPA at the deployed API (the dev proxy only applies to
`npm run dev`). CORS for the SWA origin is already configured on the Function App
by the Bicep template.

## Container alternative

`docker compose up --build` from `app/` runs the whole stack locally (API +
nginx-served SPA); the same images deploy to any container host.
