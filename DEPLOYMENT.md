# Deployment Guide

Production deployment instructions for Smart Course Management Portal.

---

## 📋 Pre-Deployment Checklist

### Code Quality
- [ ] All tests passing
- [ ] No compiler warnings/errors
- [ ] Code review completed
- [ ] Security audit completed
- [ ] Dependencies updated to latest patch versions

### Security
- [ ] JWT secret key rotated (new 32+ character key)
- [ ] Database credentials secured in vault
- [ ] CORS properly restricted (not using "*")
- [ ] HTTPS enforced
- [ ] Security headers configured
- [ ] Rate limiting configured

### Infrastructure
- [ ] Production SQL Server database created
- [ ] Database backups configured
- [ ] Web server prepared (IIS or Azure App Service)
- [ ] Monitoring/logging configured
- [ ] Environment variables set
- [ ] SSL certificates installed

### Documentation
- [ ] README.md updated for production
- [ ] API documentation complete
- [ ] Database schema documented
- [ ] Deployment runbook created
- [ ] Rollback procedure documented

---

## 🚀 Deployment Options

### Option 1: IIS (Windows Server)

#### Prerequisites
- Windows Server 2019 or later
- IIS 10 or later
- .NET 10 Hosting Bundle installed
- SQL Server database accessible from server

#### Steps

**1. Install .NET Hosting Bundle**
```powershell
# Download from https://dotnet.microsoft.com/download/dotnet/10.0
# Run installer: dotnet-hosting-10.0.X-win.exe
```

**2. Prepare Application**
```powershell
# On development machine
cd d:\Smart-Course-Management-Portal\SmartCourseManagement.API
dotnet publish -c Release -o "C:\publish\SmartCourseManagement"
```

**3. Copy to Server**
```powershell
# Copy published files to server
xcopy "C:\publish\SmartCourseManagement\*" \\server\c$\inetpub\wwwroot\api /E /Y
```

**4. Create IIS App Pool**
```powershell
# PowerShell (as Administrator)
Import-Module WebAdministration

# Create app pool
New-WebAppPool -Name "SmartCourseAPI" `
  -Force `
  -ManagedRuntimeVersion "v4.0" `
  -ManagedPipelineMode Integrated

# Set .NET version to 'No Managed Code' (ASP.NET Core uses its own runtime)
$appPool = Get-Item "IIS:\AppPools\SmartCourseAPI"
$appPool.ManagedRuntimeVersion = ""
$appPool | Set-Item -Force
```

**5. Create IIS Website**
```powershell
New-WebSite -Name "SmartCourseAPI" `
  -Port 443 `
  -Protocol https `
  -PhysicalPath "C:\inetpub\wwwroot\api" `
  -SslCertThumbprint "<CERT_THUMBPRINT>" `
  -ApplicationPool "SmartCourseAPI"
```

**6. Configure appsettings.Production.json**
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning"
    }
  },
  "Jwt": {
    "SecretKey": "your-new-production-secret-key-minimum-32-chars",
    "Issuer": "SmartCourseManagement",
    "Audience": "SmartCourseManagementUsers",
    "AccessTokenExpiryMinutes": 15,
    "RefreshTokenExpiryDays": 7
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=prod-sql-server;Database=SmartCourseManagement;User Id=sa;Password=<PASSWORD>;"
  },
  "IpRateLimiting": {
    "GeneralRules": [
      {
        "Endpoint": "*",
        "Period": "1m",
        "Limit": 100
      }
    ]
  },
  "AllowedHosts": "smartcourse.example.com,localhost"
}
```

**7. Create web.config**
```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <location path="." inheritInChildApplications="false">
    <system.webServer>
      <aspNetCore processPath="dotnet" arguments=".\SmartCourseManagement.API.dll" 
                  stdoutLogEnabled="true" stdoutLogFile=".\logs\stdout" 
                  hostingModel="OutOfProcess" />
    </system.webServer>
  </location>
