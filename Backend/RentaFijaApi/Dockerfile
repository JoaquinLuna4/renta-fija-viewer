# Etapa de compilación
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia el archivo .csproj para restaurar las dependencias primero
COPY ["RentaFijaApi.csproj", "RentaFijaApi/"] 
RUN dotnet restore "RentaFijaApi/RentaFijaApi.csproj"

# Copia el resto de los archivos del proyecto
COPY . .
WORKDIR "/src/RentaFijaApi" # Este es el WORKDIR durante la compilación

# Construye la aplicación en modo Release
# El output va a una subcarpeta del WORKDIR actual (/src/RentaFijaApi/build_output/)
RUN dotnet build "RentaFijaApi.csproj" -c Release -o ./build_output/

# Etapa de publicación
FROM build AS publish
# El output va a una subcarpeta del WORKDIR actual (/src/RentaFijaApi/publish_output/)
RUN dotnet publish "RentaFijaApi.csproj" -c Release -o ./publish_output/ /p:UseAppHost=false

# Etapa final de ejecución
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app # Este es el WORKDIR de la imagen final (usualmente /app)

# Copia los archivos publicados desde la etapa 'publish' a la etapa 'final'
# La ruta de origen es relativa al WORKDIR de la etapa 'publish'
COPY --from=publish /src/RentaFijaApi/publish_output/ . 

# Define el punto de entrada de la aplicación
ENTRYPOINT ["dotnet", "RentaFijaApi.dll"]