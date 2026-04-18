FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy shared projects
COPY ["shared/EGreetings.Shared.Domain/EGreetings.Shared.Domain.csproj", "shared/EGreetings.Shared.Domain/"]
COPY ["shared/EGreetings.Shared.Contracts/EGreetings.Shared.Contracts.csproj", "shared/EGreetings.Shared.Contracts/"]
COPY ["shared/EGreetings.Shared.Infrastructure/EGreetings.Shared.Infrastructure.csproj", "shared/EGreetings.Shared.Infrastructure/"]

# Copy greeting service projects
COPY ["services/greeting/EGreetings.Greeting.API/EGreetings.Greeting.API.csproj", "services/greeting/EGreetings.Greeting.API/"]
COPY ["services/greeting/EGreetings.Greeting.Application/EGreetings.Greeting.Application.csproj", "services/greeting/EGreetings.Greeting.Application/"]
COPY ["services/greeting/EGreetings.Greeting.Domain/EGreetings.Greeting.Domain.csproj", "services/greeting/EGreetings.Greeting.Domain/"]
COPY ["services/greeting/EGreetings.Greeting.Infrastructure/EGreetings.Greeting.Infrastructure.csproj", "services/greeting/EGreetings.Greeting.Infrastructure/"]

RUN dotnet restore "services/greeting/EGreetings.Greeting.API/EGreetings.Greeting.API.csproj"

COPY . .

WORKDIR "/src/services/greeting/EGreetings.Greeting.API"
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EGreetings.Greeting.API.dll"]
