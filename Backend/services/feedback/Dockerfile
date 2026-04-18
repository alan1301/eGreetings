FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy shared projects
COPY ["shared/EGreetings.Shared.Domain/EGreetings.Shared.Domain.csproj", "shared/EGreetings.Shared.Domain/"]
COPY ["shared/EGreetings.Shared.Contracts/EGreetings.Shared.Contracts.csproj", "shared/EGreetings.Shared.Contracts/"]
COPY ["shared/EGreetings.Shared.Infrastructure/EGreetings.Shared.Infrastructure.csproj", "shared/EGreetings.Shared.Infrastructure/"]

# Copy feedback service projects
COPY ["services/feedback/EGreetings.Feedback.API/EGreetings.Feedback.API.csproj", "services/feedback/EGreetings.Feedback.API/"]
COPY ["services/feedback/EGreetings.Feedback.Application/EGreetings.Feedback.Application.csproj", "services/feedback/EGreetings.Feedback.Application/"]
COPY ["services/feedback/EGreetings.Feedback.Domain/EGreetings.Feedback.Domain.csproj", "services/feedback/EGreetings.Feedback.Domain/"]
COPY ["services/feedback/EGreetings.Feedback.Infrastructure/EGreetings.Feedback.Infrastructure.csproj", "services/feedback/EGreetings.Feedback.Infrastructure/"]

RUN dotnet restore "services/feedback/EGreetings.Feedback.API/EGreetings.Feedback.API.csproj"

COPY . .

WORKDIR "/src/services/feedback/EGreetings.Feedback.API"
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EGreetings.Feedback.API.dll"]
