#!/bin/bash

# =============================================================================
# Author: Vladyslav Zaiets | https://sarmkadan.com
# CTO & Software Architect
# =============================================================================

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Configuration
CONFIGURATION="${1:-Release}"
OUTPUT_DIR="bin/${CONFIGURATION}/net10.0"

echo -e "${GREEN}=== DotnetMicroOrm Build Script ===${NC}\n"

# Check if dotnet is installed
if ! command -v dotnet &> /dev/null; then
    echo -e "${RED}Error: .NET SDK is not installed${NC}"
    exit 1
fi

# Display version
echo "Current .NET version:"
dotnet --version
echo ""

# Clean previous builds
echo -e "${YELLOW}Cleaning previous builds...${NC}"
dotnet clean --configuration ${CONFIGURATION} 2>/dev/null || true
rm -rf ${OUTPUT_DIR}

# Restore dependencies
echo -e "${YELLOW}Restoring dependencies...${NC}"
dotnet restore

# Build project
echo -e "${YELLOW}Building project...${NC}"
dotnet build --configuration ${CONFIGURATION} --no-restore

# Run tests
echo -e "${YELLOW}Running tests...${NC}"
dotnet test --configuration ${CONFIGURATION} --no-build --verbosity minimal

# Create NuGet package
echo -e "${YELLOW}Creating NuGet package...${NC}"
dotnet pack --configuration ${CONFIGURATION} --output ./nupkg --no-build

# Display summary
echo ""
echo -e "${GREEN}=== Build Complete ===${NC}"
echo "Configuration: ${CONFIGURATION}"
echo "Output directory: ${OUTPUT_DIR}"
echo "NuGet package: ./nupkg/"
echo ""

# Display file sizes
if [ -d "${OUTPUT_DIR}" ]; then
    SIZE=$(du -sh "${OUTPUT_DIR}" | cut -f1)
    echo "Build size: ${SIZE}"
fi

exit 0
