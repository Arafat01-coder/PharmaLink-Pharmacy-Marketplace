-- =============================================================================
--  PharmaLink - Pharmacy Marketplace
--  Complete database setup script for Microsoft SQL Server
--
--  Open this file in SQL Server Management Studio (SSMS) or Azure Data Studio
--  and press Execute.  It creates the database, all ten tables, 10 primary
--  keys, 16 foreign keys, 9 UNIQUE constraints, 20 CHECK constraints, 14
--  supporting indexes and a full set of sample data, so every screen in the
--  application has something to show the moment the script finishes.
--
--  The script is safe to run more than once: it drops the tables in foreign key
--  order before recreating them, which DELETES ALL DATA.  To bring an existing
--  PharmaLinkDB up to this schema without losing data, run
--  PharmaLinkDB_Upgrade.sql instead.
--
--  Schema at a glance (normalised to third normal form):
--      Users        - all three roles in one table, UserType decides the dashboard
--      Categories   - master medicine category list, owned by the Super Admin
--      Pharmacies   - one row per pharmacy owner (OwnerId is UNIQUE => 1:1)
--      Medicines    - the products for sale, owned by exactly one pharmacy
--      Cart         - the customer's live basket
--      Orders       - one row per completed checkout, per pharmacy
--      OrderItems   - the mandatory junction table between Orders and Medicines
--      Reviews      - verified ratings, each pointing back at its order
--      Offers       - time limited percentage discounts on one medicine
--      Prescriptions- uploaded prescription photograph for an Rx order (a
--                     re-upload adds a row; the highest PrescriptionId is current)
-- =============================================================================


-- =============================================================================
--  STEP 1 - CREATE DATABASE
-- =============================================================================

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'PharmaLinkDB')
BEGIN
    CREATE DATABASE PharmaLinkDB;
END
GO

USE PharmaLinkDB;
GO


-- =============================================================================
--  STEP 2 - DROP TABLES (children first, so the foreign keys never complain)
-- =============================================================================

IF OBJECT_ID('dbo.Prescriptions', 'U') IS NOT NULL DROP TABLE dbo.Prescriptions;
IF OBJECT_ID('dbo.Reviews',       'U') IS NOT NULL DROP TABLE dbo.Reviews;
IF OBJECT_ID('dbo.OrderItems',    'U') IS NOT NULL DROP TABLE dbo.OrderItems;
IF OBJECT_ID('dbo.Orders',        'U') IS NOT NULL DROP TABLE dbo.Orders;
IF OBJECT_ID('dbo.Offers',        'U') IS NOT NULL DROP TABLE dbo.Offers;
IF OBJECT_ID('dbo.Cart',          'U') IS NOT NULL DROP TABLE dbo.Cart;
IF OBJECT_ID('dbo.Medicines',     'U') IS NOT NULL DROP TABLE dbo.Medicines;
IF OBJECT_ID('dbo.Pharmacies',    'U') IS NOT NULL DROP TABLE dbo.Pharmacies;
IF OBJECT_ID('dbo.Categories',    'U') IS NOT NULL DROP TABLE dbo.Categories;
IF OBJECT_ID('dbo.Users',         'U') IS NOT NULL DROP TABLE dbo.Users;
GO


-- =============================================================================
--  STEP 3 - CREATE TABLES
-- =============================================================================

-- -----------------------------------------------------------------------------
--  1. Users
--  Every person on the platform lives here, whatever their role.  The login
--  query reads UserType and that single value decides which dashboard opens.
--  Passwords are never stored in plain text: PasswordSalt holds the per user
--  random salt (Base64) and PasswordHash holds
--  'PBKDF2$100000$' + Base64( PBKDF2-HMAC-SHA256(password, salt bytes,
--  100000 iterations, 32 bytes) ).  Older rows may still hold the legacy
--  unprefixed Base64 SHA-256 of (PasswordSalt + password); the application
--  accepts those and re-hashes them at the next successful login.
--  FailedLoginCount and LockoutUntil drive the login lockout: five wrong
--  passwords in a row lock the account for fifteen minutes.
--  There is no email or SMS service, so a forgotten password goes through the
--  Super Admin: PasswordResetRequestedAt records when the "Forgot password?"
--  form matched this account (NULL = no request waiting), and
--  MustChangePassword = 1 means the account is signed in with a temporary
--  password the Super Admin issued, so the next login must choose a new one
--  before any dashboard opens.
-- -----------------------------------------------------------------------------
CREATE TABLE Users (
    UserId          INT             IDENTITY(1,1)   NOT NULL,
    FullName        NVARCHAR(100)                   NOT NULL,
    Email           NVARCHAR(120)                   NOT NULL,
    PasswordHash    NVARCHAR(200)                   NOT NULL,
    PasswordSalt    NVARCHAR(50)                    NOT NULL,
    Phone           NVARCHAR(20)                    NOT NULL,
    Address         NVARCHAR(250)                       NULL,
    UserType        NVARCHAR(15)                    NOT NULL,
    Status          NVARCHAR(15)                    NOT NULL
        CONSTRAINT DF_Users_Status      DEFAULT ('Active'),
    CreatedAt       DATETIME2(0)                    NOT NULL
        CONSTRAINT DF_Users_CreatedAt   DEFAULT (SYSDATETIME()),
    FailedLoginCount INT                            NOT NULL
        CONSTRAINT DF_Users_FailedLogins DEFAULT (0),
    LockoutUntil    DATETIME2(0)                        NULL,
    MustChangePassword BIT                          NOT NULL
        CONSTRAINT DF_Users_MustChangePw DEFAULT (0),
    PasswordResetRequestedAt DATETIME2(0)               NULL,

    CONSTRAINT PK_Users            PRIMARY KEY (UserId),
    CONSTRAINT UQ_Users_Email       UNIQUE (Email),
    CONSTRAINT UQ_Users_Phone       UNIQUE (Phone),
    CONSTRAINT CK_Users_Type        CHECK (UserType IN ('SuperAdmin', 'Admin', 'Customer')),
    CONSTRAINT CK_Users_Status      CHECK (Status   IN ('Pending', 'Active', 'Suspended')),
    CONSTRAINT CK_Users_Email       CHECK (Email LIKE '%_@_%._%')
);
GO


