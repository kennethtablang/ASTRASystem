# ASTRA System Backend Documentation
# Agent Supply and Transport Routing Assistant System

## Overview

ASTRA System is a comprehensive Distribution Management System built with ASP.NET Core 8.0. It manages the complete lifecycle of product distribution, from inventory management to delivery tracking, with support for multiple warehouses, stores, and delivery routes.

## Technology Stack

- **Framework**: ASP.NET Core 8.0 Web API
- **Database**: Entity Framework Core with SQL Server
- **Authentication**: ASP.NET Core Identity with JWT
- **Documentation**: Swagger/OpenAPI
- **Mapping**: AutoMapper
- **PDF Generation**: QuestPDF
- **Excel Export**: ClosedXML
- **Email**: MailKit
- **Barcode Generation**: QRCoder, SkiaSharp

## Architecture

### Project Structure

```
ASTRASystem/
├── Controllers/          # API endpoints
├── Services/            # Business logic
├── Interfaces/          # Service contracts
├── Data/               # DbContext and seeding
├── Models/             # Entity models
├── DTO/                # Data transfer objects
├── Enum/               # Enumerations
├── Helpers/            # Utility classes
└── Assets/Uploads/     # File storage
```

### Design Patterns

- **Repository Pattern**: Data access abstraction via services
- **Dependency Injection**: Constructor-based DI throughout
- **DTO Pattern**: Separation of API models from entities
- **Service Layer**: Business logic isolation
- **Generic Response Pattern**: Consistent API responses via `ApiResponse<T>`

## Core Features

### 1. Authentication & Authorization

**Endpoints**: `/api/Auth`

- User registration with role assignment
- JWT-based authentication
- Refresh token support
- Two-factor authentication (2FA)
- Password reset flow
- Email confirmation
- Role-based access control (RBAC)

**Available Roles**:
- `Admin` - Full system access
- `DistributorAdmin` - Distributor-level management
- `Agent` - Store and order management
- `Dispatcher` - Delivery management
- `Accountant` - Financial operations

### 2. Product Management

**Endpoints**: `/api/Product`

- Product CRUD operations
- SKU and barcode support
- Category organization
- Price management with bulk update
- Perishable product tracking
- Barcode/QR code generation

**Key Features**:
- Unique SKU validation
- Barcode lookup for mobile scanning
- Category-based filtering
- Price history via audit logs

### 3. Inventory Management

**Endpoints**: `/api/Inventory`

- Multi-warehouse inventory tracking
- Stock level management
- Reorder level alerts
- Inventory adjustments
- Movement history tracking
- Stock status monitoring

**Movement Types**:
- Restock
- Adjustment
- Order
- Transfer
- Return
- Damage

### 4. Store Management

**Endpoints**: `/api/Store`

- Store registration and management
- Location tracking (City/Barangay)
- Credit limit management
- Outstanding balance tracking
- Payment preference settings

### 5. Order Management

**Endpoints**: `/api/Order`

**Order Lifecycle**:
1. **Pending** → Agent creates order
2. **Confirmed** → Admin confirms with warehouse assignment
3. **Packed** → Warehouse staff packs items
4. **Dispatched** → Assigned to trip
5. **InTransit** → En route to store
6. **AtStore** → Arrived at destination
7. **Delivered** → Completed delivery
8. **Returned** / **Cancelled** → Exception handling

**Features**:
- Batch order creation
- Priority ordering
- Order editing (pending orders only)
- Pick list generation (PDF)
- Packing slip generation (PDF)
- Real-time status tracking

### 6. Trip & Delivery Management

**Endpoints**: `/api/Trip`, `/api/Delivery`

**Trip Management**:
- Multi-order trip creation
- Route sequence optimization
- Dispatcher assignment
- Vehicle tracking
- Trip manifest generation
- Live tracking support

**Delivery Features**:
- Photo upload with GPS coordinates
- Delivery confirmation
- Exception reporting
- Delivery attempt tracking
- Location updates

### 7. Payment & Invoicing

**Endpoints**: `/api/Payment`

