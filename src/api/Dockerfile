# ── Stage 1: Build ────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files first for better layer caching.
# dotnet restore only re-runs when a .csproj changes.
COPY TaskTracker.sln .
COPY TaskTracker.Api/TaskTracker.Api.csproj             TaskTracker.Api/
COPY TaskTracker.Application/TaskTracker.Application.csproj TaskTracker.Application/
COPY TaskTracker.Domain/TaskTracker.Domain.csproj       TaskTracker.Domain/
COPY TaskTracker.Infrastructure/TaskTracker.Infrastructure.csproj TaskTracker.Infrastructure/

RUN dotnet restore TaskTracker.Api/TaskTracker.Api.csproj

# Copy the rest of the source and publish
COPY TaskTracker.Api/           TaskTracker.Api/
COPY TaskTracker.Application/   TaskTracker.Application/
COPY TaskTracker.Domain/        TaskTracker.Domain/
COPY TaskTracker.Infrastructure/ TaskTracker.Infrastructure/

RUN dotnet publish TaskTracker.Api/TaskTracker.Api.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# ── Stage 2: Runtime ──────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Create the directory used for the SQLite database volume mount
RUN mkdir -p /data

COPY --from=build /app/publish .

# .NET 9 defaults to port 8080 inside containers
EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080

ENTRYPOINT ["dotnet", "TaskTracker.Api.dll"]
