# Start from the .NET SDK image
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

# Set working directory
WORKDIR /src

# Copy the project files
COPY BankoApi/ ./BankoApi

RUN ls -R /src

# Move to the correct directory containing the .csproj file
WORKDIR /src/BankoApi

RUN ls -R /src

# Restore dependencies
RUN dotnet restore

# Build the project
RUN dotnet build -c Debug -o /app/banko-api-dev-build

# Publish the project
RUN dotnet publish -c Debug -o /app/banko-api-dev-publish

# Final stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final-banko-api-dev
WORKDIR /app
COPY --from=build /app/banko-api-dev-publish .

# Expose the necessary port (change as needed)
EXPOSE 5119

# Define the entry point for the application
ENTRYPOINT ["dotnet", "BankoApi.dll"]