**Payment Methods**:
- Cash
- GCash
- Maya
- Bank Transfer
- Other

**Features**:
- Payment recording
- Partial payment support
- Cash collection summary
- Payment reconciliation
- Invoice generation (PDF)
- Accounts Receivable (AR) tracking
- AR aging report
- Overdue invoice monitoring

### 8. Thermal Receipt Printing

**Endpoints**: `/api/Receipt`

- ESC/POS command generation
- 58mm (mobile) and 80mm (desktop) printer support
- Base64 encoded receipt for mobile apps
- Binary download for direct printing
- Batch receipt generation

**Receipt Features**:
- Order details with itemization
- Store information
- Payment details
- QR code support (optional)

### 9. Reporting & Analytics

**Endpoints**: `/api/Reports`

**Available Reports**:
- Dashboard statistics
- Daily sales report (Excel)
- Delivery performance report (Excel)
- Agent activity report (Excel)
- Stock movement report (Excel)

**Dashboard Metrics**:
- Total orders
- Pending orders
- Active trips
- Daily deliveries
- Total revenue
- Outstanding AR
- On-time delivery rate
- Active stores

### 10. Notification System

**Endpoints**: `/api/Notification`

**Notification Types**:
- Order status changes
- Trip assignments
- Delivery confirmations
- Payment receipts
- System alerts

**Features**:
- Role-based notifications
- Read/unread tracking
- Bulk mark as read
- Unread count

### 11. Audit Logging

**Endpoints**: `/api/Notification/audit-logs`

- Comprehensive action logging
- User activity tracking
- Metadata storage (JSON)
- Filterable audit trails
- User-specific logs

### 12. Location Management

**Endpoints**: `/api/City`, `/api/Barangay`

- Hierarchical location structure
- Region → Province → City → Barangay
- Location-based filtering
- Bulk barangay creation
- Store location assignment

## Database Schema

### Core Entities

```
User (AspNetUsers)
├── Roles (many-to-many)
├── Distributor (optional FK)
└── Warehouse (optional FK)

Distributor
└── Warehouses (one-to-many)

Warehouse
├── Distributor (FK)
├── Orders (one-to-many)
├── Trips (one-to-many)
└── Inventories (one-to-many)

Store
├── City (FK)
├── Barangay (FK)
└── Orders (one-to-many)

Product
├── Category (FK)
└── Inventories (one-to-many)

Order
├── Store (FK)
├── Agent (FK)
├── Distributor (FK)
├── Warehouse (FK)
├── OrderItems (one-to-many)
├── Payments (one-to-many)
└── DeliveryPhotos (one-to-many)

Trip
├── Warehouse (FK)
├── Dispatcher (FK)
└── TripAssignments (one-to-many)
```

### Relationships

- **Product → Category**: Many-to-one (nullable)
- **Store → City/Barangay**: Many-to-one (nullable)
- **Order → Store**: Many-to-one (required)
- **Order → OrderItems**: One-to-many (cascade delete)
- **Trip → TripAssignments**: One-to-many (cascade delete)
- **Inventory → InventoryMovements**: One-to-many (cascade delete)

## API Response Format

All API responses follow a consistent structure:

### Success Response
```json
{
  "success": true,
  "data": { /* response data */ },
  "message": "Operation successful",
  "errors": null
}
```

### Error Response
```json
{
  "success": false,
  "data": null,
  "message": "Error message",
  "errors": ["Detailed error 1", "Detailed error 2"]
}
```

### Paginated Response
```json
{
  "success": true,
  "data": {
    "items": [ /* array of items */ ],
    "totalCount": 100,
    "pageNumber": 1,
    "pageSize": 20,
    "totalPages": 5,
    "hasNextPage": true,
    "hasPreviousPage": false
  },
  "message": null,
  "errors": null
}
```

## Authentication

### JWT Token Structure

**Access Token Claims**:
- `sub` - User ID
- `email` - User email
- `jti` - Token ID
- `role` - User role(s)
- `FullName` - Display name
- `DistributorId` - Optional
- `WarehouseId` - Optional

