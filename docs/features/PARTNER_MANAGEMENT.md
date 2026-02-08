# Partner Management System

Complete B2B partner authentication and company management.

---

## Overview

Enable business partners to:
- Register their companies
- Login and manage profiles
- Request orders/quotes
- Track order status
- Manage company users

---

## Database Schema

### PartnerCompany

```csharp
public class PartnerCompany
{
    public string Id { get; set; }
    public string CompanyName { get; set; }
    public string TaxId { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    
    // Address
    public string Address { get; set; }
    public string City { get; set; }
    public string State { get; set; }
    public string PostalCode { get; set; }
    public string Country { get; set; }
    
    // Business Details
    public string Industry { get; set; }
    public string Website { get; set; }
    public int? EmployeeCount { get; set; }
    public decimal? AnnualRevenue { get; set; }
    
    // Status
    public PartnerStatus Status { get; set; }  // Pending, Active, Suspended
    public DateTime CreatedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? ApprovedBy { get; set; }
    
    // Navigation
    public virtual ICollection<PartnerUser> Users { get; set; }
    public virtual ICollection<PartnerContact> Contacts { get; set; }
}
```

### PartnerUser

```csharp
public class PartnerUser
{
    public string Id { get; set; }
    public string PartnerCompanyId { get; set; }
    
    // User Info
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string? Phone { get; set; }
    
    // Role
    public PartnerRole Role { get; set; }  // CompanyAdmin, User
    
    // Status
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    
    // Navigation
    public virtual PartnerCompany Company { get; set; }
}
```

### PartnerContact

```csharp
public class PartnerContact
{
    public string Id { get; set; }
    public string PartnerCompanyId { get; set; }
    
    public string Name { get; set; }
    public string Title { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public bool IsPrimary { get; set; }
    
    public virtual PartnerCompany Company { get; set; }
}
```

---

## API Endpoints

### Public Endpoints (No Auth)

```http
POST /api/partners/register
POST /api/partners/auth/login
POST /api/partners/auth/refresh
POST /api/partners/auth/forgot-password
POST /api/partners/auth/reset-password
```

### Partner Endpoints (Partner Auth Required)

```http
GET    /api/partners/profile
PUT    /api/partners/profile
GET    /api/partners/company
PUT    /api/partners/company
GET    /api/partners/users
POST   /api/partners/users
PUT    /api/partners/users/{id}
DELETE /api/partners/users/{id}
```

### Admin Endpoints (Admin Auth Required)

```http
GET    /api/admin/partners
GET    /api/admin/partners/{id}
PUT    /api/admin/partners/{id}/approve
PUT    /api/admin/partners/{id}/suspend
DELETE /api/admin/partners/{id}
```

---

## Registration Flow

### Step 1: Partner Registers Company

```json
POST /api/partners/register
{
  "companyName": "ABC Furniture Store",
  "taxId": "12-3456789",
  "email": "info@abcfurniture.com",
  "phone": "+1-555-0100",
  "address": "123 Main St",
  "city": "New York",
  "state": "NY",
  "postalCode": "10001",
  "country": "USA",
  "website": "https://abcfurniture.com",
  "adminUser": {
    "firstName": "John",
    "lastName": "Doe",
    "email": "john@abcfurniture.com",
    "password": "SecurePass123!"
  }
}
```

**Response:**
```json
{
  "companyId": "comp-123",
  "userId": "user-456",
  "status": "Pending",
  "message": "Registration submitted. Awaiting admin approval."
}
```

### Step 2: Admin Approves

**Admin Dashboard:**
- Review company details
- Verify tax ID, business info
- Click "Approve" or "Reject"

```json
PUT /api/admin/partners/comp-123/approve
{
  "approvalNotes": "Verified business information"
}
```

### Step 3: Partner Can Login

```json
POST /api/partners/auth/login
{
  "email": "john@abcfurniture.com",
  "password": "SecurePass123!"
}
```

**Response:**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIs...",
  "refreshToken": "a1b2c3d4-e5f6-...",
  "expiresIn": 1800,
  "user": {
    "id": "user-456",
    "email": "john@abcfurniture.com",
    "firstName": "John",
    "lastName": "Doe",
    "role": "CompanyAdmin",
    "company": {
      "id": "comp-123",
      "name": "ABC Furniture Store",
      "status": "Active"
    }
  }
}
```

---

## Authentication

### Separate JWT Tokens

**Admin tokens:**
- Issuer: `Storefront.Admin`
- Audience: `Storefront.Admin.Web`
- Claims: `role: Admin`, `userId: xxx`

**Partner tokens:**
- Issuer: `Storefront.Partner`
- Audience: `Storefront.Partner.Web`
- Claims: `role: Partner`, `userId: xxx`, `companyId: yyy`

### Authorization

```csharp
[Authorize(Policy = "Partner")]
public class PartnerController : ControllerBase
{
    // Only partners can access
}