-- -----------------------------------------------------------------------------
--  2. Categories
--  Master list maintained by the Super Admin.  Kept in its own table so a
--  category name is stored once and a rename is a single UPDATE.
-- -----------------------------------------------------------------------------
CREATE TABLE Categories (
    CategoryId      INT             IDENTITY(1,1)   NOT NULL,
    CategoryName    NVARCHAR(60)                    NOT NULL,
    Description     NVARCHAR(200)                       NULL,
    IsActive        BIT                             NOT NULL
        CONSTRAINT DF_Categories_IsActive DEFAULT (1),

    CONSTRAINT PK_Categories        PRIMARY KEY (CategoryId),
    CONSTRAINT UQ_Categories_Name   UNIQUE (CategoryName)
);
GO


-- -----------------------------------------------------------------------------
--  3. Pharmacies
--  One row per pharmacy owner.  OwnerId is UNIQUE, which is what enforces the
--  rule that one owner owns exactly one pharmacy.
--  Status: Pending (waiting for licence check), Approved (trading), Suspended
--  (taken off the platform), Rejected (registration refused; the row is kept
--  so the licence number stays taken).  Customers only ever see medicines of
--  an Approved pharmacy; suspending a shop does not change Medicines.IsActive.
--  WarningMessage / WarnedAt / WarningAcknowledgedAt hold the Super Admin's
--  latest formal warning to the owner (a step short of suspension).  The owner
--  sees it as a banner until WarningAcknowledgedAt is set; all three are NULL
--  for a shop that has never been warned.
-- -----------------------------------------------------------------------------
CREATE TABLE Pharmacies (
    PharmacyId      INT             IDENTITY(1,1)   NOT NULL,
    OwnerId         INT                             NOT NULL,
    PharmacyName    NVARCHAR(120)                   NOT NULL,
    LicenseNo       NVARCHAR(40)                    NOT NULL,
    Area            NVARCHAR(60)                    NOT NULL,
    Address         NVARCHAR(250)                   NOT NULL,
    ContactPhone    NVARCHAR(20)                    NOT NULL,
    LogoPath        NVARCHAR(250)                       NULL,
    CommissionRate  DECIMAL(5,2)                    NOT NULL
        CONSTRAINT DF_Pharmacies_Comm   DEFAULT (8.00),
    Status          NVARCHAR(15)                    NOT NULL
        CONSTRAINT DF_Pharmacies_Status DEFAULT ('Pending'),
    RegisteredAt    DATETIME2(0)                    NOT NULL
        CONSTRAINT DF_Pharmacies_Reg    DEFAULT (SYSDATETIME()),
    WarningMessage  NVARCHAR(500)                       NULL,
    WarnedAt        DATETIME2(0)                        NULL,
    WarningAcknowledgedAt DATETIME2(0)                  NULL,

    CONSTRAINT PK_Pharmacies           PRIMARY KEY (PharmacyId),
    CONSTRAINT UQ_Pharmacies_Owner      UNIQUE (OwnerId),
    CONSTRAINT UQ_Pharmacies_License    UNIQUE (LicenseNo),
    CONSTRAINT FK_Pharmacies_Owner      FOREIGN KEY (OwnerId) REFERENCES Users(UserId),
    CONSTRAINT CK_Pharmacies_Status     CHECK (Status IN ('Pending', 'Approved', 'Suspended', 'Rejected')),
    CONSTRAINT CK_Pharmacies_Comm       CHECK (CommissionRate >= 0 AND CommissionRate <= 30)
);
GO


-- -----------------------------------------------------------------------------
--  4. Medicines
--  The products for sale.  Two pharmacies selling the same brand are two
--  separate rows, each with its own price and its own stock.
--  PharmacyId is the isolation column: every Admin side query filters on it.
-- -----------------------------------------------------------------------------
CREATE TABLE Medicines (
    MedicineId      INT             IDENTITY(1,1)   NOT NULL,
    PharmacyId      INT                             NOT NULL,
    CategoryId      INT                             NOT NULL,
    MedicineName    NVARCHAR(120)                   NOT NULL,
    GenericName     NVARCHAR(120)                   NOT NULL,
    Manufacturer    NVARCHAR(100)                   NOT NULL,
    Strength        NVARCHAR(40)                        NULL,
    UnitPrice       DECIMAL(10,2)                   NOT NULL,
    Stock           INT                             NOT NULL
        CONSTRAINT DF_Medicines_Stock    DEFAULT (0),
    MinStock        INT                             NOT NULL
        CONSTRAINT DF_Medicines_MinStock DEFAULT (10),
    RequiresRx      BIT                             NOT NULL
        CONSTRAINT DF_Medicines_Rx       DEFAULT (0),
    ExpiryDate      DATE                            NOT NULL,
    Description     NVARCHAR(400)                       NULL,
    ImagePath       NVARCHAR(250)                       NULL,
    IsActive        BIT                             NOT NULL
        CONSTRAINT DF_Medicines_IsActive DEFAULT (1),

    CONSTRAINT PK_Medicines             PRIMARY KEY (MedicineId),
    CONSTRAINT FK_Medicines_Pharmacy    FOREIGN KEY (PharmacyId) REFERENCES Pharmacies(PharmacyId),
    CONSTRAINT FK_Medicines_Category    FOREIGN KEY (CategoryId) REFERENCES Categories(CategoryId),
    CONSTRAINT CK_Medicines_Price       CHECK (UnitPrice > 0),
    CONSTRAINT CK_Medicines_Stock       CHECK (Stock    >= 0),
    CONSTRAINT CK_Medicines_MinStock    CHECK (MinStock >= 0),
    CONSTRAINT UQ_Medicines_PerShop     UNIQUE (PharmacyId, MedicineName, Strength)
);
GO


