# Pet Love Community API - Debugging Guide

## Local Development Debugging Setup

### Prerequisites
- Docker Desktop installed and running
- .NET 9.0 SDK installed
- Your IDE (VS Code, Rider, or Visual Studio)

## Quick Start - Local API with Docker Database

### 1. Start Database Services
```bash
# Navigate to project root
cd /Users/robsoto/Developer/Projects/PetLoveCommunity/petlovecommunity-api

# Start only database services for debugging
docker-compose -f docker-compose.debug.yml up -d

# Verify services are running
docker-compose -f docker-compose.debug.yml ps
```

### 2. Run Database Migrations
```bash
# Run migrations against Docker database
dotnet ef database update --project src/backend/PetLoveCommunity.Infrastructure --startup-project src/backend/PetLoveCommunity.API
```

### 3. Start API for Debugging
Choose one of these approaches:

#### Option A: IDE Debugging
- Open project in your IDE
- Select "Debug with Docker DB" launch profile
- Start debugging (F5)
- API will run on https://localhost:7219

#### Option B: Command Line
```bash
# Navigate to API project
cd src/backend/PetLoveCommunity.API

# Run with development configuration
dotnet run --launch-profile "Debug with Docker DB"
```

### 4. Verify Everything Works
Visit these URLs to verify setup:
- **Swagger UI**: https://localhost:7219/swagger
- **Health Check**: https://localhost:7219/api/health
- **Database Health**: https://localhost:7219/api/health/database
- **Detailed Health**: https://localhost:7219/api/health/detailed
- **pgAdmin**: http://localhost:5050 (admin@petlove.com / admin123)

## Debugging Profiles Available

1. **Debug with Docker DB** - Recommended for local debugging
   - Enhanced EF Core logging
   - API runs locally with debugger
   - Database in Docker

2. **Debug Health Check** - Opens health check endpoint directly
   - Good for verifying database connectivity

3. **Production-like** - Simulates production environment
   - Minimal logging
   - HTTP only

4. **Docker** - Full container debugging
   - Both API and database in containers

## Configuration Sources (Priority Order)

1. **User Secrets** (highest priority for sensitive data)
2. **appsettings.Development.json** (development overrides)
3. **appsettings.json** (base configuration)
4. **Environment Variables**

## Database Configuration

### Local Debugging (localhost)
```json
{
  "Database": {
    "Host": "localhost",  // Docker port forwarding
    "Port": 5432
  }
}
```

### Container Debugging (docker network)
```json
{
  "Database": {
    "Host": "db",  // Docker service name
    "Port": 5432
  }
}
```

## Common Issues & Solutions

### Database Connection Issues
1. **Check if Docker services are running**:
   ```bash
   docker-compose -f docker-compose.debug.yml ps
   ```

2. **Verify database is ready**:
   ```bash
   docker-compose -f docker-compose.debug.yml logs db
   ```

3. **Test direct database connection**:
   ```bash
   docker exec -it petlove-db-debug psql -U petlove_user -d petlove_dev
   ```

### Entity Framework Issues
1. **Check if migrations are applied**:
   ```bash
   dotnet ef migrations list --project src/backend/PetLoveCommunity.Infrastructure --startup-project src/backend/PetLoveCommunity.API
   ```

2. **Apply migrations**:
   ```bash
   dotnet ef database update --project src/backend/PetLoveCommunity.Infrastructure --startup-project src/backend/PetLoveCommunity.API
   ```

### Port Conflicts
If ports 5432, 6379, or 5050 are in use:
1. Stop conflicting services
2. Or modify ports in docker-compose.debug.yml

## Useful Commands

### Database Management
```bash
# Start only database services
docker-compose -f docker-compose.debug.yml up -d

# Stop all debug services
docker-compose -f docker-compose.debug.yml down

# View database logs
docker-compose -f docker-compose.debug.yml logs -f db

# Connect to database directly
docker exec -it petlove-db-debug psql -U petlove_user -d petlove_dev
```

### API Development
```bash
# Build solution
dotnet build

# Run tests
dotnet test

# Check EF migrations
dotnet ef migrations list --project src/backend/PetLoveCommunity.Infrastructure --startup-project src/backend/PetLoveCommunity.API

# Create new migration
dotnet ef migrations add MigrationName --project src/backend/PetLoveCommunity.Infrastructure --startup-project src/backend/PetLoveCommunity.API
```

## Environment Variables in .env
```env
POSTGRES_USER=petlove_user
POSTGRES_PASSWORD=petlove_password
JWT_KEY=MkUeixa5d1pjKxSPItMLRR+Dv5zS3P3f79USvXANIow=
JWT_ISSUER=PetLoveCommunity
JWT_AUDIENCE=PetLoveCommunityUsers
```

## Health Check Endpoints

- `/api/health` - Basic API health
- `/api/health/database` - Database connectivity
- `/api/health/detailed` - Comprehensive system check

These endpoints are especially useful for debugging connectivity issues.

## Logging Configuration

Development logging shows:
- EF Core SQL queries
- Database command execution
- Request/response information
- Detailed error information

Check console output when debugging for detailed logs.