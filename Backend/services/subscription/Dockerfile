FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy shared projects
COPY ["shared/EGreetings.Shared.Domain/EGreetings.Shared.Domain.csproj", "shared/EGreetings.Shared.Domain/"]
COPY ["shared/EGreetings.Shared.Contracts/EGreetings.Shared.Contracts.csproj", "shared/EGreetings.Shared.Contracts/"]
COPY ["shared/EGreetings.Shared.Infrastructure/EGreetings.Shared.Infrastructure.csproj", "shared/EGreetings.Shared.Infrastructure/"]

# Copy subscription service projects
COPY ["services/subscription/EGreetings.Subscription.API/EGreetings.Subscription.API.csproj", "services/subscription/EGreetings.Subscription.API/"]
COPY ["services/subscription/EGreetings.Subscription.Application/EGreetings.Subscription.Application.csproj", "services/subscription/EGreetings.Subscription.Application/"]
COPY ["services/subscription/EGreetings.Subscription.Domain/EGreetings.Subscription.Domain.csproj", "services/subscription/EGreetings.Subscription.Domain/"]
COPY ["services/subscription/EGreetings.Subscription.Infrastructure/EGreetings.Subscription.Infrastructure.csproj", "services/subscription/EGreetings.Subscription.Infrastructure/"]

RUN dotnet restore "services/subscription/EGreetings.Subscription.API/EGreetings.Subscription.API.csproj"

COPY . .

WORKDIR "/src/services/subscription/EGreetings.Subscription.API"
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EGreetings.Subscription.API.dll"]
