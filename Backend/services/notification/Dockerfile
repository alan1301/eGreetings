FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy shared projects
COPY ["shared/EGreetings.Shared.Domain/EGreetings.Shared.Domain.csproj", "shared/EGreetings.Shared.Domain/"]
COPY ["shared/EGreetings.Shared.Contracts/EGreetings.Shared.Contracts.csproj", "shared/EGreetings.Shared.Contracts/"]
COPY ["shared/EGreetings.Shared.Infrastructure/EGreetings.Shared.Infrastructure.csproj", "shared/EGreetings.Shared.Infrastructure/"]

# Copy notification service projects
COPY ["services/notification/EGreetings.Notification.API/EGreetings.Notification.API.csproj", "services/notification/EGreetings.Notification.API/"]
COPY ["services/notification/EGreetings.Notification.Application/EGreetings.Notification.Application.csproj", "services/notification/EGreetings.Notification.Application/"]
COPY ["services/notification/EGreetings.Notification.Domain/EGreetings.Notification.Domain.csproj", "services/notification/EGreetings.Notification.Domain/"]
COPY ["services/notification/EGreetings.Notification.Infrastructure/EGreetings.Notification.Infrastructure.csproj", "services/notification/EGreetings.Notification.Infrastructure/"]

RUN dotnet restore "services/notification/EGreetings.Notification.API/EGreetings.Notification.API.csproj"

COPY . .

WORKDIR "/src/services/notification/EGreetings.Notification.API"
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EGreetings.Notification.API.dll"]
