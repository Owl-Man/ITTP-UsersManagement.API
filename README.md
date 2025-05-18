# ITTP-UsersManagement.API

## Для запуска используется docker compose для app и database (postgresql) контейнеров:
```Bash
docker compose up -d
```

## Помимо сделанного функционала по ТЗ из тестового:
### В проекте реализована аутентификация через JWT токены.
### Реализованы функциональные тесты для UsersController на все основные CRUD операции через TestWebAppFactory с http клиентом и тестовые контейнеры на postgresql для изоляции от продакшн БД. Также есть интеграционные тесты для UsersService
