#!/usr/bin/env bash
set -e

# Automatically find the Web .csproj file (or set the path manually if you have multiple)
PROJECT_FILE=$(find . -maxdepth 2 -name "*.csproj" | head -n 1)

if [ -z "$PROJECT_FILE" ]; then
    echo "❌ Error: Could not find a .csproj file!"
    exit 1
fi

echo "🔧 Configuring .NET User Secrets for project: $PROJECT_FILE"

# Default values for local Docker SQL Server (can be overridden by passing env vars or pressing enter)
read -p "Enter DB Server [default: localhost,1434]: " DB_SERVER
DB_SERVER=${DB_SERVER:-localhost,1434}

read -p "Enter DB User [default: sa]: " DB_USER
DB_USER=${DB_USER:-sa}

read -p "Enter DB Password [default: Hello_World12]: " DB_PASS
echo ""
DB_PASS=${DB_PASS:-Hello_World12}

# Initialize user-secrets if not already initialized
dotnet user-secrets init --project "$PROJECT_FILE"

# Set the keys to match Program.cs
dotnet user-secrets set "DbServer" "$DB_SERVER" --project "$PROJECT_FILE"
dotnet user-secrets set "DbUser" "$DB_USER" --project "$PROJECT_FILE"
dotnet user-secrets set "DbPassword" "$DB_PASS" --project "$PROJECT_FILE"

echo ""
echo "✅ Done! Current secrets configured in ~/.microsoft/usersecrets:"
dotnet user-secrets list --project "$PROJECT_FILE"