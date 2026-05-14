#!/usr/bin/env bash
# Deploy E-Greetings infra to Azure.
# Required env vars:
#   SQL_ADMIN_PASSWORD  (strong password, min 12 chars)
#   JWT_SECRET_KEY      (64-byte base64 — generate with: openssl rand -base64 64)
# Optional:
#   RG, LOCATION, APP_NAME, ENVIRONMENT
set -euo pipefail

RG="${RG:-egreetings-prod-rg}"
LOCATION="${LOCATION:-southeastasia}"
APP_NAME="${APP_NAME:-egreetings}"
ENVIRONMENT="${ENVIRONMENT:-prod}"

: "${SQL_ADMIN_PASSWORD:?Set SQL_ADMIN_PASSWORD before running.}"
: "${JWT_SECRET_KEY:?Set JWT_SECRET_KEY before running (e.g. openssl rand -base64 64).}"

SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"

echo "[deploy] Resource group: $RG ($LOCATION)"
az group create --name "$RG" --location "$LOCATION" --output none

EXTRA_ARGS=()
if [[ "${1:-}" == "--what-if" ]]; then
  EXTRA_ARGS+=(--what-if)
  echo "[deploy] Running what-if (no resources will be changed)..."
fi

az deployment group create \
  --resource-group "$RG" \
  --template-file "$SCRIPT_DIR/main.bicep" \
  --parameters \
      appName="$APP_NAME" \
      environment="$ENVIRONMENT" \
      sqlAdminPassword="$SQL_ADMIN_PASSWORD" \
      jwtSecretKey="$JWT_SECRET_KEY" \
  "${EXTRA_ARGS[@]}"

if [[ "${1:-}" != "--what-if" ]]; then
  echo "[deploy] Deployment complete. Outputs:"
  az deployment group show \
    --resource-group "$RG" \
    --name main \
    --query "properties.outputs" --output table || true
fi
