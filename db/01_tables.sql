/* ============================================================
   Информационно-справочная система «Туристические агентства»
   База данных: user87_db (сервер stud-mssql.sttec.yar.ru,38325)
   Скрипт 1. Таблицы
   ============================================================ */

USE user87_db;
GO

/* Удаление в порядке, обратном связям */
IF OBJECT_ID('dbo.KURS_Zayavka', 'U') IS NOT NULL DROP TABLE dbo.KURS_Zayavka;
IF OBJECT_ID('dbo.KURS_Status', 'U') IS NOT NULL DROP TABLE dbo.KURS_Status;
IF OBJECT_ID('dbo.KURS_Sotrudnik', 'U') IS NOT NULL DROP TABLE dbo.KURS_Sotrudnik;
IF OBJECT_ID('dbo.KURS_Klient', 'U') IS NOT NULL DROP TABLE dbo.KURS_Klient;
IF OBJECT_ID('dbo.KURS_Tur', 'U') IS NOT NULL DROP TABLE dbo.KURS_Tur;
IF OBJECT_ID('dbo.KURS_Gorod', 'U') IS NOT NULL DROP TABLE dbo.KURS_Gorod;
IF OBJECT_ID('dbo.KURS_Strana', 'U') IS NOT NULL DROP TABLE dbo.KURS_Strana;
GO

CREATE TABLE dbo.KURS_Strana
(
    Kod_strany INT IDENTITY(1,1) NOT NULL,
    Strana     NVARCHAR(60)      NOT NULL,
    CONSTRAINT PK_KURS_Strana PRIMARY KEY (Kod_strany)
);
GO

CREATE TABLE dbo.KURS_Gorod
(
    Kod_goroda INT IDENTITY(1,1) NOT NULL,
    Kod_strany INT               NOT NULL,
    Gorod      NVARCHAR(60)      NOT NULL,
    CONSTRAINT PK_KURS_Gorod PRIMARY KEY (Kod_goroda),
    CONSTRAINT FK_Gorod_Strana FOREIGN KEY (Kod_strany)
        REFERENCES dbo.KURS_Strana (Kod_strany)
);
GO

CREATE TABLE dbo.KURS_Tur
(
    Kod_tura         INT IDENTITY(1,1) NOT NULL,
    Kod_goroda       INT               NOT NULL,
    Nazvanie         NVARCHAR(150)     NOT NULL,
    Data_nachala     DATE              NOT NULL,
    Data_okonchaniya DATE              NOT NULL,
    Stoimost         DECIMAL(10,2)     NOT NULL,
    Kolichestvo_mest INT               NOT NULL,
    Foto             NVARCHAR(100)     NULL,
    CONSTRAINT PK_KURS_Tur PRIMARY KEY (Kod_tura),
    CONSTRAINT FK_Tur_Gorod FOREIGN KEY (Kod_goroda)
        REFERENCES dbo.KURS_Gorod (Kod_goroda),
    CONSTRAINT CK_Tur_Date CHECK (Data_okonchaniya > Data_nachala),
    CONSTRAINT CK_Tur_Stoimost CHECK (Stoimost > 0),
    CONSTRAINT CK_Tur_Mesta CHECK (Kolichestvo_mest >= 0)
);
GO

CREATE TABLE dbo.KURS_Klient
(
    Kod_klienta INT IDENTITY(1,1) NOT NULL,
    FIO         NVARCHAR(120)     NOT NULL,
    Pasport     NVARCHAR(20)      NOT NULL,
    Telefon     NVARCHAR(20)      NULL,
    Email       NVARCHAR(80)      NULL,
    CONSTRAINT PK_KURS_Klient PRIMARY KEY (Kod_klienta)
);
GO

CREATE TABLE dbo.KURS_Sotrudnik
(
    Kod_sotrudnika INT IDENTITY(1,1) NOT NULL,
    Login          NVARCHAR(50)      NOT NULL,
    Parol          NVARCHAR(64)      NOT NULL,
    FIO            NVARCHAR(120)     NOT NULL,
    Dolzhnost      NVARCHAR(60)      NOT NULL,
    Telefon        NVARCHAR(20)      NULL,
    Rol            NVARCHAR(20)      NOT NULL,
    Kod_klienta    INT               NULL,
    CONSTRAINT PK_KURS_Sotrudnik PRIMARY KEY (Kod_sotrudnika),
    CONSTRAINT UQ_Sotrudnik_Login UNIQUE (Login),
    CONSTRAINT CK_Sotrudnik_Rol CHECK (Rol IN (N'Клиент', N'Менеджер', N'Администратор')),
    CONSTRAINT FK_Sotrudnik_Klient FOREIGN KEY (Kod_klienta)
        REFERENCES dbo.KURS_Klient (Kod_klienta)
);
GO

CREATE TABLE dbo.KURS_Status
(
    Kod_statusa INT          NOT NULL,
    Status      NVARCHAR(30) NOT NULL,
    CONSTRAINT PK_KURS_Status PRIMARY KEY (Kod_statusa)
);
GO

CREATE TABLE dbo.KURS_Zayavka
(
    Kod_zayavki    INT IDENTITY(1,1) NOT NULL,
    Kod_klienta    INT               NOT NULL,
    Kod_tura       INT               NOT NULL,
    Kod_sotrudnika INT               NOT NULL,
    Kod_statusa    INT               NOT NULL,
    Data_podachi   DATE              NOT NULL,
    Summa          DECIMAL(10,2)     NOT NULL,
    CONSTRAINT PK_KURS_Zayavka PRIMARY KEY (Kod_zayavki),
    CONSTRAINT FK_Zayavka_Klient FOREIGN KEY (Kod_klienta)
        REFERENCES dbo.KURS_Klient (Kod_klienta),
    CONSTRAINT FK_Zayavka_Tur FOREIGN KEY (Kod_tura)
        REFERENCES dbo.KURS_Tur (Kod_tura),
    CONSTRAINT FK_Zayavka_Sotrudnik FOREIGN KEY (Kod_sotrudnika)
        REFERENCES dbo.KURS_Sotrudnik (Kod_sotrudnika),
    CONSTRAINT FK_Zayavka_Status FOREIGN KEY (Kod_statusa)
        REFERENCES dbo.KURS_Status (Kod_statusa),
    CONSTRAINT UQ_Zayavka_Unique UNIQUE (Kod_klienta, Kod_tura, Data_podachi),
    CONSTRAINT CK_Zayavka_Summa CHECK (Summa >= 0)
);
GO

PRINT N'Таблицы KURS_* созданы в базе user87_db.';
GO
