FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["IoTOperationsDrop.IOLink.csproj", "IoTOperationsDrop.IOLink/"]
RUN dotnet restore "IoTOperationsDrop.IOLink/IoTOperationsDrop.IOLink.csproj"
COPY . .
WORKDIR "/src/IoTOperationsDrop.IOLink"
RUN dotnet build "IoTOperationsDrop.IOLink.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "IoTOperationsDrop.IOLink.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=true

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "IoTOperationsDrop.IOLink.dll"]
