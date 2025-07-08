FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Bot.csproj", "."]
RUN dotnet restore "Bot.csproj"
COPY . .
WORKDIR "/src"
RUN dotnet build "Bot.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Bot.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Bot.dll"]