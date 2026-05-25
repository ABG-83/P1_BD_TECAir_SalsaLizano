# 🚀 Guía de Configuración y Ejecución del Backend - TECAir

Este proyecto está desarrollado con **.NET 10**. Sigue estos pasos para configurar tu entorno local y correr el servidor de desarrollo sin exponer credenciales en GitHub.

---

## 🛠️ Requisitos Previos

Antes de empezar, asegúrate de tener instalado en tu computadora:

- [.NET SDK 10](https://dotnet.microsoft.com/download)
- Una instancia local de **PostgreSQL** corriendo.

---

## ⚙️ Configuración Inicial (Primer Inicio)

El archivo `appsettings.json` real está protegido por el `.gitignore` para no subir contraseñas al repositorio. Debes crear tu propia plantilla local siguiendo estos pasos:

### 1. Crear el archivo de configuración

Ve a la carpeta del proyecto principal de la API (`TECAir.API/`) y crea un archivo nuevo llamado **`appsettings.json`**.

### 2. Copiar la plantilla base

Abre el archivo `appsettings.Example.json` que está en la raíz, copia todo su contenido y pégalo dentro de tu nuevo archivo `appsettings.json`.

### 3. Configurar tus credenciales reales

Busca la sección `ConnectionStrings` dentro de tu `appsettings.json` y reemplaza los valores de ejemplo por tus credenciales locales de PostgreSQL:

```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=tecair_dev;Username=TU_USUARIO;Password=TU_CONTRASEÑA"
}
```

## 🗄️ Iniciar la Base de Datos PostgreSQL

Estas instrucciones asumen que ya tienes PostgreSQL corriendo localmente y que el comando `psql` está disponible en tu terminal.

### 0. Guardar tu contraseña una sola vez

En PowerShell puedes guardar la contraseña de `postgres` en una variable de entorno para esta sesión y reutilizarla en todos los comandos:

```powershell
$env:PGPASS = "TU_CONTRASEÑA"
$PGHOST = "localhost"
$PGUSER = "postgres"
$PGDATABASE = "tecair_dev"
```

### 1. Crear la base de datos

```powershell
psql -h $PGHOST -U $PGUSER -d postgres -v ON_ERROR_STOP=1 -c "DROP DATABASE IF EXISTS $PGDATABASE;"
psql -h $PGHOST -U $PGUSER -d postgres -v ON_ERROR_STOP=1 -c "CREATE DATABASE $PGDATABASE;"
```

### 2. Cargar el esquema vacío

```powershell
psql -h $PGHOST -U $PGUSER -d $PGDATABASE -v ON_ERROR_STOP=1 -f "TECAir.Data/Scripts/EmptyState.sql"
```

### 3. Cargar los datos iniciales

```powershell
psql -h $PGHOST -U $PGUSER -d $PGDATABASE -v ON_ERROR_STOP=1 -f "TECAir.Data/Scripts/InitialState.sql"
```

### 4. Verificar que quedó cargado

```powershell
psql -h $PGHOST -U $PGUSER -d $PGDATABASE -c "SELECT COUNT(*) FROM users;"
psql -h $PGHOST -U $PGUSER -d $PGDATABASE -c "SELECT COUNT(*) FROM reservations;"
psql -h $PGHOST -U $PGUSER -d $PGDATABASE -c "SELECT COUNT(*) FROM payments;"
```

> Si prefieres, también puedes usar `PGPASSWORD` en lugar de `$env:PGPASS`.
> Si `psql` no está en tu PATH, usa la ruta completa del ejecutable o abre la terminal desde `pgAdmin`/el cliente que uses.

## 🏃 Ejecucción del Proyecto

Abre tu terminal, asegúrate de estar parado en la carpeta raíz del backend (donde se encuentra el archivo .sln) y utiliza uno de los siguientes comandos:

### Opción A: Modo Tradicional (Compilación Estática)

Para levantar el servidor de forma normal, ejecuta:

```bash
dotnet run --project TECAir.API
```

### Opción B: Modo Hot Reload (Recomendado para Desarrollo) 🔥

Para programar de forma fluida sin tener que apagar y encender el servidor cada vez que haces un cambio en el código, utiliza el modo de recarga rápida:

```bash
dotnet watch --project TECAir.API
```
