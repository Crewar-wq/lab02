# Лабораторная работа 02

**Тема:** Интерфейсы. Репозиторий. JSON.  
**Дисциплина:** Коллективная разработка приложений.

Решены все три части задания, включая повышенный уровень части 2.
Каждая часть имеет отдельный отчёт в каталоге `reports`.

## Состав решения

| Проект | Назначение |
|---|---|
| `src/Part1.Interfaces` | Интерфейсы, фигуры, ISP, оплата, логирование |
| `src/Lab02.Data` | Повторно используемая библиотека моделей и репозиториев |
| `src/Part2.RepositoryDemo` | Консольная демонстрация CRUD и Unit of Work |
| `src/Part3.UsersApi` | HTTP REST API пользователей с JSON и SQLite |
| `tests/Lab02.Checks` | Проверки расчётов, правил и настоящей SQLite |

Полные исходники доступны в `src`. Схемы таблиц находятся в `schema`,
ER-диаграммы — в `docs/ER.md`, сохранённые результаты запусков — в `evidence`.
В исходных отчётах нужно заполнить личные поля титульного листа.

## Требования

- .NET SDK 8.0.
- Python 3 для HTTP-проверок; он не требуется для самих C#-программ.
- Доступ к NuGet при первой сборке.

Использован пакет `Microsoft.EntityFrameworkCore.Sqlite` версии `8.0.31`.
SQLite создаёт файлы базы автоматически. Отдельный сервер БД не требуется.

## Запуск

Все команды выполняются из корня репозитория.

```bash
dotnet build Lab02.sln --configuration Release
dotnet run --project src/Part1.Interfaces --configuration Release
dotnet run --project src/Part2.RepositoryDemo --configuration Release
```

Часть 1 записывает сообщения в `artifacts/work.log`.
Для собственного пути передайте его после `--`:

```bash
dotnet run --project src/Part1.Interfaces -c Release -- artifacts/my-work.log
dotnet run --project src/Part2.RepositoryDemo -c Release -- artifacts/my-library.db
```

Демонстрация части 2 добавляет данные в указанную базу. Для повторения
сохранённого примера используйте новый путь к базе, например `artifacts/run2.db`.

## Часть 3 REST API

В отдельном терминале запустите сервер:

```bash
dotnet run --project src/Part3.UsersApi -c Release --no-launch-profile --urls http://localhost:5080
```

Путь к базе задаётся параметром `--DatabasePath` или переменной окружения
`DatabasePath`. По умолчанию применяется `artifacts/users.db` относительно
рабочего каталога приложения.

| Метод | Маршрут | Результат |
|---|---|---|
| POST | `/user` | 201 и созданный пользователь; 409 при повторном логине |
| GET | `/user/{id}` | 200 и пользователь; 404 при отсутствии |
| PUT | `/user/{id}` | 200 и обновлённый пользователь |
| DELETE | `/user/{id}` | 200 и JSON с подтверждением удаления |

Ошибки также возвращаются в JSON: 400 — неверные данные, 404 — запись или
маршрут не найдены, 409 — логин занят, 415 — неподдерживаемый Content-Type.

Входной контракт:

```json
{
  "Login": "example_user",
  "PassHash": "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa"
}
```

Этот пример показывает формат поля. Для запроса вычисляйте настоящий хеш
на стороне клиента. Учебный контракт использует SHA-256: ровно 64
шестнадцатеричных символа. В базе сохраняется хеш, а ответы содержат только
`Id` и `Login`. Проект демонстрирует CRUD и не реализует процедуру входа.

`Login` содержит 3–50 букв, цифр, `_`, `.` или `-`. Уникальный индекс
SQLite исключает дублирование; `NOCASE` игнорирует регистр ASCII-букв.

### Пример в PowerShell

```powershell
$labSha = [System.Security.Cryptography.SHA256]::Create()
$labBytes = [System.Text.Encoding]::UTF8.GetBytes('lab02-demo-password')
$labHash = [System.BitConverter]::ToString($labSha.ComputeHash($labBytes)).Replace('-', '').ToLowerInvariant()
$labSha.Dispose()
$labBody = @{ Login = 'student'; PassHash = $labHash } | ConvertTo-Json
$labUser = Invoke-RestMethod -Method Post -Uri 'http://localhost:5080/user' -ContentType 'application/json' -Body $labBody
Invoke-RestMethod -Uri "http://localhost:5080/user/$($labUser.Id)"
$labBody = @{ Login = 'student_updated'; PassHash = $labHash } | ConvertTo-Json
Invoke-RestMethod -Method Put -Uri "http://localhost:5080/user/$($labUser.Id)" -ContentType 'application/json' -Body $labBody
Invoke-RestMethod -Method Delete -Uri "http://localhost:5080/user/$($labUser.Id)"
```

## Проверки

```bash
dotnet run --project tests/Lab02.Checks --configuration Release
python scripts/api_smoke.py --base-url http://localhost:5080
```

Сервер должен работать во время выполнения второй команды. На Linux и macOS
при необходимости используйте `python3` вместо `python`.
HTTP-скрипт создаёт тестовых пользователей и удаляет их после успешной проверки.

Для полного запуска на Linux и macOS:

```bash
bash scripts/verify.sh
```

GitHub Actions выполняет сборку и все проверки при push и pull request,
затем сохраняет результаты как артефакт `execution-results`.

## Особенности реализации

Цена книги хранится целым числом копеек и преобразуется в `decimal` в C#.
Репозитории подготавливают изменения, а `UnitOfWork.SaveChanges` сохраняет
их через один `DbContext`. `BookCatalog` содержит бизнес-правило изменения цены.
Отсутствующий объект в репозитории вызывает `KeyNotFoundException`.

Асинхронные методы имеют контракт `Task` и используют API EF Core. SQLite
не выполняет настоящий асинхронный ввод-вывод, поэтому наличие `Async`
не означает ускорение запросов к этой СУБД.

## Документация

- [Интерфейсы C#](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/interface)
- [Начало работы с EF Core и SQLite](https://learn.microsoft.com/en-us/ef/core/get-started/overview/first-app)
- [Жизненный цикл DbContext](https://learn.microsoft.com/en-us/ef/core/dbcontext-configuration/)
- [Ограничения SQLite и асинхронных вызовов](https://learn.microsoft.com/en-us/ef/core/providers/sqlite/limitations)
- [Ответы Minimal API](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/responses?view=aspnetcore-8.0)
