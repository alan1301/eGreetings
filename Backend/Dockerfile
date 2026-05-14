# syntax=docker/dockerfile:1.7
# ── Stage 1: build & publish ─────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution + csproj first để tận dụng layer cache cho restore
COPY EGreetings.sln ./
COPY src/EGreetings.API/EGreetings.API.csproj            src/EGreetings.API/
COPY src/EGreetings.Application/EGreetings.Application.csproj src/EGreetings.Application/
COPY src/EGreetings.Domain/EGreetings.Domain.csproj      src/EGreetings.Domain/
COPY src/EGreetings.Infrastructure/EGreetings.Infrastructure.csproj src/EGreetings.Infrastructure/
COPY src/EGreetings.Shared/EGreetings.Shared.csproj      src/EGreetings.Shared/

RUN dotnet restore src/EGreetings.API/EGreetings.API.csproj

# Copy phần còn lại rồi publish
COPY . .
RUN dotnet publish src/EGreetings.API/EGreetings.API.csproj \
        -c Release \
        -o /app/publish \
        --no-restore \
        /p:UseAppHost=false

# ── Stage 2: runtime ─────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Development \
    DOTNET_RUNNING_IN_CONTAINER=true

COPY --from=build /app/publish ./

EXPOSE 8080
ENTRYPOINT ["dotnet", "EGreetings.API.dll"]
