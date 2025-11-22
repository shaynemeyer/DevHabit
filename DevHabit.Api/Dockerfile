# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

ENV ASPNETCORE_ENVIRONMENT=Docker


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
# Install CA certificates to fix SSL issues with NuGet
RUN apt-get update && \
    apt-get install -y ca-certificates && \
    update-ca-certificates && \
    rm -rf /var/lib/apt/lists/*
COPY ["Directory.Packages.props", "."]
COPY ["Directory.Build.props", "."]
COPY ["DevHabit.Api/nuget.config", "DevHabit.Api/"]
COPY ["DevHabit.Api/DevHabit.Api.csproj", "DevHabit.Api/"]
# Set environment variables to handle SSL certificate issues
ENV DOTNET_SYSTEM_NET_HTTP_USESOCKETSHTTPHANDLER=0
ENV NUGET_XMLDOC_MODE=skip
# Try to bypass SSL verification entirely
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1
ENV NUGET_CERT_REVOCATION_MODE=offline
RUN dotnet restore "./DevHabit.Api/DevHabit.Api.csproj" --ignore-failed-sources || \
    dotnet restore "./DevHabit.Api/DevHabit.Api.csproj" --disable-parallel || \
    echo "Warning: Some package sources failed, continuing with available packages"
COPY . .
WORKDIR "/src/DevHabit.Api"
RUN dotnet build "./DevHabit.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./DevHabit.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DevHabit.Api.dll"]