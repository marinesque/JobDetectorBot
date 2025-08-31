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
- [ 📁 Архитектура Бота](#-архитектура-бота)
- [ 🗄️ Структура базы данных](#-возможности)
- [ 🚀 Начало работы](#️-начало-работы)
- [ ⚙️ Конфигурация](#️-конфигурация)
- [ 🗄️ Структура базы данных](#️-структура-базы-данных)
- [ 🔄 Workflow интеграции](#️-workflow-интеграции)
- [ 📝 Лицензия](#-лицензия)
- [ Источники данных по вакансиям](#-источники-данных-по-вакансиям)


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


## 📁 Архитектура Бота

<img width="3746" height="3840" alt="mermaidArchitecture" src="https://github.com/user-attachments/assets/3bf123a1-f3a1-478a-a64c-7590aaebabb3" />



## 🗄️ Структура базы данных

<img width="686" height="447" alt="image" src="https://github.com/user-attachments/assets/7b30f7da-2191-452c-b50f-40cf83ef959d" />



## 🔄 Workflow интеграции

```mermaid
sequenceDiagram
    participant User
    participant Telegram as Telegram API
    participant MessageHandler
    participant UserStateManager
    participant Strategy as StateStrategy
    participant UserCache
    participant VacancyService
    participant Database as PostgreSQL
    participant Redis
    participant ExternalAPI as Vacancy Search API

    User->>Telegram: Send message
    Telegram->>MessageHandler: Handle message
    MessageHandler->>UserCache: Get user data
    UserCache->>Redis: Check cache
    alt Cache miss
        Redis-->>UserCache: Not found
        UserCache->>Database: Get user from DB
        Database-->>UserCache: User data
        UserCache->>Redis: Store in cache
    else Cache hit
        Redis-->>UserCache: User data
    end
    UserCache-->>MessageHandler: User data
    
    MessageHandler->>UserStateManager: Get current state
    UserStateManager-->>MessageHandler: Current state
    MessageHandler->>Strategy: Handle with strategy
    
    alt Searching vacancies
        Strategy->>VacancyService: Search vacancies
        VacancyService->>ExternalAPI: API request
        ExternalAPI-->>VacancyService: Vacancy results
        VacancyService->>Redis: Cache results
        VacancyService-->>Strategy: Vacancy data
    else Collecting criteria
        Strategy->>Database: Get criteria options
        Database-->>Strategy: Criteria data
    end
    
    Strategy-->>MessageHandler: Response data
    MessageHandler->>Telegram: Send response
    Telegram->>User: Bot response
```

## 🚀 Начало работы

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

## Источники данных по вакансиям

- **HeadHunter API** - Вакансии получены с сайта hh.ru
  - API: https://api.hh.ru/
  - Соглашение разработчика: https://dev.hh.ru/admin/developer_agreement
  - **Использование**: исключительно в учебных целях
  - **Данные**: используются в неизменном виде
