# Etapa de compilación
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copiar archivos del proyecto y restaurar dependencias
COPY ["PuestoWeb.csproj", "."]
RUN dotnet restore "./PuestoWeb.csproj"

# Copiar el resto de los archivos y compilar
COPY . .
RUN dotnet build "PuestoWeb.csproj" -c Release -o /app/build

# Etapa de publicación
FROM build AS publish
RUN dotnet publish "PuestoWeb.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Etapa final (Imagen de ejecución)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Exponer el puerto que usa Render (habitualmente el 8080 o el puerto definido por la variable PORT)
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "PuestoWeb.dll"]
