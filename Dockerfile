# =============================================================================
# Author: Vladyslav Zaiets | https://sarmkadan.com
# CTO & Software Architect
# =============================================================================

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10 AS build
WORKDIR /src

# Copy project files
COPY ["DotnetMicroOrm.csproj", "./"]
COPY ["src/", "./src/"]

# Restore dependencies
RUN dotnet restore "DotnetMicroOrm.csproj"

# Build application
RUN dotnet build "DotnetMicroOrm.csproj" -c Release -o /app/build

# Publish stage
FROM mcr.microsoft.com/dotnet/runtime:10 AS runtime
WORKDIR /app

# Copy built application
COPY --from=build /app/build .

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=40s --retries=3 \
    CMD curl -f http://localhost/health || exit 1

# Set environment
ENV DOTNET_RUNNING_IN_CONTAINER=true
ENV ASPNETCORE_URLS=http://+:80

# Expose port
EXPOSE 80

# Run application
ENTRYPOINT ["dotnet", "DotnetMicroOrm.dll"]