</configuration>
```

**8. Set Permissions**
```powershell
# Ensure IIS app pool identity has read/write permissions
icacls "C:\inetpub\wwwroot\api" /grant 'IIS APPPOOL\SmartCourseAPI:(OI)(CI)M' /T
```

---

### Option 2: Azure App Service

#### Prerequisites
- Azure subscription
- Azure CLI installed
- Git configured

#### Steps

**1. Create Azure Resources**
```bash
# Create resource group
az group create \
  --name SmartCourseRG \
  --location eastus

# Create SQL Server
az sql server create \
  --resource-group SmartCourseRG \
  --name smartcourse-sql \
  --admin-user adminuser \
  --admin-password <PASSWORD>

# Create database
az sql db create \
  --resource-group SmartCourseRG \
  --server smartcourse-sql \
  --name SmartCourseManagement \
  --service-objective S0

# Create App Service plan
az appservice plan create \
  --name SmartCoursePlan \
  --resource-group SmartCourseRG \
  --sku B1 --is-linux

# Create App Service
az webapp create \
  --resource-group SmartCourseRG \
  --plan SmartCoursePlan \
  --name smartcourse-api \
  --runtime "DOTNETCORE|10.0"
```

**2. Configure Connection String**
```bash
az webapp config appsettings set \
  --resource-group SmartCourseRG \
  --name smartcourse-api \
  --settings \
    "ConnectionStrings:DefaultConnection=Server=tcp:smartcourse-sql.database.windows.net,1433;Initial Catalog=SmartCourseManagement;Persist Security Info=False;User ID=adminuser;Password=<PASSWORD>;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
```

**3. Deploy from GitHub**
```bash
# Push repository to GitHub
git push origin main

# Enable continuous deployment
az webapp deployment github-oauth authorize

az webapp source control \
  --resource-group SmartCourseRG \
  --name smartcourse-api \
  --repo-url https://github.com/username/smart-course \
  --branch main \
  --git-token <GITHUB_TOKEN>
```

**4. Run Migrations**
```bash
# SSH into App Service
az webapp create-remote-connection \
  --resource-group SmartCourseRG \
  --name smartcourse-api

# Run migrations
dotnet ef database update --environment Production
```

---

### Option 3: Docker Container

#### Prerequisites
- Docker installed
- Docker Hub account (optional)

#### Steps

**1. Create Dockerfile**
```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS builder
WORKDIR /src

COPY SmartCourseManagement.API/ .
RUN dotnet restore
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=builder /app/publish .

EXPOSE 5202
ENV ASPNETCORE_URLS=http://+:5202
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "SmartCourseManagement.API.dll"]
```

**2. Create .dockerignore**
```
bin/
obj/
.vs/
.git/
.gitignore
README.md
*.user
*.log
```

**3. Build Image**
```bash
docker build -t smartcourse-api:latest .
```

**4. Run Container**
```bash
docker run -d \
  --name smartcourse-api \
  -p 5202:5202 \
  -e "ConnectionStrings:DefaultConnection=Server=sql-server;Database=SmartCourseManagement;..." \
  -e "Jwt:SecretKey=your-production-secret" \
  smartcourse-api:latest
```

**5. Push to Registry**
```bash
docker tag smartcourse-api:latest username/smartcourse-api:latest
docker push username/smartcourse-api:latest
```

---

## 🔐 Security Configuration

### HTTPS Configuration

**IIS**:
```powershell
# Add HTTPS binding with SSL certificate
New-WebBinding -Name "SmartCourseAPI" `
  -Protocol https `
  -Port 443 `
  -SslCertThumbprint "<THUMBPRINT>"

# Redirect HTTP to HTTPS
Add-Content web.config @"
<rewrite>
  <rules>
    <rule name="HTTP to HTTPS redirect" stopProcessing="true">
      <match url=".*" />
      <conditions>
        <add input="{HTTPS}" pattern="off" ignoreCase="true" />
      </conditions>
      <action type="Redirect" url="https://{HTTP_HOST}{REQUEST_URI}" redirectType="Permanent" />
    </rule>
  </rules>
</rewrite>
"@
```

