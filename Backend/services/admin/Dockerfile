FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy shared projects
COPY ["shared/EGreetings.Shared.Domain/EGreetings.Shared.Domain.csproj", "shared/EGreetings.Shared.Domain/"]
COPY ["shared/EGreetings.Shared.Contracts/EGreetings.Shared.Contracts.csproj", "shared/EGreetings.Shared.Contracts/"]
COPY ["shared/EGreetings.Shared.Infrastructure/EGreetings.Shared.Infrastructure.csproj", "shared/EGreetings.Shared.Infrastructure/"]

# Copy admin service projects
COPY ["services/admin/EGreetings.Admin.API/EGreetings.Admin.API.csproj", "services/admin/EGreetings.Admin.API/"]
COPY ["services/admin/EGreetings.Admin.Application/EGreetings.Admin.Application.csproj", "services/admin/EGreetings.Admin.Application/"]
COPY ["services/admin/EGreetings.Admin.Domain/EGreetings.Admin.Domain.csproj", "services/admin/EGreetings.Admin.Domain/"]
COPY ["services/admin/EGreetings.Admin.Infrastructure/EGreetings.Admin.Infrastructure.csproj", "services/admin/EGreetings.Admin.Infrastructure/"]

RUN dotnet restore "services/admin/EGreetings.Admin.API/EGreetings.Admin.API.csproj"

COPY . .

WORKDIR "/src/services/admin/EGreetings.Admin.API"
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EGreetings.Admin.API.dll"]
