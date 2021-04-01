// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

# Deployment Guide

Comprehensive guide for deploying applications using DotnetMicroOrm.

## Prerequisites

- .NET 10 SDK or runtime
- Target database (SQL Server, PostgreSQL, MySQL, SQLite)
- Basic knowledge of your deployment environment

## Local Development

### Setup

1. **Clone repository**
   ```bash
   git clone https://github.com/Sarmkadan/dotnet-micro-orm.git
   cd dotnet-micro-orm
   ```

2. **Create database**
   ```bash
   # SQL Server (local)
   sqlcmd -S (local) -U sa -P YourPassword -Q "CREATE DATABASE MyDb"
   ```

3. **Update connection string**
   ```json
   {
     "Database": {
       "ConnectionString": "Server=(local);Database=MyDb;Integrated Security=true;"
     }
   }
   ```

4. **Run application**
   ```bash
   dotnet run
   ```

## Docker Deployment

### Building Docker Image

```bash
docker build -t myapp:latest .
```

**Dockerfile example:**

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10 as build
WORKDIR /src
COPY . .
RUN dotnet build -c Release

FROM mcr.microsoft.com/dotnet/runtime:10
WORKDIR /app
COPY --from=build /src/bin/Release/net10.0/publish/ .
ENTRYPOINT ["dotnet", "MyApp.dll"]
```

### Running Container

**Single container:**

```bash
docker run -e "Database__ConnectionString=Server=db;Database=MyDb;User Id=sa;Password=YourPassword" \
           -p 5000:80 \
           myapp:latest
```

**With Docker Compose:**

```yaml
version: '3.8'

services:
  app:
    build: .
    ports:
      - "5000:80"
    environment:
      Database__ConnectionString: "Server=db;Database=MyDb;User Id=sa;Password=YourPassword"
    depends_on:
      - db

  db:
    image: mcr.microsoft.com/mssql/server:latest
    environment:
      SA_PASSWORD: "YourPassword"
      ACCEPT_EULA: "Y"
    ports:
      - "1433:1433"
    volumes:
      - sqldata:/var/opt/mssql

volumes:
  sqldata:
```

**Start services:**

```bash
docker-compose up -d
```

## Cloud Deployment

### Azure App Service

1. **Create app service plan**
   ```bash
   az appservice plan create --name myplan --resource-group mygroup --sku B1
   ```

2. **Create web app**
   ```bash
   az webapp create --name myapp --resource-group mygroup --plan myplan --runtime "DOTNET|10.0"
   ```

3. **Configure connection string**
   ```bash
   az webapp config appsettings set --name myapp \
       --resource-group mygroup \
       --settings Database__ConnectionString="Server=server.database.windows.net;Database=MyDb;User Id=admin;Password=P@ss;"
   ```

4. **Deploy**
   ```bash
   dotnet publish -c Release
   az webapp deployment source config-zip --name myapp --resource-group mygroup --src bin/Release/net10.0/publish.zip
   ```

### AWS Elastic Beanstalk

1. **Install AWS CLI and EB CLI**
   ```bash
   pip install awsebcli
   ```

2. **Initialize Elastic Beanstalk**
   ```bash
   eb init -p dotnet-6.0 myapp
   ```

3. **Create environment**
   ```bash
   eb create prod --instance-type t3.micro
   ```

4. **Set environment variables**
   ```bash
   eb setenv Database__ConnectionString="Server=...;Database=...;"
   ```

5. **Deploy**
   ```bash
   eb deploy
   ```

### Google Cloud Run

1. **Create Dockerfile** (see Docker Deployment section)

2. **Build and push image**
   ```bash
   gcloud builds submit --tag gcr.io/PROJECT_ID/myapp
   ```

3. **Deploy to Cloud Run**
   ```bash
   gcloud run deploy myapp \
       --image gcr.io/PROJECT_ID/myapp \
       --set-env-vars Database__ConnectionString="Server=...;Database=...;" \
       --memory 512Mi \
       --region us-central1
   ```

## Kubernetes Deployment

### Create Deployment YAML

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: myapp
spec:
  replicas: 3
  selector:
    matchLabels:
      app: myapp
  template:
    metadata:
      labels:
        app: myapp
    spec:
      containers:
      - name: myapp
        image: myapp:latest
        ports:
        - containerPort: 80
        env:
        - name: Database__ConnectionString
          valueFrom:
            secretKeyRef:
              name: db-secret
              key: connection-string
        resources:
          requests:
            memory: "256Mi"
            cpu: "250m"
          limits:
            memory: "512Mi"
            cpu: "500m"
---
apiVersion: v1
kind: Service
metadata:
  name: myapp-service
spec:
  selector:
    app: myapp
  ports:
  - protocol: TCP
    port: 80
    targetPort: 80
  type: LoadBalancer
```

