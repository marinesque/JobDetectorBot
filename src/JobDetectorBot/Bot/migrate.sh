#!/bin/bash
set -e

# Ждем, пока БД будет готова
until pg_isready -h db -U postgres -d botdbcont; do
  sleep 1
done

# Применяем миграции
dotnet ef database update