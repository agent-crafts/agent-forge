#!/usr/bin/env bash
set -euo pipefail

# A stale obj/project.assets.json can omit newly added package references,
# producing CS0246/CS0234 even though the csproj declares the dependencies.
root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$root"

echo "Restoring dependencies and regenerating NuGet assets..."
dotnet restore AgentForge.Web/AgentForge.Web.csproj --force-evaluate

echo "Checking resolved Application dependencies..."
dotnet list AgentForge.Application/AgentForge.Application.csproj package

echo "Building AgentForge.Web (including Application and Infrastructure)..."
dotnet build AgentForge.Web/AgentForge.Web.csproj --no-restore