-- -----------------------------------------------------------------------------
--  5. Cart
--  The customer's live basket.  The UNIQUE constraint on (CustomerId,
--  MedicineId) is what makes "add the same medicine twice" an update of the
--  quantity rather than a duplicate line.
-- -----------------------------------------------------------------------------
CREATE TABLE Cart (
    CartId          INT             IDENTITY(1,1)   NOT NULL,
    CustomerId      INT                             NOT NULL,
    MedicineId      INT                             NOT NULL,
    Quantity        INT                             NOT NULL,
    AddedDate       DATETIME2(0)                    NOT NULL
        CONSTRAINT DF_Cart_AddedDate DEFAULT (SYSDATETIME()),

    CONSTRAINT PK_Cart              PRIMARY KEY (CartId),
    CONSTRAINT FK_Cart_Customer     FOREIGN KEY (CustomerId) REFERENCES Users(UserId),
    CONSTRAINT FK_Cart_Medicine     FOREIGN KEY (MedicineId) REFERENCES Medicines(MedicineId),
    CONSTRAINT UQ_Cart_Line         UNIQUE (CustomerId, MedicineId),
    CONSTRAINT CK_Cart_Qty          CHECK (Quantity > 0)
);
GO


-- -----------------------------------------------------------------------------
--  6. Orders
--  One row per completed checkout.  A basket that spans two pharmacies becomes
--  two orders, each with its own delivery charge and its own invoice.
--  CommissionAmount is frozen here at checkout time, so a price change next
--  month can never rewrite what was owed on last month's sales.
-- -----------------------------------------------------------------------------
CREATE TABLE Orders (
    OrderId         INT             IDENTITY(1001,1) NOT NULL,   -- invoices start at 1001
    CustomerId      INT                             NOT NULL,
    PharmacyId      INT                             NOT NULL,
    OrderDate       DATETIME2(0)                    NOT NULL
        CONSTRAINT DF_Orders_Date       DEFAULT (SYSDATETIME()),
    ItemsTotal      DECIMAL(12,2)                   NOT NULL,
    DeliveryCharge  DECIMAL(10,2)                   NOT NULL
        CONSTRAINT DF_Orders_Delivery   DEFAULT (60.00),
    TotalAmount     AS (ItemsTotal + DeliveryCharge) PERSISTED,
    CommissionAmount DECIMAL(12,2)                  NOT NULL,
    DeliveryAddress NVARCHAR(250)                   NOT NULL,
    PaymentMethod   NVARCHAR(20)                    NOT NULL,
    PaymentMobile   NVARCHAR(20)                        NULL,   -- bKash / Nagad wallet number
    Status          NVARCHAR(15)                    NOT NULL
        CONSTRAINT DF_Orders_Status     DEFAULT ('Placed'),

    CONSTRAINT PK_Orders            PRIMARY KEY (OrderId),
    CONSTRAINT FK_Orders_Customer   FOREIGN KEY (CustomerId) REFERENCES Users(UserId),
    CONSTRAINT FK_Orders_Pharmacy   FOREIGN KEY (PharmacyId) REFERENCES Pharmacies(PharmacyId),
    CONSTRAINT CK_Orders_Items      CHECK (ItemsTotal     >= 0),
    CONSTRAINT CK_Orders_Delivery   CHECK (DeliveryCharge >= 0),
    CONSTRAINT CK_Orders_Commission CHECK (CommissionAmount >= 0),
    CONSTRAINT CK_Orders_Payment    CHECK (PaymentMethod IN ('CashOnDelivery', 'bKash', 'Nagad', 'Card')),
    CONSTRAINT CK_Orders_Status     CHECK (Status        IN ('Placed', 'Confirmed', 'Delivered', 'Cancelled'))
);
GO


-- -----------------------------------------------------------------------------
--  7. OrderItems
--  The mandatory junction table.  One order contains many medicines and one
--  medicine appears in many orders; this table resolves that many to many
--  relationship and stores the price on the day of purchase.
-- -----------------------------------------------------------------------------
CREATE TABLE OrderItems (
    OrderItemId     INT             IDENTITY(1,1)   NOT NULL,
    OrderId         INT                             NOT NULL,
    MedicineId      INT                             NOT NULL,
    Quantity        INT                             NOT NULL,
    UnitPrice       DECIMAL(10,2)                   NOT NULL,
    Subtotal        AS (Quantity * UnitPrice) PERSISTED,

    CONSTRAINT PK_OrderItems            PRIMARY KEY (OrderItemId),
    CONSTRAINT FK_OrderItems_Order      FOREIGN KEY (OrderId)    REFERENCES Orders(OrderId) ON DELETE CASCADE,
    CONSTRAINT FK_OrderItems_Medicine   FOREIGN KEY (MedicineId) REFERENCES Medicines(MedicineId),
    CONSTRAINT UQ_OrderItems_Line       UNIQUE (OrderId, MedicineId),
    CONSTRAINT CK_OrderItems_Qty        CHECK (Quantity  > 0),
    CONSTRAINT CK_OrderItems_Price      CHECK (UnitPrice > 0)
);
GO


