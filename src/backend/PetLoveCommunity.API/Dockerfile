# Multi-stage Dockerfile for Pet Love Community API
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 5000
EXPOSE 5001

# Install PostgreSQL client for health checks
RUN apt-get update && apt-get install -y postgresql-client && rm -rf /var/lib/apt/lists/*

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files
COPY ["src/backend/PetLoveCommunity.API/PetLoveCommunity.API.csproj", "src/backend/PetLoveCommunity.API/"]
COPY ["src/backend/PetLoveCommunity.Application/PetLoveCommunity.Application.csproj", "src/backend/PetLoveCommunity.Application/"]
COPY ["src/backend/PetLoveCommunity.Domain/PetLoveCommunity.Domain.csproj", "src/backend/PetLoveCommunity.Domain/"]
COPY ["src/backend/PetLoveCommunity.Infrastructure/PetLoveCommunity.Infrastructure.csproj", "src/backend/PetLoveCommunity.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "src/backend/PetLoveCommunity.API/PetLoveCommunity.API.csproj"

# Copy all source code
COPY . .

# Build the application
WORKDIR "/src/src/backend/PetLoveCommunity.API"
RUN dotnet build "PetLoveCommunity.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PetLoveCommunity.API.csproj" -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app

# Create uploads directory
RUN mkdir -p /app/uploads && chmod 755 /app/uploads

# Copy published application
COPY --from=publish /app/publish .

# Create non-root user for security
RUN groupadd -r petlove && useradd -r -g petlove petlove
RUN chown -R petlove:petlove /app
USER petlove

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:5000/api/health || exit 1

ENTRYPOINT ["dotnet", "PetLoveCommunity.API.dll"]