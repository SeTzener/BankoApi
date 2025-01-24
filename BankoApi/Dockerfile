# Base image
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app

# Build App 1
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-banko-api-dev
WORKDIR /src
COPY banko-api-dev/ ./
RUN dotnet restore
RUN dotnet build -c Release -o /app/banko-api-dev-build
RUN dotnet publish -c Release -o /app/banko-api-dev-publish

# Build App 2
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-banko-api
WORKDIR /src
COPY banko-api/ ./
RUN dotnet restore
RUN dotnet build -c Release -o /app/banko-api-build
RUN dotnet publish -c Release -o /app/banko-api-publish

# Final image for App 1
FROM base AS final-banko-api-dev
WORKDIR /app
COPY --from=build-banko-api-dev /app/banko-api-dev-publish .
ENTRYPOINT ["dotnet", "banko-api-dev.dll"]

# Final image for App 2
FROM base AS final-banko-api
WORKDIR /app
COPY --from=build-banko-api /app/banko-api-publish .
ENTRYPOINT ["dotnet", "banko-api.dll"]