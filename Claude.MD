# Pet Love Community - Backend API Development Guide

## Project Overview
Backend API development for Pet Love Community using ASP.NET Core 8.0, PostgreSQL, and Entity Framework Core. This guide focuses on building a robust, scalable API with authentication, pet adoption, event management, and marketplace features.

## Development Environment Setup

### Prerequisites (macOS)
- macOS 12.0+ (Apple Silicon M4 optimized)
- JetBrains Rider 2024.1+
- Docker Desktop for Mac
- Git with GitHub integration

### Required Tools Installation
```bash
# Install via Homebrew
brew install --cask dotnet-sdk
brew install --cask docker
brew install postgresql@15
brew install azure-cli
brew install git
```

### Project Structure
```
PetLoveCommunity/
├── .github/workflows/          # GitHub Actions CI/CD
├── docker-compose.yml          # Local development containers
├── Claude.MD                   # This file - Claude Code integration
├── README.md                   # Project documentation
├── .gitignore                  # Git ignore rules
├── scripts/                    # Database and deployment scripts
└── src/backend/
    ├── PetLoveCommunity.API/           # Web API project
    ├── PetLoveCommunity.Core/          # Domain models and interfaces
    ├── PetLoveCommunity.Infrastructure/ # Data access and external services
    └── PetLoveCommunity.Tests/         # Unit and integration tests
```

## Technology Stack
- **Backend**: ASP.NET Core 9.0
- **Database**: PostgreSQL 15+ with Entity Framework Core
- **Authentication**: JWT with OAuth 2.0 (Google, Facebook, Apple)
- **Real-time**: SignalR for notifications
- **AI Integration**: OpenAI API for chatbot
- **Cloud**: Microsoft Azure (App Service, Database, Blob Storage)
- **Testing**: xUnit, Moq, Entity Framework InMemory
- **IDE**: JetBrains Rider with .NET 8.0 support

## Git Workflow

### Repository Setup
1. Create GitHub repository: `pet-love-community`
2. Clone locally: `git clone https://github.com/yourusername/pet-love-community.git`
3. Set up development branch: `git checkout -b develop`

### Branch Strategy
- `main`: Production releases
- `develop`: Development integration
- `feature/`: Feature development
- `hotfix/`: Production fixes

### Commit Convention
```
feat: add user authentication
fix: resolve database connection issue
docs: update API documentation
test: add integration tests for pet endpoints
```

## Database Schema

### Core Entities
- **Users**: Authentication, profiles, roles (Free, Premium, Vendor, Shelter, Admin)
- **Pets**: Adoption listings with photos, details, status
- **Events**: Community events with location, booking, RSVP
- **Vendors**: Service providers with listings, ratings, bookings
- **Posts**: Social feed with comments, likes, shares
- **Bookings**: Service appointments and scheduling
- **Coupons**: Vendor promotions and discounts
- **Notifications**: User alerts and messaging

### Key Relationships
- User → Posts (one-to-many)
- User → Pets (many-to-many for favorites)
- Vendor → Services (one-to-many)
- Event → Bookings (one-to-many)
- Post → Comments (one-to-many)

## Development Phases

### Phase 1: Foundation (Weeks 1-4)
**Priority**: Critical - Must be completed first

#### SETUP-001: Development Environment
- [x] .NET 8.0 SDK installation
- [x] Docker Desktop setup
- [x] PostgreSQL 15+ local instance
- [x] JetBrains Rider configuration
- [x] Azure CLI setup
- [x] Git repository initialization

#### SETUP-002: Project Structure
- [x] Solution and project creation
- [x] Package references setup
- [x] Docker compose configuration
- [x] Environment configuration files
- [x] CI/CD pipeline setup

#### SETUP-003: Database Schema
- [ ] PostgreSQL database design
- [ ] Entity Framework models
- [ ] Migration scripts
- [ ] Seed data setup
- [ ] Database indexes and constraints

### Phase 2: Core Backend (Weeks 5-8)
**Priority**: Critical - Core functionality