-- -----------------------------------------------------------------------------
--  8. Reviews
--  A review carries the OrderId that proves the purchase was real, so only a
--  customer who actually received the medicine can rate it, and only once.
--  FK_Reviews_OrderItem points (OrderId, MedicineId) at UQ_OrderItems_Line, so
--  a review can only exist for a medicine that really was a line on that order.
--  Moderation sets IsHidden = 1 instead of deleting, so the audit trail stays;
--  IsReported / ReportReason / ReportedAt record a pharmacy owner flagging a
--  review for the Super Admin to look at.
-- -----------------------------------------------------------------------------
CREATE TABLE Reviews (
    ReviewId        INT             IDENTITY(1,1)   NOT NULL,
    CustomerId      INT                             NOT NULL,
    MedicineId      INT                             NOT NULL,
    OrderId         INT                             NOT NULL,
    Rating          TINYINT                         NOT NULL,
    Comment         NVARCHAR(500)                       NULL,
    ReviewDate      DATETIME2(0)                    NOT NULL
        CONSTRAINT DF_Reviews_Date      DEFAULT (SYSDATETIME()),
    IsHidden        BIT                             NOT NULL
        CONSTRAINT DF_Reviews_Hidden    DEFAULT (0),
    IsReported      BIT                             NOT NULL
        CONSTRAINT DF_Reviews_Reported  DEFAULT (0),
    ReportReason    NVARCHAR(200)                       NULL,
    ReportedAt      DATETIME2(0)                        NULL,

    CONSTRAINT PK_Reviews           PRIMARY KEY (ReviewId),
    CONSTRAINT FK_Reviews_Customer  FOREIGN KEY (CustomerId) REFERENCES Users(UserId),
    CONSTRAINT FK_Reviews_Medicine  FOREIGN KEY (MedicineId) REFERENCES Medicines(MedicineId),
    CONSTRAINT FK_Reviews_Order     FOREIGN KEY (OrderId)    REFERENCES Orders(OrderId),
    CONSTRAINT FK_Reviews_OrderItem FOREIGN KEY (OrderId, MedicineId) REFERENCES OrderItems(OrderId, MedicineId),
    CONSTRAINT UQ_Reviews_OneEach   UNIQUE (CustomerId, MedicineId, OrderId),
    CONSTRAINT CK_Reviews_Rating    CHECK (Rating BETWEEN 1 AND 5)
);
GO


-- -----------------------------------------------------------------------------
--  9. Offers
--  A percentage discount on one medicine, valid between two dates.  The
--  discounted price is always calculated inside the query, so the offers
--  screen, the details screen, the cart and the invoice can never disagree.
-- -----------------------------------------------------------------------------
CREATE TABLE Offers (
    OfferId         INT             IDENTITY(1,1)   NOT NULL,
    MedicineId      INT                             NOT NULL,
    OfferTitle      NVARCHAR(120)                   NOT NULL,
    DiscountPercent DECIMAL(5,2)                    NOT NULL,
    StartDate       DATE                            NOT NULL,
    EndDate         DATE                            NOT NULL,
    IsActive        BIT                             NOT NULL
        CONSTRAINT DF_Offers_IsActive DEFAULT (1),

    CONSTRAINT PK_Offers            PRIMARY KEY (OfferId),
    CONSTRAINT FK_Offers_Medicine   FOREIGN KEY (MedicineId) REFERENCES Medicines(MedicineId),
    CONSTRAINT CK_Offers_Percent    CHECK (DiscountPercent > 0 AND DiscountPercent <= 70),
    CONSTRAINT CK_Offers_Dates      CHECK (EndDate >= StartDate)
);
GO


-- -----------------------------------------------------------------------------
--  10. Prescriptions
--  The photograph of the doctor's prescription that a customer must upload
--  when the basket contains a medicine whose RequiresRx flag is set.  The
--  pharmacy owner approves or rejects it before the order can be confirmed.
--  A rejection records RejectReason; the customer may then upload again, which
--  adds a new row for the same order.  The current prescription for an order
--  is the one with the highest PrescriptionId.
-- -----------------------------------------------------------------------------
CREATE TABLE Prescriptions (
    PrescriptionId  INT             IDENTITY(1,1)   NOT NULL,
    OrderId         INT                             NOT NULL,
    CustomerId      INT                             NOT NULL,
    ImagePath       NVARCHAR(250)                   NOT NULL,
    DoctorName      NVARCHAR(100)                       NULL,
    UploadedAt      DATETIME2(0)                    NOT NULL
        CONSTRAINT DF_Rx_Uploaded DEFAULT (SYSDATETIME()),
    VerifyStatus    NVARCHAR(15)                    NOT NULL
        CONSTRAINT DF_Rx_Status   DEFAULT ('Pending'),
    RejectReason    NVARCHAR(200)                       NULL,

    CONSTRAINT PK_Prescriptions             PRIMARY KEY (PrescriptionId),
    CONSTRAINT FK_Prescriptions_Order       FOREIGN KEY (OrderId)    REFERENCES Orders(OrderId) ON DELETE CASCADE,
    CONSTRAINT FK_Prescriptions_Customer    FOREIGN KEY (CustomerId) REFERENCES Users(UserId),
    CONSTRAINT CK_Prescriptions_Status      CHECK (VerifyStatus IN ('Pending', 'Approved', 'Rejected'))
);
GO


-- =============================================================================
--  STEP 4 - INDEXES
--  The columns the application filters and joins on most often.
--  OrderItems(OrderId) and Cart(CustomerId) get no index of their own: the
--  UNIQUE constraints UQ_OrderItems_Line (OrderId, MedicineId) and
--  UQ_Cart_Line (CustomerId, MedicineId) already build an index that leads
--  with those columns, so a second one would only slow every insert down.
-- =============================================================================

CREATE INDEX IX_Medicines_Pharmacy  ON Medicines(PharmacyId);
CREATE INDEX IX_Medicines_Category  ON Medicines(CategoryId);
CREATE INDEX IX_Medicines_Name      ON Medicines(MedicineName);
CREATE INDEX IX_Medicines_Generic   ON Medicines(GenericName);
CREATE INDEX IX_Orders_Customer     ON Orders(CustomerId);
CREATE INDEX IX_Orders_Pharmacy     ON Orders(PharmacyId);
CREATE INDEX IX_Orders_Date         ON Orders(OrderDate);
CREATE INDEX IX_OrderItems_Medicine ON OrderItems(MedicineId);
CREATE INDEX IX_Reviews_Medicine    ON Reviews(MedicineId);
CREATE INDEX IX_Reviews_Order       ON Reviews(OrderId);
CREATE INDEX IX_Reviews_Customer    ON Reviews(CustomerId);
CREATE INDEX IX_Offers_Medicine     ON Offers(MedicineId);
CREATE INDEX IX_Prescriptions_Order ON Prescriptions(OrderId);
CREATE INDEX IX_Pharmacies_Status   ON Pharmacies(Status);
GO


