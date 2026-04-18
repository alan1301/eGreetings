FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy shared projects
COPY ["shared/EGreetings.Shared.Domain/EGreetings.Shared.Domain.csproj", "shared/EGreetings.Shared.Domain/"]
COPY ["shared/EGreetings.Shared.Contracts/EGreetings.Shared.Contracts.csproj", "shared/EGreetings.Shared.Contracts/"]
COPY ["shared/EGreetings.Shared.Infrastructure/EGreetings.Shared.Infrastructure.csproj", "shared/EGreetings.Shared.Infrastructure/"]

# Copy identity service projects
COPY ["services/identity/EGreetings.Identity.API/EGreetings.Identity.API.csproj", "services/identity/EGreetings.Identity.API/"]
COPY ["services/identity/EGreetings.Identity.Application/EGreetings.Identity.Application.csproj", "services/identity/EGreetings.Identity.Application/"]
COPY ["services/identity/EGreetings.Identity.Domain/EGreetings.Identity.Domain.csproj", "services/identity/EGreetings.Identity.Domain/"]
COPY ["services/identity/EGreetings.Identity.Infrastructure/EGreetings.Identity.Infrastructure.csproj", "services/identity/EGreetings.Identity.Infrastructure/"]

RUN dotnet restore "services/identity/EGreetings.Identity.API/EGreetings.Identity.API.csproj"

COPY . .

WORKDIR "/src/services/identity/EGreetings.Identity.API"
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EGreetings.Identity.API.dll"]
