# 🤝 Guía de Contribución

## Cómo Contribuir al Proyecto

¡Gracias por tu interés en contribuir al proyecto Star Wars API! Esta guía te ayudará a empezar.

## 🚀 Configuración del Entorno de Desarrollo

### Prerrequisitos

- .NET 8 SDK
- Docker Desktop
- PostgreSQL 16 (opcional si usas Docker)
- Git
- IDE recomendado: Visual Studio 2022 o VS Code con extensión C#

### Configuración Inicial

1. **Fork del repositorio**
```bash
git clone <your-fork-url>
cd StarWars
```

2. **Instalar dependencias**
```bash
dotnet restore
```

3. **Configurar base de datos**
```bash
docker-compose up -d postgres
```

4. **Aplicar migraciones**
```bash
cd src/StarWars.Api
dotnet ef database update
```

5. **Ejecutar la aplicación**
```bash
dotnet run
```

## 📝 Estándares de Código

### Convenciones de Nomenclatura

- **Clases**: PascalCase (`FavoriteCharacter`)
- **Métodos**: PascalCase (`GetCharactersAsync`)
- **Variables**: camelCase (`characterId`)
- **Constantes**: PascalCase (`DefaultCacheExpiration`)
- **Privados**: _camelCase (`_httpClient`)

### Estructura de Archivos

```
- Una clase por archivo
- Nombre del archivo = Nombre de la clase
- Organizar por feature/responsabilidad
```

### Comentarios

```csharp
/// <summary>
/// Descripción breve del método
/// </summary>
/// <param name="id">Descripción del parámetro</param>
/// <returns>Descripción del retorno</returns>
public async Task<Character?> GetCharacterByIdAsync(string id)
{
    // Implementación
}
```

## 🏗️ Arquitectura

### Capas del Proyecto

1. **Domain**: Entidades y modelos
2. **Application**: Interfaces y lógica de negocio
3. **Infrastructure**: Implementaciones concretas
4. **Api**: Controladores y configuración
5. **Client**: Aplicación de consola

### Flujo de Datos

```
Request → Controller → Service → Repository → Database
                    ↓
                 Response
```

## 🔀 Proceso de Contribución

### 1. Crear un Issue

Antes de empezar a trabajar, crea un issue describiendo:
- ¿Qué problema resuelve?
- ¿Qué propones cambiar?
- ¿Alguna consideración especial?

### 2. Crear una Rama

```bash
git checkout -b feature/nombre-descriptivo
# o
git checkout -b fix/descripcion-del-bug
```

### 3. Hacer Commits

```bash
git add .
git commit -m "feat: agregar funcionalidad X"
```

#### Convención de Commits

- `feat:` Nueva funcionalidad
- `fix:` Corrección de bug
- `docs:` Cambios en documentación
- `style:` Formato, punto y coma, etc.
- `refactor:` Refactorización de código
- `test:` Agregar o modificar tests
- `chore:` Mantenimiento

### 4. Push y Pull Request

```bash
git push origin feature/nombre-descriptivo
```

Luego crea un Pull Request en GitHub con:
- Título descriptivo
- Descripción detallada de los cambios
- Referencias a issues relacionados
- Screenshots (si aplica)

## ✅ Checklist Antes de Enviar PR

- [ ] El código compila sin errores
- [ ] Los tests pasan (si existen)
- [ ] La documentación está actualizada
- [ ] El código sigue los estándares del proyecto
- [ ] Los commits tienen mensajes descriptivos
- [ ] No hay conflictos con main

## 🧪 Testing

### Ejecutar Tests

```bash
dotnet test
```

### Agregar Tests

Los tests deben estar en el proyecto `StarWars.Tests` (a crear):

```csharp
[Fact]
public async Task GetCharacterById_ShouldReturnCharacter()
{
    // Arrange
    var service = new SwapiService(_httpClient);
    
    // Act
    var result = await service.GetCharacterByIdAsync("1");
    
    // Assert
    Assert.NotNull(result);
    Assert.Equal("Luke Skywalker", result.Name);
}
```

## 📚 Documentación

### Actualizar README

Si agregas una nueva funcionalidad, actualiza:
- README.md con la descripción
- SETUP.md si afecta la instalación
- TESTING.md si requiere nuevas pruebas

### Comentarios en Código

- Usa XML comments para clases y métodos públicos
- Explica el "por qué", no el "qué"
- Mantén los comentarios actualizados

## 🐛 Reportar Bugs

### Información a Incluir

1. **Descripción**: ¿Qué estaba haciendo cuando ocurrió?
2. **Pasos para Reproducir**:
   - Paso 1
   - Paso 2
   - Paso 3
3. **Comportamiento Esperado**: ¿Qué debería pasar?
4. **Comportamiento Actual**: ¿Qué pasó realmente?
5. **Entorno**:
   - OS: Windows/Linux/Mac
   - .NET Version
   - Docker Version

### Ejemplo

```markdown
## Bug: El caché no expira correctamente

**Descripción**: Los datos en caché no se eliminan después del TTL.

**Pasos para Reproducir**:
1. Hacer GET /api/v1/characters/1
2. Esperar 1 hora
3. Hacer GET /api/v1/characters/1 de nuevo
4. Los datos siguen siendo los mismos (deberían haberse actualizado)

**Comportamiento Esperado**: El caché debería expirar después de 1 hora.

**Comportamiento Actual**: Los datos permanecen en caché indefinidamente.

**Entorno**:
- OS: Windows 11
- .NET 8.0
- Docker 24.0.7
```

## 💡 Proponer Funcionalidades

### Template de Feature Request

```markdown
## Feature: Agregar autenticación JWT

**Problema**: Actualmente la API no tiene autenticación.

**Solución Propuesta**: Implementar JWT con refresh tokens.

**Alternativas Consideradas**: OAuth2, API Keys

**Beneficios**:
- Mayor seguridad
- Control de acceso
- Tracking de usuarios

**Complejidad**: Media

**¿Estás dispuesto a implementarlo?**: Sí
```

## 🎯 Áreas de Contribución

### Fácil
- Mejorar documentación
- Agregar ejemplos
- Corregir typos
- Actualizar dependencias

### Medio
- Agregar tests
- Mejorar logging
- Optimizar queries
- Agregar validaciones

### Avanzado
- Nuevas funcionalidades
- Refactorización mayor
- Performance optimization
- Integración con servicios externos

## 📞 Contacto

Si tienes preguntas:
- Abre un issue con la etiqueta `question`
- Revisa la documentación existente
- Consulta los issues cerrados

## 📄 Licencia

Al contribuir, aceptas que tus contribuciones se licencien bajo la misma licencia del proyecto.

## 🌟 Reconocimientos

Todos los contribuidores serán reconocidos en el README.md.

---

**¡Gracias por contribuir! May the Force be with you!** 🚀