### Security Headers

Add to `Program.cs`:
```csharp
app.UseHsts();

app.Use(async (context, next) =>
{
    // HSTS
    context.Response.Headers.Add("Strict-Transport-Security", "max-age=31536000; includeSubDomains");
    
    // Content Security Policy
    context.Response.Headers.Add("Content-Security-Policy", "default-src 'self'");
    
    // Prevent MIME sniffing
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    
    // Clickjacking protection
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    
    // XSS Protection
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    
    await next();
});
```

### CORS Configuration

Production configuration in `Program.cs`:
```csharp
services.AddCors(options =>
{
    options.AddPolicy("ProductionPolicy", builder =>
        builder
            .WithOrigins(
                "https://smartcourse.example.com",
                "https://admin.smartcourse.example.com"
            )
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials()
    );
});

app.UseCors("ProductionPolicy");
```

---

## 📊 Monitoring & Logging

### Application Insights (Azure)

**Configuration**:
```json
{
  "ApplicationInsights": {
    "InstrumentationKey": "your-key-here"
  }
}
```

**Add to Program.cs**:
```csharp
builder.Services.AddApplicationInsightsTelemetry();
```

### Logging Configuration

**appsettings.Production.json**:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning"
    },
    "ApplicationInsights": {
      "LogLevel": {
        "Default": "Information"
      }
    }
  }
}
```

### Database Backups

**SQL Server Automated Backup**:
```sql
-- Full backup daily at 2 AM
EXEC msdb.dbo.sp_add_schedule 
    @schedule_name = 'Daily_Full_Backup',
    @freq_type = 4,
    @freq_interval = 1,
    @active_start_time = 020000;

-- Transaction log backup every 15 minutes
EXEC msdb.dbo.sp_add_schedule 
    @schedule_name = 'TLog_Backup_15min',
    @freq_type = 4,
    @freq_interval = 1,
    @freq_subday_type = 4,
    @freq_subday_interval = 15;
```

---

## 🔄 Database Migration

### First-Time Setup

```bash
# On production server
cd /var/app/SmartCourseManagement.API

# Apply all pending migrations
dotnet ef database update --environment Production

# Verify migrations applied
dotnet ef migrations list --environment Production
```

### Zero-Downtime Migration

```bash
# 1. Create new database as copy
# 2. Run migrations on new database
# 3. Switch connection string during maintenance window
# 4. Run smoke tests
# 5. Keep old database as backup for rollback
```

---

## 🔄 Rollback Procedures

### Application Rollback

```powershell
# IIS - If latest version is broken
# 1. Stop app pool
iisreset /stop

# 2. Restore previous version
xcopy "\\backup\SmartCourseManagement-v1.0.0\*" C:\inetpub\wwwroot\api /E /Y

# 3. Start app pool
iisreset /start

# 4. Verify application
# Visit https://smartcourse.example.com/health
```

### Database Rollback

```sql
-- If migration breaks something
-- Option 1: Revert specific migration
dotnet ef migrations remove
dotnet ef database update <previous-migration-name>

-- Option 2: Restore from backup
RESTORE DATABASE SmartCourseManagement 
FROM DISK = '\\backup\SmartCourseManagement_2026-04-01.bak'
```

---

## 📈 Performance Tuning

### Database Indexing

```sql
-- Add indexes for frequently queried columns
CREATE INDEX IX_UserEmail ON Users(Email);
CREATE INDEX IX_EnrollmentUserId ON Enrollments(UserId);
CREATE INDEX IX_CourseInstructorId ON Courses(InstructorId);
CREATE INDEX IX_RefreshTokenUserId ON RefreshTokens(UserId);
```

### Connection Pooling

**appsettings.Production.json**:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=sql-server;Database=SmartCourseManagement;...;Max Pool Size=100;"
  }
}
```

### Caching

Add to `Program.cs`:
```csharp
services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
});
```

---

## 🧪 Post-Deployment Verification

### Health Check

