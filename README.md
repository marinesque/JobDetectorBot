# JobDetectorBot
Карманный помощник в поиске вакансии мечты от команды Безработных.NET в составе:
 
@biktashevtimur
@marinesque


# 🤖 Telegram Bot для поиска вакансий

<div align="center">

[![.NET 8](https://img.shields.io/badge/.NET-8-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![Telegram Bot](https://img.shields.io/badge/Telegram-Bot-2CA5E0?logo=telegram)](https://core.telegram.org/bots)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-336791?logo=postgresql)](https://www.postgresql.org/)
[![Redis](https://img.shields.io/badge/Redis-7-DC382D?logo=redis)](https://redis.io/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

</div>

## 📋 Оглавление

- [ ✨ Возможности](#-возможности)
- [ 📁 Структура проекта](#-структура-проекта)
- [ 🚀 Быстрый старт](#️-быстрый-старт)
- [ ⚙️ Конфигурация](#️-конфигурация)
- [ 🗄️ Структура базы данных](#️-структура-базы-данных)
- [ 🔄 Workflow интеграции](#️-workflow-интеграции)
- [ 📝 Лицензия](#-лицензия)


## ✨ Возможности

### 🎯 Основной функционал
- **Пошаговая настройка критериев** - интуитивный интерфейс ввода параметров поиска
- **Умный поиск вакансий** - интеграция с внешними API вакансий
- **Кэширование результатов** - быстрый доступ к предыдущим поискам
- **Пагинация** - удобная навигация по результатам

### 🎨 Пользовательский опыт
- **Интерактивные клавиатуры** - удобное управление через кнопки
- **Поддержка кастомных значений** - ввод произвольных параметров
- **Редактирование критериев** - возможность изменения сохраненных настроек
- **Состояние диалога** - запоминание позиции в разговоре

### ⚡ Технические особенности
- **Асинхронная обработка** - высокая производительность
- **Кэширование в Redis** - быстрое время отклика
- **Автоматические миграции** - простое обновление БД
- **Логирование** - детальный мониторинг работы

### 🛠️ Технологический стек

| Технология | Назначение | Версия |
|------------|------------|---------|
| **.NET** | Основная платформа | 8.0 |
| **Telegram.Bot** | Работа с Telegram API | 19.0 |
| **Entity Framework** | ORM для PostgreSQL | 8.0 |
| **PostgreSQL** | Основная база данных | 14+ |
| **Redis** | Кэширование данных | 6+ |
| **Docker** | Контейнеризация | 20+ |


## 📁 Структура проекта
Bot/

├── 📁 Application/ # Бизнес-логика приложения

│ └── 📁 Handlers/ # Обработчики сообщений

│ ├── MessageHandler.cs

│ └── CallbackHandler.cs

├── 📁 Domain/ # Доменный слой

│ ├── 📁 DataAccess/ # Модели данных

│ │ ├── 📁 Model/ # Entity-модели

│ │ ├── 📁 Dto/ # Data Transfer Objects

│ │ └── 📁 Repositories/ # Репозитории

│ ├── 📁 Enums/ # Перечисления

│ └── 📁 Request/ # Модели запросов

├── 📁 Infrastructure/ # Инфраструктурный слой

│ ├── 📁 Configuration/ # Конфигурации

│ ├── 📁 Interfaces/ # Интерфейсы сервисов

│ ├── 📁 Services/ # Реализации сервисов

│ └── BackgroundServices/ # Фоновые службы

├── 📁 Test/ # Тесты

└── Program.cs # Точка входа



## 🚀 Быстрый старт
📋 Предварительные требования
- .NET 8 SDK
- PostgreSQL 14+
- Redis 6+
- Docker (опционально)

### ⚡ Локальная установка
Клонирование репозитория

```
git clone https://github.com/marinesque/JobDetectorBot.git
cd JobDetectorBot
```

## ⚙️ Конфигурация

### 📄 appsettings.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "Telegram": {
    "Token": "YOUR_BOT_TOKEN"
  },
  "ConnectionStrings": {
    "PostgreSQL": "Host=localhost;Database=botdb;Username=postgres;Password=password;Port=5432",
    "Redis": "localhost:6379"
  },
  "VacancySearchService": {
    "BaseUrl": "http://localhost:5018"
  },
  "RedisOptions": {
    "ClearOnStartup": false
  }
}
```

## 🗄️ Структура базы данных

<img width="686" height="447" alt="image" src="https://github.com/user-attachments/assets/7b30f7da-2191-452c-b50f-40cf83ef959d" />

## 🔄 Workflow интеграции

<img width="3069" height="1449" alt="база2" src="https://github.com/user-attachments/assets/3ab40d1a-d671-4e95-872e-fe477728f7e0" />


