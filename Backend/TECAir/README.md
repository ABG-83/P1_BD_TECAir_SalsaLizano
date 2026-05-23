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
  "DefaultConnection": "Host=localhost;Port=5432;Database=tec_air_db;Username=TU_USUARIO;Password=TU_CONTRASEÑA"
}
```

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
