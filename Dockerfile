FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build-env
WORKDIR /app

# Desactivar telemetría y optimizar para Docker
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1

# Copiar csproj y restaurar
COPY *.csproj ./
RUN dotnet restore

# Copiar todo y publicar con optimización moderada
COPY . ./
RUN dotnet publish -c Release -o out --no-restore

# Imagen de ejecución (Debian robusta)
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build-env /app/out .

# SOLUCIÓN AL ERROR 139: Modo invariante de globalización
# Esto evita que .NET dependa de librerías ICU externas que fallan en algunos servidores
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1
ENV ASPNETCORE_URLS=http://+:8080
ENV PORT=8080

EXPOSE 8080

ENTRYPOINT ["dotnet", "PuestoWeb.dll"]
