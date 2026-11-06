using AutoService.Models;
using AutoService.Services;
using Microsoft.EntityFrameworkCore;

namespace AutoService.Data;

public static class DbInitializer
{
    public static void Initialize()
    {
        using var db = new AutoServiceDbContext();
        db.Database.EnsureCreated();

        if (!db.Employees.Any())
            SeedEmployees(db);

        if (!db.Brands.Any())
            SeedCatalogs(db);

        if (AppSettings.Current.SeedDemoData && !db.WorkOrders.Any() && !db.Clients.Any())
            SeedDemoData(db);
    }

    private static void SeedEmployees(AutoServiceDbContext db)
    {
        Employee E(string last, string first, string middle, string phone, string position,
            string role = Roles.Employee, string? login = null, string? password = null) => new()
        {
            LastName = last,
            FirstName = first,
            MiddleName = middle,
            Phone = phone,
            Position = position,
            Role = role,
            Login = login,
            PasswordHash = password == null ? null : PasswordHasher.Hash(password)
        };

        db.Employees.AddRange(
            E("Смирнова", "Ольга", "Ивановна", "+7 900 111-11-11", "Администратор", Roles.Admin, "admin", "admin123"),
            E("Кузнецов", "Дмитрий", "Сергеевич", "+7 900 222-22-22", "Приёмщик", Roles.Employee, "staff", "staff123"),
            E("Горшков", "Алексей", "Викторович", "+7 900 333-33-33", "Мастер-механик"),
            E("Белов", "Артём", "Евгеньевич", "+7 900 444-44-44", "Мастер-механик"),
            E("Волков", "Сергей", "Петрович", "+7 900 555-55-01", "Мастер-электрик"),
            E("Новиков", "Андрей", "Николаевич", "+7 900 555-55-02", "Моторист"),
            E("Морозов", "Павел", "Андреевич", "+7 900 555-55-03", "Слесарь по ремонту ходовой"),
            E("Фёдоров", "Игорь", "Валерьевич", "+7 900 555-55-04", "Шиномонтажник"),
            E("Соколов", "Максим", "Олегович", "+7 900 555-55-05", "Диагност"),
            E("Лебедев", "Николай", "Борисович", "+7 900 555-55-06", "Кузовной мастер"),
            E("Зайцев", "Виктор", "Сергеевич", "+7 900 555-55-07", "Мастер-приёмщик"),
            E("Орлова", "Наталья", "Викторовна", "+7 900 555-55-08", "Кладовщик"));
        db.SaveChanges();
    }

