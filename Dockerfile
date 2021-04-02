# =============================================================================
# Author: Vladyslav Zaiets | https://sarmkadan.com
# CTO & Software Architect
# =============================================================================

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files and restore
COPY ["DotnetMicroOrm.csproj", "./"]
RUN dotnet restore "DotnetMicroOrm.csproj"

# Copy source and build
COPY ["src/", "./src/"]
RUN dotnet build "DotnetMicroOrm.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "DotnetMicroOrm.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

RUN addgroup --system appgroup && adduser --system --ingroup appgroup appuser

COPY --from=publish /app/publish .

RUN chown -R appuser:appgroup /app
USER appuser

ENV ASPNETCORE_URLS=http://+:8080
ENV DOTNET_RUNNING_IN_CONTAINER=true

EXPOSE 8080

HEALTHCHECK --interval=30s --timeout=5s --start-period=10s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "DotnetMicroOrm.dll"]
