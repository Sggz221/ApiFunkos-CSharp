FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 5180
EXPOSE 5181

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["cSharpApiFunko/cSharpApiFunko.csproj", "cSharpApiFunko/"]
RUN dotnet restore "cSharpApiFunko/cSharpApiFunko.csproj"
COPY . .
WORKDIR "/src/cSharpApiFunko"
RUN dotnet build "cSharpApiFunko.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "cSharpApiFunko.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "cSharpApiFunko.dll"]
