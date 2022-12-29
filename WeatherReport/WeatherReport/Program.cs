using Microsoft.VisualBasic;
using Newtonsoft.Json;
using Npgsql;
using System.Net;
using System.Xml;

namespace WeatherReport
{
    static class UtilsPostgres
    {
        public static NpgsqlConnection Connect(string connectionString)
        {
            NpgsqlConnection conn = new NpgsqlConnection(connectionString);
            try
            {
                conn.Open();
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                throw new Exception("Ошибка подключения к базе данных", e);
                Console.ResetColor();
            }
            return conn;
        }

        public static async Task ExecuteSelectAsJson(NpgsqlConnection conn, string sql, Action<string> callback)
        {
            try
            {
                using (var command = conn.CreateCommand())
                {
                    command.CommandText = sql;
                    var result = await command.ExecuteScalarAsync();
                    if (result is string json)
                    {
                        callback(json);
                    }
                }
            }
            catch (Exception e)
            {
            }
        }
    }

    public class Meteostation
    {
        public string MeteostationName { get; set; }
        public string MeteostationAddress { get; set; }
        public float seaLEVEL { get; set; }
    }
    public class Device
    {
        public string nameDevice { get; set; }
        public string serviceLife { get; set; }
        public float accuracy { get; set; }
        public string nameMeteostation { get; set; }
        public string addressMeteostation { get; set; }
    }
    public class Measurement
    {
        public string time { get; set; }
        public string unit { get; set; }
        public float result { get; set; }
        public string nameMeteostation { get; set; }
        public string addressMeteostation { get; set; }
        public string nameDevice { get; set; }
    }
    public class Monitoring
    {
        public string time { get; set; }
        public string namePerson { get; set; }
        public string phenomenon { get; set; }
        public string nameMeteostation { get; set; }
        public string addressMeteostation { get; set; }
    }
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Авторизация\n\nПо умолчанию:\n\nАдрес сервера: localhost\nПорт: 5432\nНазвание базы данных: WeatherReport\nЛогин: postgres\nПароль: postgres\n\nИзменить? (1.Да/2.Нет)");
            int authorization = Convert.ToInt32(Console.ReadLine());
            string connectionString;
            if (authorization == 1)
            {
                Console.WriteLine("Введите адрес сервера:");
                string addressServer= Console.ReadLine();
                Console.WriteLine("Введите порт:");
                int port = Convert.ToInt32(Console.ReadLine());
                Console.WriteLine("Введите название БД:");
                string nameBD = Console.ReadLine();
                Console.WriteLine("Введите логин пользователя:");
                string userId = Console.ReadLine();
                Console.WriteLine("Введите пароль пользователя:");
                string password = Console.ReadLine();

                connectionString = $"Server={addressServer}; Port={port}; Database={nameBD}; UserId={userId}; Password={password}; commandTimeout=120;";
            }
            else
            {
                connectionString = "Server=localhost; Port=5432; Database=WeatherReport; UserId=postgres; Password=postgres; commandTimeout=120;";

            }
            // connect
            var conn = UtilsPostgres.Connect(connectionString);
            Console.WriteLine("Создать таблицы? И наполнить их данными \n1.Да (если их нет) \n2.Нет (если они есть)\n");
            int table = Convert.ToInt32(Console.ReadLine());
            if (table == 1)
            {
                string createTable = $@"
                                    CREATE TABLE Meteostation (
                                    name varchar(256) NOT NULL,
                                    address varchar(256) NOT NULL,
                                    seaLEVEL real NOT NULL
                                    );
                                    ALTER TABLE ONLY Meteostation
                                    ADD CONSTRAINT Meteostation_pkey PRIMARY KEY (name, address);

                                    INSERT INTO Meteostation VALUES ('', '', 147);
                                    INSERT INTO Meteostation VALUES ('Метеостанция Екатеринбурга', 'г. Екатеринбург, ул. Народной воли, 64', 283);
                                    INSERT INTO Meteostation VALUES ('Метеостанция Кашира', 'Московская область, Кашира г., Стрелецкая улица, 67', 220);
                                    INSERT INTO Meteostation VALUES ('ФГБУ Центральное УГМС Метеостанция Москва ВДНХ', 'просп. Мира, 119, стр. 423, Москва этаж 1-2', 147);


                                    CREATE TABLE Device (
                                    name varchar(256) NOT NULL,
                                    serviceLife date NOT NULL,
                                    accuracy real,
                                    nameMeteostation varchar(256) NOT NULL,
                                    addressMeteostation varchar(256) NOT NULL
                                    );

                                    ALTER TABLE ONLY Device
                                    ADD CONSTRAINT Device_pkey PRIMARY KEY (nameMeteostation, addressMeteostation, name);
	
                                    ALTER TABLE ONLY Device
                                    ADD CONSTRAINT Meteostation_Device_fkey FOREIGN KEY (nameMeteostation, addressMeteostation) REFERENCES Meteostation (name, address);

                                    INSERT INTO Device VALUES ('Термометр', '2023-01-01', 99.9, 'Метеостанция Екатеринбурга','г. Екатеринбург, ул. Народной воли, 64');
                                    INSERT INTO Device VALUES ('Гигрометр', '2023-04-05', 98.9, 'Метеостанция Екатеринбурга','г. Екатеринбург, ул. Народной воли, 64');
                                    INSERT INTO Device VALUES ('Барометр', '2023-02-14', 88.9, 'Метеостанция Екатеринбурга','г. Екатеринбург, ул. Народной воли, 64');
                                    INSERT INTO Device VALUES ('Осадкомер', '2022-12-22', 90.9, 'Метеостанция Екатеринбурга','г. Екатеринбург, ул. Народной воли, 64');
                                    INSERT INTO Device VALUES ('Термометр', '2022-12-26', 90.8, 'Метеостанция Кашира', 'Московская область, Кашира г., Стрелецкая улица, 67');
                                    INSERT INTO Device VALUES ('Барометр', '2023-06-01', 97.7, 'Метеостанция Кашира', 'Московская область, Кашира г., Стрелецкая улица, 67');
                                    INSERT INTO Device VALUES ('Термометр', '2023-01-08', 99.9, 'ФГБУ Центральное УГМС Метеостанция Москва ВДНХ', 'просп. Мира, 119, стр. 423, Москва этаж 1-2');
                                    INSERT INTO Device VALUES ('Барометр', '2023-02-01', 98.7, 'ФГБУ Центральное УГМС Метеостанция Москва ВДНХ', 'просп. Мира, 119, стр. 423, Москва этаж 1-2');
                                    INSERT INTO Device VALUES ('Гигрометр', '2023-03-01', 96.7, 'ФГБУ Центральное УГМС Метеостанция Москва ВДНХ', 'просп. Мира, 119, стр. 423, Москва этаж 1-2');


                                    CREATE TABLE Measurement (
                                    time timestamp NOT NULL,
                                    unit varchar(100) NOT NULL,
                                    result real NOT NULL,
                                    nameMeteostation varchar(256) NOT NULL,
                                    addressMeteostation varchar(256) NOT NULL,
                                    nameDevice varchar(256) NOT NULL
                                    );
                                    ALTER TABLE ONLY Measurement
                                    ADD CONSTRAINT Measurement_pkey PRIMARY KEY (time, nameMeteostation, addressMeteostation, nameDevice);
	
                                    ALTER TABLE ONLY Measurement
                                    ADD CONSTRAINT Device_Measurement_fkey FOREIGN KEY (nameMeteostation, addressMeteostation, nameDevice) REFERENCES Device (nameMeteostation, addressMeteostation, name);

                                    INSERT INTO Measurement VALUES ('2023-01-01 18:14:10','°C',-5,'Метеостанция Екатеринбурга','г. Екатеринбург, ул. Народной воли, 64','Термометр');
                                    INSERT INTO Measurement VALUES ('2023-01-01 23:15:01','°C',-10,'Метеостанция Екатеринбурга','г. Екатеринбург, ул. Народной воли, 64','Термометр');
                                    INSERT INTO Measurement VALUES ('2023-01-01 12:14:01','%',20,'Метеостанция Екатеринбурга','г. Екатеринбург, ул. Народной воли, 64','Гигрометр');
                                    INSERT INTO Measurement VALUES ('2023-01-01 23:01:01','%',10,'Метеостанция Екатеринбурга','г. Екатеринбург, ул. Народной воли, 64','Гигрометр');
                                    INSERT INTO Measurement VALUES ('2023-01-01 15:20:19','мм рт.ст.',758,'Метеостанция Екатеринбурга','г. Екатеринбург, ул. Народной воли, 64','Барометр');
                                    INSERT INTO Measurement VALUES ('2023-01-01 23:21:15','мм рт.ст.',710,'Метеостанция Екатеринбурга','г. Екатеринбург, ул. Народной воли, 64','Барометр');
                                    INSERT INTO Measurement VALUES ('2023-01-01 18:17:44','°C',+10,'ФГБУ Центральное УГМС Метеостанция Москва ВДНХ', 'просп. Мира, 119, стр. 423, Москва этаж 1-2','Термометр');
                                    INSERT INTO Measurement VALUES ('2023-01-01 22:16:01','°C',-1,'ФГБУ Центральное УГМС Метеостанция Москва ВДНХ', 'просп. Мира, 119, стр. 423, Москва этаж 1-2','Термометр');
                                    INSERT INTO Measurement VALUES ('2023-01-01 10:14:55','°C',+7,'Метеостанция Кашира', 'Московская область, Кашира г., Стрелецкая улица, 67','Термометр');


                                    CREATE TABLE Monitoring (
                                    time timestamp NOT NULL,
                                    namePerson varchar(256) NOT NULL,
                                    phenomenon varchar(256) NOT NULL,
                                    nameMeteostation varchar(256) NOT NULL,
                                    addressMeteostation varchar(256) NOT NULL
                                    );

                                    ALTER TABLE ONLY Monitoring
                                    ADD CONSTRAINT Monitoring_pkey PRIMARY KEY (nameMeteostation, addressMeteostation, time);
	
                                    ALTER TABLE ONLY Monitoring
                                    ADD CONSTRAINT Meteostation_Monitoring_fkey FOREIGN KEY (nameMeteostation, addressMeteostation) REFERENCES Meteostation (name, address);
	
                                    INSERT INTO Monitoring VALUES ('2022-12-27 18:14:10', 'Ларин Сергей Александрович', 'снегопад', 'Метеостанция Екатеринбурга','г. Екатеринбург, ул. Народной воли, 64');
                                    INSERT INTO Monitoring VALUES ('2022-12-29 10:20:00', 'Кузьминых Игорь Михайлович', 'крупный град', 'Метеостанция Екатеринбурга','г. Екатеринбург, ул. Народной воли, 64');
                                    INSERT INTO Monitoring VALUES ('2022-12-11 11:55:13', 'Исаев Павел Максимович', 'метель', 'Метеостанция Кашира', 'Московская область, Кашира г., Стрелецкая улица, 67');
                                    INSERT INTO Monitoring VALUES ('2022-12-26 23:55:55', 'Исаев Павел Максимович', 'дождь', 'Метеостанция Кашира', 'Московская область, Кашира г., Стрелецкая улица, 67');
                                    INSERT INTO Monitoring VALUES ('2022-12-22 11:59:31', 'Маслов Анатолий Иосифович', 'радуга', 'ФГБУ Центральное УГМС Метеостанция Москва ВДНХ', 'просп. Мира, 119, стр. 423, Москва этаж 1-2');
                              ";
                NpgsqlCommand create = new NpgsqlCommand(createTable, conn);
                create.ExecuteNonQuery();
            }
            while (true)
            {
                Console.WriteLine("База Данных Метеорологических наблюдений\n");
                Console.WriteLine("Выберите таблицу, с которой хотите проделать операцию\n1. Метеостанция\n2. Приборы\n3. Измерения\n4. Наблюдения\n0. Выйти\n");
                int choise = Convert.ToInt32(Console.ReadLine());
                if (choise == 1)
                {
                    while (true)
                    {
                        Console.WriteLine("Что хотите сделать?\n1. Увидеть данные\n2. Добавить данные\n3. Изменить данные\n4. Удалить данные\n5. Найти данные\n0. Назад\n");
                        int choise2 = Convert.ToInt32(Console.ReadLine());

                        if (choise2 == 1)
                        {
                            string sql = $@"
                                   SELECT json_agg(row_to_json(Meteostations))
                                   FROM
                                   (
                                        SELECT
                                             name AS MeteostationName,
                                             address AS MeteostationAddress,
                                             seaLEVEL AS seaLEVEL
                                        FROM Meteostation
                                   ) AS Meteostations;
                              ";
                            await UtilsPostgres.ExecuteSelectAsJson(conn, sql, json =>
                            {

                                // показать json
                                //Console.WriteLine(json);

                                // преобразование json в список
                                var Meteostations = JsonConvert.DeserializeObject<List<Meteostation>>(json);

                                // выводим список
                                foreach (var item in Meteostations)
                                {
                                    Console.WriteLine($"Название: {item.MeteostationName}\nАдрес: {item.MeteostationAddress}\nВысота над уровнем моря(м): {item.seaLEVEL}\n\n");
                                }

                            });

                        }
                        else if (choise2 == 2)
                        {

                            Console.WriteLine("Введите данные\nНазвание метеостанции: ");
                            string name = Console.ReadLine();
                            Console.WriteLine("Адрес метеостанции: ");
                            string address = Console.ReadLine();
                            Console.WriteLine("Высоту над уровнем моря: ");
                            string seaLEVEL = Console.ReadLine();


                            if (name == "" || address == "" || seaLEVEL == "")
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("Ошибка. Имя или адрес несут пустое значение");
                                Console.ResetColor();
                            }
                            else
                            {
                                bool target = false;
                                string sql = $@"
                                   SELECT json_agg(row_to_json(Meteostations))
                                   FROM
                                   (
                                        SELECT
                                             name AS MeteostationName,
                                             address AS MeteostationAddress,
                                             seaLEVEL AS seaLEVEL
                                        FROM Meteostation
                                            WHERE name='{name}'AND address='{address}'
                                   ) AS Meteostations;
                                   ";
                                await UtilsPostgres.ExecuteSelectAsJson(conn, sql, json =>
                                {
                                    // преобразование json в список
                                    var Meteostations = JsonConvert.DeserializeObject<List<Meteostation>>(json);

                                    // выводим список
                                    foreach (var item in Meteostations)
                                    {
                                        if (item.MeteostationName == name && item.MeteostationAddress == address)
                                        {
                                            Console.ForegroundColor = ConsoleColor.Red;
                                            Console.WriteLine("Ошибка. Такие данные уже существуют");
                                            Console.ResetColor();
                                            target = true;
                                        }
                                    }

                                });
                                NpgsqlCommand insert = new NpgsqlCommand("INSERT INTO Meteostation(name, address, seaLEVEL) VALUES(@name, @address, @seaLEVEL);", conn);
                                if (target == false)
                                {
                                    insert.Parameters.AddWithValue("@name", name);
                                    insert.Parameters.AddWithValue("@address", address);
                                    insert.Parameters.AddWithValue("@seaLEVEL", float.Parse(seaLEVEL));
                                    try { insert.ExecuteNonQuery(); }
                                    catch
                                    {
                                        Console.ForegroundColor = ConsoleColor.Red;
                                        Console.WriteLine("Ошибка ввода данных.");
                                        Console.ResetColor();
                                    }
                                }

                            }
                        }
                        else if (choise2 == 3)
                        {
                            Console.WriteLine("Что хотите изменить?\n1. Название Метеостанции\n2. Адрес Метеостанции\n3. Высоту над уровнем моря\n ");
                            int changeNumber = Convert.ToInt32(Console.ReadLine());
                            string nameTable;
                            if (changeNumber == 1)
                                nameTable = "name";
                            else if (changeNumber == 2)
                                nameTable = "address";
                            else if (changeNumber == 3)
                                nameTable = "seaLEVEL";
                            else
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("Ошибка. Вы не ввели, что хотите изменить");
                                Console.ResetColor();
                                break;
                            }
                            Console.WriteLine("На какое значение изменить? ");
                            string change = Console.ReadLine();
                            if (change == "")
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("Ошибка. Вы не ввели, значение, на которое хотите поменять");
                                Console.ResetColor();
                                break;
                            }
                            Console.WriteLine("Введите ключ, по которому произойдёт изменение\nНазвание метеостанции: ");
                            string name = Console.ReadLine();
                            Console.WriteLine("Адрес метеостанции: ");
                            string address = Console.ReadLine();
                            if (name == "" || address == "")
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("Ошибка. Вы не правильно ввели ключ");
                                Console.ResetColor();
                                break;
                            }
                            string res = null;
                            if (changeNumber != 3)
                                res = "'" + change + "'";
                            else
                                res = change;
                            string sql = "";
                            if (changeNumber == 1)
                                sql = $@"
                                            ALTER TABLE Device
                                                    DROP CONSTRAINT Meteostation_Device_fkey;
                                            ALTER TABLE Measurement
                                                    DROP CONSTRAINT  Device_Measurement_fkey;
                                            ALTER TABLE Monitoring
                                                    DROP CONSTRAINT Meteostation_Monitoring_fkey;
                                            UPDATE Device SET nameMeteostation = {res} WHERE nameMeteostation='{name}' AND addressMeteostation='{address}'; 
                                            UPDATE Measurement SET nameMeteostation = {res} WHERE nameMeteostation='{name}' AND addressMeteostation='{address}'; 
                                            UPDATE Monitoring SET nameMeteostation = {res} WHERE nameMeteostation='{name}' AND addressMeteostation='{address}'; 
                                            UPDATE Meteostation SET {nameTable} = {res} WHERE name='{name}' AND address='{address}';
                                            ALTER TABLE ONLY Device
                                                ADD CONSTRAINT Meteostation_Device_fkey FOREIGN KEY (nameMeteostation, addressMeteostation) REFERENCES Meteostation (name, address);
                                            ALTER TABLE ONLY Measurement
                                                ADD CONSTRAINT Device_Measurement_fkey FOREIGN KEY (nameMeteostation, addressMeteostation, nameDevice) REFERENCES Device (nameMeteostation, addressMeteostation, name);
                                            ALTER TABLE ONLY Monitoring
                                                ADD CONSTRAINT Meteostation_Monitoring_fkey FOREIGN KEY (nameMeteostation, addressMeteostation) REFERENCES Meteostation (name, address);
	

