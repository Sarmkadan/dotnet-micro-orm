.PHONY: help build test clean restore run publish docker docker-up docker-down coverage lint format

# Variables
DOTNET ?= dotnet
CONFIGURATION ?= Release
OUTPUT_DIR ?= bin/$(CONFIGURATION)/net10.0

help:
	@echo "DotnetMicroOrm - Build and Development Commands"
	@echo ""
	@echo "Usage: make [target]"
	@echo ""
	@echo "Available targets:"
	@echo "  help           Show this help message"
	@echo "  restore        Restore dependencies"
	@echo "  build          Build the project"
	@echo "  clean          Clean build artifacts"
	@echo "  test           Run unit tests"
	@echo "  coverage       Run tests with coverage"
	@echo "  run            Run the application"
	@echo "  publish        Publish release build"
	@echo "  pack           Create NuGet package"
	@echo "  docker         Build Docker image"
	@echo "  docker-up      Start Docker Compose services"
	@echo "  docker-down    Stop Docker Compose services"
	@echo "  lint           Run code analysis"
	@echo "  format         Format code according to .editorconfig"
	@echo "  docs           Generate documentation"

restore:
	@echo "Restoring dependencies..."
	$(DOTNET) restore

build: restore
	@echo "Building project..."
	$(DOTNET) build -c $(CONFIGURATION)

clean:
	@echo "Cleaning build artifacts..."
	$(DOTNET) clean
	@rm -rf bin/ obj/
	@echo "Clean complete"

test: build
	@echo "Running tests..."
	$(DOTNET) test -c $(CONFIGURATION) --no-build --verbosity minimal

coverage: build
	@echo "Running tests with coverage..."
	$(DOTNET) test -c $(CONFIGURATION) --no-build \
		/p:CollectCoverage=true \
		/p:CoverageFormat=opencover \
		/p:CoverageFilename=coverage.xml

run: build
	@echo "Running application..."
	$(DOTNET) run -c $(CONFIGURATION)

publish: clean
	@echo "Publishing release build..."
	$(DOTNET) publish -c $(CONFIGURATION) --output $(OUTPUT_DIR)
	@echo "Published to $(OUTPUT_DIR)"

pack: clean
	@echo "Creating NuGet package..."
	$(DOTNET) pack -c $(CONFIGURATION)
	@echo "Package created in bin/Release/"

docker:
	@echo "Building Docker image..."
	docker build -t dotnet-micro-orm:latest .
	@echo "Docker image built successfully"

docker-up:
	@echo "Starting Docker Compose services..."
	docker-compose up -d
	@echo "Services started"

docker-down:
	@echo "Stopping Docker Compose services..."
	docker-compose down
	@echo "Services stopped"

lint:
	@echo "Running code analysis..."
	$(DOTNET) build -c $(CONFIGURATION) /p:EnforceCodeStyleInBuild=true

format:
	@echo "Formatting code..."
	$(DOTNET) format --verify-no-changes

format-fix:
	@echo "Formatting code and applying fixes..."
	$(DOTNET) format

docs:
	@echo "Generating documentation..."
	@echo "Documentation located in docs/ directory"

# Development targets
dev: restore
	@echo "Setting up development environment..."
	$(DOTNET) build

watch:
	@echo "Watching for changes..."
	$(DOTNET) watch run

info:
	@echo "DotnetMicroOrm Build Information"
	@echo "================================="
	@echo "Configuration: $(CONFIGURATION)"
	@echo ".NET Runtime:"
	@$(DOTNET) --version

all: clean restore build test
	@echo "Build complete"

.DEFAULT_GOAL := help