-- =============================================================================
--  STEP 5 - SAMPLE DATA
--
--  LOGIN DETAILS FOR THE DEMONSTRATION
--  -----------------------------------------------------------------------
--   Role         Email                          Password
--   -----------------------------------------------------------------------
--   SuperAdmin   admin@pharmalink.com.bd        Admin@123
--   Admin        kamrul@mitfordpharma.com       Pharma@123   (Mitford Pharma)
--   Admin        shirin@dhanmondimedico.com     Pharma@123   (Dhanmondi Medico)
--   Admin        tanvir@lazzcare.com            Pharma@123   (Lazz Care Pharmacy)
--   Admin        imran@newlifepharmacy.com      Pharma@123   (New Life - PENDING,
--                                                             cannot log in until
--                                                             the Super Admin
--                                                             approves it)
--   Customer     rahim@gmail.com                Cust@123
--   Customer     nusrat@gmail.com               Cust@123
--   Customer     tanjila@gmail.com              Cust@123
--   Customer     sabbir@gmail.com               Cust@123
--  -----------------------------------------------------------------------
--
--  PasswordHash is 'PBKDF2$100000$' + Base64( PBKDF2-HMAC-SHA256( password,
--  Base64-decoded PasswordSalt, 100000 iterations, 32 bytes ) ), exactly what
--  Helpers/PasswordHelper.cs computes, so these accounts log in straight away.
-- =============================================================================

SET IDENTITY_INSERT Users ON;
INSERT INTO Users (UserId, FullName, Email, PasswordHash, PasswordSalt, Phone, Address, UserType, Status) VALUES
 (1, N'PharmaLink Control',  'admin@pharmalink.com.bd',    'PBKDF2$100000$cKxEgAewBx4bBqid2aYl61lf2Te1h8IT2PhAGBCmCuk=', 'ygDW+CL6333SQXxL', '01700000001', N'Level 7, Bashundhara City, Dhaka', 'SuperAdmin', 'Active'),
 (2, N'Kamrul Hasan',        'kamrul@mitfordpharma.com',   'PBKDF2$100000$EiMEHPzNXaCLO53zYvhgVPSNyZ0ck1atXOO2XL9GoAc=', '3zlk7M4M/hTU/IM6', '01710000002', N'12 Mitford Road, Dhaka 1100',      'Admin',      'Active'),
 (3, N'Shirin Sultana',      'shirin@dhanmondimedico.com', 'PBKDF2$100000$kWeBTJzgeqNVs1YvQkq0xcG1JyytbKPCWOHhZWDoLxs=', 'gqUQMntzJ8KyDM4X', '01710000003', N'House 41, Road 9A, Dhanmondi',     'Admin',      'Active'),
 (4, N'Tanvir Ahmed',        'tanvir@lazzcare.com',        'PBKDF2$100000$F2CBv2tPGKfGSugKrLE+TFbn2F8l20k9hHxcGGjnHP0=', 'QKClFD/GEbF8lDC6', '01710000004', N'Plot 15, Mirpur 10, Dhaka',        'Admin',      'Active'),
 (5, N'Imran Hossain',       'imran@newlifepharmacy.com',  'PBKDF2$100000$VFno581gd6giWGXvnXVXCH72cl8mILUk5va1g6mh1fI=', 'eRZJWQpBPkOvDnsX', '01710000005', N'Sector 7, Uttara, Dhaka',          'Admin',      'Pending'),
 (6, N'Rahim Uddin',         'rahim@gmail.com',            'PBKDF2$100000$HNEapwmLPeYIxutnAWhK6wTe7+eJRQQYQTtFDMOzE6Q=', 'agpAQtVp1Iuum6bS', '01811000006', N'House 7, Road 3, Mirpur 10, Dhaka','Customer',   'Active'),
 (7, N'Nusrat Jahan',        'nusrat@gmail.com',           'PBKDF2$100000$tPEvP7cYzIgxRAy/5PPpLWY7LvqXlM7nL5H6RVzeEVY=', 'R6AFNwtUC78zTQRG', '01811000007', N'Flat 4B, Green Road, Dhanmondi',   'Customer',   'Active'),
 (8, N'Tanjila Akter',       'tanjila@gmail.com',          'PBKDF2$100000$2uGxffKLms5K8zSymEJzam0IOhbylksiKzGYkLRqiqQ=', 'TZuhvWDIC1n+FTey', '01811000008', N'23 Mohammadpur, Dhaka',            'Customer',   'Active'),
 (9, N'Sabbir Ahmed',        'sabbir@gmail.com',           'PBKDF2$100000$eJD0dDxOaujaeyZAid/zbg/1KIIwXMJluTL5qL1SKdM=', 'LSGBFaTSvTls/bZJ', '01811000009', N'Sector 4, Uttara, Dhaka',          'Customer',   'Active');
SET IDENTITY_INSERT Users OFF;
GO


SET IDENTITY_INSERT Categories ON;
INSERT INTO Categories (CategoryId, CategoryName, Description, IsActive) VALUES
 (1, N'Antibiotic',          N'Prescription medicines that treat bacterial infection', 1),
 (2, N'Painkiller',          N'Analgesic, antipyretic and anti-inflammatory medicines', 1),
 (3, N'Diabetes Care',       N'Oral antidiabetics, insulin and glucose monitoring',     1),
 (4, N'Cardiac Care',        N'Blood pressure, cholesterol and heart medicines',        1),
 (5, N'Gastric & Antacid',   N'Proton pump inhibitors, antacids and digestives',        1),
 (6, N'Vitamins & Supplements', N'Multivitamins, minerals and food supplements',        1),
 (7, N'Skin Care',           N'Dermatological creams and ointments',                    1),
 (8, N'Baby Care',           N'Paediatric syrups, drops and baby products',             1),
 (9, N'Devices & Equipment', N'Thermometers, glucometers, nebulisers and BP machines',  1),
 (10,N'Herbal',              N'Discontinued line, kept for old order history',          0);
