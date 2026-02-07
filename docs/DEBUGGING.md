# Backend Debugging Guide

## VS Code Debug Configurations

I've created 4 debug profiles for you:

### 1. `.NET Core Launch (API)` - **RECOMMENDED FOR TESTING**
- **What it does**: Builds and runs the API, then opens Swagger in your browser
- **Use when**: You want to test APIs interactively
- **How to use**:
  1. Press `F5` or click "Run and Debug" in VS Code
  2. Select ".NET Core Launch (API)"
  3. Swagger UI will open automatically at `http://localhost:8080/swagger`
  4. Set breakpoints in your code and they'll be hit when you call APIs

### 2. `.NET Core Launch (API - No Browser)`
- **What it does**: Same as above but doesn't open browser
- **Use when**: You're testing from another tool (Postman, Thunder Client, etc.)

### 3. `.NET Core Launch (API + Watch)`
- **What it does**: Runs with hot reload - code changes apply automatically
- **Use when**: You're actively developing and want to see changes immediately
- **Note**: Slower startup, but saves time on rebuilds

### 4. `.NET Core Attach`
- **What it does**: Attaches debugger to an already-running process
- **Use when**: API is already running and you want to debug it

## How to Debug

### Quick Start
1. Open VS Code in the project root
2. Press `F5`
3. Choose `.NET Core Launch (API)`
4. Wait for Swagger to open
5. Start testing your endpoints!

### Setting Breakpoints
- Click in the left margin (red dot appears)
- Or press `F9` on any line
- Debugger will pause when that line executes

### Debug Controls
- `F5` - Continue
- `F10` - Step Over (next line)
- `F11` - Step Into (enter function)
- `Shift+F11` - Step Out (exit function)
- `Shift+F5` - Stop Debugging

## Testing Your APIs

### Available Endpoints

#### **Authentication** (`/api/identity/auth`)
```bash
POST /api/identity/auth/login
POST /api/identity/auth/refresh
```

**Default Admin Credentials:**
- Email: `admin@storefront.com`
- Password: `AdminPassword123!`

#### **Products** (`/api/catalog/products`)
```bash
GET    /api/catalog/products           # List all products
GET    /api/catalog/products/{id}      # Get by ID
POST   /api/catalog/products           # Create (Auth required)
PUT    /api/catalog/products/{id}      # Update (Auth required)
DELETE /api/catalog/products/{id}      # Delete (Auth required)
GET    /api/catalog/products/search    # Search products
POST   /api/catalog/products/{id}/images # Upload image (Auth required)
```

#### **Categories** (`/api/catalog/categories`)
```bash
GET    /api/catalog/categories         # List all categories
GET    /api/catalog/categories/{id}    # Get by ID
POST   /api/catalog/categories         # Create (Auth required)
PUT    /api/catalog/categories/{id}    # Update (Auth required)
DELETE /api/catalog/categories/{id}    # Delete (Auth required)
```

#### **Blog** (`/api/content/blog`)
```bash
GET    /api/content/blog               # List blog posts
GET    /api/content/blog/{id}          # Get by ID
POST   /api/content/blog               # Create (Auth required)
PUT    /api/content/blog/{id}          # Update (Auth required)
DELETE /api/content/blog/{id}          # Delete (Auth required)
```

#### **Pages** (`/api/content/pages`)
```bash
GET    /api/content/pages/{slug}       # Get page by slug
GET    /api/content/pages/sitemap      # Get sitemap
```

## Common Debugging Scenarios

### 1. Test Authentication Flow
```csharp
// Set breakpoint in: src/Modules/Identity/Core/Application/Commands/LoginUserCommandHandler.cs
// Line: public async Task<Result<LoginResponse>> Handle(...)
```
Then call `POST /api/identity/auth/login` from Swagger

### 2. Test Product Creation
```csharp
// Set breakpoint in: src/Modules/Catalog/Core/Application/Commands/CreateProductCommandHandler.cs
// Line: public async Task<Result<ProductDto>> Handle(...)
```
Then call `POST /api/catalog/products` (after logging in)

### 3. Test Image Processing
```csharp
// Set breakpoint in: src/Modules/Catalog/Infrastructure/BackgroundJobs/ImageProcessingBackgroundService.cs
// Line: await foreach (var message in _channel.Reader.ReadAllAsync(...))
```
Then upload an image via `POST /api/catalog/products/{id}/images`

### 4. Test Category Hierarchy
```csharp
// Set breakpoint in: src/Modules/Catalog/Core/Application/Commands/CreateCategoryCommandHandler.cs
// Line: if (command.ParentId.HasValue)
```
Then create a category with a parent

## Troubleshooting

### Port Already in Use (8080)
```bash
# Kill existing dotnet processes
pkill -9 dotnet

# Or find and kill specific process
lsof -ti:8080 | xargs kill -9
```

### Database Not Created
- The database should create automatically on startup
- Check console output for: "✅ Identity database created successfully"
- If failed, check PostgreSQL is running: `docker ps`

### Breakpoints Not Hit
- Make sure you're in Debug mode (not Release)
- Check the Output panel for build errors
- Try Clean + Rebuild: `Cmd+Shift+P` → ".NET: Clean"

### Hot Reload Not Working
- Use the "API + Watch" profile instead
- Or manually restart the debugger (`Shift+F5` then `F5`)

## Tips for Finding Missing Implementations

1. **Check Swagger UI**: All implemented endpoints are listed there
2. **Look for 404s**: Endpoint exists but handler missing
3. **Look for 500s**: Endpoint exists but logic has bugs
4. **Check Response DTOs**: May return partial data (missing fields)
5. **Test CRUD Operations**: Create → Read → Update → Delete
6. **Test Filters/Search**: Query parameters may not work
7. **Test Authorization**: Some endpoints may lack [Authorize] attribute

## Next Steps After Analysis

Once you find missing features:
1. List them out
2. We'll implement them one by one
3. Test each implementation via debugger
4. Then move to Mobile App development

## Mobile App Plan
- **Flutter/React Native**: Which do you prefer?
- **Backend**: Already API-ready (RESTful)
- **Features**:
  - Customer-facing catalog (browse, search, view)
  - Backoffice panel (admin features)
  - No web frontend (only mobile + admin)

---

**Ready to debug?** Press `F5` and let's find those missing features! 🚀