#### BACKEND-001: Infrastructure Setup
- [ ] ASP.NET Core 8.0 foundation
- [ ] Dependency injection configuration
- [ ] Entity Framework Core with PostgreSQL
- [ ] CORS policies configuration
- [ ] Structured logging with Serilog
- [ ] Global exception handling middleware
- [ ] Base repository pattern implementation

#### BACKEND-002: Authentication System
- [ ] JWT authentication implementation
- [ ] User registration and login endpoints
- [ ] Password reset functionality
- [ ] OAuth 2.0 integration (Google, Facebook, Apple)
- [ ] Role-based authorization
- [ ] Account verification system
- [ ] Security middleware setup

#### BACKEND-003: Core APIs
- [ ] User management APIs
- [ ] Pet adoption CRUD operations
- [ ] Event management system
- [ ] Basic social features
- [ ] File upload functionality
- [ ] Search and filtering

### Phase 3: Advanced Features (Weeks 9-12)
**Priority**: Medium - Enhanced functionality

#### BACKEND-004: Marketplace & Vendors
- [ ] Vendor registration and verification
- [ ] Service/product listing management
- [ ] Booking system implementation
- [ ] Payment processing integration
- [ ] Rating and review system
- [ ] Analytics dashboard

#### BACKEND-005: Social & Community
- [ ] Post creation and management
- [ ] Comment and like system
- [ ] User following/followers
- [ ] News feed algorithm
- [ ] Content moderation tools
- [ ] Real-time notifications with SignalR

#### BACKEND-006: AI Integration
- [ ] OpenAI API integration
- [ ] Chatbot conversation management
- [ ] Pet matching algorithms
- [ ] Content recommendation engine
- [ ] Automated content moderation

### Phase 4: Testing & Deployment (Weeks 13-16)
**Priority**: High - Quality and launch

#### QA-001: Testing Suite
- [ ] Unit tests for all services
- [ ] Integration tests for APIs
- [ ] Performance testing
- [ ] Security testing
- [ ] Load testing

#### DEPLOY-001: Production Deployment
- [ ] Azure App Service configuration
- [ ] Database deployment scripts
- [ ] SSL certificate setup
- [ ] CI/CD pipeline optimization
- [ ] Monitoring and logging setup
- [ ] Backup and disaster recovery

## API Specifications

### Authentication Endpoints
```
POST /api/auth/register
POST /api/auth/login
POST /api/auth/refresh
POST /api/auth/logout
POST /api/auth/forgot-password
POST /api/auth/reset-password
GET  /api/auth/verify-email
```

### Pet Adoption Endpoints
```
GET    /api/pets                 # List pets with filtering
GET    /api/pets/{id}            # Get pet details
POST   /api/pets                 # Create pet listing
PUT    /api/pets/{id}            # Update pet listing
DELETE /api/pets/{id}            # Delete pet listing
POST   /api/pets/{id}/favorite   # Add to favorites
DELETE /api/pets/{id}/favorite   # Remove from favorites
POST   /api/pets/{id}/apply      # Submit adoption application
```

### Event Management Endpoints
```
GET    /api/events               # List events
GET    /api/events/{id}          # Get event details
POST   /api/events               # Create event
PUT    /api/events/{id}          # Update event
DELETE /api/events/{id}          # Delete event
POST   /api/events/{id}/rsvp     # RSVP to event
DELETE /api/events/{id}/rsvp     # Cancel RSVP
```

### Marketplace Endpoints
```
GET    /api/vendors              # List vendors
GET    /api/vendors/{id}         # Get vendor details
POST   /api/vendors              # Register vendor
GET    /api/services             # List services
POST   /api/services             # Create service listing
POST   /api/bookings             # Create booking
GET    /api/bookings/{id}        # Get booking details
```

## Security Requirements

### Authentication & Authorization
- JWT tokens with 15-minute expiry
- Refresh tokens with 7-day expiry
- Role-based access control
- Rate limiting on all endpoints
- Input validation and sanitization

### Data Protection
- Encryption at rest and in transit
- PII data handling compliance
- Secure file upload validation
- API key management with Azure Key Vault
- Audit logging for sensitive operations

## Performance Requirements