SET IDENTITY_INSERT Categories OFF;
GO


SET IDENTITY_INSERT Pharmacies ON;
INSERT INTO Pharmacies (PharmacyId, OwnerId, PharmacyName, LicenseNo, Area, Address, ContactPhone, CommissionRate, Status) VALUES
 (1, 2, N'Mitford Pharma',      'DGDA-DH-10021', N'Mitford',   N'Shop 24, Mitford Road, Dhaka 1100',        '02955000021',  8.00, 'Approved'),
 (2, 3, N'Dhanmondi Medico',    'DGDA-DH-10044', N'Dhanmondi', N'House 41, Road 9A, Dhanmondi, Dhaka 1209', '02958000044', 10.00, 'Approved'),
 (3, 4, N'Lazz Care Pharmacy',  'DGDA-DH-10067', N'Mirpur',    N'Plot 15, Mirpur 10 Circle, Dhaka 1216',    '02950000067',  8.00, 'Approved'),
 (4, 5, N'New Life Pharmacy',   'DGDA-DH-10099', N'Uttara',    N'House 9, Sector 7, Uttara, Dhaka 1230',    '02955000099',  8.00, 'Pending');
SET IDENTITY_INSERT Pharmacies OFF;
GO

-- Dhanmondi Medico is the badly reviewed shop (see Reviews below), so the Super
-- Admin has already warned it three days ago and the owner has not yet
-- acknowledged the warning: the owner's dashboard banner shows it at first login.
UPDATE Pharmacies
SET    WarningMessage        = N'Several customers report opened boxes and near-expiry stock. Please check your packing and expiry dates.',
       WarnedAt              = DATEADD(DAY, -3, SYSDATETIME()),
       WarningAcknowledgedAt = NULL
WHERE  PharmacyId = 2;
GO


-- Medicines 1-8 belong to Mitford Pharma, 9-15 to Dhanmondi Medico,
-- 16-22 to Lazz Care Pharmacy and 23-24 to the still Pending New Life Pharmacy,
-- which is why New Life's stock never appears on a customer screen.
SET IDENTITY_INSERT Medicines ON;
INSERT INTO Medicines (MedicineId, PharmacyId, CategoryId, MedicineName, GenericName, Manufacturer, Strength, UnitPrice, Stock, MinStock, RequiresRx, ExpiryDate, Description) VALUES
 (1,  1, 2, N'Napa',        N'Paracetamol',            N'Beximco Pharmaceuticals', N'500mg',  1.20, 900, 100, 0, '2028-03-31', N'Fever and mild to moderate pain'),
 (2,  1, 2, N'Ace Plus',    N'Paracetamol + Caffeine', N'Square Pharmaceuticals',  N'500mg',  2.00, 420,  80, 0, '2027-11-30', N'Paracetamol with caffeine for headache'),
 (3,  1, 5, N'Seclo',       N'Omeprazole',             N'Square Pharmaceuticals',  N'20mg',   7.00, 120,  30, 0, '2027-12-31', N'Proton pump inhibitor for acidity and ulcer'),
 (4,  1, 1, N'Azithrocin',  N'Azithromycin',           N'Square Pharmaceuticals',  N'500mg', 35.00,  18,  40, 1, '2027-06-30', N'Macrolide antibiotic, prescription only'),
 (5,  1, 4, N'Amlovas',     N'Amlodipine',             N'Beximco Pharmaceuticals', N'5mg',    4.50, 260,  60, 1, '2028-01-31', N'Calcium channel blocker for hypertension'),
 (6,  1, 6, N'Vitamin C',   N'Ascorbic Acid',          N'ACME Laboratories',       N'250mg',  1.50, 700, 100, 0, '2028-08-31', N'Daily vitamin C supplement'),
 (7,  1, 8, N'Napa Syrup',  N'Paracetamol',            N'Beximco Pharmaceuticals', N'60ml',  35.00,  95,  25, 0, '2027-09-30', N'Paediatric paracetamol suspension'),
 (8,  1, 9, N'Digital Thermometer', N'Thermometer',    N'Omron',                   N'MC-246',290.00,  12,  15, 0, '2030-12-31', N'Digital clinical thermometer, 10 second read'),

 (9,  2, 2, N'Napa Extra',  N'Paracetamol + Caffeine', N'Beximco Pharmaceuticals', N'500mg',  2.50, 380,  90, 0, '2027-10-31', N'Stronger paracetamol formulation'),
 (10, 2, 1, N'Cef-3',       N'Cefixime',               N'Square Pharmaceuticals',  N'400mg', 60.00,  40,  25, 1, '2027-08-31', N'Third generation cephalosporin, prescription only'),
 (11, 2, 3, N'Comet',       N'Metformin',              N'Square Pharmaceuticals',  N'500mg',  4.00, 500,  80, 1, '2028-04-30', N'First line oral antidiabetic'),
 (12, 2, 5, N'Losectil',    N'Omeprazole',             N'Eskayef Pharmaceuticals', N'20mg',   6.50, 210,  50, 0, '2028-02-28', N'Omeprazole capsule for gastric ulcer'),
 (13, 2, 9, N'Glucometer Kit', N'Glucose Meter',       N'Accu-Chek',               N'Active',1750.00,  8,  10, 0, '2031-01-31', N'Blood glucose monitor with 10 test strips'),
 (14, 2, 7, N'Fungidal HC', N'Miconazole + Hydrocortisone', N'Square Pharmaceuticals', N'15g', 90.00, 60, 20, 0, '2027-07-31', N'Antifungal cream with hydrocortisone'),
 (15, 2, 6, N'Calbo-D',     N'Calcium + Vitamin D3',   N'Renata Limited',          N'500mg',  8.00, 300,  70, 0, '2028-05-31', N'Calcium and vitamin D3 supplement'),

 (16, 3, 2, N'Napa',        N'Paracetamol',            N'Beximco Pharmaceuticals', N'500mg',  1.15, 640, 120, 0, '2028-03-31', N'Fever and mild to moderate pain'),
 (17, 3, 4, N'Atova',       N'Atorvastatin',           N'ACME Laboratories',       N'10mg',   9.00, 180,  50, 1, '2028-06-30', N'Statin for high cholesterol'),
 (18, 3, 3, N'Insulin 30/70', N'Human Insulin',        N'Novo Nordisk',            N'10ml', 480.00,  22,  25, 1, '2027-05-31', N'Premixed human insulin, keep refrigerated'),
 (19, 3, 5, N'Antacid Plus',N'Aluminium Hydroxide',    N'ACME Laboratories',       N'200ml', 95.00, 140,  40, 0, '2027-12-31', N'Antacid suspension for heartburn'),
 (20, 3, 8, N'Baby Zinc',   N'Zinc Sulphate',          N'Renata Limited',          N'20mg',   3.00, 250,  60, 0, '2028-01-31', N'Paediatric zinc for diarrhoea management'),
 (21, 3, 1, N'Moxacil',     N'Amoxicillin',            N'Opsonin Pharma',          N'500mg',  9.50,  15,  35, 1, '2027-04-30', N'Broad spectrum penicillin, prescription only'),
 (22, 3, 6, N'Zinc-B',      N'Zinc + Vitamin B',       N'ACME Laboratories',       N'50mg',   5.00, 410,  80, 0, '2028-09-30', N'Zinc and B complex supplement'),

 (23, 4, 2, N'Fast',        N'Paracetamol',            N'Eskayef Pharmaceuticals', N'500mg',  1.30, 300,  60, 0, '2028-02-28', N'Paracetamol tablet - not visible, pharmacy is Pending'),
 (24, 4, 5, N'Pantonix',    N'Pantoprazole',           N'Incepta Pharmaceuticals', N'20mg',   8.00, 150,  40, 0, '2028-03-31', N'Pantoprazole - not visible, pharmacy is Pending');
