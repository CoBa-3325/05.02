/* ============================================================
   Скрипт 2. Тестовые данные
   ============================================================ */

USE user87_db;
GO

INSERT INTO dbo.KURS_Status (Kod_statusa, Status) VALUES
    (1, N'Новая'),
    (2, N'Подтверждена'),
    (3, N'Оплачена'),
    (4, N'Отменена');
GO

INSERT INTO dbo.KURS_Strana (Strana) VALUES
    (N'Россия'), (N'Турция'), (N'Египет'), (N'Франция'), (N'Италия');
GO

INSERT INTO dbo.KURS_Gorod (Kod_strany, Gorod) VALUES
    (1, N'Ярославль'), (1, N'Сочи'), (1, N'Москва'),
    (2, N'Анталия'), (2, N'Стамбул'),
    (3, N'Хургада'),
    (4, N'Париж'),
    (5, N'Рим');
GO

INSERT INTO dbo.KURS_Tur
    (Kod_goroda, Nazvanie, Data_nachala, Data_okonchaniya, Stoimost, Kolichestvo_mest, Foto)
VALUES
    (4, N'Отдых у моря',         '2026-06-01', '2026-06-11',  85000.00, 12, N'tur1.png'),
    (1, N'Золотое кольцо',       '2026-06-15', '2026-06-20',  32000.00,  8, N'tur2.png'),
    (7, N'Париж',                '2026-07-20', '2026-07-27', 110000.00,  5, N'tur3.png'),
    (2, N'Сочи',                 '2026-08-05', '2026-08-12',  48000.00, 20, N'tur4.png'),
    (6, N'Египет',               '2026-09-10', '2026-09-20',  76000.00,  3, N'tur5.png'),
    (8, N'Италия',               '2026-10-12', '2026-10-19',  98000.00,  0, N'tur6.png'),
    (5, N'Стамбул на выходные',  '2026-07-03', '2026-07-07',  41000.00, 10, N'tur7.png'),
    (3, N'Москва экскурсионная', '2026-08-15', '2026-08-18',  27000.00, 15, N'tur8.png');
GO

INSERT INTO dbo.KURS_Klient (FIO, Pasport, Telefon, Email) VALUES
    (N'Иванов Иван Иванович',       N'7800 123456', N'+7 900 100-10-01', N'ivanov@mail.ru'),
    (N'Петрова Анна Сергеевна',     N'7801 234567', N'+7 900 100-10-02', N'petrova@mail.ru'),
    (N'Сидоров Пётр Васильевич',    N'7802 345678', N'+7 900 100-10-03', N'sidorov@mail.ru'),
    (N'Кузнецова Мария Андреевна',  N'7803 456789', N'+7 900 100-10-04', N'kuznetsova@mail.ru'),
    (N'Смирнов Алексей Николаевич', N'7804 567890', N'+7 900 100-10-05', N'smirnov@mail.ru'),
    (N'Волкова Екатерина Игоревна', N'7805 678901', N'+7 900 100-10-06', N'volkova@mail.ru'),
    (N'Морозов Дмитрий Павлович',   N'7806 789012', N'+7 900 100-10-07', N'morozov@mail.ru'),
    (N'Новикова Ольга Дмитриевна',  N'7807 890123', N'+7 900 100-10-08', N'novikova@mail.ru'),
    (N'Фёдоров Сергей Михайлович',  N'7808 901234', N'+7 900 100-10-09', N'fedorov@mail.ru'),
    (N'Егорова Наталья Викторовна', N'7809 012345', N'+7 900 100-10-10', N'egorova@mail.ru');
GO

INSERT INTO dbo.KURS_Sotrudnik (Login, Parol, FIO, Dolzhnost, Telefon, Rol, Kod_klienta) VALUES
    (N'admin', CONVERT(NVARCHAR(64), HASHBYTES('SHA2_256', CAST('12345' AS VARCHAR(20))), 2),
     N'Неделяева Наталья Александровна', N'Руководитель отдела', N'+7 4852 20-10-01', N'Администратор', NULL),
    (N'manager', CONVERT(NVARCHAR(64), HASHBYTES('SHA2_256', CAST('12345' AS VARCHAR(20))), 2),
     N'Матулов Александр Сергеевич', N'Менеджер по туризму', N'+7 4852 20-10-02', N'Менеджер', NULL),
    (N'ivanova', CONVERT(NVARCHAR(64), HASHBYTES('SHA2_256', CAST('12345' AS VARCHAR(20))), 2),
     N'Иванова Светлана Петровна', N'Менеджер по туризму', N'+7 4852 20-10-03', N'Менеджер', NULL),
    (N'client', CONVERT(NVARCHAR(64), HASHBYTES('SHA2_256', CAST('12345' AS VARCHAR(20))), 2),
     N'Иванов Иван Иванович', N'Клиент', N'+7 900 100-10-01', N'Клиент', 1),
    (N'client2', CONVERT(NVARCHAR(64), HASHBYTES('SHA2_256', CAST('12345' AS VARCHAR(20))), 2),
     N'Петрова Анна Сергеевна', N'Клиент', N'+7 900 100-10-02', N'Клиент', 2);
GO

INSERT INTO dbo.KURS_Zayavka
    (Kod_klienta, Kod_tura, Kod_sotrudnika, Kod_statusa, Data_podachi, Summa)
VALUES
    (1, 1, 1, 3, '2026-05-04',  85000.00),
    (2, 1, 1, 2, '2026-05-05',  85000.00),
    (3, 2, 1, 3, '2026-05-06',  32000.00),
    (4, 3, 1, 1, '2026-05-07', 110000.00),
    (5, 4, 2, 2, '2026-05-08',  48000.00),
    (6, 5, 2, 1, '2026-05-11',  76000.00),
    (7, 1, 1, 4, '2026-05-12',  85000.00),
    (8, 2, 2, 3, '2026-05-13',  32000.00),
    (9, 7, 1, 2, '2026-05-14',  41000.00),
    (10, 8, 2, 1, '2026-05-15', 27000.00),
    (2, 3, 1, 3, '2026-05-18', 110000.00),
    (4, 5, 2, 4, '2026-05-19',  76000.00),
    (6, 4, 1, 3, '2026-05-20',  48000.00),
    (1, 7, 2, 1, '2026-05-21',  41000.00),
    (3, 8, 1, 2, '2026-05-22',  27000.00);
GO

PRINT N'Тестовые данные загружены.';
GO

SELECT N'Стран' AS Показатель, COUNT(*) AS Значение FROM dbo.KURS_Strana
UNION ALL SELECT N'Городов', COUNT(*) FROM dbo.KURS_Gorod
UNION ALL SELECT N'Туров', COUNT(*) FROM dbo.KURS_Tur
UNION ALL SELECT N'Клиентов', COUNT(*) FROM dbo.KURS_Klient
UNION ALL SELECT N'Заявок', COUNT(*) FROM dbo.KURS_Zayavka;
GO
