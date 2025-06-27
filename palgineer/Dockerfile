# ───────────────────────────────
# 1. Build stage
# ───────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src

# Copy only your .csproj and solution (if you have one) so restore layers cache
# (adjust the .sln name if yours is different)
COPY *.sln   ./
COPY palgineer/palgineer.csproj palgineer/

# Restore just that project
RUN dotnet restore palgineer/palgineer.csproj

# Copy everything else and publish
COPY . .
WORKDIR /src/palgineer
RUN dotnet publish -c Release -o /app/out

# ───────────────────────────────
# 2. Runtime stage
# ───────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS runtime
WORKDIR /app

# Pull in the published output  
COPY --from=build /app/out ./

# Expose the port Render sets
ENV ASPNETCORE_URLS=http://*:${PORT}

ENTRYPOINT ["dotnet", "palgineer.dll"]