                                            ";
                            else if (changeNumber == 2)
                                sql = $@"
                                            ALTER TABLE Device
                                                    DROP CONSTRAINT Meteostation_Device_fkey;
                                            ALTER TABLE Measurement
                                                    DROP CONSTRAINT  Device_Measurement_fkey;
                                            ALTER TABLE Monitoring
                                                    DROP CONSTRAINT Meteostation_Monitoring_fkey;
                                            UPDATE Device SET addressMeteostation = {res} WHERE nameMeteostation='{name}' AND addressMeteostation='{address}'; 
                                            UPDATE Measurement SET addressMeteostation = {res} WHERE nameMeteostation='{name}' AND addressMeteostation='{address}';  
                                            UPDATE Monitoring SET addressMeteostation = {res} WHERE nameMeteostation='{name}' AND addressMeteostation='{address}'; 
                                            UPDATE Meteostation SET {nameTable} = {res} WHERE name='{name}' AND address='{address}';
                                            ALTER TABLE ONLY Device
                                                ADD CONSTRAINT Meteostation_Device_fkey FOREIGN KEY (nameMeteostation, addressMeteostation) REFERENCES Meteostation (name, address);
                                            ALTER TABLE ONLY Measurement
                                                ADD CONSTRAINT Device_Measurement_fkey FOREIGN KEY (nameMeteostation, addressMeteostation, nameDevice) REFERENCES Device (nameMeteostation, addressMeteostation, name);
                                            ALTER TABLE ONLY Monitoring
                                                ADD CONSTRAINT Meteostation_Monitoring_fkey FOREIGN KEY (nameMeteostation, addressMeteostation) REFERENCES Meteostation (name, address);
	
