# Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/AgendAI.Api/AgendAI.Api.csproj", "AgendAI.Api/"]
RUN dotnet restore "AgendAI.Api/AgendAI.Api.csproj"

COPY src/AgendAI.Api/ AgendAI.Api/
WORKDIR /src/AgendAI.Api
RUN dotnet publish "AgendAI.Api.csproj" -c Release -f net10.0 -o /app/publish --no-restore

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "AgendAI.Api.dll"]
