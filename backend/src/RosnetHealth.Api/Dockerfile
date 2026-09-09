# Build context is the backend/ directory (so this Dockerfile can see sibling projects).
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY RosnetHealth.sln ./
COPY src/RosnetHealth.Domain/RosnetHealth.Domain.csproj src/RosnetHealth.Domain/
COPY src/RosnetHealth.Application/RosnetHealth.Application.csproj src/RosnetHealth.Application/
COPY src/RosnetHealth.Infrastructure/RosnetHealth.Infrastructure.csproj src/RosnetHealth.Infrastructure/
COPY src/RosnetHealth.Api/RosnetHealth.Api.csproj src/RosnetHealth.Api/
RUN dotnet restore src/RosnetHealth.Api/RosnetHealth.Api.csproj

COPY src/ src/
RUN dotnet publish src/RosnetHealth.Api/RosnetHealth.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "RosnetHealth.Api.dll"]
