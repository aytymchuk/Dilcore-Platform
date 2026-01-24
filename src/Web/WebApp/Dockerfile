FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["Directory.Packages.props", "."]
COPY ["Directory.Build.props", "."]
COPY ["nuget.config", "."]

COPY ["src/Web/WebApp/WebApp.csproj", "src/Web/WebApp/"]
COPY ["src/Web/WebApp.Components/WebApp.Components.csproj", "src/Web/WebApp.Components/"]
RUN dotnet restore "src/Web/WebApp/WebApp.csproj"

COPY . .
WORKDIR "/src/src/Web/WebApp"
RUN dotnet build "WebApp.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "WebApp.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
ARG BUILD_VERSION=1.0.0
WORKDIR /app
COPY --from=publish /app/publish .
ENV BUILD_VERSION=$BUILD_VERSION
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:8080/ || exit 1
ENTRYPOINT ["dotnet", "WebApp.dll"]