**Token Expiration**: 60 minutes
**Refresh Token**: 7 days

### Authorization Headers

```
Authorization: Bearer {access_token}
```

### Role-Based Access Examples

```csharp
[Authorize] // Any authenticated user
[Authorize(Roles = "Admin")] // Admin only
[Authorize(Roles = "Admin,DistributorAdmin")] // Multiple roles
```

## Configuration

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=...;Database=ASTRASystem;..."
  },
  "JwtSettings": {
    "SecretKey": "your-secret-key-here",
    "Issuer": "ASTRASystem",
    "Audience": "ASTRASystemUsers",
    "ExpiresInMinutes": 60
  },
  "Smtp": {
    "Host": "smtp.example.com",
    "Port": 587,
    "UserName": "your-email@example.com",
    "Password": "your-password",
    "FromName": "ASTRA System",
    "FromAddress": "noreply@astra.local"
  }
}
```

## Database Seeding

The system includes automatic seeding via `AstraSeeder.cs`:

### Default Roles
- Admin
- DistributorAdmin
- Agent
- Dispatcher
- Accountant

### Default Users
- **Admin**: `admin@astra.local` / `Admin#123`
- **Distributor Admin**: `distadmin@demo.local` / `Admin#123`
- **Agent**: `agent1@demo.local` / `Admin#123`
- **Dispatcher**: `dispatcher1@demo.local` / `Admin#123`
- **Accountant**: `accountant1@demo.local` / `Admin#123`

### Sample Data
- Demo Distributor
- Demo Warehouse
- Product Categories (Beverages, Snacks)
- Cities and Barangays (Meycauayan, Bulacan)
- Sample Stores

## Common Use Cases

### 1. Create an Order

```
POST /api/Order
Authorization: Bearer {token}

{
  "storeId": 1,
  "distributorId": 1,
  "warehouseId": 1,
  "priority": false,
  "scheduledFor": "2025-12-11T00:00:00",
  "items": [
    {
      "productId": 1,
      "quantity": 10,
      "unitPrice": 50.00
    }
  ]
}
```

### 2. Create a Trip

```
POST /api/Trip
Authorization: Bearer {token}

{
  "warehouseId": 1,
  "dispatcherId": "user-id",
  "vehicle": "Truck 001",
  "departureAt": "2025-12-11T08:00:00",
  "estimatedReturn": "2025-12-11T17:00:00",
  "orderIds": [1, 2, 3, 4, 5]
}
```

### 3. Record Payment

```
POST /api/Payment
Authorization: Bearer {token}

{
  "orderId": 1,
  "amount": 500.00,
  "method": "Cash",
  "reference": "CASH-20251211-001"
}
```

### 4. Mark Order as Delivered

```
POST /api/Delivery/mark-delivered
Authorization: Bearer {token}
Content-Type: multipart/form-data

orderId: 1
recipientName: Store Owner
recipientPhone: 09171234567
latitude: 14.5995
longitude: 120.9842
photos: [file1.jpg, file2.jpg]
notes: Delivered successfully
```

## File Storage

The system stores uploaded files in:
```
{ProjectRoot}/Assets/Uploads/
```

**Supported Operations**:
- Photo uploads (delivery confirmation)
- Document storage
- File retrieval with signed URLs
- File deletion

## PDF Generation

### Available PDF Documents

1. **Pick List** (`/api/Order/pick-list`)
   - Order details grouped by warehouse
   - Product list with quantities
   - Store locations

2. **Packing Slip** (`/api/Order/{id}/packing-slip`)
   - Order items checklist
   - Store information
   - Barcode/QR code

3. **Trip Manifest** (`/api/Trip/{id}/manifest/pdf`)
   - Complete trip route
   - All order details
   - Store contact information

4. **Invoice** (`/api/Payment/invoice/{id}/pdf`)
   - Itemized invoice
   - Payment terms
   - AR tracking

## Excel Reports

All Excel reports are generated using ClosedXML:

- Auto-fitted columns
- Formatted headers
- Currency formatting
- Date formatting
- Summary calculations

## Error Handling

### Global Exception Handling

