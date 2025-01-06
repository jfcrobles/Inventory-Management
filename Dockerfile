# Imagen base para ejecución
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080  

# Imagen para compilación
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore "InventoryManagementApi/InventoryManagementApi.csproj"
RUN dotnet publish "InventoryManagementApi/InventoryManagementApi.csproj" -c Release -o /app/publish

# Imagen final
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "InventoryManagementApi.dll"]

# Establecer la URL de escucha a http://+:8080
ENV ASPNETCORE_URLS=http://*:8080