### Deploy to Kubernetes

```bash
# Create secret for connection string
kubectl create secret generic db-secret --from-literal=connection-string="Server=...;Database=...;"

# Deploy application
kubectl apply -f deployment.yaml

# Check status
kubectl get pods
kubectl get svc
```

## Database Migration

### Schema Creation

```sql
-- SQL Server
CREATE TABLE Products (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(255) NOT NULL,
    Price DECIMAL(18,2) NOT NULL,
    CreatedAt DATETIME NOT NULL,
    UpdatedAt DATETIME NULL,
    Version INT NOT NULL DEFAULT 1
);
```

### Pre-deployment Steps

1. **Backup existing database**
   ```bash
   # SQL Server
   BACKUP DATABASE MyDb TO DISK = 'C:\backup.bak'
   ```

2. **Create migration script**
   - Use version control for schema changes
   - Test against staging database
   - Have rollback plan

3. **Run migrations**
   ```bash
   sqlcmd -S server -U user -P password -i migration.sql
   ```

## Performance Tuning

### Database Indexing

```sql
-- Add index on frequently queried columns
CREATE INDEX IX_Products_Price ON Products(Price);
CREATE INDEX IX_Products_Name ON Products(Name);
```

### Connection Pooling

```json
{
  "Database": {
    "MaxPoolSize": 100,
    "MinPoolSize": 5
  }
}
```

### Caching Configuration

```json
{
  "Caching": {
    "DefaultTTLSeconds": 300,
    "MaxEntriesPerKey": 1000
  }
}
```

## Monitoring

### Application Insights (Azure)

```json
{
  "ApplicationInsights": {
    "InstrumentationKey": "your-key-here"
  }
}
```

### Logging Configuration

```json
{
  "Logging": {
    "LogLevel": "Information",
    "EnableDetailedErrors": false,
    "LogQueryPerformance": true,
    "SlowQueryThresholdMs": 500
  }
}
```

## Health Checks

### Implement Health Check Endpoint

```csharp
app.MapHealthChecks("/health");
```

### Docker Health Check

```dockerfile
HEALTHCHECK --interval=30s --timeout=3s --start-period=40s --retries=3 \
  CMD curl -f http://localhost/health || exit 1
```

## Scaling

### Horizontal Scaling

- Use load balancer (Azure Load Balancer, AWS ELB, etc.)
- Stateless application design
- Distributed caching for shared state

### Vertical Scaling

- Increase machine CPU/memory
- Optimize database queries
- Enable query result caching

## Troubleshooting

### Connection Issues

```
Error: Cannot connect to database
Solution:
- Verify connection string format
- Check firewall rules
- Ensure database service is running
- Validate credentials
```

### Performance Issues

```
Error: Slow queries
Solution:
- Enable query logging (SlowQueryThresholdMs)
- Check database indexes
- Review cache settings
- Use specifications to filter at database level
```

### Memory Issues

```
Error: Out of memory
Solution:
- Reduce cache TTL
- Lower MaxPoolSize
- Use AsNoTracking() for read-only queries
- Monitor identity map size
```

## Rollback Procedure

```bash
# Azure App Service
az webapp deployment slot swap -g mygroup -n myapp --slot staging

# AWS Elastic Beanstalk
eb swap

# Docker
docker pull myapp:previous
docker run myapp:previous
```

## Checklist

- [ ] Database connectivity verified
- [ ] Connection strings configured
- [ ] Indexes created on frequently queried columns
- [ ] Caching enabled and configured
- [ ] Logging configured
- [ ] Health checks implemented
- [ ] Load balancer configured
- [ ] Backup and recovery plan in place
- [ ] Monitoring alerts set up
- [ ] Rollback procedure tested
