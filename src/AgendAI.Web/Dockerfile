# Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/AgendAI.Web/AgendAI.Web.csproj", "AgendAI.Web/"]
RUN dotnet restore "AgendAI.Web/AgendAI.Web.csproj"

COPY src/AgendAI.Web/ AgendAI.Web/
WORKDIR /src/AgendAI.Web
RUN dotnet publish "AgendAI.Web.csproj" -c Release -f net10.0 -o /app/publish --no-restore

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "AgendAI.Web.dll"]
