FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["UavRoutePlanner.Api/UavRoutePlanner.Api.csproj", "UavRoutePlanner.Api/"]
COPY ["UavRoutePlanner.BLL/UavRoutePlanner.BLL.csproj", "UavRoutePlanner.BLL/"]
COPY ["UavRoutePlanner.DAL/UavRoutePlanner.DAL.csproj", "UavRoutePlanner.DAL/"]
RUN dotnet restore "UavRoutePlanner.Api/UavRoutePlanner.Api.csproj"
COPY . .
WORKDIR "/src/UavRoutePlanner.Api"
RUN dotnet build "UavRoutePlanner.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "UavRoutePlanner.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "UavRoutePlanner.Api.dll"]