SET IDENTITY_INSERT Medicines OFF;
GO


-- -----------------------------------------------------------------------------
--  Orders 1001 to 1007.  Dates are written relative to today so that the
--  dashboards, the date range reports and the offers screen always have
--  something recent to show, whenever the script is run.
--  Orders 1006 and 1007 are the two halves of one basket that spanned two
--  pharmacies and was therefore split into two orders at checkout.
-- -----------------------------------------------------------------------------
INSERT INTO Orders (CustomerId, PharmacyId, OrderDate, ItemsTotal, DeliveryCharge, CommissionAmount, DeliveryAddress, PaymentMethod, Status) VALUES
 (6, 1, DATEADD(DAY, -21, SYSDATETIME()),  182.00, 60.00,  14.56, N'House 7, Road 3, Mirpur 10, Dhaka',  'bKash',          'Delivered'),   -- 1001
 (6, 1, DATEADD(DAY, -17, SYSDATETIME()),   70.00, 60.00,   5.60, N'House 7, Road 3, Mirpur 10, Dhaka',  'CashOnDelivery', 'Delivered'),   -- 1002
 (7, 2, DATEADD(DAY, -14, SYSDATETIME()), 1750.00, 60.00, 175.00, N'Flat 4B, Green Road, Dhanmondi',     'Card',           'Delivered'),   -- 1003
 (8, 2, DATEADD(DAY,  -9, SYSDATETIME()),  190.00, 60.00,  19.00, N'23 Mohammadpur, Dhaka',              'Nagad',          'Delivered'),   -- 1004
 (9, 3, DATEADD(DAY,  -4, SYSDATETIME()),  960.00, 60.00,  76.80, N'Sector 4, Uttara, Dhaka',            'bKash',          'Placed'),      -- 1005 (waits on Rx)
 (6, 1, DATEADD(DAY,  -2, SYSDATETIME()),   72.00, 60.00,   5.76, N'House 7, Road 3, Mirpur 10, Dhaka',  'bKash',          'Confirmed'),   -- 1006
 (6, 3, DATEADD(DAY,  -2, SYSDATETIME()),   57.50, 60.00,   4.60, N'House 7, Road 3, Mirpur 10, Dhaka',  'bKash',          'Confirmed');   -- 1007
GO

INSERT INTO OrderItems (OrderId, MedicineId, Quantity, UnitPrice) VALUES
 (1001, 1,  20,   1.20),
 (1001, 4,   4,  35.00),
 (1001, 6,  12,   1.50),
 (1002, 3,  10,   7.00),
 (1003, 13,  1,1750.00),
 (1004, 14,  1,  90.00),
 (1004, 15, 10,   8.00),
 (1004, 11,  5,   4.00),
 (1005, 18,  2, 480.00),
 (1006, 1,  60,   1.20),
 (1007, 16, 50,   1.15);
GO


-- -----------------------------------------------------------------------------
--  Reviews.  Dhanmondi Medico deliberately collects poor ratings so that the
--  Super Admin's "rated below 2.5" report (HAVING AVG(Rating) < 2.5 AND
--  COUNT(ReviewId) >= 2) returns a row the moment the application starts.
--  Review 6 is a one star insult, which is what the moderation queue is for.
--  Every (OrderId, MedicineId) below is a line in OrderItems above, as
--  FK_Reviews_OrderItem requires.
-- -----------------------------------------------------------------------------
INSERT INTO Reviews (CustomerId, MedicineId, OrderId, Rating, Comment, ReviewDate, IsHidden) VALUES
 (6, 1,  1001, 5, N'Sealed pack, delivered within the hour. Exactly what I needed.', DATEADD(DAY, -20, SYSDATETIME()), 0),
 (6, 4,  1001, 4, N'Genuine strip and the pharmacist checked my prescription properly.', DATEADD(DAY, -19, SYSDATETIME()), 0),
 (6, 3,  1002, 4, N'Good price for Seclo, packaging was fine.',                      DATEADD(DAY, -16, SYSDATETIME()), 0),
 (7, 13, 1003, 2, N'Glucometer box was already opened and two strips were missing.', DATEADD(DAY, -13, SYSDATETIME()), 0),
 (8, 14, 1004, 2, N'Cream expires in four months, they should have told me.',        DATEADD(DAY,  -8, SYSDATETIME()), 0),
 (8, 15, 1004, 1, N'Worst shop in Dhanmondi, absolute cheats, do not order.',        DATEADD(DAY,  -8, SYSDATETIME()), 0),
 (8, 11, 1004, 3, N'Metformin was fine but the delivery took two days.',             DATEADD(DAY,  -7, SYSDATETIME()), 0);
