# образ .NET SDK для сборки
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# копирование файлов и восстановление зависимостей
COPY . .
RUN dotnet restore

# сборка проекта
RUN dotnet publish -c Release -o /app

# .NET Runtime образ
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# кпируем собранные файлы из сборки
COPY --from=build /app .

# точка входа
ENTRYPOINT ["dotnet", "Bot.dll"]