                                            ";
                            else if (changeNumber == 3)
                                sql = $"UPDATE Meteostation SET {nameTable} = {res} WHERE name='{name}' AND address='{address}';";
                            NpgsqlCommand update = new NpgsqlCommand(sql, conn);
                            try { update.ExecuteNonQuery(); }
                            catch
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("Ошибка. Вы не правильно ввели значения, которые хотите изменить");
                                Console.ResetColor();
                            }


                        }
                        else if (choise2 == 4)
                        {
                            Console.WriteLine("Введите ключ, по которому произойдёт удаление\nНазвание метеостанции: ");
                            string name = Console.ReadLine();
                            Console.WriteLine("Адрес метеостанции: ");
                            string address = Console.ReadLine();
                            if (name == "" || address == "")
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("Ошибка. Вы не правильно ввели ключ");
                                Console.ResetColor();
                                break;
                            }
                            string sql = $@"
                                            ALTER TABLE Device
		                                            DROP CONSTRAINT Meteostation_Device_fkey;
                                            ALTER TABLE Measurement
                                                    DROP CONSTRAINT  Device_Measurement_fkey;
                                            ALTER TABLE Monitoring
                                                    DROP CONSTRAINT Meteostation_Monitoring_fkey;
                                            DELETE FROM Device WHERE nameMeteostation='{name}' AND addressMeteostation='{address}';   
                                            DELETE FROM Measurement WHERE nameMeteostation='{name}' AND addressMeteostation='{address}';   
                                            DELETE FROM Monitoring WHERE nameMeteostation='{name}' AND addressMeteostation='{address}';   

                                            DELETE FROM Meteostation WHERE name='{name}' AND address='{address}';
                                            ALTER TABLE ONLY Device
	                                                ADD CONSTRAINT Meteostation_Device_fkey FOREIGN KEY (nameMeteostation, addressMeteostation) REFERENCES Meteostation (name, address);
                                            ALTER TABLE ONLY Measurement
                                                ADD CONSTRAINT Device_Measurement_fkey FOREIGN KEY (nameMeteostation, addressMeteostation, nameDevice) REFERENCES Device (nameMeteostation, addressMeteostation, name);
                                            ALTER TABLE ONLY Monitoring
                                                ADD CONSTRAINT Meteostation_Monitoring_fkey FOREIGN KEY (nameMeteostation, addressMeteostation) REFERENCES Meteostation (name, address);
	
                                            ";
                            NpgsqlCommand delete = new NpgsqlCommand(sql, conn);
                            try { delete.ExecuteNonQuery(); }
                            catch
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("Ошибка. Нет таких данных для удаления");
                                Console.ResetColor();
                            }
                        }
                        else if (choise2 == 5)
                        {
                            Console.WriteLine("Введите ключ данных, которые хотите найти\nНазвание метеостанции: ");
                            string name = Console.ReadLine();
                            Console.WriteLine("Адрес метеостанции: ");
                            string address = Console.ReadLine();
                            if (name == "" || address == "")
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("Ошибка. Вы не правильно ввели ключ");
                                Console.ResetColor();
                                break;
                            }
                            bool target = false;
                            string sql = $@"
                                   SELECT json_agg(row_to_json(Meteostations))
                                   FROM
                                   (
                                        SELECT
                                             name AS MeteostationName,
                                             address AS MeteostationAddress,
                                             seaLEVEL AS seaLEVEL
                                        FROM Meteostation
                                            WHERE name='{name}'AND address='{address}'
                                   ) AS Meteostations;
                              ";
                            await UtilsPostgres.ExecuteSelectAsJson(conn, sql, json =>
                            {
                                // преобразование json в список
                                var Meteostations = JsonConvert.DeserializeObject<List<Meteostation>>(json);

                                // выводим список
                                foreach (var item in Meteostations)
                                {
                                    Console.WriteLine($"Название: {item.MeteostationName}\nАдрес: {item.MeteostationAddress}\nВысота над уровнем моря(м): {item.seaLEVEL}\n\n");
                                    target = true;
                                }

                            });
                            if (target == false)
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("Таких данных не существует");
                                Console.ResetColor();
                            }
                        }
                        else
                            break;
                    }

                }
                else if (choise == 2)
                {
                    while (true)
                    {
                        Console.WriteLine("Что хотите сделать?\n1. Увидеть данные\n2. Добавить данные\n3. Изменить данные\n4. Удалить данные\n5. Найти данные\n0. Назад\n");
                        int choise2 = Convert.ToInt32(Console.ReadLine());

                        if (choise2 == 1)
                        {
                            string sql = $@"
                                   SELECT json_agg(row_to_json(Devices))
                                   FROM
                                   (
                                        SELECT
                                             name AS nameDevice,
                                             serviceLife AS serviceLife,
                                             accuracy AS accuracy,
                                             nameMeteostation AS nameMeteostation,
                                             addressMeteostation AS addressMeteostation
                                        FROM Device
                                   ) AS Devices;
                              ";
                            await UtilsPostgres.ExecuteSelectAsJson(conn, sql, json =>
                            {

                                // показать json
                                //Console.WriteLine(json);

                                // преобразование json в список
                                var Devices = JsonConvert.DeserializeObject<List<Device>>(json);

                                // выводим список
                                foreach (var item in Devices)
                                {
                                    Console.WriteLine($"Название: {item.nameDevice}\nСрок годности до: {item.serviceLife}\nТочность: {item.accuracy}\nНазвание Метеостанции: {item.nameMeteostation}\nАдрес метеостанции: {item.addressMeteostation}\n\n");
                                }

                            });

                        }
                        else if (choise2 == 2)
                        {

                            Console.WriteLine("Введите данные\nНазвание прибора: ");
                            string name = Console.ReadLine();
                            Console.WriteLine("Название метеостанции: ");
                            string nameMeteostation = Console.ReadLine();
                            Console.WriteLine("Адрес метеостанции: ");
                            string addressMeteostation = Console.ReadLine();

                            bool target = false;
                            string sql = $@"
                                   SELECT json_agg(row_to_json(Meteostations))
                                   FROM
                                   (
                                        SELECT
                                             name AS MeteostationName,
                                             address AS MeteostationAddress,
                                             seaLEVEL AS seaLEVEL
                                        FROM Meteostation
                                            WHERE name='{nameMeteostation}'AND address='{addressMeteostation}'
                                   ) AS Meteostations;
                              ";
                            await UtilsPostgres.ExecuteSelectAsJson(conn, sql, json =>
                            {
                                // преобразование json в список
                                var Meteostations = JsonConvert.DeserializeObject<List<Meteostation>>(json);

                                // выводим список
                                foreach (var item in Meteostations)
                                {
                                    target = true;
                                }

                            });
                            if (target == false)
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("Такой метеостанции не существует в БД");
                                Console.ResetColor();
                                break;
                            }
                            Console.WriteLine("Срок годности до (гггг-мм-дд): ");
                            string serviceLife = Console.ReadLine();
                            Console.WriteLine("Точность (через ','): ");
                            string accuracy = Console.ReadLine();

                            if (name == "" || serviceLife == "" || accuracy == "" || nameMeteostation == "" || addressMeteostation == "")
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("Ошибка. Некоторые данные пропущены");
                                Console.ResetColor();
                            }
                            else
                            {
                                target = false;
                                sql = $@"
                                   SELECT json_agg(row_to_json(Devices))
                                   FROM
                                   (
                                        SELECT
                                             name AS nameDevice,
                                             serviceLife AS serviceLife,
                                             accuracy AS accuracy,
                                             nameMeteostation AS nameMeteostation,
                                             addressMeteostation AS addressMeteostation
                                        FROM Device
                                            WHERE name='{name}'AND nameMeteostation='{nameMeteostation}'AND addressMeteostation='{addressMeteostation}'
                                   ) AS Devices;
                                   ";
                                await UtilsPostgres.ExecuteSelectAsJson(conn, sql, json =>
                                {
                                    // преобразование json в список
                                    var Devices = JsonConvert.DeserializeObject<List<Device>>(json);

                                    // выводим список
                                    foreach (var item in Devices)
                                    {
                                        if (item.nameDevice == name && item.nameMeteostation == nameMeteostation && item.addressMeteostation == addressMeteostation)
                                        {
                                            Console.ForegroundColor = ConsoleColor.Red;
                                            Console.WriteLine("Ошибка. Такие данные уже существуют");
                                            Console.ResetColor();
                                            target = true;
                                        }
                                    }

                                });
                                NpgsqlCommand insert = new NpgsqlCommand("INSERT INTO Device(name, serviceLife, accuracy, nameMeteostation, addressMeteostation) VALUES(@name, @serviceLife, @accuracy, @nameMeteostation, @addressMeteostation);", conn);
                                if (target == false)
                                {
                                    insert.Parameters.AddWithValue("@name", name);
                                    try { insert.Parameters.AddWithValue("@serviceLife", DateTime.Parse(serviceLife)); }
                                    catch
                                    {
                                        Console.ForegroundColor = ConsoleColor.Red;
                                        Console.WriteLine("Ошибка при вводе даты.");
                                        Console.ResetColor();
                                    }
                                    try { insert.Parameters.AddWithValue("@accuracy", float.Parse(accuracy)); }
                                    catch
                                    {
                                        Console.ForegroundColor = ConsoleColor.Red;
                                        Console.WriteLine("Ошибка при вводе точности.");
                                        Console.ResetColor();
                                    }
                                    insert.Parameters.AddWithValue("@nameMeteostation", nameMeteostation);
                                    insert.Parameters.AddWithValue("@addressMeteostation", addressMeteostation);
                                    try { insert.ExecuteNonQuery(); }
                                    catch
                                    {
                                        Console.ForegroundColor = ConsoleColor.Red;
                                        Console.WriteLine("Ошибка при вводе данных");
                                        Console.ResetColor();
                                    }
                                }

                            }
                        }
                        else if (choise2 == 3)
                        {
                            Console.WriteLine("Что хотите изменить?\n1. Название Прибора\n2. Срок эксплуатации\n3. Точность (через '.')\n4. Название Метеостанции\n5. Адрес Метеостанции\n ");
                            int changeNumber = Convert.ToInt32(Console.ReadLine());
                            string nameTable;
                            if (changeNumber == 1)
                                nameTable = "name";
                            else if (changeNumber == 2)
                                nameTable = "serviceLife";
                            else if (changeNumber == 3)
                                nameTable = "accuracy";
                            else if (changeNumber == 4)
                                nameTable = "nameMeteostation";
                            else if (changeNumber == 5)
                                nameTable = "addressMeteostation";
                            else
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("Ошибка. Вы не ввели, что хотите изменить");
                                Console.ResetColor();
                                break;
                            }
                            Console.WriteLine("На какое значение изменить? ");
                            string change = Console.ReadLine();
                            if (change == "")
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("Ошибка. Вы не ввели, значение, на которое хотите поменять");
                                Console.ResetColor();
                                break;
                            }
                            Console.WriteLine("Введите ключ, по которому произойдёт изменение\nНазвание прибора: ");
                            string name = Console.ReadLine();
                            Console.WriteLine("Название метеостанции: ");
                            string nameMeteostation = Console.ReadLine();
                            Console.WriteLine("Адрес метеостанции: ");
                            string addressMeteostation = Console.ReadLine();
                            if (name == "" || nameMeteostation == "" || addressMeteostation == "")
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("Ошибка. Вы не правильно ввели ключ");
                                Console.ResetColor();
                                break;
                            }

                            string res = "'" + change + "'";
                            
                            string sql = "";
                            if (changeNumber == 1)
                                sql = $@"
                                            ALTER TABLE Measurement
                                                    DROP CONSTRAINT  Device_Measurement_fkey;
                                            UPDATE Measurement SET nameDevice = {res} WHERE nameMeteostation='{nameMeteostation}' AND addressMeteostation='{addressMeteostation}' AND nameDevice='{name}';  
                                            UPDATE Device SET name = {res} WHERE nameMeteostation='{nameMeteostation}' AND addressMeteostation='{addressMeteostation}' AND name='{name}';  
                                            ALTER TABLE ONLY Measurement
                                                ADD CONSTRAINT Device_Measurement_fkey FOREIGN KEY (nameMeteostation, addressMeteostation, nameDevice) REFERENCES Device (nameMeteostation, addressMeteostation, name);

                                            ";
                            else if (changeNumber == 2)
                                sql = $@"
                                            UPDATE Device SET serviceLife = {res} WHERE nameMeteostation='{nameMeteostation}' AND addressMeteostation='{addressMeteostation}' AND name='{name}'; 
                                            ";
                            else if (changeNumber == 3)
                                sql = $@"
                                            UPDATE Device SET accuracy = {res} WHERE nameMeteostation='{nameMeteostation}' AND addressMeteostation='{addressMeteostation}' AND name='{name}'; 
                                            ";
                            else if (changeNumber == 4)
                                sql = $@"
                                            ALTER TABLE Device
                                                    DROP CONSTRAINT Meteostation_Device_fkey;
                                            ALTER TABLE Measurement
                                                    DROP CONSTRAINT  Device_Measurement_fkey;
                                            UPDATE Measurement SET nameMeteostation = {res} WHERE nameMeteostation='{nameMeteostation}' AND addressMeteostation='{addressMeteostation} nameDevice='{name}';    
                                            UPDATE Device SET nameMeteostation = {res} WHERE nameMeteostation='{nameMeteostation}' AND addressMeteostation='{addressMeteostation} AND name='{name}';    
                                            ALTER TABLE ONLY Device
                                                ADD CONSTRAINT Meteostation_Device_fkey FOREIGN KEY (nameMeteostation, addressMeteostation) REFERENCES Meteostation (name, address);
                                            ALTER TABLE ONLY Measurement
                                                ADD CONSTRAINT Device_Measurement_fkey FOREIGN KEY (nameMeteostation, addressMeteostation, nameDevice) REFERENCES Device (nameMeteostation, addressMeteostation, name);

                                            ";
                            else if (changeNumber == 5)
                                sql = $@"
                                            ALTER TABLE Device
                                                    DROP CONSTRAINT Meteostation_Device_fkey;
                                            ALTER TABLE Measurement
                                                    DROP CONSTRAINT  Device_Measurement_fkey;
                                            UPDATE Measurement SET addressMeteostation = {res} WHERE nameMeteostation='{nameMeteostation}' AND addressMeteostation='{addressMeteostation} nameDevice='{name}';    
                                            
                                            UPDATE Device SET addressMeteostation = {res} WHERE nameMeteostation='{nameMeteostation}' AND addressMeteostation='{addressMeteostation} AND name='{name}';    
                                            ALTER TABLE ONLY Device
                                                ADD CONSTRAINT Meteostation_Device_fkey FOREIGN KEY (nameMeteostation, addressMeteostation) REFERENCES Meteostation (name, address);
                                            ALTER TABLE ONLY Measurement
                                                ADD CONSTRAINT Device_Measurement_fkey FOREIGN KEY (nameMeteostation, addressMeteostation, nameDevice) REFERENCES Device (nameMeteostation, addressMeteostation, name);

                                            ";
                            NpgsqlCommand update = new NpgsqlCommand(sql, conn);
                            try { update.ExecuteNonQuery(); }
                            catch
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("Ошибка. \n1. Если изменение названия или адреса метеостанции, то: Такой метеостанции нет. Попробуйте удалить данные и ввести новые. Либо изменить название самой метеостанции\n2. Возможен некорректный ввод точности, попробуйте ввести через '.' или некорректный ввод другого значения");
                                Console.ResetColor();
                            }
                        }
                        else if (choise2 == 4)
                        {
                            Console.WriteLine("Введите ключ, по которому произойдёт удаление\nНазвание прибора: ");
                            string name = Console.ReadLine();
                            Console.WriteLine("Название метеостанции: ");
                            string nameMeteostation = Console.ReadLine();
                            Console.WriteLine("Адрес метеостанции: ");
                            string addressMeteostation = Console.ReadLine();
                            if (name == "" || nameMeteostation == "" || addressMeteostation == "")
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("Ошибка. Вы не правильно ввели ключ");
                                Console.ResetColor();
                                break;
                            }
                            string sql = $@"
                                            ALTER TABLE Measurement
                                                    DROP CONSTRAINT  Device_Measurement_fkey;
                                            DELETE FROM Measurement WHERE nameMeteostation='{nameMeteostation}' AND addressMeteostation='{addressMeteostation}' AND nameDevice='{name}'; 
                                            DELETE FROM Device WHERE nameMeteostation='{nameMeteostation}' AND addressMeteostation='{addressMeteostation}' AND name='{name}'; 
                                            ALTER TABLE ONLY Measurement
                                                ADD CONSTRAINT Device_Measurement_fkey FOREIGN KEY (nameMeteostation, addressMeteostation, nameDevice) REFERENCES Device (nameMeteostation, addressMeteostation, name);

                                            ";
                            NpgsqlCommand delete = new NpgsqlCommand(sql, conn);
                            try { delete.ExecuteNonQuery(); }
                            catch
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("Ошибка. Нет таких данных для удаления");
                                Console.ResetColor();
                            }
                        }
                        else if (choise2 == 5)
                        {
                            Console.WriteLine("Введите ключ данных, которые хотите найти\nНазвание прибора: ");
                            string name = Console.ReadLine();
                            Console.WriteLine("Название метеостанции: ");
                            string nameMeteostation = Console.ReadLine();
                            Console.WriteLine("Адрес метеостанции: ");
                            string addressMeteostation = Console.ReadLine();
                            if (name == "" || nameMeteostation == "" || addressMeteostation == "")
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("Ошибка. Вы не правильно ввели ключ");
                                Console.ResetColor();
                                break;
                            }
                            bool target = false;
                            string sql = $@"
                                   SELECT json_agg(row_to_json(Devices))
                                   FROM
                                   (
                                        SELECT
                                             name AS nameDevice,
                                             serviceLife AS serviceLife,
                                             accuracy AS accuracy,
                                             nameMeteostation AS nameMeteostation,
                                             addressMeteostation AS addressMeteostation
                                        FROM Device
                                            WHERE name='{name}'AND addressMeteostation='{addressMeteostation}'AND nameMeteostation='{nameMeteostation}'
                                   ) AS Devices;
                              ";
                            await UtilsPostgres.ExecuteSelectAsJson(conn, sql, json =>
                            {
                                // преобразование json в список
                                var Devices = JsonConvert.DeserializeObject<List<Device>>(json);

                                // выводим список
                                foreach (var item in Devices)
                                {
                                    Console.WriteLine($"Название: {item.nameDevice}\nСрок годности до: {item.serviceLife}\nТочность: {item.accuracy}\nНазвание Метеостанции: {item.nameMeteostation}\nАдрес метеостанции: {item.addressMeteostation}\n\n");
                                    target = true;
                                }

                            });
                            if (target == false)
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("Таких данных не существует");
                                Console.ResetColor();
                            }
                        }
                        else
                            break;
                    }
                }
                else if (choise == 3)
                {
                    while (true)
                    {
                        Console.WriteLine("Что хотите сделать?\n1. Увидеть данные\n2. Добавить данные\n3. Изменить данные\n4. Удалить данные\n5. Найти данные\n0. Назад\n");
                        int choise2 = Convert.ToInt32(Console.ReadLine());

                        if (choise2 == 1)
                        {
                            string sql = $@"
                                   SELECT json_agg(row_to_json(Measurements))
                                   FROM
                                   (
                                        SELECT
                                             time AS time,
                                             unit AS unit,
                                             result AS result,
                                             nameMeteostation AS nameMeteostation,
                                             addressMeteostation AS addressMeteostation,
                                             nameDevice AS nameDevice
                                        FROM Measurement
                                   ) AS Measurements;
                              ";
                            await UtilsPostgres.ExecuteSelectAsJson(conn, sql, json =>
                            {

                                // показать json
                                //Console.WriteLine(json);

                                // преобразование json в список
                                var Measurements = JsonConvert.DeserializeObject<List<Measurement>>(json);

                                // выводим список
                                foreach (var item in Measurements)
                                {
                                    Console.WriteLine($"Время измерения: {item.time}\nЕдиницы измерения: {item.unit}\nРезультат измерения: {item.result}\nНазвание Метеостанции: {item.nameMeteostation}\nАдрес метеостанции: {item.addressMeteostation}\nНазвание прибора: {item.nameDevice}\n\n");
                                }

                            });

                        }
                        else if (choise2 == 2)
                        {

                            Console.WriteLine("Введите данные\nНазвание прибора: ");
                            string name = Console.ReadLine();
                            Console.WriteLine("Название метеостанции: ");
                            string nameMeteostation = Console.ReadLine();
                            Console.WriteLine("Адрес метеостанции: ");
                            string addressMeteostation = Console.ReadLine();

                            bool target = false;
                            string sql = $@"
                                   SELECT json_agg(row_to_json(Meteostations))
                                   FROM
                                   (
                                        SELECT
                                             name AS MeteostationName,
                                             address AS MeteostationAddress,
                                             seaLEVEL AS seaLEVEL
                                        FROM Meteostation
                                            WHERE name='{nameMeteostation}'AND address='{addressMeteostation}'
                                   ) AS Meteostations;
                              ";
                            await UtilsPostgres.ExecuteSelectAsJson(conn, sql, json =>
                            {
                                // преобразование json в список
                                var Meteostations = JsonConvert.DeserializeObject<List<Meteostation>>(json);

                                // выводим список
                                foreach (var item in Meteostations)
                                {
                                    target = true;
                                }

                            });
                            if (target == false)
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("Такой метеостанции не существует в БД");
                                Console.ResetColor();
                                break;
                            }
                            target = false;
                            sql = $@"
                                   SELECT json_agg(row_to_json(Devices))
                                   FROM
                                   (
                                        SELECT
                                             name AS nameDevice,
                                             serviceLife AS serviceLife,
                                             accuracy AS accuracy,
                                             nameMeteostation AS nameMeteostation,
                                             addressMeteostation AS addressMeteostation
                                        FROM Device
                                            WHERE name='{name}'AND nameMeteostation='{nameMeteostation}'AND addressMeteostation='{addressMeteostation}'
                                   ) AS Devices;
                                   ";
                            await UtilsPostgres.ExecuteSelectAsJson(conn, sql, json =>
                            {
                                // преобразование json в список
                                var Devices = JsonConvert.DeserializeObject<List<Device>>(json);

                                // выводим список
                                foreach (var item in Devices)
                                {
                                    target = true;
                                }

                            });
                            if (target == false)
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("У данной метеостанции нет такого прибора в БД");
                                Console.ResetColor();
                                break;
                            }
                            Console.WriteLine("Время измерения (гггг-мм-дд чч:мм:сс): ");
                            string time = Console.ReadLine();
                            Console.WriteLine("Единицы измерения: ");
                            string unit = Console.ReadLine();
                            Console.WriteLine("Результат измерения: ");
                            string result = Console.ReadLine();

                            if (name == "" || time == "" || unit == "" || result == "" || nameMeteostation == "" || addressMeteostation == "")
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("Ошибка. Некоторые данные пропущены");
                                Console.ResetColor();
                            }
                            else
                            {
                                target = false;
                                sql = $@"
                                   SELECT json_agg(row_to_json(Measurements))
                                   FROM
                                   (
                                        SELECT
                                             time AS time,
                                             unit AS unit,
                                             result AS result,
                                             nameMeteostation AS nameMeteostation,
                                             addressMeteostation AS addressMeteostation,
                                             nameDevice AS nameDevice
                                        FROM Measurement
                                   ) AS Measurements;
                                   ";
                                await UtilsPostgres.ExecuteSelectAsJson(conn, sql, json =>
                                {
                                    // преобразование json в список
                                    var Measurements = JsonConvert.DeserializeObject<List<Measurement>>(json);

                                    // выводим список
                                    foreach (var item in Measurements)
                                    {
                                        if (item.nameDevice == name && item.nameMeteostation == nameMeteostation && item.addressMeteostation == addressMeteostation && item.time==time)
                                        {
                                            Console.ForegroundColor = ConsoleColor.Red;
                                            Console.WriteLine("Ошибка. Такие данные уже существуют");
                                            Console.ResetColor();
                                            target = true;
                                        }
                                    }

                                });
                                NpgsqlCommand insert = new NpgsqlCommand("INSERT INTO Measurement(time, unit, result, nameMeteostation, addressMeteostation, nameDevice) VALUES(@time, @unit, @result, @nameMeteostation, @addressMeteostation, @nameDevice);", conn);
                                if (target == false)
                                {
                                    try { insert.Parameters.AddWithValue("@time", DateTime.Parse(time)); }
                                    catch
                                    {
                                        Console.ForegroundColor = ConsoleColor.Red;
                                        Console.WriteLine("Ошибка при вводе даты и времени.");
                                        Console.ResetColor();
                                    }
                                    insert.Parameters.AddWithValue("@unit", unit);
                                    try { insert.Parameters.AddWithValue("@result", float.Parse(result)); }
                                    catch
                                    {
                                        Console.ForegroundColor = ConsoleColor.Red;
                                        Console.WriteLine("Ошибка при вводе результата измерения.");
                                        Console.ResetColor();
                                    }
                                    insert.Parameters.AddWithValue("@nameMeteostation", nameMeteostation);
                                    insert.Parameters.AddWithValue("@addressMeteostation", addressMeteostation);
                                    insert.Parameters.AddWithValue("@nameDevice", name);
                                    try { insert.ExecuteNonQuery(); }
                                    catch
                                    {
                                        Console.ForegroundColor = ConsoleColor.Red;
                                        Console.WriteLine("Ошибка при вводе данных");
                                        Console.ResetColor();
                                    }
                                }

                            }
                        }
                        else if (choise2 == 3)
                        {
                            Console.WriteLine("Что хотите изменить?\n1. Время измерения\n2. Единицы измерения\n3. Результат измерения\n4. Название Метеостанции\n5. Адрес Метеостанции\n6. Название Прибора\n ");
                            int changeNumber = Convert.ToInt32(Console.ReadLine());
                            string nameTable;
                            if (changeNumber == 1)
                                nameTable = "time";
                            else if (changeNumber == 2)
                                nameTable = "unit";
                            else if (changeNumber == 3)
                                nameTable = "result";
                            else if (changeNumber == 4)
                                nameTable = "nameMeteostation";
                            else if (changeNumber == 5)
                                nameTable = "addressMeteostation";
                            else if (changeNumber == 6)
                                nameTable = "nameDevice";
                            else
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("Ошибка. Вы не ввели, что хотите изменить");
                                Console.ResetColor();
                                break;
                            }
                            Console.WriteLine("На какое значение изменить? ");
                            string change = Console.ReadLine();
                            if (change == "")
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("Ошибка. Вы не ввели, значение, на которое хотите поменять");
                                Console.ResetColor();
                                break;
                            }
                            Console.WriteLine("Введите ключ, по которому произойдёт изменение\nВремя измерения (гггг-мм-дд чч:мм:сс): ");
                            string time = Console.ReadLine();
                            Console.WriteLine("Название метеостанции: ");
                            string nameMeteostation = Console.ReadLine();
                            Console.WriteLine("Адрес метеостанции: "); 
                            string addressMeteostation = Console.ReadLine();
                            Console.WriteLine("Название прибора: ");
                            string name = Console.ReadLine();

                            if (time == "" || nameMeteostation == "" || addressMeteostation == ""|| name == "")
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("Ошибка. Вы не правильно ввели ключ");
                                Console.ResetColor();
                                break;
                            }

                            string res = "'" + change + "'";

                            string sql = "";
                            if (changeNumber == 1)
                                sql = $@"
                                            UPDATE Measurement SET time = {res} WHERE time='{time}' AND nameMeteostation='{nameMeteostation}' AND addressMeteostation='{addressMeteostation}' AND nameDevice='{name}';  
                                            ";
                            else if (changeNumber == 2)
                                sql = $@"
                                            UPDATE Measurement SET unit = {res} WHERE time='{time}' AND nameMeteostation='{nameMeteostation}' AND addressMeteostation='{addressMeteostation}' AND nameDevice='{name}';  
                                            ";
                            else if (changeNumber == 3)
                                sql = $@"
                                            UPDATE Measurement SET result = {res} WHERE time='{time}' AND nameMeteostation='{nameMeteostation}' AND addressMeteostation='{addressMeteostation}' AND nameDevice='{name}';   
                                            ";
                            else if (changeNumber == 4)
                                sql = $@"
                                            ALTER TABLE Device
                                                    DROP CONSTRAINT Meteostation_Device_fkey;
                                            ALTER TABLE Measurement
                                                    DROP CONSTRAINT  Device_Measurement_fkey;
                                            UPDATE Measurement SET nameMeteostation = {res} WHERE time='{time}' AND nameMeteostation='{nameMeteostation}' AND addressMeteostation='{addressMeteostation}' AND nameDevice='{name}';      
                                            ALTER TABLE ONLY Device
                                                ADD CONSTRAINT Meteostation_Device_fkey FOREIGN KEY (nameMeteostation, addressMeteostation) REFERENCES Meteostation (name, address);
                                            ALTER TABLE ONLY Measurement
                                                ADD CONSTRAINT Device_Measurement_fkey FOREIGN KEY (nameMeteostation, addressMeteostation, nameDevice) REFERENCES Device (nameMeteostation, addressMeteostation, name);

                                            ";
                            else if (changeNumber == 5)
                                sql = $@"
                                            ALTER TABLE Device
                                                    DROP CONSTRAINT Meteostation_Device_fkey;
                                            ALTER TABLE Measurement
                                                    DROP CONSTRAINT  Device_Measurement_fkey;
                                            UPDATE Measurement SET addressMeteostation = {res} WHERE time='{time}' AND nameMeteostation='{nameMeteostation}' AND addressMeteostation='{addressMeteostation}' AND nameDevice='{name}';      
                                            ALTER TABLE ONLY Device
                                                ADD CONSTRAINT Meteostation_Device_fkey FOREIGN KEY (nameMeteostation, addressMeteostation) REFERENCES Meteostation (name, address);
                                            ALTER TABLE ONLY Measurement
                                                ADD CONSTRAINT Device_Measurement_fkey FOREIGN KEY (nameMeteostation, addressMeteostation, nameDevice) REFERENCES Device (nameMeteostation, addressMeteostation, name);

                                            ";
                            else if (changeNumber == 6)
                                sql = $@"
                                            ALTER TABLE Device
                                                    DROP CONSTRAINT Meteostation_Device_fkey;
                                            ALTER TABLE Measurement
                                                    DROP CONSTRAINT  Device_Measurement_fkey;
                                            UPDATE Measurement SET nameDevice = {res} WHERE time='{time}' AND nameMeteostation='{nameMeteostation}' AND addressMeteostation='{addressMeteostation}' AND nameDevice='{name}';      
                                            ALTER TABLE ONLY Device
                                                ADD CONSTRAINT Meteostation_Device_fkey FOREIGN KEY (nameMeteostation, addressMeteostation) REFERENCES Meteostation (name, address);
                                            ALTER TABLE ONLY Measurement
                                                ADD CONSTRAINT Device_Measurement_fkey FOREIGN KEY (nameMeteostation, addressMeteostation, nameDevice) REFERENCES Device (nameMeteostation, addressMeteostation, name);

                                            ";
                            NpgsqlCommand update = new NpgsqlCommand(sql, conn);
                            try { update.ExecuteNonQuery(); }
                            catch
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("Ошибка. \n1. Если изменение названия или адреса метеостанции и её прибора, то: Такой метеостанции или прибора нет. Попробуйте удалить данные и ввести новые. Либо изменить название самой метеостанции или прибора\n2. Возможен некорректный ввод резултата измерения, попробуйте ввести через '.' или некорректный ввод другого значения");
                                Console.ResetColor();
                            }
                        }
                        else if (choise2 == 4)
                        {
                            Console.WriteLine("Введите ключ, по которому произойдёт удаление\nВремя измерения (гггг-мм-дд чч:мм:сс): ");
                            string time = Console.ReadLine();
                            Console.WriteLine("Название метеостанции: ");
                            string nameMeteostation = Console.ReadLine();
                            Console.WriteLine("Адрес метеостанции: ");
                            string addressMeteostation = Console.ReadLine();
                            Console.WriteLine("Название прибора: ");
                            string name = Console.ReadLine();

                            if (time == "" || nameMeteostation == "" || addressMeteostation == ""|| name == "")
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("Ошибка. Вы не правильно ввели ключ");
                                Console.ResetColor();
                                break;
                            }
                            string sql = $@"
                                             DELETE FROM Measurement WHERE time='{time}' AND nameMeteostation='{nameMeteostation}' AND addressMeteostation='{addressMeteostation}' AND nameDevice='{name}';
                                            ";
                            NpgsqlCommand delete = new NpgsqlCommand(sql, conn);
                            try { delete.ExecuteNonQuery(); }
                            catch
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("Ошибка. Нет таких данных для удаления");
                                Console.ResetColor();
                            }
                        }
                        else if (choise2 == 5)
                        {
                            Console.WriteLine("Введите ключ данных, которые хотите найти\nВремя измерения (гггг-мм-дд чч:мм:сс): ");
                            string time = Console.ReadLine();
                            Console.WriteLine("Название метеостанции: ");
                            string nameMeteostation = Console.ReadLine();
                            Console.WriteLine("Адрес метеостанции: ");
                            string addressMeteostation = Console.ReadLine();
                            Console.WriteLine("Название прибора: ");
                            string name = Console.ReadLine();

                            if (time == "" || nameMeteostation == "" || addressMeteostation == "" || name == "")
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("Ошибка. Вы не правильно ввели ключ");
                                Console.ResetColor();
                                break;
                            }
                            bool target = false;
                            string sql = $@"
                                   SELECT json_agg(row_to_json(Measurements))
                                   FROM
                                   (
                                        SELECT
                                             time AS time,
                                             unit AS unit,
                                             result AS result,
                                             nameMeteostation AS nameMeteostation,
                                             addressMeteostation AS addressMeteostation,
                                             nameDevice AS nameDevice
                                        FROM Measurement
                                   ) AS Measurements;
                                   ";
                            await UtilsPostgres.ExecuteSelectAsJson(conn, sql, json =>
                            {
                                // преобразование json в список
                                var Measurements = JsonConvert.DeserializeObject<List<Measurement>>(json);

                                // выводим список
                                foreach (var item in Measurements)
                                {
                                    Console.WriteLine($"Время измерения: {item.time}\nЕдиницы измерения: {item.unit}\nРезультат измерения: {item.result}\nНазвание Метеостанции: {item.nameMeteostation}\nАдрес метеостанции: {item.addressMeteostation}\nНазвание прибора: {item.nameDevice}\n\n");
                                    target = true;
                                }

                            });
                            if (target == false)
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("Таких данных не существует");
                                Console.ResetColor();
                            }
                        }
                        else
                            break;
                    }
                }
                else if (choise == 4)
                {
                    while (true)
                    {
                        Console.WriteLine("Что хотите сделать?\n1. Увидеть данные\n2. Добавить данные\n3. Изменить данные\n4. Удалить данные\n5. Найти данные\n0. Назад\n");
                        int choise2 = Convert.ToInt32(Console.ReadLine());

                        if (choise2 == 1)
                        {
                            string sql = $@"
                                   SELECT json_agg(row_to_json(Monitorings))
                                   FROM
                                   (
                                        SELECT
                                             time AS time,
                                             namePerson AS namePerson,
                                             phenomenon AS phenomenon,
                                             nameMeteostation AS nameMeteostation,
                                             addressMeteostation AS addressMeteostation
                                        FROM Monitoring
                                   ) AS Monitorings;
                              ";
                            await UtilsPostgres.ExecuteSelectAsJson(conn, sql, json =>
                            {

                                // показать json
                                //Console.WriteLine(json);

                                // преобразование json в список
                                var Monitorings = JsonConvert.DeserializeObject<List<Monitoring>>(json);

                                // выводим список
                                foreach (var item in Monitorings)
                                {
                                    Console.WriteLine($"Время установления явления: {item.time}\nИмя человека, установившего явление: {item.namePerson}\nЯвление: {item.phenomenon}\nНазвание Метеостанции: {item.nameMeteostation}\nАдрес метеостанции: {item.addressMeteostation}\n\n");
                                }

                            });

                        }
                        else if (choise2 == 2)
                        {

                            Console.WriteLine("Введите данные\nВремя установления явления (гггг-мм-дд чч:мм:сс): ");
                            string time = Console.ReadLine();
                            Console.WriteLine("Название метеостанции: ");
                            string nameMeteostation = Console.ReadLine();
                            Console.WriteLine("Адрес метеостанции: ");
                            string addressMeteostation = Console.ReadLine();

                            bool target = false;
                            string sql = $@"
                                   SELECT json_agg(row_to_json(Meteostations))
                                   FROM
                                   (
                                        SELECT
                                             name AS MeteostationName,
                                             address AS MeteostationAddress,
                                             seaLEVEL AS seaLEVEL
                                        FROM Meteostation
                                            WHERE name='{nameMeteostation}'AND address='{addressMeteostation}'
                                   ) AS Meteostations;
                              ";
                            await UtilsPostgres.ExecuteSelectAsJson(conn, sql, json =>
                            {
                                // преобразование json в список
                                var Meteostations = JsonConvert.DeserializeObject<List<Meteostation>>(json);

                                // выводим список
                                foreach (var item in Meteostations)
                                {
                                    target = true;
                                }

                            });
                            if (target == false)
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("Такой метеостанции не существует в БД");
                                Console.ResetColor();
                                break;
                            }
                            Console.WriteLine("Имя человека, установившего явление: ");
                            string namePerson = Console.ReadLine();
                            Console.WriteLine("Явление: ");
                            string phenomenon = Console.ReadLine();

                            if (time == "" || namePerson == "" || phenomenon == "" || nameMeteostation == "" || addressMeteostation == "")
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("Ошибка. Некоторые данные пропущены");
                                Console.ResetColor();
                            }
                            else
                            {
                                target = false;
                                sql = $@"
                                   SELECT json_agg(row_to_json(Monitorings))
                                   FROM
                                   (
                                        SELECT
                                             time AS time,
                                             namePerson AS namePerson,
                                             phenomenon AS phenomenon,
                                             nameMeteostation AS nameMeteostation,
                                             addressMeteostation AS addressMeteostation
                                        FROM Monitoring
                                            WHERE time='{time}' AND nameMeteostation='{nameMeteostation}' AND addressMeteostation='{addressMeteostation}'
                                   ) AS Monitorings;
                                   ";
                                await UtilsPostgres.ExecuteSelectAsJson(conn, sql, json =>
                                {
                                    // преобразование json в список
                                    var Monitorings = JsonConvert.DeserializeObject<List<Monitoring>>(json);

                                    // выводим список
                                    foreach (var item in Monitorings)
                                    {
                                        if (item.time == time && item.nameMeteostation == nameMeteostation && item.addressMeteostation == addressMeteostation)
                                        {
                                            Console.ForegroundColor = ConsoleColor.Red;
                                            Console.WriteLine("Ошибка. Такие данные уже существуют");
                                            Console.ResetColor();
                                            target = true;
                                        }
                                    }

                                });
                                NpgsqlCommand insert = new NpgsqlCommand("INSERT INTO Monitoring(time, namePerson, phenomenon, nameMeteostation, addressMeteostation) VALUES(@time, @namePerson, @phenomenon, @nameMeteostation, @addressMeteostation);", conn);
                                if (target == false)
                                {
                                    try { insert.Parameters.AddWithValue("@time", DateTime.Parse(time)); }
                                    catch
                                    {
                                        Console.ForegroundColor = ConsoleColor.Red;
                                        Console.WriteLine("Ошибка при вводе даты и времени.");
                                        Console.ResetColor();
                                    }
                                    insert.Parameters.AddWithValue("@namePerson", namePerson); 
                                    insert.Parameters.AddWithValue("@phenomenon", phenomenon); 
                                    insert.Parameters.AddWithValue("@nameMeteostation", nameMeteostation);
                                    insert.Parameters.AddWithValue("@addressMeteostation", addressMeteostation);
                                    try { insert.ExecuteNonQuery(); }
                                    catch
                                    {
                                        Console.ForegroundColor = ConsoleColor.Red;
                                        Console.WriteLine("Ошибка при вводе данных");
                                        Console.ResetColor();
                                    }
                                }

                            }
                        }
                        else if (choise2 == 3)
                        {
                            Console.WriteLine("Что хотите изменить?\n1. Время установления явления\n2. Имя человека, установившего явление\n3. Явление\n4. Название Метеостанции\n5. Адрес Метеостанции\n ");
                            int changeNumber = Convert.ToInt32(Console.ReadLine());
                            string nameTable;
                            if (changeNumber == 1)
                                nameTable = "time";
                            else if (changeNumber == 2)
                                nameTable = "namePerson";
                            else if (changeNumber == 3)
                                nameTable = "phenomenon";
                            else if (changeNumber == 4)
                                nameTable = "nameMeteostation";
                            else if (changeNumber == 5)
                                nameTable = "addressMeteostation";
                            else
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("Ошибка. Вы не ввели, что хотите изменить");
                                Console.ResetColor();
                                break;
                            }
                            Console.WriteLine("На какое значение изменить? ");
                            string change = Console.ReadLine();
                            if (change == "")
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("Ошибка. Вы не ввели, значение, на которое хотите поменять");
                                Console.ResetColor();
                                break;
                            }
                            Console.WriteLine("Введите ключ, по которому произойдёт изменение\nВремя установления явления: ");
                            string time = Console.ReadLine();
                            Console.WriteLine("Название метеостанции: ");
                            string nameMeteostation = Console.ReadLine();
                            Console.WriteLine("Адрес метеостанции: ");
                            string addressMeteostation = Console.ReadLine();
                            if (time == "" || nameMeteostation == "" || addressMeteostation == "")
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("Ошибка. Вы не правильно ввели ключ");
                                Console.ResetColor();
                                break;
                            }

                            string res = "'" + change + "'";

                            string sql = "";
                            if (changeNumber == 1)
                                sql = $@"
                                            ALTER TABLE Monitoring
                                                    DROP CONSTRAINT Meteostation_Monitoring_fkey;
                                            UPDATE Monitoring SET time = {res} WHERE nameMeteostation='{nameMeteostation}' AND addressMeteostation='{addressMeteostation}' AND time='{time}';  
                                            ALTER TABLE ONLY Monitoring
                                                ADD CONSTRAINT Meteostation_Monitoring_fkey FOREIGN KEY (nameMeteostation, addressMeteostation) REFERENCES Meteostation (name, address);
                                            ";
                            else if (changeNumber == 2)
                                sql = $@"
                                            UPDATE Monitoring SET namePerson = {res} WHERE nameMeteostation='{nameMeteostation}' AND addressMeteostation='{addressMeteostation}' AND time='{time}'; 
                                            ";
                            else if (changeNumber == 3)
                                sql = $@"
                                            UPDATE Monitoring SET phenomenon = {res} WHERE nameMeteostation='{nameMeteostation}' AND addressMeteostation='{addressMeteostation}' AND time='{time}'; 
                                            ";
                            else if (changeNumber == 4)
                                sql = $@"
                                            ALTER TABLE Monitoring
                                                    DROP CONSTRAINT Meteostation_Monitoring_fkey;
                                            UPDATE Monitoring SET nameMeteostation = {res} WHERE nameMeteostation='{nameMeteostation}' AND addressMeteostation='{addressMeteostation} AND time='{time}';    
                                            ALTER TABLE ONLY Monitoring
                                                ADD CONSTRAINT Meteostation_Monitoring_fkey FOREIGN KEY (nameMeteostation, addressMeteostation) REFERENCES Meteostation (name, address);
                                           
                                            ";
                            else if (changeNumber == 5)
                                sql = $@"
                                            ALTER TABLE Monitoring
                                                    DROP CONSTRAINT Meteostation_Monitoring_fkey;
                                            UPDATE Monitoring SET addressMeteostation = {res} WHERE nameMeteostation='{nameMeteostation}' AND addressMeteostation='{addressMeteostation} AND time='{time}';    
                                            ALTER TABLE ONLY Monitoring
                                                ADD CONSTRAINT Meteostation_Monitoring_fkey FOREIGN KEY (nameMeteostation, addressMeteostation) REFERENCES Meteostation (name, address);
                                           
                                            ";
                            NpgsqlCommand update = new NpgsqlCommand(sql, conn);
                            try { update.ExecuteNonQuery(); }
                            catch
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("Ошибка. Если изменение названия или адреса метеостанции, то: Такой метеостанции нет. Попробуйте удалить данные и ввести новые. Либо изменить название самой метеостанции");
                                Console.ResetColor();
                            }
                        }
                        else if (choise2 == 4)
                        {
                            Console.WriteLine("Введите ключ, по которому произойдёт удаление\nВремя установления явления: ");
                            string time = Console.ReadLine();
                            Console.WriteLine("Название метеостанции: ");
                            string nameMeteostation = Console.ReadLine();
                            Console.WriteLine("Адрес метеостанции: ");
                            string addressMeteostation = Console.ReadLine();
                            if (time == "" || nameMeteostation == "" || addressMeteostation == "")
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("Ошибка. Вы не правильно ввели ключ");
                                Console.ResetColor();
                                break;
                            }
                            string sql = $@"
                                            ALTER TABLE Monitoring
                                                    DROP CONSTRAINT Meteostation_Monitoring_fkey;
                                            DELETE FROM Monitoring WHERE nameMeteostation='{nameMeteostation}' AND addressMeteostation='{addressMeteostation}' AND time='{time}'; 
                                            ALTER TABLE ONLY Monitoring
                                                ADD CONSTRAINT Meteostation_Monitoring_fkey FOREIGN KEY (nameMeteostation, addressMeteostation) REFERENCES Meteostation (name, address);
                                           
                                            ";
                            NpgsqlCommand delete = new NpgsqlCommand(sql, conn);
                            try { delete.ExecuteNonQuery(); }
                            catch
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("Ошибка. Нет таких данных для удаления");
                                Console.ResetColor();
                            }
                        }
                        else if (choise2 == 5)
                        {
                            Console.WriteLine("Введите ключ данных, которые хотите найти\nВремя установления явления: ");
                            string time = Console.ReadLine();
                            Console.WriteLine("Название метеостанции: ");
                            string nameMeteostation = Console.ReadLine();
                            Console.WriteLine("Адрес метеостанции: ");
                            string addressMeteostation = Console.ReadLine();
                            if (time == "" || nameMeteostation == "" || addressMeteostation == "")
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("Ошибка. Вы не правильно ввели ключ");
                                Console.ResetColor();
                                break;
                            }
                            bool target = false;
                            string sql = $@"
                                   SELECT json_agg(row_to_json(Monitorings))
                                   FROM
                                   (
                                        SELECT
                                             time AS time,
                                             namePerson AS namePerson,
                                             phenomenon AS phenomenon,
                                             nameMeteostation AS nameMeteostation,
                                             addressMeteostation AS addressMeteostation
                                        FROM Monitoring
                                            WHERE time='{time}'AND addressMeteostation='{addressMeteostation}'AND nameMeteostation='{nameMeteostation}'
                                   ) AS Monitorings;
                              ";
                            await UtilsPostgres.ExecuteSelectAsJson(conn, sql, json =>
                            {
                                // преобразование json в список
                                var Monitorings = JsonConvert.DeserializeObject<List<Monitoring>>(json);

                                // выводим список
                                foreach (var item in Monitorings)
                                {
                                    Console.WriteLine($"Время установления явления: {item.time}\nИмя человека, установившего явление: {item.namePerson}\nЯвление: {item.phenomenon}\nНазвание Метеостанции: {item.nameMeteostation}\nАдрес метеостанции: {item.addressMeteostation}\n\n");
                                    target = true;
                                }

                            });
                            if (target == false)
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("Таких данных не существует");
                                Console.ResetColor();
                            }
                        }
                        else
                            break;
                    }
                }
                else break;
            }
        }
    }
}
