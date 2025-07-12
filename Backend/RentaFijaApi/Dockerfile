# Etapa de compilación
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia el archivo .csproj para restaurar las dependencias primero
COPY ["RentaFijaApi.csproj", "RentaFijaApi/"] # Se define el nombre del proyecto 
RUN dotnet restore "RentaFijaApi/RentaFijaApi.csproj"

# Copia el resto de los archivos del proyecto
COPY . .
WORKDIR "/src/RentaFijaApi" 
# Construye la aplicación en modo Release
RUN dotnet build "RentaFijaApi.csproj" -c Release -o /app/build

# Etapa de publicación
FROM build AS publish
RUN dotnet publish "RentaFijaApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Etapa final de ejecución
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Copia los archivos publicados al directorio final
COPY --from=publish /app/publish .

# Define el punto de entrada de la aplicación
ENTRYPOINT ["dotnet", "RentaFijaApi.dll"] 