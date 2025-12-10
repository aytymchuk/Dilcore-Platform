FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Dilcore.WebApi/Dilcore.WebApi.csproj", "src/Dilcore.WebApi/"]
RUN dotnet restore "src/Dilcore.WebApi/Dilcore.WebApi.csproj"
COPY . .
WORKDIR "/src/src/Dilcore.WebApi"
RUN dotnet build "Dilcore.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Dilcore.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV BUILD_VERSION="1.0.0"
ENTRYPOINT ["dotnet", "Dilcore.WebApi.dll"]
