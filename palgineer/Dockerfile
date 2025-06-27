# ───────────────────────────────
# 1. Build stage
# ───────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution & project
COPY *.sln   ./
COPY palgineer/palgineer.csproj palgineer/

# Restore only that project
RUN dotnet restore palgineer/palgineer.csproj

# Copy rest & publish
COPY . .
WORKDIR /src/palgineer
RUN dotnet publish -c Release -o /app/out

# ───────────────────────────────
# 2. Runtime stage
# ───────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/out ./

ENV ASPNETCORE_URLS=http://*:${PORT}
ENTRYPOINT ["dotnet", "palgineer.dll"]
