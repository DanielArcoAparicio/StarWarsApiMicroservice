# GitHub Actions Workflows

Este repositorio incluye workflows de GitHub Actions para automatizar la ejecución de tests y la generación de reportes de cobertura.

## Workflows Disponibles

### CI - Tests and Coverage

Este workflow se ejecuta automáticamente cuando:
- Se hace push a las ramas `main` o `develop`
- Se crea un Pull Request hacia `main` o `develop`
- Se ejecuta manualmente desde la pestaña "Actions"

**Qué hace:**
1. ✅ Restaura las dependencias del proyecto
2. 🔨 Compila la solución en modo Release
3. 🧪 Ejecuta todos los tests unitarios
4. 📊 Genera reporte de cobertura de código
5. 📤 Sube el reporte a Codecov (opcional)
6. 📄 Publica el reporte de cobertura en GitHub Pages (solo en `main`)

## Acceso al Reporte de Cobertura

Una vez que el workflow se ejecute en la rama `main`, el reporte de cobertura estará disponible en:

**GitHub Pages**: `https://[TU-USUARIO].github.io/[NOMBRE-REPO]/coverage/`

Para habilitar GitHub Pages:
1. Ve a **Settings** → **Pages** en tu repositorio
2. En **Source**, selecciona **GitHub Actions**
3. Guarda los cambios

## Configuración Opcional

### Codecov

El workflow incluye integración con Codecov. Para habilitarla completamente:
1. Regístrate en [codecov.io](https://codecov.io)
2. Conecta tu repositorio
3. El workflow automáticamente subirá los reportes

### Badges de Cobertura

Puedes agregar un badge de cobertura a tu README:

```markdown
![Coverage](https://codecov.io/gh/[TU-USUARIO]/[NOMBRE-REPO]/branch/main/graph/badge.svg)
```

## Ver Resultados

1. Ve a la pestaña **Actions** en tu repositorio
2. Selecciona el workflow "CI - Tests and Coverage"
3. Haz clic en la ejecución más reciente
4. Revisa los logs y artifacts generados

## Artifacts Generados

- **test-results**: Resultados de los tests en formato TRX y XML
- **coverage-report**: Reporte HTML completo de cobertura de código

