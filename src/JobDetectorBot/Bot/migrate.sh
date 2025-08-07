#!/bin/bash
set -e

# Ждем, пока БД будет готова
until pg_isready -h db -U postgres -d botdbcont; do
  echo "Ожидание готовности PostgreSQL..."
  sleep 1
done

# Применяем миграции
dotnet ef database update

# Запускаем основное приложение
exec dotnet Bot.dll