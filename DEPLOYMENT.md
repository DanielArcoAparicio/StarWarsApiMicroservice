# 🚀 Guía de Despliegue - Star Wars API

## Opciones de Despliegue

Esta guía cubre diferentes opciones para desplegar la aplicación Star Wars API en producción.

## 📋 Tabla de Contenidos

1. [Docker Compose (Local/Testing)](#docker-compose)
2. [Azure App Service](#azure-app-service)
3. [AWS ECS](#aws-ecs)
4. [Kubernetes](#kubernetes)
5. [Variables de Entorno](#variables-de-entorno)
6. [Seguridad en Producción](#seguridad-en-producción)

---

## 🐳 Docker Compose

### Despliegue en Servidor

1. **Preparar el servidor**

```bash
# Instalar Docker y Docker Compose
curl -fsSL https://get.docker.com -o get-docker.sh
sh get-docker.sh

# Instalar Docker Compose
sudo curl -L "https://github.com/docker/compose/releases/latest/download/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose
sudo chmod +x /usr/local/bin/docker-compose
```

2. **Clonar el repositorio**

```bash
git clone <repository-url>
cd StarWars
```

3. **Configurar variables de entorno**

```bash
# Crear archivo .env
cat > .env << EOF
POSTGRES_PASSWORD=TuPasswordSeguro123!
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=Host=postgres;Port=5432;Database=starwarsdb;Username=starwars;Password=TuPasswordSeguro123!
EOF
```

4. **Iniciar servicios**

```bash
docker-compose up -d
```

5. **Verificar**

```bash
# Ver logs
docker-compose logs -f

# Verificar salud
curl http://localhost:5000/health
```

### Configuración con SSL/HTTPS

Modificar `docker-compose.yml`:

```yaml
services:
  starwars-api:
    environment:
      - ASPNETCORE_URLS=https://+:443;http://+:80
      - ASPNETCORE_Kestrel__Certificates__Default__Path=/app/certificates/cert.pfx
      - ASPNETCORE_Kestrel__Certificates__Default__Password=YourCertPassword
    ports:
      - "443:443"
      - "80:80"
    volumes:
      - ./certificates:/app/certificates:ro
```

---

## ☁️ Azure App Service

### 1. Crear Recursos en Azure

```bash
# Login
az login

# Crear grupo de recursos
az group create --name StarWarsRG --location eastus

# Crear PostgreSQL
az postgres flexible-server create \
  --name starwars-postgres \
  --resource-group StarWarsRG \
  --location eastus \
  --admin-user starwarsadmin \
  --admin-password YourSecurePassword123! \
  --sku-name Standard_B1ms

# Crear base de datos
az postgres flexible-server db create \
  --resource-group StarWarsRG \
  --server-name starwars-postgres \
  --database-name starwarsdb

# Crear App Service Plan
az appservice plan create \
  --name StarWarsAppPlan \
  --resource-group StarWarsRG \
  --sku B1 \
  --is-linux

# Crear Web App
az webapp create \
  --name starwars-api-app \
  --resource-group StarWarsRG \
  --plan StarWarsAppPlan \
  --runtime "DOTNETCORE:8.0"
```

### 2. Configurar Cadena de Conexión

```bash
az webapp config connection-string set \
  --name starwars-api-app \
  --resource-group StarWarsRG \
  --settings DefaultConnection="Host=starwars-postgres.postgres.database.azure.com;Port=5432;Database=starwarsdb;Username=starwarsadmin;Password=YourSecurePassword123!;SSL Mode=Require" \
  --connection-string-type PostgreSQL
```

### 3. Desplegar desde GitHub

```bash
# Configurar despliegue continuo
az webapp deployment source config \
  --name starwars-api-app \
  --resource-group StarWarsRG \
  --repo-url https://github.com/your-username/StarWars \
  --branch main \
  --manual-integration
```

### 4. Configurar Variables de Entorno

```bash
az webapp config appsettings set \
  --name starwars-api-app \
  --resource-group StarWarsRG \
  --settings \
    ASPNETCORE_ENVIRONMENT=Production \
    IpRateLimiting__GeneralRules__0__Limit=100
```

---

## 🚀 AWS ECS

### 1. Crear ECR Repository

```bash
# Crear repositorio
aws ecr create-repository --repository-name starwars-api

# Login a ECR
aws ecr get-login-password --region us-east-1 | docker login --username AWS --password-stdin <account-id>.dkr.ecr.us-east-1.amazonaws.com

# Build y push
docker build -t starwars-api .
docker tag starwars-api:latest <account-id>.dkr.ecr.us-east-1.amazonaws.com/starwars-api:latest
docker push <account-id>.dkr.ecr.us-east-1.amazonaws.com/starwars-api:latest
```

### 2. Crear RDS PostgreSQL

```bash
aws rds create-db-instance \
  --db-instance-identifier starwars-postgres \
  --db-instance-class db.t3.micro \
  --engine postgres \
  --master-username starwars \
  --master-user-password YourSecurePassword123! \
  --allocated-storage 20 \
  --backup-retention-period 7
```

### 3. Crear Task Definition

`task-definition.json`:

```json
{
  "family": "starwars-api",
  "networkMode": "awsvpc",
  "requiresCompatibilities": ["FARGATE"],
  "cpu": "256",
  "memory": "512",
  "containerDefinitions": [
    {
      "name": "starwars-api",
      "image": "<account-id>.dkr.ecr.us-east-1.amazonaws.com/starwars-api:latest",
      "portMappings": [
        {
          "containerPort": 8080,
          "protocol": "tcp"
        }
      ],
      "environment": [
        {
          "name": "ASPNETCORE_ENVIRONMENT",
          "value": "Production"
        },
        {
          "name": "ConnectionStrings__DefaultConnection",
          "value": "Host=starwars-postgres.xxx.us-east-1.rds.amazonaws.com;Port=5432;Database=starwarsdb;Username=starwars;Password=YourSecurePassword123!"
        }
      ],
      "logConfiguration": {
        "logDriver": "awslogs",
        "options": {
          "awslogs-group": "/ecs/starwars-api",
          "awslogs-region": "us-east-1",
          "awslogs-stream-prefix": "ecs"
        }
      }
    }
  ]
}
```

### 4. Crear ECS Service

```bash
# Registrar task definition
aws ecs register-task-definition --cli-input-json file://task-definition.json

# Crear cluster
aws ecs create-cluster --cluster-name starwars-cluster

# Crear servicio
aws ecs create-service \
  --cluster starwars-cluster \
  --service-name starwars-api-service \
  --task-definition starwars-api:1 \
  --desired-count 2 \
  --launch-type FARGATE \
  --network-configuration "awsvpcConfiguration={subnets=[subnet-xxx],securityGroups=[sg-xxx],assignPublicIp=ENABLED}"
```

---

## ⚓ Kubernetes

### 1. Crear Namespace

```yaml
# namespace.yaml
apiVersion: v1
kind: Namespace
metadata:
  name: starwars
```

### 2. Secrets para Base de Datos

```yaml
# secrets.yaml
apiVersion: v1
kind: Secret
metadata:
  name: postgres-secret
  namespace: starwars
type: Opaque
stringData:
  postgres-password: YourSecurePassword123!
  connection-string: "Host=postgres-service;Port=5432;Database=starwarsdb;Username=starwars;Password=YourSecurePassword123!"
```

### 3. PostgreSQL Deployment

```yaml
# postgres-deployment.yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: postgres
  namespace: starwars
spec:
  replicas: 1
  selector:
    matchLabels:
      app: postgres
  template:
    metadata:
      labels:
        app: postgres
    spec:
      containers:
      - name: postgres
        image: postgres:16-alpine
        env:
        - name: POSTGRES_DB
          value: starwarsdb
        - name: POSTGRES_USER
          value: starwars
        - name: POSTGRES_PASSWORD
          valueFrom:
            secretKeyRef:
              name: postgres-secret
              key: postgres-password
        ports:
        - containerPort: 5432
        volumeMounts:
        - name: postgres-storage
          mountPath: /var/lib/postgresql/data
      volumes:
      - name: postgres-storage
        persistentVolumeClaim:
          claimName: postgres-pvc
---
apiVersion: v1
kind: Service
metadata:
  name: postgres-service
  namespace: starwars
spec:
  selector:
    app: postgres
  ports:
  - port: 5432
    targetPort: 5432
```

### 4. API Deployment

```yaml
# api-deployment.yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: starwars-api
  namespace: starwars
spec:
  replicas: 3
  selector:
    matchLabels:
      app: starwars-api
  template:
    metadata:
      labels:
        app: starwars-api
    spec:
      containers:
      - name: api
        image: your-registry/starwars-api:latest
        env:
        - name: ASPNETCORE_ENVIRONMENT
          value: Production
        - name: ConnectionStrings__DefaultConnection
          valueFrom:
            secretKeyRef:
              name: postgres-secret
              key: connection-string
        ports:
        - containerPort: 8080
        livenessProbe:
          httpGet:
            path: /health
            port: 8080
          initialDelaySeconds: 30
          periodSeconds: 10
        readinessProbe:
          httpGet:
            path: /health
            port: 8080
          initialDelaySeconds: 10
          periodSeconds: 5
---
apiVersion: v1
kind: Service
metadata:
  name: starwars-api-service
  namespace: starwars
spec:
  selector:
    app: starwars-api
  type: LoadBalancer
  ports:
  - port: 80
    targetPort: 8080
```

### 5. Desplegar

```bash
kubectl apply -f namespace.yaml
kubectl apply -f secrets.yaml
kubectl apply -f postgres-deployment.yaml
kubectl apply -f api-deployment.yaml

# Verificar
kubectl get pods -n starwars
kubectl get services -n starwars
```

---

## 🔒 Variables de Entorno

### Variables Requeridas

```bash
# Entorno
ASPNETCORE_ENVIRONMENT=Production

# Base de datos
ConnectionStrings__DefaultConnection="Host=your-host;Port=5432;Database=starwarsdb;Username=user;Password=pass"

# URLs de escucha
ASPNETCORE_URLS=http://+:8080

# Rate Limiting (opcional)
IpRateLimiting__GeneralRules__0__Limit=100
IpRateLimiting__GeneralRules__1__Limit=2000
```

### Variables Opcionales

```bash
# Logging
Logging__LogLevel__Default=Information
Logging__LogLevel__Microsoft=Warning

# Swagger (deshabilitado en prod)
EnableSwagger=false
```

---

## 🛡️ Seguridad en Producción

### 1. Passwords y Secrets

```bash
# Usar gestores de secretos
# Azure Key Vault
az keyvault secret set --vault-name YourVault --name DbPassword --value "SecurePass"

# AWS Secrets Manager
aws secretsmanager create-secret --name starwars/db-password --secret-string "SecurePass"

# Kubernetes Secrets
kubectl create secret generic db-secret --from-literal=password=SecurePass
```

### 2. HTTPS/SSL

```csharp
// Program.cs para forzar HTTPS
app.UseHttpsRedirection();
app.UseHsts();
```

### 3. CORS en Producción

```json
// appsettings.Production.json
{
  "Cors": {
    "AllowedOrigins": [
      "https://your-frontend-domain.com"
    ]
  }
}
```

### 4. Rate Limiting Ajustado

```json
{
  "IpRateLimiting": {
    "GeneralRules": [
      {
        "Endpoint": "*",
        "Period": "1m",
        "Limit": 30
      },
      {
        "Endpoint": "*",
        "Period": "1h",
        "Limit": 500
      }
    ]
  }
}
```

---

## 📊 Monitoreo

### Application Insights (Azure)

```csharp
// Program.cs
builder.Services.AddApplicationInsightsTelemetry();
```

### CloudWatch (AWS)

Configurado automáticamente con ECS Task Definition.

### Prometheus + Grafana

```yaml
# metrics-exporter.yaml
apiVersion: v1
kind: Service
metadata:
  name: metrics
spec:
  ports:
  - port: 9090
```

---

## 🔄 CI/CD

### GitHub Actions

```yaml
# .github/workflows/deploy.yml
name: Deploy to Production

on:
  push:
    branches: [ main ]

jobs:
  deploy:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v3
    
    - name: Build Docker Image
      run: docker build -t starwars-api .
    
    - name: Push to Registry
      run: |
        echo "${{ secrets.DOCKER_PASSWORD }}" | docker login -u "${{ secrets.DOCKER_USERNAME }}" --password-stdin
        docker push starwars-api:latest
    
    - name: Deploy to Kubernetes
      run: |
        kubectl set image deployment/starwars-api api=starwars-api:${{ github.sha }}
```

---

## ✅ Checklist de Despliegue

- [ ] Base de datos PostgreSQL configurada
- [ ] Variables de entorno configuradas
- [ ] Secrets gestionados de forma segura
- [ ] SSL/HTTPS configurado
- [ ] CORS configurado para producción
- [ ] Rate limiting ajustado
- [ ] Logging configurado
- [ ] Monitoreo configurado
- [ ] Backups automatizados
- [ ] Health checks funcionando
- [ ] Documentación actualizada

---

**¡Listo para producción!** 🚀

