# Etapa de compilación y publicación
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore
# Publicamos como "Self-Contained" para evitar dependencias externas en el servidor
RUN dotnet publish "PuestoWeb.csproj" -c Release -o /app/publish \
    --runtime linux-x64 \
    --self-contained true \
    /p:PublishTrimmed=false \
    /p:PublishReadyToRun=false

# Etapa final (Ubuntu robusto para evitar errores de segmentación)
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/publish .

# CONFIGURACIONES CRÍTICAS PARA ERROR 139:
# 1. Desactivar optimizaciones JIT que fallan en servidores pequeños
ENV DOTNET_TieredCompilation=0
# 2. Modo invariante de globalización
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1
# 3. Desactivar diagnósticos innecesarios
ENV COMPlus_EnableDiagnostics=0

ENV ASPNETCORE_URLS=http://+:8080
ENV PORT=8080

EXPOSE 8080

ENTRYPOINT ["./PuestoWeb"]