[Authorize(Policy = "Admin")]
public class AdminPartnersController : ControllerBase
{
    // Only admins can access
}
```

---

## Partner Dashboard

### Overview
- Company profile
- Pending orders/quotes
- Order history
- Active users
- Recent activity

### Features
- View all orders
- Request new quotes
- Track order status
- Manage company users
- Update profile

---

## User Management

### Company Admin Can:
- ✅ Add new users to company
- ✅ Edit user details
- ✅ Deactivate users
- ✅ Assign roles (User, CompanyAdmin)

### Example: Add User

```json
POST /api/partners/users
{
  "firstName": "Jane",
  "lastName": "Smith",
  "email": "jane@abcfurniture.com",
  "role": "User"
}
```

**System generates temporary password, sends email:**
- "Welcome to Storefront!"
- "Your temporary password: TempPass123"
- "Please login and change your password"

---

## Status Management

### PartnerStatus

```csharp
public enum PartnerStatus
{
    Pending = 0,      // Awaiting admin approval
    Active = 1,       // Approved and active
    Suspended = 2,    // Temporarily disabled
    Rejected = 3      // Registration rejected
}
```

### Workflows

**Pending → Active:**
- Admin approves
- Email sent to partner
- Partner can login

**Active → Suspended:**
- Admin suspends (e.g., non-payment)
- Partner cannot login
- Shows "Account suspended" message

**Suspended → Active:**
- Admin reactivates
- Email sent to partner
- Partner can login again

---

## Email Notifications

### Registration Submitted
**To:** Partner  
**Subject:** "Registration Received"  
**Body:** "Thank you for registering. We'll review your application and respond within 24-48 hours."

### Registration Approved
**To:** Partner  
**Subject:** "Welcome to Storefront!"  
**Body:** "Your account has been approved. Login at: https://storefront.com/partner/login"

### User Invited
**To:** New Partner User  
**Subject:** "You've been invited to Storefront"  
**Body:** "Your company admin has created an account for you. Temporary password: XXX"

---

## Security Features

### Password Requirements
- Minimum 8 characters
- At least 1 uppercase letter
- At least 1 lowercase letter
- At least 1 number
- At least 1 special character

### Account Protection
- ✅ Password hashing (BCrypt)
- ✅ Refresh token rotation
- ✅ Rate limiting on login
- ✅ Account lockout after 5 failed attempts
- ✅ Email verification (optional)

---

## Integration with Order System

### When Partner Places Order

```json
POST /api/orders
{
  "partnerCompanyId": "comp-123",
  "requestedBy": "user-456",
  "items": [
    {
      "productId": "prod-789",
      "quantity": 10
    }
  ],
  "notes": "Need delivery by next Friday"
}
```

### Admin Can See:
- Which company placed order
- Company details
- Contact information
- Order history with this company

---

## Frontend Components

### Partner Portal Routes

```
/partner/register              → Registration form
/partner/login                 → Login page
/partner/dashboard             → Dashboard overview
/partner/profile               → Company profile
/partner/users                 → User management
/partner/orders                → Order history
/partner/orders/new            → Request new quote
/partner/orders/{id}           → Order details
```

### Admin Portal Routes

```
/admin/partners                → List all partners
/admin/partners/{id}           → Partner details
/admin/partners/{id}/approve   → Approve registration
/admin/partners/{id}/orders    → Partner's orders
```

---

## Implementation Order

### Phase 1: Backend (Current)
1. ✅ Create Partner entities
2. ✅ Database migrations
3. ✅ Registration endpoints
4. ✅ Authentication endpoints
5. ✅ Partner profile endpoints
6. ✅ Admin partner management endpoints

### Phase 2: Admin Portal
1. ✅ Partner list/approval UI
2. ✅ Partner details view
3. ✅ Approve/reject actions

### Phase 3: Partner Portal
1. ✅ Registration form
2. ✅ Login page
3. ✅ Dashboard
4. ✅ Profile management
5. ✅ User management

### Phase 4: Order Integration
1. ✅ Connect orders to partners
2. ✅ Partner can view orders
3. ✅ Partner can request quotes

---

## Testing

### Unit Tests
- Registration validation
- Password hashing
- JWT token generation
- Status workflows

### Integration Tests
- Partner registration flow
- Login with valid credentials
- Admin approval process
- User management

---

## Benefits

✅ **Self-Service** - Partners register themselves  
✅ **Admin Control** - Approval workflow  
✅ **Multi-User** - Multiple users per company  
✅ **Secure** - JWT tokens, password requirements  
✅ **Scalable** - Ready for thousands of partners  
✅ **Auditable** - Track all actions  

---

**Ready to implement! Let's start with the backend. 🚀**
