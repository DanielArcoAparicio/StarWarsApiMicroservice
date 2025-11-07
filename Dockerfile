# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY StarWars.sln .
COPY src/StarWars.Api/StarWars.Api.csproj src/StarWars.Api/
COPY src/StarWars.Domain/StarWars.Domain.csproj src/StarWars.Domain/
COPY src/StarWars.Application/StarWars.Application.csproj src/StarWars.Application/
COPY src/StarWars.Infrastructure/StarWars.Infrastructure.csproj src/StarWars.Infrastructure/
COPY src/StarWars.Client/StarWars.Client.csproj src/StarWars.Client/

# Restore dependencies (solo los proyectos necesarios, excluyendo tests)
RUN dotnet restore src/StarWars.Api/StarWars.Api.csproj

# Copy the rest of the source code
COPY . .

# Build the API project
WORKDIR /src/src/StarWars.Api
RUN dotnet build -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Final runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

EXPOSE 8080
EXPOSE 8081

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "StarWars.Api.dll"]

