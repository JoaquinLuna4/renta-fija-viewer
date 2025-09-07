# Rentapp: Visualizador de Activos de Renta Fija

Rentapp es una aplicación web diseñada para facilitar el acceso y la visualización de datos financieros del mercado de capitales argentino, específicamente los informes de Letras del Tesoro (Letes) publicados por el Instituto Argentino de Mercado de Capitales (IAMC).

La aplicación extrae la información de los complejos informes en formato PDF y la presenta en una interfaz limpia, moderna y amigable, permitiendo a los usuarios buscar, filtrar y analizar los datos de una manera mucho más eficiente.

## ✨ Features

- **Visualización Clara:** Convierte los datos de un PDF a una tabla interactiva y fácil de leer.
- **Búsqueda y Filtrado:** Permite buscar activos por su ticker o cualquier otro campo de la tabla.
- **Actualización Automática:** El sistema verifica diariamente si hay nuevos informes en el sitio del IAMC para mantener los datos actualizados.
- **Interfaz Moderna:** Desarrollada con Material-UI para una experiencia de usuario intuitiva.
- **Procesamiento con IA:** Utiliza la inteligencia artificial de Google Gemini para interpretar los informes y extraer la información de manera estructurada.

## 🚀 Tecnologías Implementadas

La aplicación sigue una arquitectura de cliente-servidor:

### Backend

- **Framework:** .NET / ASP.NET Core
- **Lenguaje:** C#
- **Procesamiento de Datos:** La API de Google Gemini se utiliza para leer e interpretar el contenido de los informes PDF.
- **Caching:** Los resultados de la API de Gemini se cachean para optimizar el rendimiento y la velocidad en consultas posteriores.
- **Hosting:** Desplegado en [Render](https://render.com/).

**Nota:** El backend está alojado en un plan gratuito de Render, lo que significa que el servidor puede entrar en modo "sleep" tras un período de inactividad. La primera carga puede tardar unos segundos mientras el servidor se "despierta".

### Frontend

- **Framework:** React
- **Bundler:** Vite
- **UI Kit:** Material-UI
- **Lenguaje:** JavaScript (JSX)
- **Hosting:** Desplegado en [Vercel](https://vercel.com/).

## ⚙️ Guía de Inicio (Getting Started)

Para correr este proyecto de forma local, necesitarás tener instalado .NET SDK, Node.js y pnpm.

### 1. Clonar el Repositorio

```bash
git clone https://github.com/JoaquinLuna4/renta-fija-viewer.git
cd RentaFija
```

### 2. Configurar y Correr el Backend

Navega a la carpeta del backend:

```bash
cd Backend/RentaFijaApi
```

Restaura las dependencias de .NET:

```bash
dotnet restore
```

Ejecuta el servidor de desarrollo:

```bash
dotnet run
```

El backend estará corriendo en `http://localhost:5000` (o el puerto que tengas configurado).

### 3. Configurar y Correr el Frontend

En otra terminal, navega a la carpeta del frontend:

```bash
cd Frontend/rentafija
```

Instala las dependencias usando pnpm:

```bash
pnpm install
```

Ejecuta el cliente de desarrollo:

```bash
pnpm dev
```

La aplicación frontend estará disponible en `http://localhost:5173` (o el puerto que indique Vite).

## 🌐 Fuente de los Datos

Todos los datos son extraídos de los informes públicos del **Instituto Argentino de Mercado de Capitales (IAMC)**. Puedes consultar la fuente original en [iamc.com.ar](https://www.iamc.com.ar/).
