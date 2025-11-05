#!/bin/bash
# Script de inicio para Linux/Mac
# Star Wars API - Quick Start

set -e

# Colores
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
MAGENTA='\033[0;35m'
GRAY='\033[0;90m'
NC='\033[0m' # No Color

echo -e "${CYAN}╔═══════════════════════════════════════════════════════════════╗${NC}"
echo -e "${CYAN}║          Star Wars API - Initialization Script               ║${NC}"
echo -e "${CYAN}╚═══════════════════════════════════════════════════════════════╝${NC}"
echo ""

# Verificar Docker
echo -e "${YELLOW}1️⃣ Verificando Docker...${NC}"
if ! command -v docker &> /dev/null; then
    echo -e "${RED}❌ Docker no está instalado${NC}"
    echo -e "${RED}   Descarga Docker desde: https://www.docker.com/get-started${NC}"
    exit 1
fi
DOCKER_VERSION=$(docker --version)
echo -e "${GREEN}✅ Docker encontrado: $DOCKER_VERSION${NC}"

# Verificar Docker Compose
echo -e "\n${YELLOW}2️⃣ Verificando Docker Compose...${NC}"
if ! command -v docker-compose &> /dev/null; then
    echo -e "${RED}❌ Docker Compose no está instalado${NC}"
    exit 1
fi
COMPOSE_VERSION=$(docker-compose --version)
echo -e "${GREEN}✅ Docker Compose encontrado: $COMPOSE_VERSION${NC}"

# Verificar si Docker está corriendo
echo -e "\n${YELLOW}3️⃣ Verificando que Docker esté corriendo...${NC}"
if ! docker ps > /dev/null 2>&1; then
    echo -e "${RED}❌ Docker no está corriendo${NC}"
    echo -e "${RED}   Inicia Docker y vuelve a ejecutar este script${NC}"
    exit 1
fi
echo -e "${GREEN}✅ Docker está corriendo${NC}"

# Detener servicios existentes
echo -e "\n${YELLOW}4️⃣ Limpiando servicios anteriores...${NC}"
docker-compose down -v > /dev/null 2>&1 || true
echo -e "${GREEN}✅ Limpieza completada${NC}"

# Iniciar servicios
echo -e "\n${YELLOW}5️⃣ Iniciando servicios...${NC}"
echo -e "${GRAY}   Esto puede tomar 1-2 minutos la primera vez...${NC}"
if ! docker-compose up -d; then
    echo -e "${RED}❌ Error al iniciar los servicios${NC}"
    echo -e "${RED}   Ver logs con: docker-compose logs${NC}"
    exit 1
fi
echo -e "${GREEN}✅ Servicios iniciados${NC}"

# Esperar a que la API esté lista
echo -e "\n${YELLOW}6️⃣ Esperando a que la API esté lista...${NC}"
MAX_RETRIES=30
RETRY_COUNT=0
API_READY=false

while [ $RETRY_COUNT -lt $MAX_RETRIES ] && [ "$API_READY" = false ]; do
    sleep 2
    if curl -s -f http://localhost:5000/health > /dev/null 2>&1; then
        API_READY=true
    fi
    RETRY_COUNT=$((RETRY_COUNT + 1))
    echo -n "."
done
echo ""

if [ "$API_READY" = false ]; then
    echo -e "${YELLOW}⚠️  La API tardó mucho en iniciar. Verifica los logs:${NC}"
    echo -e "${YELLOW}   docker-compose logs -f starwars-api${NC}"
    exit 1
fi

echo -e "${GREEN}✅ API lista y respondiendo${NC}"

# Verificar health check
echo -e "\n${YELLOW}7️⃣ Verificando health check...${NC}"
HEALTH=$(curl -s http://localhost:5000/health | grep -o '"status":"[^"]*"' || echo "")
if [ -n "$HEALTH" ]; then
    echo -e "${GREEN}✅ Health Status: $HEALTH${NC}"
else
    echo -e "${YELLOW}⚠️  Health check falló, pero la API responde${NC}"
fi

# Prueba rápida de la API
echo -e "\n${YELLOW}8️⃣ Probando endpoints...${NC}"
CHARACTERS=$(curl -s http://localhost:5000/api/v1/characters?page=1)
if [ $? -eq 0 ]; then
    echo -e "${GREEN}✅ Endpoint de personajes funciona${NC}"
    echo -e "${GRAY}   Primeros 3 personajes:${NC}"
    echo "$CHARACTERS" | grep -o '"name":"[^"]*"' | head -3 | sed 's/"name":"/   - /g' | sed 's/"//g' | while read line; do
        echo -e "${CYAN}$line${NC}"
    done
else
    echo -e "${YELLOW}⚠️  Error al obtener personajes${NC}"
fi

# Resumen
echo ""
echo -e "${GREEN}╔═══════════════════════════════════════════════════════════════╗${NC}"
echo -e "${GREEN}║                    ¡INSTALACIÓN EXITOSA!                      ║${NC}"
echo -e "${GREEN}╚═══════════════════════════════════════════════════════════════╝${NC}"
echo ""
echo -e "${CYAN}🌐 Swagger UI (Documentación Interactiva):${NC}"
echo -e "   http://localhost:5000"
echo ""
echo -e "${CYAN}🔍 Health Check:${NC}"
echo -e "   http://localhost:5000/health"
echo ""
echo -e "${CYAN}📚 Ejemplos de uso:${NC}"
echo -e "${GRAY}   # Listar personajes${NC}"
echo -e "   curl http://localhost:5000/api/v1/characters"
echo ""
echo -e "${GRAY}   # Buscar personajes${NC}"
echo -e '   curl "http://localhost:5000/api/v1/characters/search?name=Luke"'
echo ""
echo -e "${GRAY}   # Ver favoritos${NC}"
echo -e "   curl http://localhost:5000/api/v1/favorites"
echo ""
echo -e "${CYAN}🖥️  Cliente de Consola:${NC}"
echo -e "   cd src/StarWars.Client"
echo -e "   dotnet run"
echo ""
echo -e "${CYAN}📊 Ver logs:${NC}"
echo -e "   docker-compose logs -f"
echo ""
echo -e "${CYAN}🛑 Detener servicios:${NC}"
echo -e "   docker-compose down"
echo ""
echo -e "${MAGENTA}May the Force be with you! 🌟${NC}"
echo ""

