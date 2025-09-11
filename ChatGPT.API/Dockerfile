FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
ENV ASPNETCORE_URLS=http://+:80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ./ChatGPT.API/ChatGPT.API.csproj ChatGPT.API/
COPY ./ChatGPT.Application/ChatGPT.Application.csproj ChatGPT.Application/
COPY ./ChatGPT.Infrastructure/ChatGPT.Infrastructure.csproj ChatGPT.Infrastructure/
COPY ./ChatGPT.Domain/ChatGPT.Domain.csproj ChatGPT.Domain/
COPY ./BuildingBlock/BuildingBlock.csproj BuildingBlock/
RUN dotnet restore ChatGPT.API/ChatGPT.API.csproj
COPY . .
RUN dotnet publish ChatGPT.API/ChatGPT.API.csproj -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ChatGPT.API.dll"]

# Use the official .NET 8 runtime as base image
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS base
WORKDIR /app
EXPOSE 80

# Use the official .NET 8 SDK for building
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /src

# Copy project files
COPY ["ChatGPT.API/ChatGPT.API.csproj", "ChatGPT.API/"]
COPY ["ChatGPT.Application/ChatGPT.Application.csproj", "ChatGPT.Application/"]
COPY ["ChatGPT.Domain/ChatGPT.Domain.csproj", "ChatGPT.Domain/"]
COPY ["ChatGPT.Infrastructure/ChatGPT.Infrastructure.csproj", "ChatGPT.Infrastructure/"]
COPY ["BuildingBlock/BuildingBlock.csproj", "BuildingBlock/"]

# Restore dependencies
RUN dotnet restore "ChatGPT.API/ChatGPT.API.csproj"

# Copy source code
COPY . .

# Build the application
WORKDIR "/src/ChatGPT.API"
RUN dotnet build "ChatGPT.API.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "ChatGPT.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Create final image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create non-root user
RUN adduser -D -s /bin/sh appuser
USER appuser

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:80/health || exit 1

ENTRYPOINT ["dotnet", "ChatGPT.API.dll"]


