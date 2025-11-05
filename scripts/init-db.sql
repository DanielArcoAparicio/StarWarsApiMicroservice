-- Script de inicialización de base de datos
-- Este script se ejecutará automáticamente al crear el contenedor de PostgreSQL

-- Crear extensiones necesarias
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Crear índices adicionales para mejor performance
-- (Las tablas se crean automáticamente mediante EF Core Migrations)

-- Comentarios sobre las tablas
COMMENT ON DATABASE starwarsdb IS 'Base de datos para el microservicio Star Wars API';

