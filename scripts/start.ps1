# Script de inicio para Windows PowerShell
# Star Wars API - Quick Start

Write-Host "╔═══════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║          Star Wars API - Initialization Script               ║" -ForegroundColor Cyan
Write-Host "╚═══════════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""

# Verificar Docker
Write-Host "1️⃣ Verificando Docker..." -ForegroundColor Yellow
$dockerVersion = docker --version 2>$null
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Docker no está instalado o no está en el PATH" -ForegroundColor Red
    Write-Host "   Descarga Docker Desktop desde: https://www.docker.com/products/docker-desktop" -ForegroundColor Red
    exit 1
}
Write-Host "✅ Docker encontrado: $dockerVersion" -ForegroundColor Green

# Verificar Docker Compose
Write-Host "`n2️⃣ Verificando Docker Compose..." -ForegroundColor Yellow
$composeVersion = docker-compose --version 2>$null
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Docker Compose no está instalado" -ForegroundColor Red
    exit 1
}
Write-Host "✅ Docker Compose encontrado: $composeVersion" -ForegroundColor Green

# Verificar si Docker está corriendo
Write-Host "`n3️⃣ Verificando que Docker esté corriendo..." -ForegroundColor Yellow
docker ps >$null 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Docker no está corriendo. Por favor inicia Docker Desktop" -ForegroundColor Red
    exit 1
}
Write-Host "✅ Docker está corriendo" -ForegroundColor Green

# Detener servicios existentes si los hay
Write-Host "`n4️⃣ Limpiando servicios anteriores..." -ForegroundColor Yellow
docker-compose down -v 2>$null
Write-Host "✅ Limpieza completada" -ForegroundColor Green

# Iniciar servicios
Write-Host "`n5️⃣ Iniciando servicios..." -ForegroundColor Yellow
Write-Host "   Esto puede tomar 1-2 minutos la primera vez..." -ForegroundColor Gray
docker-compose up -d

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Error al iniciar los servicios" -ForegroundColor Red
    Write-Host "   Ver logs con: docker-compose logs" -ForegroundColor Red
    exit 1
}
Write-Host "✅ Servicios iniciados" -ForegroundColor Green

# Esperar a que la API esté lista
Write-Host "`n6️⃣ Esperando a que la API esté lista..." -ForegroundColor Yellow
$maxRetries = 30
$retryCount = 0
$apiReady = $false

while ($retryCount -lt $maxRetries -and -not $apiReady) {
    Start-Sleep -Seconds 2
    try {
        $response = Invoke-WebRequest -Uri "http://localhost:5000/health" -TimeoutSec 2 -UseBasicParsing 2>$null
        if ($response.StatusCode -eq 200) {
            $apiReady = $true
        }
    }
    catch {
        # Continuar intentando
    }
    $retryCount++
    Write-Host "." -NoNewline -ForegroundColor Gray
}
Write-Host ""

if (-not $apiReady) {
    Write-Host "⚠️  La API tardó mucho en iniciar. Verifica los logs:" -ForegroundColor Yellow
    Write-Host "   docker-compose logs -f starwars-api" -ForegroundColor Yellow
    exit 1
}

Write-Host "✅ API lista y respondiendo" -ForegroundColor Green

# Verificar health check
Write-Host "`n7️⃣ Verificando health check..." -ForegroundColor Yellow
try {
    $health = Invoke-RestMethod -Uri "http://localhost:5000/health" -Method Get
    Write-Host "✅ Health Status: $($health.status)" -ForegroundColor Green
}
catch {
    Write-Host "⚠️  Health check falló, pero la API responde" -ForegroundColor Yellow
}

# Prueba rápida de la API
Write-Host "`n8️⃣ Probando endpoints..." -ForegroundColor Yellow
try {
    $characters = Invoke-RestMethod -Uri "http://localhost:5000/api/v1/characters?page=1" -Method Get
    Write-Host "✅ Personajes encontrados: $($characters.count)" -ForegroundColor Green
    Write-Host "   Primeros 3 personajes:" -ForegroundColor Gray
    $characters.results[0..2] | ForEach-Object {
        Write-Host "   - $($_.name)" -ForegroundColor Cyan
    }
}
catch {
    Write-Host "⚠️  Error al obtener personajes" -ForegroundColor Yellow
}

# Resumen
Write-Host ""
Write-Host "╔═══════════════════════════════════════════════════════════════╗" -ForegroundColor Green
Write-Host "║                    ¡INSTALACIÓN EXITOSA!                      ║" -ForegroundColor Green
Write-Host "╚═══════════════════════════════════════════════════════════════╝" -ForegroundColor Green
Write-Host ""
Write-Host "🌐 Swagger UI (Documentación Interactiva):" -ForegroundColor Cyan
Write-Host "   http://localhost:5000" -ForegroundColor White
Write-Host ""
Write-Host "🔍 Health Check:" -ForegroundColor Cyan
Write-Host "   http://localhost:5000/health" -ForegroundColor White
Write-Host ""
Write-Host "📚 Ejemplos de uso:" -ForegroundColor Cyan
Write-Host "   # Listar personajes" -ForegroundColor Gray
Write-Host '   Invoke-RestMethod http://localhost:5000/api/v1/characters' -ForegroundColor White
Write-Host ""
Write-Host "   # Buscar personajes" -ForegroundColor Gray
Write-Host '   Invoke-RestMethod "http://localhost:5000/api/v1/characters/search?name=Luke"' -ForegroundColor White
Write-Host ""
Write-Host "   # Ver favoritos" -ForegroundColor Gray
Write-Host '   Invoke-RestMethod http://localhost:5000/api/v1/favorites' -ForegroundColor White
Write-Host ""
Write-Host "🖥️  Cliente de Consola:" -ForegroundColor Cyan
Write-Host "   cd src\StarWars.Client" -ForegroundColor White
Write-Host "   dotnet run" -ForegroundColor White
Write-Host ""
Write-Host "📊 Ver logs:" -ForegroundColor Cyan
Write-Host "   docker-compose logs -f" -ForegroundColor White
Write-Host ""
Write-Host "🛑 Detener servicios:" -ForegroundColor Cyan
Write-Host "   docker-compose down" -ForegroundColor White
Write-Host ""
Write-Host "May the Force be with you! 🌟" -ForegroundColor Magenta
Write-Host ""

