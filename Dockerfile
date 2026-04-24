# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app
COPY . .
RUN dotnet restore
RUN dotnet publish -c Release -o out /p:PublishReadyToRun=false /p:TieredCompilation=false

# Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0-bookworm-slim
WORKDIR /app
COPY --from=build /app/out .

# LIMITAR MEMORIA Y DESACTIVAR OPTIMIZACIONES QUE CAUSAN CRASH 139
ENV DOTNET_GCHeapHardLimit=1C000000 
ENV DOTNET_gcServer=0
ENV DOTNET_TieredCompilation=0
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1
ENV DOTNET_EnableHWIntrinsic=0

ENV ASPNETCORE_URLS=http://+:8080
ENV PORT=8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "PuestoWeb.dll"]
