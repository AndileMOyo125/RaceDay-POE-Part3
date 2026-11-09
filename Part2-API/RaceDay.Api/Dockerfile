FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["RaceDay.Api/RaceDay.Api.csproj", "RaceDay.Api/"]
RUN dotnet restore "RaceDay.Api/RaceDay.Api.csproj"
COPY . .
WORKDIR "/src/RaceDay.Api"
RUN dotnet publish "RaceDay.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
USER $APP_UID
ENTRYPOINT ["dotnet", "RaceDay.Api.dll"]
