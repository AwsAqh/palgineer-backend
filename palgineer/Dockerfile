# 1. Build stage
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY *.sln .
COPY **/*.csproj ./
RUN dotnet restore

# Copy everything else and publish
COPY . .
RUN dotnet publish -c Release -o out

# 2. Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS runtime
WORKDIR /app

# Copy the published output
COPY --from=build /app/out .

# Let Kestrel listen on the PORT Render provides
ENV ASPNETCORE_URLS=http://*:${PORT}

ENTRYPOINT ["dotnet", "YourAppName.dll"]