The system includes middleware for:
- Unhandled exceptions
- Validation errors
- Database errors
- Authorization failures

### Common HTTP Status Codes

- `200 OK` - Success
- `201 Created` - Resource created
- `400 Bad Request` - Validation error
- `401 Unauthorized` - Authentication required
- `403 Forbidden` - Insufficient permissions
- `404 Not Found` - Resource not found
- `500 Internal Server Error` - Server error

## Security Features

### Implemented Security

- Password hashing (ASP.NET Core Identity)
- JWT token-based authentication
- Refresh token rotation
- Email confirmation
- Two-factor authentication
- Role-based authorization
- Audit logging
- SQL injection protection (EF Core)
- CSRF protection
- HTTPS enforcement (production)

### Security Best Practices

1. **Passwords**: Minimum 8 characters, requires uppercase, lowercase, number, and special character
2. **Tokens**: Short-lived access tokens (60 min), long-lived refresh tokens (7 days)
3. **File Uploads**: Validate file types and sizes
4. **SQL**: Always use parameterized queries (EF Core)
5. **Logging**: Never log sensitive data (passwords, tokens)

## Performance Considerations

### Database Optimization

- Indexed columns on frequently queried fields
- `.AsNoTracking()` for read-only queries
- Eager loading with `.Include()` to prevent N+1 queries
- Pagination for large datasets
- Connection pooling (EF Core default)

### Caching Opportunities

Consider implementing caching for:
- Product catalog
- Category list
- Location data (cities/barangays)
- User roles and permissions
- Dashboard statistics

## Deployment

### Prerequisites

- .NET 8.0 Runtime
- SQL Server 2019+
- IIS or Linux with Nginx
- SSL Certificate (production)

### Environment Variables

```bash
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=https://+:443;http://+:80
```

### Database Migration

```bash
dotnet ef database update
```

### Running the Application

```bash
dotnet run --project ASTRASystem
```

## API Testing

### Swagger UI

Access at: `https://localhost:7071/swagger`

### Sample Postman Collection

A Postman collection would include:
- Authentication endpoints
- CRUD operations for all entities
- File upload examples
- PDF generation tests
- Excel export tests

## Monitoring & Logging

### Logging Levels

- `Information` - Normal operations
- `Warning` - Recoverable issues
- `Error` - Failures with stack traces
- `Critical` - System failures

### Logged Events

- User authentication
- Order creation/updates
- Payment processing
- Delivery confirmations
- System errors
- Audit trail

## Future Enhancements

### Potential Features

1. **Real-time Updates**: SignalR for live order tracking
2. **Advanced Analytics**: Power BI integration
3. **Mobile App API**: Optimized endpoints for mobile
4. **Geofencing**: Automatic delivery confirmation
5. **Route Optimization**: AI-based route planning
6. **Multi-tenant**: Support for multiple distributors
7. **API Rate Limiting**: Throttling for public endpoints
8. **Webhooks**: Event-driven integrations
9. **GraphQL**: Alternative API interface
10. **Microservices**: Domain-driven architecture

## Support & Maintenance

### Common Issues

1. **JWT Token Expired**
   - Solution: Use refresh token endpoint

2. **Database Connection Failure**
   - Check connection string
   - Verify SQL Server is running
   - Check firewall rules

3. **File Upload Failure**
   - Verify `Assets/Uploads` directory exists
   - Check write permissions
   - Validate file size limits

4. **Email Not Sending**
   - Verify SMTP settings
   - Check spam folders
   - Validate email credentials

## Contributing

### Code Standards

- Follow C# naming conventions
- Use async/await for I/O operations
- Add XML documentation comments
- Write unit tests for business logic
- Use dependency injection
- Keep controllers thin, services fat

### Git Workflow

1. Create feature branch from `main`
2. Implement feature with tests
3. Submit pull request
4. Code review
5. Merge to `main`

## License

This project is proprietary software for internal use.

## Contact

For questions or support, contact the development team.

---

**Version**: 1.0.0  
**Last Updated**: December 2025  
**Framework**: ASP.NET Core 8.0