    private static void SeedCatalogs(AutoServiceDbContext db)
    {
        var catalog = new Dictionary<string, string[]>
        {
            ["Lada"] = new[] { "Granta", "Vesta", "Priora", "Kalina", "Largus", "Niva Legend" },
            ["Toyota"] = new[] { "Camry", "Corolla", "RAV4", "Land Cruiser" },
            ["Kia"] = new[] { "Rio", "Ceed", "Sportage", "Optima" },
            ["Hyundai"] = new[] { "Solaris", "Creta", "Tucson", "Elantra" },
            ["Renault"] = new[] { "Logan", "Duster", "Sandero", "Kaptur" },
            ["Volkswagen"] = new[] { "Polo", "Tiguan", "Passat", "Golf" },
            ["Skoda"] = new[] { "Octavia", "Rapid", "Fabia", "Kodiaq" },
            ["Ford"] = new[] { "Focus", "Fiesta", "Mondeo", "Kuga" },
            ["Chevrolet"] = new[] { "Niva", "Cruze", "Aveo" },
            ["UAZ"] = new[] { "Patriot", "Hunter", "Pickup" },
            ["BMW"] = new[] { "3 Series", "5 Series", "X5" },
            ["Mercedes-Benz"] = new[] { "C-Class", "E-Class", "GLC" },
            ["Nissan"] = new[] { "Qashqai", "X-Trail", "Almera" },
            ["Haval"] = new[] { "Jolion", "F7", "Dargo" }
        };

        foreach (var (brandName, models) in catalog)
        {
            var brand = new Brand { Name = brandName };
            foreach (var m in models) brand.Models.Add(new CarModel { Name = m });
            db.Brands.Add(brand);
        }

        db.Services.AddRange(
            new Service { Name = "Защита картера - снятие/установка", Price = 500m },
            new Service { Name = "Комплект сцепления - с/у (при снятой коробке передач)", Price = 884m },
            new Service { Name = "Коробка передач в сборе - с/у (при слитом масле)", Price = 9171.50m },
            new Service { Name = "Подшипник переднего колеса в сборе - с/у правого подшипника", Price = 3204.50m },
            new Service { Name = "Расходные материалы", Price = 92.50m },
            new Service { Name = "Регулировка углов подвески ВАЗ (2108-2190)", Price = 2300m },
            new Service { Name = "Стойка телескопическая передней подвески левая - с/у", Price = 1215.50m },
            new Service { Name = "Стойка телескопическая передней подвески правая - с/у", Price = 1215.50m },
            new Service { Name = "Трос привода сцепления - с/у", Price = 1215.50m },
            new Service { Name = "Компьютерная диагностика", Price = 1200m, Description = "Считывание и расшифровка ошибок" },
            new Service { Name = "Замена масла в двигателе (работа)", Price = 600m },
            new Service { Name = "Шиномонтаж (комплект 4 колеса)", Price = 1600m },
            new Service { Name = "Колодки тормозные передние - замена", Price = 1330m },
            new Service { Name = "Ремень ГРМ - замена", Price = 3980m },
            new Service { Name = "Замена свечей зажигания", Price = 800m },
            new Service { Name = "Замена охлаждающей жидкости", Price = 1500m });

        Part P(string article, string name, decimal price, int stock) =>
            new() { Article = article, Name = name, Price = price, StockQuantity = stock };

        db.Parts.AddRange(
            P("21011601215", "Болт опора вилки сцепления 2101-07", 180m, 40),
            P("00001-16101511-118", "Гайка М12*1,25 крепления балки 2101-07, стремянки 2141, головки блока АЗЛК,ГАЗ", 60m, 80),
            P("00001-0005170-708", "Гровер 12", 15m, 150),
            P("2181-1601180", "Направляющая выжимного подшипника ВАЗ-2181 АвтоВАЗ (тросовая КПП)", 1350m, 12),
            P("2190-2902820", "Опора передней стойки 2190 Granta/Granta FL с эл/усил с подшипником", 2950m, 14),
            P("CS-20", "Подшипник ступицы 1118, 2170 передний CS-20", 2400m, 14),
            P("11180-3103012-00", "Ступица 1118 Калина, 2170 Priora передняя АвтоВАЗ", 1850m, 10),
            P("2190-1601000", "Сцепление 2190 для тросовых приводов КПП АвтоВАЗ", 11400m, 8),
            P("21901-1602210-01", "Трос сцепления 2190 Granta АвтоВАЗ", 1500m, 12),
            P("00001-0007346-018", "Шплинт 3,2*30 рулевые тяги", 20m, 300),
            P("OIL-5W40-4L", "Масло моторное 5W-40 синтетика, 4 л", 2800m, 40),
            P("FILTER-OIL-2112", "Фильтр масляный ВАЗ 2108-2112", 350m, 40),
            P("BRAKE-PADS-F-2190", "Колодки тормозные передние 2190 Granta", 1900m, 20),
            P("AIR-FILTER-2190", "Фильтр воздушный 2190 Granta", 450m, 25),
            P("SPARK-PLUG-NGK-4", "Свечи зажигания NGK, комплект 4 шт.", 1200m, 20),
            P("TIMING-KIT-1118", "Комплект ГРМ 1118 (ремень и ролики)", 3600m, 10),
            P("COOLANT-G12-5L", "Антифриз G12, 5 л", 900m, 30),
            P("WIPER-600", "Щётки стеклоочистителя 600 мм", 700m, 25));

        db.SaveChanges();
    }

