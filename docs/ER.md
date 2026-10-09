# Схемы данных

## Часть 2 Библиотека книг

```mermaid
erDiagram
    Authors ||--o{ Books : writes
    Authors {
        INTEGER Id PK
        TEXT Name
    }
    Books {
        INTEGER Id PK
        TEXT Title
        INTEGER Price
        INTEGER AuthorId FK
    }
```

`Price` хранится в SQLite целым числом копеек, а в модели C# имеет тип `decimal`.
Удаление автора при наличии его книг запрещено внешним ключом `RESTRICT`.

## Часть 3 Пользователи

```mermaid
erDiagram
    User {
        INTEGER Id PK
        TEXT Login UK
        TEXT PassHash
    }
```

В базе пользователей одна прикладная таблица `User`. Логин уникален без учёта
регистра ASCII-букв благодаря индексу и сопоставлению SQLite `NOCASE`.
Для букв за пределами ASCII `NOCASE` не выполняет полноценное приведение регистра.
Служебная таблица SQLite `sqlite_sequence` обслуживает автоинкремент ключей.
