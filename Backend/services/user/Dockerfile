FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy shared projects
COPY ["shared/EGreetings.Shared.Domain/EGreetings.Shared.Domain.csproj", "shared/EGreetings.Shared.Domain/"]
COPY ["shared/EGreetings.Shared.Contracts/EGreetings.Shared.Contracts.csproj", "shared/EGreetings.Shared.Contracts/"]
COPY ["shared/EGreetings.Shared.Infrastructure/EGreetings.Shared.Infrastructure.csproj", "shared/EGreetings.Shared.Infrastructure/"]

# Copy user service projects
COPY ["services/user/EGreetings.User.API/EGreetings.User.API.csproj", "services/user/EGreetings.User.API/"]
COPY ["services/user/EGreetings.User.Application/EGreetings.User.Application.csproj", "services/user/EGreetings.User.Application/"]
COPY ["services/user/EGreetings.User.Domain/EGreetings.User.Domain.csproj", "services/user/EGreetings.User.Domain/"]
COPY ["services/user/EGreetings.User.Infrastructure/EGreetings.User.Infrastructure.csproj", "services/user/EGreetings.User.Infrastructure/"]

RUN dotnet restore "services/user/EGreetings.User.API/EGreetings.User.API.csproj"

COPY . .

WORKDIR "/src/services/user/EGreetings.User.API"
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EGreetings.User.API.dll"]