    private sealed record OrderSeed(
        string Plate,
        DateTime Start,
        DateTime? End,
        string Reason,
        (string Service, string Executor, decimal? Hours)[] Works,
        (string Article, int Quantity)[] Parts);

    private static void SeedDemoData(AutoServiceDbContext db)
    {
        Client C(string last, string first, string middle, string phone, string? email) =>
            new() { LastName = last, FirstName = first, MiddleName = middle, Phone = phone, Email = email };

        var clients = new[]
        {
            C("Иванов", "Иван", "Иванович", "+7 900 000-00-01", "ivanov@example.com"),
            C("Петрова", "Мария", "Сергеевна", "+7 900 000-00-02", null),
            C("Сидоров", "Алексей", "Павлович", "+7 900 000-00-03", "sidorov@example.com"),
            C("Кузьмин", "Денис", "Андреевич", "+7 900 000-00-04", null),
            C("Васильева", "Елена", "Николаевна", "+7 900 000-00-05", "vasilieva@example.com"),
            C("Михайлов", "Роман", "Олегович", "+7 900 000-00-06", null),
            C("Попова", "Анна", "Викторовна", "+7 900 000-00-07", "popova@example.com"),
            C("Смирнов", "Георгий", "Данилович", "+7 900 000-00-08", null),
            C("Фролова", "Ксения", "Игоревна", "+7 900 000-00-09", "frolova@example.com"),
            C("Егоров", "Владимир", "Сергеевич", "+7 900 000-00-10", null),
            C("Никитина", "Ольга", "Алексеевна", "+7 900 000-00-11", "nikitina@example.com"),
            C("Тихонов", "Артём", "Максимович", "+7 900 000-00-12", null)
        };

        int ModelId(string brand, string model) =>
            db.CarModels.Include(m => m.Brand).First(m => m.Brand!.Name == brand && m.Name == model).Id;

        Car A(Client client, string plate, string brand, string model, string? vin, int year, string engine, int mileage) =>
            new()
            {
                RegistrationNumber = plate,
                Client = client,
                ModelId = ModelId(brand, model),
                VIN = vin,
                Year = year,
                EngineType = engine,
                Mileage = mileage
            };

        db.Cars.AddRange(
            A(clients[0], "А247ХХ33", "Lada", "Granta", null, 2017, "Бензин", 152328),
            A(clients[1], "В512ОР33", "Lada", "Vesta", "XTA21900060123456", 2019, "Бензин", 98400),
            A(clients[2], "Е089КМ33", "Toyota", "Camry", "JTNB11HK103456789", 2018, "Бензин", 121700),
            A(clients[3], "К321НТ33", "Kia", "Rio", null, 2016, "Бензин", 187250),
            A(clients[4], "М654СУ33", "Hyundai", "Solaris", "Z94CT41CBGR123456", 2020, "Бензин", 64300),
            A(clients[5], "Н118ЕХ33", "Renault", "Logan", null, 2015, "Газ/бензин", 204100),
            A(clients[6], "О902РА33", "Volkswagen", "Polo", "XW8ZZZ61ZJG012345", 2021, "Бензин", 45800),
            A(clients[7], "Р275ВК33", "Skoda", "Octavia", "TMBJJ7NE5J0234567", 2017, "Дизель", 163500),
            A(clients[8], "С430ТМ33", "Ford", "Focus", null, 2014, "Бензин", 211900),
            A(clients[9], "Т816ХН33", "Chevrolet", "Niva", "X9L21230070345678", 2013, "Бензин", 178600),
            A(clients[10], "У159КЕ33", "UAZ", "Patriot", null, 2019, "Дизель", 89200),
            A(clients[11], "Х663МО33", "Haval", "Jolion", "LGWEF6A59NH456789", 2022, "Бензин", 31500),
            A(clients[0], "А777ВС133", "Nissan", "Qashqai", null, 2018, "Бензин", 110400));
        db.SaveChanges();

        var employees = db.Employees.ToDictionary(e => e.LastName, e => e.Id);
        var services = db.Services.ToDictionary(s => s.Name, s => s);
        var parts = db.Parts.ToDictionary(p => p.Article, p => p);

        const string Guard = "Защита картера - снятие/установка";
        const string Clutch = "Комплект сцепления - с/у (при снятой коробке передач)";
        const string Gearbox = "Коробка передач в сборе - с/у (при слитом масле)";
        const string Bearing = "Подшипник переднего колеса в сборе - с/у правого подшипника";
        const string Consumables = "Расходные материалы";
        const string Alignment = "Регулировка углов подвески ВАЗ (2108-2190)";
        const string StrutLeft = "Стойка телескопическая передней подвески левая - с/у";
        const string StrutRight = "Стойка телескопическая передней подвески правая - с/у";
        const string Cable = "Трос привода сцепления - с/у";
        const string Diagnostics = "Компьютерная диагностика";
        const string OilChange = "Замена масла в двигателе (работа)";
        const string Tires = "Шиномонтаж (комплект 4 колеса)";
        const string Pads = "Колодки тормозные передние - замена";
        const string Belt = "Ремень ГРМ - замена";
        const string Plugs = "Замена свечей зажигания";
        const string Coolant = "Замена охлаждающей жидкости";

        var orders = new[]
        {
            new OrderSeed("А247ХХ33", new DateTime(2026, 9, 13, 7, 41, 0), new DateTime(2026, 9, 13, 15, 53, 0),
                "Замена сцепления, подшипника ступицы, стоек передней подвески",
                new (string, string, decimal?)[]
                {
                    (Guard, "Горшков", null), (Clutch, "Горшков", 0.40m), (Gearbox, "Горшков", 4.15m),
                    (Bearing, "Горшков", 1.45m), (Consumables, "Горшков", null), (Alignment, "Белов", null),
                    (StrutLeft, "Горшков", 0.55m), (StrutRight, "Горшков", 0.55m), (Cable, "Горшков", 0.55m)
                },
                new (string, int)[]
                {
                    ("21011601215", 1), ("00001-16101511-118", 2), ("00001-0005170-708", 2), ("2181-1601180", 1),
                    ("2190-2902820", 2), ("CS-20", 1), ("11180-3103012-00", 1), ("2190-1601000", 1),
                    ("21901-1602210-01", 1), ("00001-0007346-018", 2)
                }),
            new OrderSeed("В512ОР33", new DateTime(2026, 9, 18, 9, 10, 0), new DateTime(2026, 9, 18, 11, 30, 0),
                "Плановое ТО, замена масла и фильтра",
                new (string, string, decimal?)[] { (OilChange, "Горшков", 0.50m), (Diagnostics, "Соколов", 0.40m) },
                new (string, int)[] { ("OIL-5W40-4L", 1), ("FILTER-OIL-2112", 1) }),
            new OrderSeed("Е089КМ33", new DateTime(2026, 9, 22, 10, 0, 0), new DateTime(2026, 9, 22, 13, 15, 0),
                "Скрип при торможении",
                new (string, string, decimal?)[] { (Pads, "Белов", 1.00m), (Diagnostics, "Соколов", 0.50m) },
                new (string, int)[] { ("BRAKE-PADS-F-2190", 1) }),
            new OrderSeed("К321НТ33", new DateTime(2026, 9, 27, 8, 20, 0), new DateTime(2026, 9, 27, 12, 0, 0),
                "Сезонная замена колёс, увод автомобиля в сторону",
                new (string, string, decimal?)[] { (Tires, "Фёдоров", 1.00m), (Alignment, "Белов", 1.20m) },
                new (string, int)[] { ("WIPER-600", 1) }),
            new OrderSeed("М654СУ33", new DateTime(2026, 10, 2, 9, 0, 0), new DateTime(2026, 10, 2, 16, 40, 0),
                "Плановая замена ремня ГРМ",
                new (string, string, decimal?)[] { (Belt, "Новиков", 3.50m), (OilChange, "Новиков", 0.50m), (Coolant, "Новиков", 1.00m) },
                new (string, int)[] { ("TIMING-KIT-1118", 1), ("OIL-5W40-4L", 1), ("FILTER-OIL-2112", 1), ("COOLANT-G12-5L", 1) }),
            new OrderSeed("Н118ЕХ33", new DateTime(2026, 10, 8, 11, 5, 0), new DateTime(2026, 10, 8, 15, 20, 0),
                "Стук в передней подвеске",
                new (string, string, decimal?)[] { (StrutLeft, "Морозов", 1.10m), (StrutRight, "Морозов", 1.10m), (Alignment, "Белов", 1.00m) },
                new (string, int)[] { ("2190-2902820", 2) }),
            new OrderSeed("О902РА33", new DateTime(2026, 10, 14, 13, 30, 0), new DateTime(2026, 10, 14, 15, 0, 0),
                "Горит индикатор неисправности двигателя",
                new (string, string, decimal?)[] { (Diagnostics, "Соколов", 0.60m), (Plugs, "Волков", 0.70m) },
                new (string, int)[] { ("SPARK-PLUG-NGK-4", 1) }),
            new OrderSeed("Р275ВК33", new DateTime(2026, 10, 21, 10, 15, 0), new DateTime(2026, 10, 21, 14, 45, 0),
                "Замена колодок, подготовка к зиме",
                new (string, string, decimal?)[] { (Pads, "Белов", 1.00m), (Tires, "Фёдоров", 1.00m), (Coolant, "Новиков", 0.80m) },
                new (string, int)[] { ("BRAKE-PADS-F-2190", 1), ("COOLANT-G12-5L", 1) }),
            new OrderSeed("С430ТМ33", new DateTime(2026, 10, 29, 8, 45, 0), new DateTime(2026, 10, 29, 17, 10, 0),
                "Не выжимается сцепление",
                new (string, string, decimal?)[] { (Cable, "Горшков", 0.60m), (Clutch, "Горшков", 4.50m), (Consumables, "Горшков", null) },
                new (string, int)[] { ("21901-1602210-01", 1), ("2190-1601000", 1) }),
            new OrderSeed("Т816ХН33", new DateTime(2026, 11, 3, 9, 30, 0), null,
                "Течь масла из-под картера",
                new (string, string, decimal?)[] { (Guard, "Горшков", 0.30m), (Consumables, "Горшков", null) },
                new (string, int)[] { ("OIL-5W40-4L", 1), ("FILTER-OIL-2112", 1) }),
            new OrderSeed("У159КЕ33", new DateTime(2026, 11, 4, 12, 0, 0), null,
                "Неустойчивая работа двигателя",
                new (string, string, decimal?)[] { (Diagnostics, "Соколов", 0.50m), (Plugs, "Волков", 0.70m) },
                new (string, int)[] { ("SPARK-PLUG-NGK-4", 1), ("AIR-FILTER-2190", 1) }),
            new OrderSeed("Х663МО33", new DateTime(2026, 11, 5, 8, 30, 0), null,
                "Плановое ТО",
                new (string, string, decimal?)[] { (OilChange, "Горшков", 0.50m) },
                new (string, int)[] { ("OIL-5W40-4L", 1), ("FILTER-OIL-2112", 1), ("WIPER-600", 1) })
        };

        foreach (var seed in orders)
        {
            var order = new WorkOrder
            {
                RegistrationNumber = seed.Plate,
                StartDate = seed.Start,
                EndDate = seed.End,
                Reason = seed.Reason,
                IsCompleted = seed.End.HasValue
            };

            foreach (var (serviceName, executor, hours) in seed.Works)
            {
                var service = services[serviceName];
                order.Works.Add(new OrderService
                {
                    ServiceId = service.Id,
                    EmployeeId = employees[executor],
                    Price = service.Price,
                    Duration = hours
                });
            }

            foreach (var (article, quantity) in seed.Parts)
            {
                var part = parts[article];
                part.StockQuantity = Math.Max(0, part.StockQuantity - quantity);
                order.Parts.Add(new OrderPart { Article = article, Quantity = quantity, Price = part.Price });
            }

            db.WorkOrders.Add(order);
        }

        db.SaveChanges();
    }
}
