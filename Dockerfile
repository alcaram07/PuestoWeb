# Etapa de compilación
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copiar todo y restaurar
COPY . .
RUN dotnet restore

# Publicar la aplicación
RUN dotnet publish "PuestoWeb.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Etapa final
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/publish .

# Render usa la variable PORT, así que configuramos .NET para que la escuche
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "PuestoWeb.dll"]
