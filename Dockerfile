# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore
RUN dotnet publish "PuestoWeb.csproj" -c Release -o /app/publish

# Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/publish .

# SOLUCIONES CRÍTICAS PARA ERROR 139 EN RENDER:
# 1. Desactivar intrínsecas de hardware (EVITA EL SEGFAULT 139)
ENV DOTNET_EnableHWIntrinsic=0
# 2. Desactivar compilación escalonada
ENV DOTNET_TieredCompilation=0
# 3. Modo invariante de globalización (ahorra memoria y evita fallos de ICU)
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1
# 4. Usar GC de estación de trabajo (usa menos RAM que el de servidor)
ENV DOTNET_gcServer=0

ENV ASPNETCORE_URLS=http://+:8080
ENV PORT=8080

EXPOSE 8080

ENTRYPOINT ["dotnet", "PuestoWeb.dll"]