GO


-- -----------------------------------------------------------------------------
--  Offers.  Three are running today and one has already expired, so the
--  customer's Offers screen can prove that expired offers are filtered out by
--  the query rather than by the form.
-- -----------------------------------------------------------------------------
INSERT INTO Offers (MedicineId, OfferTitle, DiscountPercent, StartDate, EndDate, IsActive) VALUES
 (1,  N'Fever Season Pack - 12% off Napa',        12.00, DATEADD(DAY, -10, CAST(SYSDATETIME() AS DATE)), DATEADD(DAY, 20, CAST(SYSDATETIME() AS DATE)), 1),
 (12, N'Gastric Care Week - 15% off Losectil',    15.00, DATEADD(DAY,  -5, CAST(SYSDATETIME() AS DATE)), DATEADD(DAY, 10, CAST(SYSDATETIME() AS DATE)), 1),
 (22, N'Immunity Bundle - 20% off Zinc-B',        20.00, DATEADD(DAY,  -3, CAST(SYSDATETIME() AS DATE)), DATEADD(DAY, 25, CAST(SYSDATETIME() AS DATE)), 1),
 (13, N'Glucometer Clearance - 10% off (expired)',10.00, DATEADD(DAY, -70, CAST(SYSDATETIME() AS DATE)), DATEADD(DAY,-40, CAST(SYSDATETIME() AS DATE)), 1);
GO


-- -----------------------------------------------------------------------------
--  Prescriptions.
--  Order 1001 contained Azithrocin, which is prescription only, so Mitford
--  Pharma approved Rahim's prescription before confirming it - which is what
--  his review of that order says. Without this row the order history would
--  show a delivered Rx order with no prescription on file.
--  One prescription is still waiting in Lazz Care Pharmacy's verification
--  queue: order 1005 contains Insulin, so that order cannot be moved to
--  Confirmed until the row is Approved.
-- -----------------------------------------------------------------------------
INSERT INTO Prescriptions (OrderId, CustomerId, ImagePath, DoctorName, UploadedAt, VerifyStatus) VALUES
 (1001, 6, N'Uploads\Prescriptions\rx-1001-sample.jpg', N'Dr. Farhana Kabir, MBBS, FCPS',     DATEADD(DAY, -21, SYSDATETIME()), 'Approved'),
 (1004, 8, N'Uploads\Prescriptions\rx-1004-sample.jpg', N'Dr. Mahmudul Hasan, MBBS, MD',      DATEADD(DAY,  -9, SYSDATETIME()), 'Approved'),
 (1005, 9, N'Uploads\Prescriptions\rx-1005-sample.jpg', N'Dr. Anisur Rahman, MBBS',           DATEADD(DAY,  -4, SYSDATETIME()), 'Pending');
GO


-- -----------------------------------------------------------------------------
--  Two live cart lines for Rahim Uddin (customer 6), spanning two pharmacies,
--  so the Cart screen demonstrates the "one order per pharmacy" split without
--  anyone having to add items first.
-- -----------------------------------------------------------------------------
INSERT INTO Cart (CustomerId, MedicineId, Quantity) VALUES
 (6, 2,  10),   -- Ace Plus, Mitford Pharma
 (6, 19,  1);   -- Antacid Plus, Lazz Care Pharmacy
GO


-- =============================================================================
--  STEP 6 - VERIFY
--  A quick sanity check so you can see the script did what it promised.
-- =============================================================================

SELECT 'Users' AS TableName, COUNT(*) AS [RowsInserted] FROM Users
UNION ALL SELECT 'Categories',    COUNT(*) FROM Categories
UNION ALL SELECT 'Pharmacies',    COUNT(*) FROM Pharmacies
UNION ALL SELECT 'Medicines',     COUNT(*) FROM Medicines
UNION ALL SELECT 'Cart',          COUNT(*) FROM Cart
UNION ALL SELECT 'Orders',        COUNT(*) FROM Orders
UNION ALL SELECT 'OrderItems',    COUNT(*) FROM OrderItems
UNION ALL SELECT 'Reviews',       COUNT(*) FROM Reviews
UNION ALL SELECT 'Offers',        COUNT(*) FROM Offers
UNION ALL SELECT 'Prescriptions', COUNT(*) FROM Prescriptions;
GO

-- The pharmacies the Super Admin should see on the low rated report.
SELECT  ph.PharmacyName, COUNT(r.ReviewId) AS TotalReviews,
        CAST(AVG(CAST(r.Rating AS DECIMAL(4,2))) AS DECIMAL(4,2)) AS AverageRating
FROM    Pharmacies ph
        INNER JOIN Medicines m ON m.PharmacyId = ph.PharmacyId
        INNER JOIN Reviews   r ON r.MedicineId = m.MedicineId
WHERE   r.IsHidden = 0
GROUP BY ph.PharmacyId, ph.PharmacyName
HAVING  AVG(CAST(r.Rating AS DECIMAL(4,2))) < 2.5
   AND  COUNT(r.ReviewId) >= 2;
GO

PRINT 'PharmaLinkDB is ready. Log in as admin@pharmalink.com.bd / Admin@123';
GO
