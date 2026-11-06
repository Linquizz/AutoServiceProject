# Автосервис - информационная система

Настольное приложение: **Avalonia UI (C#, .NET 8)** + **Entity Framework Core 8 (Code First)** + **Microsoft SQL Server**.
Архитектура MVVM. PDF заказ-нарядов - **QuestPDF**, письма - **MailKit**.

## 1. Что нужно установить

| Что | Зачем |
|---|---|
| Visual Studio 2022/2026 с нагрузкой «Разработка классических приложений .NET» | сборка проекта (.NET 8 SDK) |
| SQL Server 2022 Express (экземпляр `SQLEXPRESS`) | сервер БД |
| SQL Server Management Studio 20 (SSMS) | просмотр и администрирование БД |

### NuGet-пакеты (подтягиваются сами при сборке, ничего ставить вручную не нужно)

| Пакет | Назначение |
|---|---|
| `Avalonia`, `Avalonia.Desktop`, `Avalonia.Themes.Fluent` (11.3.*) | интерфейс |
| `Avalonia.Controls.DataGrid` (11.3.*) | таблицы |
| `CommunityToolkit.Mvvm` | MVVM: `[ObservableProperty]`, `[RelayCommand]` |
| `Microsoft.EntityFrameworkCore.SqlServer` (8.0.*) | ORM + провайдер SQL Server |
| `Microsoft.EntityFrameworkCore.Design`, `...Tools` (8.0.*) | инструменты EF (при желании - миграции) |
| `QuestPDF` (2024.12.*, лицензия Community) | формирование PDF заказ-наряда |
| `MailKit` (4.*) | отправка писем по SMTP |

Если в Visual Studio проект не восстанавливает пакеты: ПКМ по решению -> «Восстановить пакеты NuGet».
Если плавающая версия `11.3.*` не нравится - замените её в `AutoService.csproj` на конкретную, но у **всех** пакетов Avalonia версия должна быть одинаковой.

## 2. Первый запуск

1. Откройте `AutoService.sln` в Visual Studio.
2. Проверьте строку подключения в `AutoService/appsettings.json`:
   - `Server=.\\SQLEXPRESS;Database=AutoServiceDB;Trusted_Connection=True;TrustServerCertificate=True;`
   - если у вас другой экземпляр - замените `.\\SQLEXPRESS` (например, `(localdb)\\MSSQLLocalDB` или `localhost`).
3. Нажмите **F5**. База данных `AutoServiceDB` и все таблицы создаются **автоматически** при первом запуске (Code First) и заполняются справочниками и демо-данными (в том числе завершённым заказ-нарядом, повторяющим образец).
4. Откройте SSMS -> подключитесь к `.\SQLEXPRESS` -> Databases -> **AutoServiceDB**: там таблицы Brand, Model, Client, Car, Employee, Service, Part, WorkOrder, OrderService, OrderPart. Диаграмма: ПКМ по *Database Diagrams* -> New Database Diagram.

### Учётные записи (созданы при первом запуске)

| Логин | Пароль | Роль |
|---|---|---|
| `admin` | `admin123` | Администратор |
| `staff` | `staff123` | Сотрудник |

Сменить пароли и добавить сотрудников: раздел «Администрирование». Пароли хранятся как хеш PBKDF2.
Демо-данные можно отключить: `"SeedDemoData": false` в `appsettings.json` (до первого запуска).

## 3. Тестовые данные

При первом запуске (если `SeedDemoData` = true) заполняются таблицы:

| Таблица | Записей |
|---|---|
| Brand / Model | 14 / 52 |
| Client | 12 (у половины указан e-mail) |
| Car | 13 |
| Employee | 12 |
| Service | 16 |
| Part | 18 |
| WorkOrder | 12 (9 завершённых и 3 «В работе») |
| OrderService | 34 |
| OrderPart | 31 |

## 4. PDF заказ-наряда

- Кнопка **«Сформировать PDF»** в окне заказ-наряда и в разделе «Отчёты».
- PDF автоматически формируется при **завершении работ**.
- Файлы сохраняются в `Документы\AutoService\ЗаказНаряды` (папку можно изменить: `Paths.WorkOrdersFolder`).
- Шрифт PDF - Arial (есть в Windows).

## 5. Отправка письма клиенту

Сервис: `Services/EmailService.cs` (+ оркестрация в `WorkOrderDocumentService`).
Если у клиента указан e-mail, то при завершении работ PDF уходит ему автоматически; вручную - кнопка «Отправить клиенту» (окно заказ-наряда, раздел «Отчёты»). Если почты у клиента нет - просто сообщается, что письмо не отправлялось.

Настройка - раздел `Smtp` в `appsettings.json`:

```json
"Smtp": {
  "Host": "smtp.yandex.ru",
  "Port": 465,
  "SecureMode": "SslOnConnect",
  "UserName": "ваш_ящик@yandex.ru",
  "Password": "ПАРОЛЬ_ПРИЛОЖЕНИЯ",
  "FromAddress": "ваш_ящик@yandex.ru",
  "FromName": "Автосервис"
}
```

| Почта | Host | Port | SecureMode |
|---|---|---|---|
| Яндекс | smtp.yandex.ru | 465 | SslOnConnect |
| Mail.ru | smtp.mail.ru | 465 | SslOnConnect |
| Gmail | smtp.gmail.com | 587 | StartTls |

**Нужен именно «пароль приложения»**, а не пароль от ящика: создаётся в настройках безопасности почты (для Яндекса/Mail.ru предварительно включить доступ по протоколу IMAP/SMTP или двухфакторную защиту). Чтобы пароль не попал в Git, можно положить секцию `Smtp` в файл `appsettings.Local.json` (он в `.gitignore`, формат такой же, рядом с `appsettings.json`; не забудьте поставить ему «Копировать в выходной каталог»).

Для проверки: в карточке демо-клиента (Иванов Иван) укажите **свой** e-mail, завершите заказ-наряд или нажмите «Отправить клиенту».

## 6. Структура проекта

```
AutoService/
  Models/        сущности EF (по ER-диаграмме Лаб3)
  Data/          AutoServiceDbContext, DbInitializer (создание БД и начальные данные)
  Services/      бизнес-логика: клиенты, авто, заказ-наряды, справочники, сотрудники, резервные копии,
                 WorkOrderPdfGenerator (PDF), EmailService (почта), Validator, ...
  ViewModels/    MVVM: логика экранов
  Views/         окна и страницы (.axaml)
  appsettings.json        строка подключения, организация, SMTP, пути
```

## Интерфейс

- Окна подстраиваются под размер экрана (ограничение по рабочей области, минимальные размеры, перенос рядов кнопок и фильтров).
- Светлая и тёмная темы: по умолчанию как в Windows, переключение - кнопка в левом меню.
- Выделение строки в таблице снимается щелчком по пустому месту таблицы или клавишей Esc.

## Частые проблемы

- **«Не удалось подключиться к базе данных»** - проверьте, что служба *SQL Server (SQLEXPRESS)* запущена, и строку подключения.
- **Таблицы/колонки не совпадают** - в БД остались таблицы из Лаб5: удалите БД и запустите приложение заново.
- **Резервная копия не создаётся** - файл пишет служба SQL Server; оставьте поле папки пустым (сервер использует свою папку по умолчанию) или выдайте права на папку службе `MSSQL$SQLEXPRESS`.
- **Ошибка QuestPDF/SkiaSharp при запуске** - установите «Распространяемый пакет Visual C++ (x64)».
- **Письмо не уходит** - см. п. 5 (пароль приложения, порт и SecureMode).