### Response Times
- API endpoints: < 200ms average
- Database queries: < 100ms
- File uploads: < 5MB, processed in < 30s
- Real-time notifications: < 1s delivery

### Scalability
- Support 10,000 concurrent users
- Handle 1M+ pet listings
- Process 100K+ events monthly
- Store 10M+ images efficiently

## Error Handling

### HTTP Status Codes
- 200: Success
- 201: Created
- 400: Bad Request (with detailed error messages)
- 401: Unauthorized
- 403: Forbidden
- 404: Not Found
- 422: Validation Error
- 500: Internal Server Error

### Error Response Format
```json
{
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "Invalid input data",
    "details": [
      {
        "field": "email",
        "message": "Email is required"
      }
    ],
    "timestamp": "2024-01-01T00:00:00Z",
    "correlationId": "abc123"
  }
}
```

## Development Guidelines

### Code Standards
- Follow C# best practices and naming conventions
- Use async/await for all I/O operations
- Implement proper error handling and logging
- Write comprehensive unit tests (80%+ coverage)
- Use meaningful variable and method names
- Document complex business logic
- Follow SOLID principles

### Testing Strategy
- Unit tests: 80%+ code coverage
- Integration tests for API endpoints
- Performance testing for high-load scenarios
- Security testing for authentication flows
- End-to-end testing for critical user journeys

## Local Development

### Quick Start
1. Clone repository: `git clone https://github.com/yourusername/pet-love-community.git`
2. Run setup script: `./scripts/setup.sh`
3. Start services: `docker-compose up -d`
4. Run migrations: `dotnet ef database update`
5. Start API: `dotnet run --project src/backend/PetLoveCommunity.API`

### Development Workflow
1. Create feature branch: `git checkout -b feature/user-authentication`
2. Implement feature with tests
3. Run tests: `dotnet test`
4. Create pull request to `develop`
5. Code review and merge

## Business Logic Rules

### Pet Adoption Rules
- Only verified shelters can list pets without payment
- Premium users can list up to 5 pets monthly
- Pet listings expire after 90 days
- Adoption applications require user verification
- Shelter response time: 48 hours maximum

### Event Management Rules
- Free users can view all events
- Premium users can create public events
- Shelters can create adoption events
- Event capacity management required
- Cancellation policy: 24 hours notice

### Marketplace Rules
- Vendor verification required
- Service area limitations by subscription
- Booking cancellation: 24 hours notice
- Review system: verified bookings only
- Coupon usage: once per user per vendor

## Monitoring & Logging

### Application Monitoring
- Response time tracking
- Error rate monitoring
- Database performance metrics
- Resource utilization alerts

### Logging Strategy
- Structured logging with Serilog
- Log levels: Debug, Info, Warning, Error, Fatal
- Correlation IDs for request tracing
- PII data exclusion from logs
- Log retention: 30 days development, 1 year production

## Claude Code Integration

### Usage Instructions
1. Ensure Claude.MD is in project root
2. Run `claude-code` from terminal in project directory
3. Use natural language to describe development tasks
4. Claude will read this file for context and project understanding

### Example Commands
```bash
# Start development session
claude-code

# Example prompts:
"Create the User entity with authentication fields"
"Implement JWT authentication middleware"
"Add unit tests for the pet adoption service"
"Set up Entity Framework migrations"
"Create the database schema based on the requirements"
```

### Best Practices
- Keep Claude.MD updated with current progress
- Use checkboxes to track completed tasks
- Include relevant code snippets and examples
- Document any deviations from the original plan

## Next Steps

### Immediate Actions
1. **SETUP-001**: Complete development environment setup
2. **SETUP-002**: Initialize project structure
3. **SETUP-003**: Design and implement database schema
4. **BACKEND-001**: Set up core infrastructure

### Development Process
1. Start with Phase 1: Foundation
2. Build incrementally with testing
3. Regular code reviews and refactoring
4. Performance monitoring and optimization
5. Security audits and penetration testing

### Documentation Maintenance
- Update Claude.MD with progress
- Maintain API documentation
- Keep deployment guides current
- Document architectural decisions

---

*This document serves as the primary reference for Claude Code integration and project development. Keep it updated as the project evolves.*