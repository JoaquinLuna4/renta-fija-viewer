# Etapa de compilación
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Establece el directorio de trabajo para todas las operaciones de compilación
# Aquí es donde se copiarán todos los archivos del proyecto.
WORKDIR /app_build_src 

# Copia los archivos del proyecto para la restauración de dependencias
# Asumimos que el .csproj está directamente en el contexto de Docker
COPY ["RentaFijaApi.csproj", "./"] 

# Restaura las dependencias
RUN dotnet restore "RentaFijaApi.csproj"

# Copia el resto de los archivos del proyecto al WORKDIR
# Asumimos que todos los archivos (controladores, servicios, Program.cs) 
# están directamente en el contexto de Docker.
COPY . . 

# Construye la aplicación en modo Release
# El output va a una subcarpeta del WORKDIR actual (/app_build_src/publish_build/)
RUN dotnet build "RentaFijaApi.csproj" -c Release -o ./publish_build/

# Etapa de publicación
FROM build AS publish
# Publica la aplicación, el output va a una subcarpeta del WORKDIR actual
RUN dotnet publish "RentaFijaApi.csproj" -c Release -o ./publish_app/ /p:UseAppHost=false

# Etapa final de ejecución
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app # Este es el WORKDIR de la imagen final (usualmente /app)

# Copia los archivos publicados desde la etapa 'publish' a la etapa 'final'
# La ruta de origen es consistente con el WORKDIR de la etapa 'build' (/app_build_src/)
COPY --from=publish /app_build_src/publish_app/ . 

# Define el punto de entrada de la aplicación
ENTRYPOINT ["dotnet", "RentaFijaApi.dll"]