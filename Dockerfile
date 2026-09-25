FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore GameOfLife.sln
RUN dotnet publish src/GameOfLife.Api/GameOfLife.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080
RUN mkdir -p /app/data
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "GameOfLife.Api.dll"]