```bash
# Test API endpoint
curl -I https://smartcourse.example.com/api/health

# Expected response: 200 OK
```

### Smoke Tests

```powershell
# Login
$response = Invoke-RestMethod -Uri "https://smartcourse.example.com/api/auth/login" `
    -Method Post `
    -Body '{"email":"instructor@example.com","password":"Password123!"}' `
    -ContentType "application/json"

# Verify token received
if ($response.accessToken) {
    Write-Host "✓ Authentication working"
} else {
    Write-Host "✗ Authentication failed"
}

# Get courses
$courses = Invoke-RestMethod -Uri "https://smartcourse.example.com/api/courses" `
    -Headers @{"Authorization"="Bearer $($response.accessToken)"}

if ($courses.items) {
    Write-Host "✓ API responding correctly"
} else {
    Write-Host "✗ API response error"
}
```

### Frontend Verification

1. Open https://smartcourse.example.com in browser
2. Test login with test credentials
3. Verify courses load
4. Test enrollment flow
5. Test dark mode toggle
6. Test responsive design (mobile view)

---

## 📞 Troubleshooting Deployment Issues

### Port Already in Use

```powershell
# Find process using port
netstat -ano | findstr :5202

# Kill process
taskkill /PID <PID> /F
```

### Database Connection Issues

```powershell
# Test SQL Server connection
$connectionString = "Server=sql-server;Database=SmartCourseManagement;User=sa;Password=X;"
$connection = New-Object System.Data.SqlClient.SqlConnection
$connection.ConnectionString = $connectionString
$connection.Open()
Write-Host "✓ Connection successful"
$connection.Close()
```

### Certificate Issues

```powershell
# List installed certificates
Get-ChildItem Cert:\LocalMachine\My

# Install new certificate
Import-PfxCertificate -FilePath "certificate.pfx" -CertStoreLocation "Cert:\LocalMachine\My"
```

### Migrations Not Applying

```bash
# Check pending migrations
dotnet ef migrations list

# Apply specific migration
dotnet ef database update <migration-name>

# If completely stuck, recreate database
dotnet ef database drop --force
dotnet ef database update
```

---

## 📋 Maintenance Schedule

### Daily
- Monitor application logs
- Check database disk space
- Verify backup completion

### Weekly
- Review security logs
- Check performance metrics
- Update NuGet packages if needed

### Monthly
- Security patches
- Performance optimization
- Database maintenance (index rebuild)
- Capacity planning review

### Quarterly
- Load testing
- Security audit
- Database statistics update
- Documentation review

---

## 🚨 Incident Response

### High CPU Usage

```powershell
# 1. Identify top processes
Get-Process | Sort-Object CPU -Descending | Select-Object -First 10

# 2. Check database queries
# SELECT * FROM sys.dm_exec_requests WHERE status <> 'sleeping';

# 3. Restart application pool if needed
Restart-WebAppPool -Name "SmartCourseAPI"
```

### Database Connection Errors

```sql
-- 1. Check active connections
SELECT COUNT(*) FROM sys.dm_exec_sessions WHERE database_id = DB_ID();

-- 2. Kill long-running queries
KILL <session_id>;

-- 3. Increase max pool size if needed (in connection string)
```

### Memory Leak

```powershell
# 1. Monitor memory usage
Get-Process dotnet | Select-Object Name, WorkingSet

# 2. Restart app pool
Stop-WebAppPool -Name "SmartCourseAPI"
Start-WebAppPool -Name "SmartCourseAPI"

# 3. Review application code for disposal issues
```

---

## 📞 Support & Resources

- [Microsoft Learn: Deploy ASP.NET Core](https://learn.microsoft.com/aspnet/core/host-and-deploy)
- [Azure App Service Documentation](https://learn.microsoft.com/azure/app-service)
- [SQL Server Administration](https://learn.microsoft.com/sql/t-sql/statements/statements)
- [Docker Containerization](https://docs.docker.com/)

---

**Last Updated**: April 1, 2026  
**Version**: 1.0.0
