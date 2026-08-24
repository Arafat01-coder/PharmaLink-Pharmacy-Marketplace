/* =====================================================================================
   PharmaLink  -  Online Pharmacy Marketplace Management System
   Course      : CSC 2210 - Object Oriented Programming 2
   Institution : American International University-Bangladesh (AIUB)
   Engine      : Microsoft SQL Server (SQL Server Express 2019 or newer)
   File        : database/schema.sql
   Contents    : 1. CREATE DATABASE
                 2. CREATE TABLE  (10 tables, normalised to 3NF)
                 3. INSERT sample data (minimum 3 rows per table)
                 4. Feature queries (12 groups, using JOIN / GROUP BY / HAVING / aggregates)
   ===================================================================================== */


/* =====================================================================================
   SECTION 1  -  CREATE DATABASE
   ===================================================================================== */

IF DB_ID('PharmaLinkDB') IS NOT NULL
BEGIN
    ALTER DATABASE PharmaLinkDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE PharmaLinkDB;
END
GO

CREATE DATABASE PharmaLinkDB;
GO

USE PharmaLinkDB;
GO


/* =====================================================================================
   SECTION 2  -  CREATE TABLE
   ---------------------------------------------------------------------------------
   Table order follows foreign key dependency order so the script runs top to bottom
   without error.
   ===================================================================================== */

/* -------------------------------------------------------------------------------------
   2.1  Users
   All three roles live in ONE table. The UserType column is what the login form reads
   to decide which dashboard to open. Status supports the Super Admin approval workflow.
   ------------------------------------------------------------------------------------- */
CREATE TABLE Users (
    UserId        INT           IDENTITY(1,1) NOT NULL,
    FullName      NVARCHAR(100) NOT NULL,
    Email         NVARCHAR(120) NOT NULL,
    PasswordHash  NVARCHAR(200) NOT NULL,
    PasswordSalt  NVARCHAR(50)  NOT NULL,
    Phone         NVARCHAR(20)  NOT NULL,
    Address       NVARCHAR(250) NULL,
    UserType      NVARCHAR(15)  NOT NULL,
    Status        NVARCHAR(15)  NOT NULL CONSTRAINT DF_Users_Status DEFAULT ('Active'),
    CreatedAt     DATETIME2(0)  NOT NULL CONSTRAINT DF_Users_CreatedAt DEFAULT (SYSDATETIME()),

    CONSTRAINT PK_Users        PRIMARY KEY (UserId),
    CONSTRAINT UQ_Users_Email  UNIQUE (Email),
    CONSTRAINT UQ_Users_Phone  UNIQUE (Phone),
    CONSTRAINT CK_Users_Type   CHECK (UserType IN ('SuperAdmin', 'Admin', 'Customer')),
    CONSTRAINT CK_Users_Status CHECK (Status   IN ('Pending', 'Active', 'Suspended')),
    CONSTRAINT CK_Users_Email  CHECK (Email LIKE '%_@_%._%')
);
GO

/* -------------------------------------------------------------------------------------
   2.2  Categories
   Master list of medicine categories, maintained by the Super Admin only.
   Kept in its own table so that a category name is stored once. If the category name
   sat inside Medicines it would depend on a non-key attribute, breaking 3NF.
   ------------------------------------------------------------------------------------- */
CREATE TABLE Categories (
    CategoryId   INT           IDENTITY(1,1) NOT NULL,
    CategoryName NVARCHAR(60)  NOT NULL,
    Description  NVARCHAR(200) NULL,
    IsActive     BIT           NOT NULL CONSTRAINT DF_Categories_IsActive DEFAULT (1),

    CONSTRAINT PK_Categories      PRIMARY KEY (CategoryId),
    CONSTRAINT UQ_Categories_Name UNIQUE (CategoryName)
);
GO

/* -------------------------------------------------------------------------------------
   2.3  Pharmacies
   One row per Admin (pharmacy owner). OwnerId is UNIQUE, which enforces the business
   rule "one pharmacy owner owns exactly one pharmacy" at the database level.
   Every Admin-side query in the application is filtered by this PharmacyId, and that
   is how data isolation between two pharmacy owners is enforced.
   ------------------------------------------------------------------------------------- */
CREATE TABLE Pharmacies (
    PharmacyId       INT            IDENTITY(1,1) NOT NULL,
    OwnerId          INT            NOT NULL,
    PharmacyName     NVARCHAR(120)  NOT NULL,
    LicenseNo        NVARCHAR(40)   NOT NULL,
    Area             NVARCHAR(60)   NOT NULL,
    Address          NVARCHAR(250)  NOT NULL,
    ContactPhone     NVARCHAR(20)   NOT NULL,
    LogoPath         NVARCHAR(250)  NULL,
    CommissionRate   DECIMAL(5,2)   NOT NULL CONSTRAINT DF_Pharmacies_Comm DEFAULT (8.00),
    Status           NVARCHAR(15)   NOT NULL CONSTRAINT DF_Pharmacies_Status DEFAULT ('Pending'),
    RegisteredAt     DATETIME2(0)   NOT NULL CONSTRAINT DF_Pharmacies_Reg DEFAULT (SYSDATETIME()),

    CONSTRAINT PK_Pharmacies         PRIMARY KEY (PharmacyId),
    CONSTRAINT UQ_Pharmacies_Owner   UNIQUE (OwnerId),
    CONSTRAINT UQ_Pharmacies_License UNIQUE (LicenseNo),
    CONSTRAINT FK_Pharmacies_Owner   FOREIGN KEY (OwnerId) REFERENCES Users(UserId),
    CONSTRAINT CK_Pharmacies_Status  CHECK (Status IN ('Pending', 'Approved', 'Suspended')),
    CONSTRAINT CK_Pharmacies_Comm    CHECK (CommissionRate >= 0 AND CommissionRate <= 30)
);
GO

/* -------------------------------------------------------------------------------------
   2.4  Medicines
   The products for sale. Every row belongs to exactly one pharmacy, so two pharmacies
   selling Napa are two separate rows with their own price and stock.
   ------------------------------------------------------------------------------------- */
CREATE TABLE Medicines (
    MedicineId        INT            IDENTITY(1,1) NOT NULL,
    PharmacyId        INT            NOT NULL,
    CategoryId        INT            NOT NULL,
    MedicineName      NVARCHAR(120)  NOT NULL,
    GenericName       NVARCHAR(120)  NOT NULL,
    Manufacturer      NVARCHAR(100)  NOT NULL,
    Strength          NVARCHAR(40)   NULL,
    UnitPrice         DECIMAL(10,2)  NOT NULL,
    Stock             INT            NOT NULL CONSTRAINT DF_Medicines_Stock DEFAULT (0),
    MinStock          INT            NOT NULL CONSTRAINT DF_Medicines_MinStock DEFAULT (10),
    RequiresRx        BIT            NOT NULL CONSTRAINT DF_Medicines_Rx DEFAULT (0),
    ExpiryDate        DATE           NOT NULL,
    Description       NVARCHAR(400)  NULL,
    ImagePath         NVARCHAR(250)  NULL,
    IsActive          BIT            NOT NULL CONSTRAINT DF_Medicines_IsActive DEFAULT (1),

    CONSTRAINT PK_Medicines          PRIMARY KEY (MedicineId),
    CONSTRAINT FK_Medicines_Pharmacy FOREIGN KEY (PharmacyId) REFERENCES Pharmacies(PharmacyId),
    CONSTRAINT FK_Medicines_Category FOREIGN KEY (CategoryId) REFERENCES Categories(CategoryId),
    CONSTRAINT CK_Medicines_Price    CHECK (UnitPrice > 0),
    CONSTRAINT CK_Medicines_Stock    CHECK (Stock >= 0),
    CONSTRAINT CK_Medicines_MinStock CHECK (MinStock >= 0),
    CONSTRAINT UQ_Medicines_PerShop  UNIQUE (PharmacyId, MedicineName, Strength)
);
GO

/* -------------------------------------------------------------------------------------
   2.5  Cart
   The customer's live basket. A customer may hold one line per medicine, which is why
   (CustomerId, MedicineId) is UNIQUE. Adding the same medicine twice updates quantity.
   ------------------------------------------------------------------------------------- */
CREATE TABLE Cart (
    CartId      INT          IDENTITY(1,1) NOT NULL,
    CustomerId  INT          NOT NULL,
    MedicineId  INT          NOT NULL,
    Quantity    INT          NOT NULL,
    AddedDate   DATETIME2(0) NOT NULL CONSTRAINT DF_Cart_AddedDate DEFAULT (SYSDATETIME()),

    CONSTRAINT PK_Cart           PRIMARY KEY (CartId),
    CONSTRAINT FK_Cart_Customer  FOREIGN KEY (CustomerId) REFERENCES Users(UserId),
    CONSTRAINT FK_Cart_Medicine  FOREIGN KEY (MedicineId) REFERENCES Medicines(MedicineId),
    CONSTRAINT UQ_Cart_Line      UNIQUE (CustomerId, MedicineId),
    CONSTRAINT CK_Cart_Qty       CHECK (Quantity > 0)
);
GO

/* -------------------------------------------------------------------------------------
   2.6  Orders
   One row per completed checkout. ItemsTotal and CommissionAmount are stored on purpose:
   they are a legal snapshot of what the customer paid on that day, and must not change
   later if the pharmacy edits the medicine price. TotalAmount is a computed PERSISTED
   column, so the bill total can never disagree with its two parts. The platform takes
   commission on the medicines only, never on the delivery charge.
   ------------------------------------------------------------------------------------- */
CREATE TABLE Orders (
    OrderId          INT            IDENTITY(1001,1) NOT NULL,   -- invoice numbers start at 1001
    CustomerId       INT            NOT NULL,
    PharmacyId       INT            NOT NULL,
    OrderDate        DATETIME2(0)   NOT NULL CONSTRAINT DF_Orders_Date DEFAULT (SYSDATETIME()),
    ItemsTotal       DECIMAL(12,2)  NOT NULL,
    DeliveryCharge   DECIMAL(10,2)  NOT NULL CONSTRAINT DF_Orders_Delivery DEFAULT (60.00),
    TotalAmount      AS (ItemsTotal + DeliveryCharge) PERSISTED,
    CommissionAmount DECIMAL(12,2)  NOT NULL,
    DeliveryAddress  NVARCHAR(250)  NOT NULL,
    PaymentMethod    NVARCHAR(20)   NOT NULL,
    Status           NVARCHAR(15)   NOT NULL CONSTRAINT DF_Orders_Status DEFAULT ('Placed'),

    CONSTRAINT PK_Orders           PRIMARY KEY (OrderId),
    CONSTRAINT FK_Orders_Customer  FOREIGN KEY (CustomerId) REFERENCES Users(UserId),
    CONSTRAINT FK_Orders_Pharmacy  FOREIGN KEY (PharmacyId) REFERENCES Pharmacies(PharmacyId),
    CONSTRAINT CK_Orders_Items     CHECK (ItemsTotal >= 0),
    CONSTRAINT CK_Orders_Delivery  CHECK (DeliveryCharge >= 0),
    CONSTRAINT CK_Orders_Payment   CHECK (PaymentMethod IN ('CashOnDelivery', 'bKash', 'Nagad', 'Card')),
    CONSTRAINT CK_Orders_Status    CHECK (Status IN ('Placed', 'Confirmed', 'Delivered', 'Cancelled'))
);
GO

/* -------------------------------------------------------------------------------------
   2.7  OrderItems   *** MANDATORY JUNCTION TABLE ***
   One order contains many medicines and one medicine appears in many orders. That
   many-to-many relationship is resolved here. Subtotal is a PERSISTED computed column,
   so it is never stored independently of the values it is derived from.
   ------------------------------------------------------------------------------------- */
CREATE TABLE OrderItems (
    OrderItemId  INT           IDENTITY(1,1) NOT NULL,
    OrderId      INT           NOT NULL,
    MedicineId   INT           NOT NULL,
    Quantity     INT           NOT NULL,
    UnitPrice    DECIMAL(10,2) NOT NULL,
    Subtotal     AS (Quantity * UnitPrice) PERSISTED,

    CONSTRAINT PK_OrderItems          PRIMARY KEY (OrderItemId),
    CONSTRAINT FK_OrderItems_Order    FOREIGN KEY (OrderId)    REFERENCES Orders(OrderId) ON DELETE CASCADE,
    CONSTRAINT FK_OrderItems_Medicine FOREIGN KEY (MedicineId) REFERENCES Medicines(MedicineId),
    CONSTRAINT UQ_OrderItems_Line     UNIQUE (OrderId, MedicineId),
    CONSTRAINT CK_OrderItems_Qty      CHECK (Quantity > 0),
    CONSTRAINT CK_OrderItems_Price    CHECK (UnitPrice > 0)
);
GO

/* -------------------------------------------------------------------------------------
   2.8  Reviews
   A customer rates a medicine after buying it. OrderId is carried so the application
   can prove the reviewer actually purchased the item (verified review).
   ------------------------------------------------------------------------------------- */
CREATE TABLE Reviews (
    ReviewId    INT           IDENTITY(1,1) NOT NULL,
    CustomerId  INT           NOT NULL,
    MedicineId  INT           NOT NULL,
    OrderId     INT           NOT NULL,
    Rating      TINYINT       NOT NULL,
    Comment     NVARCHAR(500) NULL,
    ReviewDate  DATETIME2(0)  NOT NULL CONSTRAINT DF_Reviews_Date DEFAULT (SYSDATETIME()),
    IsHidden    BIT           NOT NULL CONSTRAINT DF_Reviews_Hidden DEFAULT (0),

    CONSTRAINT PK_Reviews           PRIMARY KEY (ReviewId),
    CONSTRAINT FK_Reviews_Customer  FOREIGN KEY (CustomerId) REFERENCES Users(UserId),
    CONSTRAINT FK_Reviews_Medicine  FOREIGN KEY (MedicineId) REFERENCES Medicines(MedicineId),
    CONSTRAINT FK_Reviews_Order     FOREIGN KEY (OrderId)    REFERENCES Orders(OrderId),
    CONSTRAINT UQ_Reviews_OneEach   UNIQUE (CustomerId, MedicineId, OrderId),
    CONSTRAINT CK_Reviews_Rating    CHECK (Rating BETWEEN 1 AND 5)
);
GO

/* -------------------------------------------------------------------------------------
   2.9  Offers
   Percentage discount on one medicine, valid between two dates. Created by the Admin.
   ------------------------------------------------------------------------------------- */
CREATE TABLE Offers (
    OfferId         INT           IDENTITY(1,1) NOT NULL,
    MedicineId      INT           NOT NULL,
    OfferTitle      NVARCHAR(120) NOT NULL,
    DiscountPercent DECIMAL(5,2)  NOT NULL,
    StartDate       DATE          NOT NULL,
    EndDate         DATE          NOT NULL,
    IsActive        BIT           NOT NULL CONSTRAINT DF_Offers_IsActive DEFAULT (1),

    CONSTRAINT PK_Offers           PRIMARY KEY (OfferId),
    CONSTRAINT FK_Offers_Medicine  FOREIGN KEY (MedicineId) REFERENCES Medicines(MedicineId),
    CONSTRAINT CK_Offers_Percent   CHECK (DiscountPercent > 0 AND DiscountPercent <= 70),
    CONSTRAINT CK_Offers_Dates     CHECK (EndDate >= StartDate)
);
GO

/* -------------------------------------------------------------------------------------
   2.10 Prescriptions
   When an order contains a medicine with RequiresRx = 1, the customer uploads a
   photograph of the doctor's prescription and the pharmacy verifies it before dispatch.
   ------------------------------------------------------------------------------------- */
CREATE TABLE Prescriptions (
    PrescriptionId INT           IDENTITY(1,1) NOT NULL,
    OrderId        INT           NOT NULL,
    CustomerId     INT           NOT NULL,
    ImagePath      NVARCHAR(250) NOT NULL,
    DoctorName     NVARCHAR(100) NULL,
    UploadedAt     DATETIME2(0)  NOT NULL CONSTRAINT DF_Rx_Uploaded DEFAULT (SYSDATETIME()),
    VerifyStatus   NVARCHAR(15)  NOT NULL CONSTRAINT DF_Rx_Status DEFAULT ('Pending'),

    CONSTRAINT PK_Prescriptions          PRIMARY KEY (PrescriptionId),
    CONSTRAINT FK_Prescriptions_Order    FOREIGN KEY (OrderId)    REFERENCES Orders(OrderId) ON DELETE CASCADE,
    CONSTRAINT FK_Prescriptions_Customer FOREIGN KEY (CustomerId) REFERENCES Users(UserId),
    CONSTRAINT CK_Prescriptions_Status   CHECK (VerifyStatus IN ('Pending', 'Approved', 'Rejected'))
);
GO


/* =====================================================================================
   SECTION 3  -  INSERT SAMPLE DATA
   PasswordHash below holds the placeholder text HASH_<password> so that the demo password is
   readable in the script. The C# application replaces these with a real salted SHA-256 hash the
   first time it runs, and it always hashes the typed password before comparing.
   Demo passwords: Admin@123 (Super Admin), Pharma@123 (owners), Cust@123 (customers).
   ===================================================================================== */

/* 3.1  Users : 1 Super Admin, 4 Admins (pharmacy owners), 4 Customers */
INSERT INTO Users (FullName, Email, PasswordHash, PasswordSalt, Phone, Address, UserType, Status) VALUES
('Nafiul Islam',        'superadmin@pharmalink.com.bd', 'HASH_Admin@123',  'S1A', '01711000001', 'Banani, Dhaka',            'SuperAdmin', 'Active'),
('Mohammad Rafiqul',    'mitford@pharmalink.com.bd',    'HASH_Pharma@123', 'S2A', '01711000002', 'Mitford Road, Dhaka',      'Admin',      'Active'),
('Sultana Razia',       'shahbagh@pharmalink.com.bd',   'HASH_Pharma@123', 'S3A', '01711000003', 'Shahbagh, Dhaka',          'Admin',      'Active'),
('Kazi Nazmul Haque',   'lazz@pharmalink.com.bd',       'HASH_Pharma@123', 'S4A', '01711000004', 'Dhanmondi 27, Dhaka',      'Admin',      'Active'),
('Farhana Yeasmin',     'newlife@pharmalink.com.bd',    'HASH_Pharma@123', 'S5A', '01711000005', 'Agrabad, Chattogram',      'Admin',      'Pending'),
('Rahim Uddin',         'rahim@gmail.com',              'HASH_Cust@123',   'S6A', '01811000006', 'Mirpur 10, Dhaka',         'Customer',   'Active'),
('Karim Sheikh',        'karim@gmail.com',              'HASH_Cust@123',   'S7A', '01811000007', 'Uttara Sector 7, Dhaka',   'Customer',   'Active'),
('Nusrat Jahan',        'nusrat@gmail.com',             'HASH_Cust@123',   'S8A', '01811000008', 'Bashundhara R/A, Dhaka',   'Customer',   'Active'),
('Shakib Al Hasan',     'shakib@gmail.com',             'HASH_Cust@123',   'S9A', '01811000009', 'Khulshi, Chattogram',      'Customer',   'Active');
GO

/* 3.2  Categories */
INSERT INTO Categories (CategoryName, Description) VALUES
('Antibiotic',       'Prescription medicines that treat bacterial infection'),
('Painkiller',       'Analgesic and anti-inflammatory medicines'),
('Vitamin',          'Vitamin and mineral supplements'),
('Diabetes Care',    'Oral antidiabetic medicines and insulin'),
('Baby Care',        'Paediatric syrups, drops and nutrition'),
('Medical Device',   'Thermometers, glucometers, pressure machines'),
('Gastric Care',     'Antacids, proton pump inhibitors and digestive medicines');
GO

/* 3.3  Pharmacies  (OwnerId 2..5 are the Admin users above) */
INSERT INTO Pharmacies (OwnerId, PharmacyName, LicenseNo, Area, Address, ContactPhone, CommissionRate, Status) VALUES
(2, 'Mitford Pharma',        'DGDA-DH-10021', 'Mitford',   '22 Mitford Road, Babubazar, Dhaka 1100',  '02955000021',  8.00, 'Approved'),
(3, 'Shahbagh Medicine Hub', 'DGDA-DH-10044', 'Shahbagh',  '5 Bangabandhu Sheikh Mujib Medical Rd',   '02955000044',  8.00, 'Approved'),
(4, 'Lazz Care Pharmacy',    'DGDA-DH-10077', 'Dhanmondi', 'House 12, Road 27, Dhanmondi, Dhaka',     '02955000077', 10.00, 'Approved'),
(5, 'New Life Pharmacy',     'DGDA-CT-20015', 'Agrabad',   'Agrabad C/A, Chattogram 4100',            '03155000015',  8.00, 'Pending');
GO

/* 3.4  Medicines */
INSERT INTO Medicines (PharmacyId, CategoryId, MedicineName, GenericName, Manufacturer, Strength, UnitPrice, Stock, MinStock, RequiresRx, ExpiryDate, Description) VALUES
(1, 2, 'Napa',        'Paracetamol',        'Beximco Pharmaceuticals', '500mg',  1.20,  850,  100, 0, '2027-06-30', 'Fever and mild to moderate pain relief'),
(1, 1, 'Azithro',     'Azithromycin',       'Square Pharmaceuticals',  '500mg', 35.00,   60,   40, 1, '2027-01-31', 'Broad spectrum antibiotic, full course required'),
(1, 3, 'Cavit-D',     'Calcium + Vit D3',   'Renata Limited',          '500mg',  8.50,  300,   50, 0, '2028-03-31', 'Calcium supplement for bone health'),
(2, 2, 'Ace Plus',    'Paracetamol+Caffeine','Square Pharmaceuticals', '500mg',  2.00,   35,   20, 0, '2027-09-30', 'Stronger relief for headache and migraine'),
(2, 4, 'Comet',       'Metformin HCl',      'ACI Limited',             '500mg',  4.00,  420,   80, 1, '2027-11-30', 'Oral antidiabetic for type 2 diabetes'),
(2, 6, 'Accu-Chek Active','Glucometer Kit', 'Roche Diagnostics',       'Kit',  1850.00,   12,    5, 0, '2029-12-31', 'Blood glucose monitoring kit with 10 strips'),
(3, 1, 'Cef-3',       'Ceftriaxone',        'Incepta Pharmaceuticals', '1g',   120.00,   18,   25, 1, '2026-12-31', 'Injectable antibiotic, hospital use'),
(3, 5, 'Pediamin',    'Multivitamin Syrup', 'Beximco Pharmaceuticals', '100ml', 95.00,  140,   30, 0, '2027-08-31', 'Paediatric multivitamin syrup for children'),
(3, 3, 'Zinc-B',      'Zinc Sulphate',      'Opsonin Pharma',          '20mg',   3.00,   22,   45, 0, '2027-05-31', 'Zinc supplement, supports immunity'),
(3, 2, 'Tufnil',      'Tolfenamic Acid',    'Beximco Pharmaceuticals', '200mg', 12.00,  200,   40, 0, '2027-10-31', 'Migraine and menstrual pain relief');
GO

/* 3.5  Cart  (live baskets, not yet checked out) */
INSERT INTO Cart (CustomerId, MedicineId, Quantity) VALUES
(6,  3, 20),      -- Cavit-D   (Mitford Pharma)
(6,  8,  1),      -- Pediamin  (Lazz Care Pharmacy)
(6,  1, 30),      -- Napa      (Mitford Pharma)
(7,  4, 12),      -- Ace Plus  (Shahbagh Medicine Hub)
(8, 10, 10);      -- Tufnil    (Lazz Care Pharmacy)
GO

/* 3.6  Orders  (commission = TotalAmount * pharmacy CommissionRate) */
/* ItemsTotal = SUM(Quantity * UnitPrice) of the order's OrderItems rows, after any offer discount.
   CommissionAmount = ItemsTotal * the pharmacy's CommissionRate (never the delivery charge) (8 % for pharmacies 1 and 2,
   10 % for pharmacy 3). Delivery is charged to the customer but earns no commission. */
INSERT INTO Orders (CustomerId, PharmacyId, OrderDate, ItemsTotal, DeliveryCharge, CommissionAmount, DeliveryAddress, PaymentMethod, Status) VALUES
(6, 1, '2026-08-02T10:15:00',  211.00, 60.00,  16.88, 'Mirpur 10, Dhaka',       'bKash',          'Delivered'),
(7, 1, '2026-08-05T18:40:00',   72.50, 60.00,   5.80, 'Uttara Sector 7, Dhaka', 'CashOnDelivery', 'Delivered'),
(8, 2, '2026-08-09T12:05:00', 1910.00, 60.00, 152.80, 'Bashundhara R/A, Dhaka', 'Card',           'Delivered'),
(6, 3, '2026-08-12T09:25:00',  285.00, 60.00,  28.50, 'Mirpur 10, Dhaka',       'Nagad',          'Delivered'),
(9, 3, '2026-08-16T16:50:00',  120.00, 60.00,  12.00, 'Khulshi, Chattogram',    'CashOnDelivery', 'Delivered'),
(7, 2, '2026-08-21T11:30:00',   20.00, 60.00,   1.60, 'Uttara Sector 7, Dhaka', 'bKash',          'Placed');
GO

/* 3.7  OrderItems  (junction table) */
INSERT INTO OrderItems (OrderId, MedicineId, Quantity, UnitPrice) VALUES
(1001, 1,  30,   1.20),
(1001, 2,   5,  35.00),
(1002, 1,  25,   1.20),
(1002, 3,   5,   8.50),
(1003, 6,   1,1850.00),
(1003, 4,  20,   2.00),
(1003, 5,   5,   4.00),   -- Comet is RequiresRx = 1, which is why order 1003 carries a prescription
(1004, 8,   3,  95.00),
(1005, 7,   1, 120.00),
(1006, 4,  10,   2.00);
GO

/* 3.8  Reviews */
INSERT INTO Reviews (CustomerId, MedicineId, OrderId, Rating, Comment) VALUES
(6, 1, 1001, 5, 'Delivered within three hours, strip date was fresh.'),
(6, 2, 1001, 4, 'Genuine Square product, price is fair.'),
(7, 1, 1002, 5, 'Cheapest Napa I found online, will order again.'),
(8, 6, 1003, 2, 'Glucometer works but the box was already opened.'),
(6, 8, 1004, 1, 'Syrup arrived leaking and the seal was broken.'),
(9, 7, 1005, 2, 'Delivery took two days for an injection, too slow.');
GO

/* 3.9  Offers */
INSERT INTO Offers (MedicineId, OfferTitle, DiscountPercent, StartDate, EndDate) VALUES
( 1, 'Monsoon Fever Pack - 10% off Napa',   10.00, '2026-08-01', '2026-08-31'),
( 3, 'Bone Health Week - 20% off Cavit-D',  20.00, '2026-08-10', '2026-08-25'),
( 5, 'Diabetes Awareness - 12% off Comet',  12.00, '2026-08-15', '2026-09-15'),
( 6, 'Glucometer Kit Festival Offer',       20.00, '2026-07-01', '2026-07-31');
GO

/* 3.10 Prescriptions */
/* One row per order that contains at least one medicine with RequiresRx = 1:
   order 1001 (Azithro), order 1003 (Comet), order 1005 (Cef-3). */
INSERT INTO Prescriptions (OrderId, CustomerId, ImagePath, DoctorName, VerifyStatus) VALUES
(1001, 6, 'rx/2026/08/rx-1001.jpg', 'Dr. Shirin Akter, MBBS',   'Approved'),
(1003, 8, 'rx/2026/08/rx-1003.jpg', 'Dr. Tanvir Hossain, MBBS', 'Approved'),
(1005, 9, 'rx/2026/08/rx-1005.jpg', 'Dr. Anisur Rahman, MBBS',  'Approved');
GO


/* =====================================================================================
   SECTION 4  -  FEATURE QUERIES
   ---------------------------------------------------------------------------------
   Every query below sits behind a named form and a numbered functional requirement.
   In the C# application each @parameter is supplied through SqlCommand.Parameters and
   nothing is ever concatenated into a query string, which is what keeps the application
   safe from SQL injection. So that this file also runs straight through in SQL Server
   Management Studio, each batch declares and seeds its own parameters first.

   JOIN, GROUP BY, HAVING and the aggregates SUM, AVG and COUNT are each used more
   than once below.
   ===================================================================================== */


/* -------------------------------------------------------------------------------------
   1.  Login and role routing
   Form: LoginForm
  This one query does the whole of authentication. It returns the user id, name, user
  type and, through a LEFT JOIN on Pharmacies, the PharmacyId when the user happens to
  be a pharmacy owner. That PharmacyId is stored in the session and every later Admin
  query filters on it. Because the WHERE clause also demands Status = 'Active', a
  Pending or Suspended account is refused by the same statement that checks the
  password, with no second round trip.
   ------------------------------------------------------------------------------------- */
DECLARE @Email NVARCHAR(120) = 'rahim@gmail.com', @PasswordHash NVARCHAR(200) = 'HASH_Cust@123';

SELECT  u.UserId, u.FullName, u.UserType, u.Status,
        p.PharmacyId                       -- NULL for SuperAdmin and Customer
FROM    Users u
        LEFT JOIN Pharmacies p ON p.OwnerId = u.UserId
WHERE   u.Email        = @Email
  AND   u.PasswordHash = @PasswordHash
  AND   u.Status       = 'Active';
GO


/* -------------------------------------------------------------------------------------
   2.  Filter medicines by price range
   Form: CustomerHomeForm
  The price range ComboBox supplies @MinPrice and @MaxPrice. The join to Pharmacies is
  not decoration: it lets the WHERE clause exclude any pharmacy that is not Approved, so
  a suspended shop's stock disappears from the catalogue without a single row being
  deleted.
   ------------------------------------------------------------------------------------- */
DECLARE @MinPrice DECIMAL(10,2) = 0, @MaxPrice DECIMAL(10,2) = 50;

SELECT  m.MedicineId, m.MedicineName, m.Strength, m.UnitPrice, m.Stock, ph.PharmacyName
FROM    Medicines m
        INNER JOIN Pharmacies ph ON ph.PharmacyId = m.PharmacyId
WHERE   m.IsActive  = 1
  AND   ph.Status   = 'Approved'
  AND   m.UnitPrice BETWEEN @MinPrice AND @MaxPrice
ORDER BY m.UnitPrice ASC;
GO


/* -------------------------------------------------------------------------------------
   3.  Filter by category and availability
   Form: CustomerHomeForm
  Two filters combined in one statement. Stock > 0 backs the In stock only dropdown, and
  the second join brings in the category name so the grid can show it without a second
  query. Results are ordered by area so that customers see nearby pharmacies grouped
  together.
   ------------------------------------------------------------------------------------- */
DECLARE @CategoryId INT = 2;   -- Painkiller

SELECT  m.MedicineId, m.MedicineName, c.CategoryName, m.UnitPrice, m.Stock,
        ph.PharmacyName, ph.Area
FROM    Medicines m
        INNER JOIN Categories  c  ON c.CategoryId  = m.CategoryId
        INNER JOIN Pharmacies  ph ON ph.PharmacyId = m.PharmacyId
WHERE   c.CategoryId = @CategoryId
  AND   m.Stock      > 0
  AND   m.IsActive   = 1
  AND   ph.Status    = 'Approved'
ORDER BY ph.Area, m.MedicineName;
GO


/* -------------------------------------------------------------------------------------
   4.  Keyword search
   Form: shared search box on every dashboard
  The search deliberately covers three columns. A customer who types 'paracetamol' does
  not know that the brand is called Napa, and a customer who types 'Square' wants
  everything that manufacturer makes. Searching the generic name is what makes the
  catalogue useful to someone holding a doctor's chit rather than a box.
   ------------------------------------------------------------------------------------- */
DECLARE @Keyword NVARCHAR(60) = 'paracetamol';

SELECT  m.MedicineId, m.MedicineName, m.GenericName, m.Manufacturer,
        m.UnitPrice, m.Stock, ph.PharmacyName
FROM    Medicines m
        INNER JOIN Pharmacies ph ON ph.PharmacyId = m.PharmacyId
WHERE   m.IsActive = 1
  AND  (m.MedicineName LIKE '%' + @Keyword + '%'
    OR  m.GenericName  LIKE '%' + @Keyword + '%'
    OR  m.Manufacturer LIKE '%' + @Keyword + '%')
ORDER BY m.MedicineName;
GO


/* -------------------------------------------------------------------------------------
   5.  Cart: add, remove and view with total
   Form: MedicineDetailsForm and CartForm
  MERGE handles the add case in one statement: if the customer already has that medicine
  in the basket the quantity is increased, otherwise a new line is inserted. That is
  what keeps the UNIQUE constraint on (CustomerId, MedicineId) from ever being violated.
  The view query is offer aware: OUTER APPLY finds the best discount running today for
  each medicine, so the price the customer sees in the cart is the same price the
  checkout will charge. The second SELECT groups the basket by pharmacy, because a cart
  that spans two pharmacies becomes two separate orders at checkout and each one carries
  its own delivery charge.
   ------------------------------------------------------------------------------------- */
DECLARE @CustomerId INT = 6, @MedicineId INT = 1, @Quantity INT = 30;

-- add an item (insert a new line, or increase the quantity if the line already exists)
MERGE Cart AS target
USING (SELECT @CustomerId AS CustomerId, @MedicineId AS MedicineId, @Quantity AS Quantity) AS source
    ON  target.CustomerId = source.CustomerId
    AND target.MedicineId = source.MedicineId
WHEN MATCHED THEN
    UPDATE SET target.Quantity = target.Quantity + source.Quantity
WHEN NOT MATCHED THEN
    INSERT (CustomerId, MedicineId, Quantity)
    VALUES (source.CustomerId, source.MedicineId, source.Quantity);

-- change the quantity of one line, or remove it
UPDATE Cart SET Quantity = @Quantity WHERE CustomerId = @CustomerId AND MedicineId = @MedicineId;
DELETE FROM Cart            WHERE CustomerId = @CustomerId AND MedicineId = @MedicineId;

-- view the basket, with today's offer applied to each line
SELECT  ph.PharmacyName,
        m.MedicineName,
        ct.Quantity,
        m.UnitPrice                                   AS ListPrice,
        ISNULL(d.Pct, 0)                              AS DiscountPercent,
        CAST(m.UnitPrice * (1 - ISNULL(d.Pct,0)/100.0) AS DECIMAL(10,2)) AS PriceYouPay,
        CAST(ct.Quantity * m.UnitPrice * (1 - ISNULL(d.Pct,0)/100.0) AS DECIMAL(12,2)) AS LineTotal
FROM    Cart ct
        INNER JOIN Medicines  m  ON m.MedicineId  = ct.MedicineId
        INNER JOIN Pharmacies ph ON ph.PharmacyId = m.PharmacyId
        OUTER APPLY (SELECT MAX(ofr.DiscountPercent) AS Pct
                     FROM   Offers ofr
                     WHERE  ofr.MedicineId = m.MedicineId
                       AND  ofr.IsActive   = 1
                       AND  CAST(GETDATE() AS DATE) BETWEEN ofr.StartDate AND ofr.EndDate) d
WHERE   ct.CustomerId = @CustomerId
ORDER BY ph.PharmacyName, m.MedicineName;

-- basket summary per pharmacy: this is how many orders checkout will create
SELECT  ph.PharmacyId,
        ph.PharmacyName,
        COUNT(*)                                                                        AS Lines,
        CAST(SUM(ct.Quantity * m.UnitPrice) AS DECIMAL(12,2))                           AS BeforeDiscount,
        CAST(SUM(ct.Quantity * m.UnitPrice * (1 - ISNULL(d.Pct,0)/100.0)) AS DECIMAL(12,2)) AS ItemsTotal,
        60.00                                                                            AS DeliveryCharge
FROM    Cart ct
        INNER JOIN Medicines  m  ON m.MedicineId  = ct.MedicineId
        INNER JOIN Pharmacies ph ON ph.PharmacyId = m.PharmacyId
        OUTER APPLY (SELECT MAX(ofr.DiscountPercent) AS Pct
                     FROM   Offers ofr
                     WHERE  ofr.MedicineId = m.MedicineId AND ofr.IsActive = 1
                       AND  CAST(GETDATE() AS DATE) BETWEEN ofr.StartDate AND ofr.EndDate) d
WHERE   ct.CustomerId = @CustomerId
GROUP BY ph.PharmacyId, ph.PharmacyName
ORDER BY ph.PharmacyName;
GO


/* -------------------------------------------------------------------------------------
   6.  Checkout: order, line items and stock in one transaction
   Form: CheckoutForm
  This is the most important query group in the system. Note the @PharmacyId filter on
  all four cart statements: the customer's basket may hold medicines from two
  pharmacies, and each pharmacy becomes its own order with its own delivery charge, so
  checkout runs this batch once per pharmacy in the cart. Five things must then happen
  together: the order header is written, one line is written per cart row at the price
  the customer was shown, the stock of every medicine is reduced, that pharmacy's cart
  lines are cleared, and the commission is frozen on the order row. If any one of them
  failed on its own the database would be left with an order that has no items, or stock
  that was sold twice. Wrapping them in a single transaction is what makes the checkout
  safe.
   ------------------------------------------------------------------------------------- */
DECLARE @CustomerId      INT           = 6;      -- from the session
DECLARE @PharmacyId      INT           = 1;      -- checkout runs once per pharmacy in the cart
DECLARE @DeliveryCharge  DECIMAL(10,2) = 60.00;  -- Tk 60 per pharmacy
DECLARE @DeliveryAddress NVARCHAR(250) = 'House 7, Road 3, Mirpur 10, Dhaka';
DECLARE @PaymentMethod   NVARCHAR(20)  = 'bKash';
DECLARE @NewOrderId INT, @Total DECIMAL(12,2), @CommRate DECIMAL(5,2);

BEGIN TRANSACTION;

    -- 1. what this pharmacy's slice of the basket costs, with today's offers applied
    SELECT @Total = CAST(SUM(ct.Quantity * m.UnitPrice * (1 - ISNULL(d.Pct,0)/100.0)) AS DECIMAL(12,2))
    FROM   Cart ct
           INNER JOIN Medicines m ON m.MedicineId = ct.MedicineId
           OUTER APPLY (SELECT MAX(ofr.DiscountPercent) AS Pct FROM Offers ofr
                        WHERE ofr.MedicineId = m.MedicineId AND ofr.IsActive = 1
                          AND CAST(GETDATE() AS DATE) BETWEEN ofr.StartDate AND ofr.EndDate) d
    WHERE  ct.CustomerId = @CustomerId AND m.PharmacyId = @PharmacyId;

    SELECT @CommRate = CommissionRate FROM Pharmacies WHERE PharmacyId = @PharmacyId;

    -- 2. the order header, with the commission frozen at today's rate
    INSERT INTO Orders (CustomerId, PharmacyId, ItemsTotal, DeliveryCharge, CommissionAmount,
                        DeliveryAddress, PaymentMethod, Status)
    VALUES (@CustomerId, @PharmacyId, @Total, @DeliveryCharge,
            CAST(@Total * @CommRate / 100.0 AS DECIMAL(12,2)),
            @DeliveryAddress, @PaymentMethod, 'Placed');

    SET @NewOrderId = SCOPE_IDENTITY();

    -- 3. one line per cart row, at the discounted price the customer actually saw
    INSERT INTO OrderItems (OrderId, MedicineId, Quantity, UnitPrice)
    SELECT @NewOrderId, ct.MedicineId, ct.Quantity,
           CAST(m.UnitPrice * (1 - ISNULL(d.Pct,0)/100.0) AS DECIMAL(10,2))
    FROM   Cart ct
           INNER JOIN Medicines m ON m.MedicineId = ct.MedicineId
           OUTER APPLY (SELECT MAX(ofr.DiscountPercent) AS Pct FROM Offers ofr
                        WHERE ofr.MedicineId = m.MedicineId AND ofr.IsActive = 1
                          AND CAST(GETDATE() AS DATE) BETWEEN ofr.StartDate AND ofr.EndDate) d
    WHERE  ct.CustomerId = @CustomerId AND m.PharmacyId = @PharmacyId;

    -- 4. take the stock off the shelf
    UPDATE m SET m.Stock = m.Stock - ct.Quantity
    FROM   Medicines m INNER JOIN Cart ct ON ct.MedicineId = m.MedicineId
    WHERE  ct.CustomerId = @CustomerId AND m.PharmacyId = @PharmacyId;

    -- 5. clear only this pharmacy's lines; the rest of the basket becomes the next order
    DELETE ct
    FROM   Cart ct INNER JOIN Medicines m ON m.MedicineId = ct.MedicineId
    WHERE  ct.CustomerId = @CustomerId AND m.PharmacyId = @PharmacyId;

COMMIT TRANSACTION;
GO


/* -------------------------------------------------------------------------------------
   7.  Pharmacy earnings (JOIN + GROUP BY + SUM)
   Form: SuperAdminSalesReportForm and AdminEarningsForm
  The Super Admin runs this as it stands to see every pharmacy. The pharmacy owner runs
  the same query with an added WHERE ph.PharmacyId = @PharmacyId and sees only his own
  row, which is the clearest possible demonstration of data isolation: one query, two
  role scopes. Commission is recomputed from the pharmacy rate rather than summed from
  Orders, because the join to OrderItems multiplies the order rows and would inflate a
  plain SUM of the header column.
   ------------------------------------------------------------------------------------- */
SELECT  ph.PharmacyId, ph.PharmacyName, ph.Area,
        COUNT(DISTINCT o.OrderId) AS TotalOrders,
        SUM(oi.Quantity)          AS UnitsSold,
        SUM(oi.Subtotal)          AS GrossSales,
        CAST(SUM(oi.Subtotal) * ph.CommissionRate / 100.0 AS DECIMAL(12,2)) AS PlatformCommission,
        CAST(AVG(oi.UnitPrice) AS DECIMAL(10,2))                            AS AverageItemPrice
FROM    Pharmacies ph
        INNER JOIN Orders     o  ON o.PharmacyId = ph.PharmacyId
        INNER JOIN OrderItems oi ON oi.OrderId   = o.OrderId
WHERE   o.Status <> 'Cancelled'
GROUP BY ph.PharmacyId, ph.PharmacyName, ph.Area, ph.CommissionRate
ORDER BY GrossSales DESC;
GO


/* -------------------------------------------------------------------------------------
   8.  Low stock alert
   Form: AdminInventoryForm
  Comparing two columns of the same row is what makes this alert useful. A fixed
  threshold would be wrong, because ten boxes of a glucometer is plenty while ten strips
  of Napa is nothing. The shortfall column tells the owner how many units to order. The
  WHERE clause carries the PharmacyId, so the alert can never leak another shop's
  inventory.
   ------------------------------------------------------------------------------------- */
DECLARE @PharmacyId INT = 3;   -- Lazz Care Pharmacy, from the session

SELECT  m.MedicineId, m.MedicineName, m.Strength, c.CategoryName,
        m.Stock, m.MinStock, (m.MinStock - m.Stock) AS ShortfallUnits
FROM    Medicines m
        INNER JOIN Categories c ON c.CategoryId = m.CategoryId
WHERE   m.PharmacyId = @PharmacyId          -- data isolation: own pharmacy only
  AND   m.Stock      < m.MinStock
  AND   m.IsActive   = 1
ORDER BY ShortfallUnits DESC;
GO


/* -------------------------------------------------------------------------------------
   9.  Reviews for one medicine (JOIN)
   Form: MedicineDetailsForm
  Reviews store only a CustomerId, so the join to Users is what turns a number into the
  reviewer's name on screen. Hidden reviews are excluded here rather than deleted at
  source, which means Super Admin moderation takes effect immediately on every customer
  screen while the row survives for audit.
   ------------------------------------------------------------------------------------- */
DECLARE @MedicineId INT = 1;   -- Napa 500mg

SELECT  r.ReviewId, u.FullName AS ReviewerName, r.Rating, r.Comment, r.ReviewDate
FROM    Reviews r
        INNER JOIN Users     u ON u.UserId     = r.CustomerId
        INNER JOIN Medicines m ON m.MedicineId = r.MedicineId
WHERE   r.MedicineId = @MedicineId
  AND   r.IsHidden   = 0
ORDER BY r.ReviewDate DESC;
GO


/* -------------------------------------------------------------------------------------
   10.  Pharmacies rated below 2.5 (JOIN + GROUP BY + HAVING + AVG)
   Form: SuperAdminLowRatedShopsForm
  Ratings sit on medicines, not on pharmacies, so the average has to be built by joining
  three tables and grouping back up to the pharmacy. HAVING is the right clause rather
  than WHERE because the condition is on the aggregate itself. The second HAVING
  condition, COUNT(ReviewId) >= 2, is a deliberate fairness rule: one angry customer
  should not be enough to put a shop on the suspension list.
   ------------------------------------------------------------------------------------- */
SELECT  ph.PharmacyId, ph.PharmacyName, ph.Area,
        u.FullName AS OwnerName, u.Phone AS OwnerPhone,
        COUNT(r.ReviewId) AS TotalReviews,
        CAST(AVG(CAST(r.Rating AS DECIMAL(4,2))) AS DECIMAL(4,2)) AS AverageRating
FROM    Pharmacies ph
        INNER JOIN Users     u ON u.UserId     = ph.OwnerId
        INNER JOIN Medicines m ON m.PharmacyId = ph.PharmacyId
        INNER JOIN Reviews   r ON r.MedicineId = m.MedicineId
WHERE   r.IsHidden = 0
GROUP BY ph.PharmacyId, ph.PharmacyName, ph.Area, u.FullName, u.Phone
HAVING  AVG(CAST(r.Rating AS DECIMAL(4,2))) < 2.5
   AND  COUNT(r.ReviewId) >= 2
ORDER BY AverageRating ASC;
GO


/* -------------------------------------------------------------------------------------
   11.  Super Admin: approve and suspend a pharmacy owner
   Form: SuperAdminManageShopsForm
  Approval flips two rows, because the pharmacy record and the login account are
  separate concerns. Suspension flips three: the pharmacy, the account and every
  medicine that pharmacy lists. Nothing is deleted anywhere, so the invoices customers
  already hold and the sales figures in last month's report stay exactly as they were.
   ------------------------------------------------------------------------------------- */
DECLARE @PharmacyId INT = 4, @OwnerId INT = 5;   -- New Life Pharmacy, still Pending

-- approve a pending pharmacy owner
UPDATE Pharmacies SET Status = 'Approved'  WHERE PharmacyId = @PharmacyId;
UPDATE Users      SET Status = 'Active'    WHERE UserId     = @OwnerId;

-- suspend an owner and hide their medicines from customers
UPDATE Pharmacies SET Status   = 'Suspended' WHERE PharmacyId = @PharmacyId;
UPDATE Users      SET Status   = 'Suspended' WHERE UserId     = @OwnerId;
UPDATE Medicines  SET IsActive = 0           WHERE PharmacyId = @PharmacyId;
GO


/* -------------------------------------------------------------------------------------
   12.  Active offers today with the discounted price
   Form: CustomerOffersForm
  The discounted price is calculated in the query rather than in C#, so the same number
  appears on the offers screen, the details screen and the cart without three chances to
  disagree. Filtering on the date range inside the query means an expired offer can
  never be shown by mistake, whatever the form does.
   ------------------------------------------------------------------------------------- */
SELECT  o.OfferId, o.OfferTitle, m.MedicineName, m.Strength, ph.PharmacyName,
        m.UnitPrice AS OriginalPrice, o.DiscountPercent,
        CAST(m.UnitPrice * (1 - o.DiscountPercent / 100.0) AS DECIMAL(10,2)) AS DiscountedPrice,
        o.EndDate
FROM    Offers o
        INNER JOIN Medicines  m  ON m.MedicineId  = o.MedicineId
        INNER JOIN Pharmacies ph ON ph.PharmacyId = m.PharmacyId
WHERE   o.IsActive = 1
  AND   CAST(GETDATE() AS DATE) BETWEEN o.StartDate AND o.EndDate
  AND   m.Stock    > 0
  AND   ph.Status  = 'Approved'
ORDER BY o.DiscountPercent DESC;
GO


/* -------------------------------------------------------------------------------------
   13.  Revenue by area (JOIN + GROUP BY + HAVING + COUNT + SUM)
   Form: SuperAdminDashboard tiles
  An extra reporting query that answers a question the Super Admin actually asks: which
  parts of the city are worth expanding into. It groups delivered orders by pharmacy
  area and uses HAVING to drop areas that have not yet crossed a meaningful revenue
  figure.
   ------------------------------------------------------------------------------------- */
SELECT  ph.Area,
        COUNT(DISTINCT ph.PharmacyId) AS PharmaciesInArea,
        COUNT(DISTINCT o.OrderId)     AS Orders,
        SUM(o.TotalAmount)            AS Revenue,
        SUM(o.CommissionAmount)       AS CommissionEarned
FROM    Pharmacies ph
        INNER JOIN Orders o ON o.PharmacyId = ph.PharmacyId
WHERE   o.Status = 'Delivered'
GROUP BY ph.Area
HAVING  SUM(o.TotalAmount) > 100
ORDER BY Revenue DESC;
GO


/* -------------------------------------------------------------------------------------
   14.  Sign up: register a customer or a pharmacy owner
   Form: SignUpForm
  A customer is created Active and can use the platform immediately. A pharmacy owner is
  created Pending together with a Pending pharmacy row, and neither becomes usable until
  the Super Admin approves it, which is the workflow behind query 11. The UNIQUE
  constraints on Email and Phone are what actually stop a duplicate account, so the form
  checks first only to give a friendly message.
   ------------------------------------------------------------------------------------- */
DECLARE @NewUserId INT;

-- a customer signs up: usable straight away
INSERT INTO Users (FullName, Email, PasswordHash, PasswordSalt, Phone, Address, UserType, Status)
VALUES (N'Tanjila Akter', 'tanjila@gmail.com', 'HASH_Cust@123', 'SA1', '01811000010',
        N'Mohammadpur, Dhaka', 'Customer', 'Active');

-- a pharmacy owner signs up: the account and the pharmacy both start Pending
INSERT INTO Users (FullName, Email, PasswordHash, PasswordSalt, Phone, Address, UserType, Status)
VALUES (N'Imran Hossain', 'popular@pharmalink.com.bd', 'HASH_Pharma@123', 'SA2', '01711000011',
        N'Panthapath, Dhaka', 'Admin', 'Pending');
SET @NewUserId = SCOPE_IDENTITY();

INSERT INTO Pharmacies (OwnerId, PharmacyName, LicenseNo, Area, Address, ContactPhone, Status)
VALUES (@NewUserId, N'Popular Pharmacy', 'DGDA-DH-10099', N'Panthapath',
        N'House 9, Panthapath, Dhaka 1205', '02955000099', 'Pending');
GO


/* -------------------------------------------------------------------------------------
   15.  Medicine CRUD for the logged in pharmacy
   Form: AdminMedicineForm and the Add / Edit Medicine dialog
  The statements behind the Add, Update, Delete and Create Offer buttons. Every one of
  them carries PharmacyId, so an Admin cannot create a medicine under someone else's
  shop, and cannot update or delete a row that is not his. Delete is a soft delete:
  setting IsActive to 0 keeps the foreign keys from OrderItems intact, so old invoices
  still resolve.
   ------------------------------------------------------------------------------------- */
DECLARE @PharmacyId INT = 1, @MedicineId INT = 3;

-- CREATE
INSERT INTO Medicines (PharmacyId, CategoryId, MedicineName, GenericName, Manufacturer,
                       Strength, UnitPrice, Stock, MinStock, RequiresRx, ExpiryDate, Description)
VALUES (@PharmacyId, 7, N'Seclo', N'Omeprazole', N'Square Pharmaceuticals',
        N'20mg', 7.00, 120, 30, 1, '2027-12-31', N'Proton pump inhibitor for acidity and ulcer');

-- READ (the DataGridView)
SELECT  m.MedicineId, m.MedicineName, m.GenericName, c.CategoryName, m.Manufacturer,
        m.Strength, m.UnitPrice, m.Stock, m.MinStock, m.RequiresRx, m.ExpiryDate
FROM    Medicines m INNER JOIN Categories c ON c.CategoryId = m.CategoryId
WHERE   m.PharmacyId = @PharmacyId AND m.IsActive = 1
ORDER BY m.MedicineName;

-- UPDATE
UPDATE Medicines
SET    UnitPrice = 9.00, Stock = 320, MinStock = 60, ExpiryDate = '2028-06-30'
WHERE  MedicineId = @MedicineId AND PharmacyId = @PharmacyId;

-- DELETE (soft, so OrderItems rows stay valid)
UPDATE Medicines SET IsActive = 0
WHERE  MedicineId = @MedicineId AND PharmacyId = @PharmacyId;

-- create and pause a discount offer on one of my own medicines  (requirement 14)
INSERT INTO Offers (MedicineId, OfferTitle, DiscountPercent, StartDate, EndDate)
SELECT m.MedicineId, N'Winter Cold Pack - 12% off', 12.00, '2026-11-01', '2026-11-30'
FROM   Medicines m
WHERE  m.MedicineId = @MedicineId AND m.PharmacyId = @PharmacyId;  -- own medicines only

UPDATE ofr SET ofr.IsActive = 0
FROM   Offers ofr INNER JOIN Medicines m ON m.MedicineId = ofr.MedicineId
WHERE  ofr.OfferId = 4 AND m.PharmacyId = @PharmacyId;
GO


/* -------------------------------------------------------------------------------------
   16.  Super Admin: user list, category CRUD and commission rate
   Form: SuperAdminManageUsersForm, ManageCategoriesForm
  The small administrative statements that the requirements list but that have no report
  of their own. The user list joins Users to Pharmacies so that a pharmacy owner's shop
  name appears beside his name, and it accepts a keyword and a status from the search
  box and the Status ComboBox. Categories are deactivated rather than deleted, because
  Medicines rows point at them.
   ------------------------------------------------------------------------------------- */
DECLARE @Keyword NVARCHAR(60) = '', @Status NVARCHAR(15) = '', @CategoryId INT = 6, @PharmacyId INT = 1;

-- all users, with the pharmacy name for owners  (requirement 4)
SELECT  u.UserId, u.FullName, u.Email, u.Phone, u.UserType, u.Status,
        ph.PharmacyName, u.CreatedAt
FROM    Users u
        LEFT JOIN Pharmacies ph ON ph.OwnerId = u.UserId
WHERE  (@Keyword = '' OR u.FullName LIKE '%' + @Keyword + '%' OR u.Email LIKE '%' + @Keyword + '%')
  AND  (@Status  = '' OR u.Status = @Status)
ORDER BY u.UserType, u.FullName;

-- category master list  (requirement 7)
INSERT INTO Categories (CategoryName, Description) VALUES (N'Skin Care', N'Dermatological creams and ointments');
UPDATE Categories SET Description = N'Analgesic, antipyretic and anti-inflammatory medicines' WHERE CategoryId = 2;
UPDATE Categories SET IsActive = 0 WHERE CategoryId = @CategoryId;   -- deactivate, never delete

-- set one pharmacy's commission rate  (requirement 9)
UPDATE Pharmacies SET CommissionRate = 9.50 WHERE PharmacyId = @PharmacyId;

-- the moderation queue, and hiding an abusive review  (requirement 8)
SELECT  r.ReviewId, u.FullName AS Reviewer, m.MedicineName, ph.PharmacyName,
        r.Rating, r.Comment, r.ReviewDate, r.OrderId
FROM    Reviews r
        INNER JOIN Users      u  ON u.UserId      = r.CustomerId
        INNER JOIN Medicines  m  ON m.MedicineId  = r.MedicineId
        INNER JOIN Pharmacies ph ON ph.PharmacyId = m.PharmacyId
WHERE   r.Rating <= 2 AND r.IsHidden = 0
ORDER BY r.ReviewDate DESC;

UPDATE Reviews SET IsHidden = 1 WHERE ReviewId = 5;   -- hidden, never deleted
GO


/* -------------------------------------------------------------------------------------
   17.  Customer: place a review, and read the order history
   Form: GiveRatingForm and OrderHistoryForm
  The review INSERT is guarded twice. The WHERE EXISTS clause proves the customer really
  bought that medicine on that order, and the UNIQUE constraint on (CustomerId,
  MedicineId, OrderId) stops the same purchase being rated twice, so a duplicate attempt
  fails at the database even if the form is bypassed. The order history query below it
  is what fills the customer's Orders grid, including the invoice total from the
  computed TotalAmount column.
   ------------------------------------------------------------------------------------- */
DECLARE @CustomerId INT = 6, @MedicineId INT = 3, @OrderId INT = 1002, @Rating TINYINT = 4;

-- write a review, but only for a delivered order that actually contained the medicine
INSERT INTO Reviews (CustomerId, MedicineId, OrderId, Rating, Comment)
SELECT @CustomerId, @MedicineId, @OrderId, @Rating, N'Sealed pack, delivered on time.'
WHERE EXISTS (SELECT 1
              FROM   OrderItems oi INNER JOIN Orders o ON o.OrderId = oi.OrderId
              WHERE  oi.OrderId    = @OrderId
                AND  oi.MedicineId = @MedicineId
                AND  o.CustomerId  = @CustomerId
                AND  o.Status      = 'Delivered');

-- order history with item count and the reviewable flag  (requirement 27)
SELECT  o.OrderId, o.OrderDate, ph.PharmacyName,
        COUNT(oi.OrderItemId)   AS Items,
        o.ItemsTotal, o.DeliveryCharge, o.TotalAmount,
        o.PaymentMethod, o.Status,
        CASE WHEN o.Status = 'Delivered'
              AND EXISTS (SELECT 1 FROM OrderItems x
                          WHERE x.OrderId = o.OrderId
                            AND NOT EXISTS (SELECT 1 FROM Reviews r
                                            WHERE r.OrderId = o.OrderId AND r.MedicineId = x.MedicineId))
             THEN 1 ELSE 0 END  AS CanReview
FROM    Orders o
        INNER JOIN Pharmacies ph ON ph.PharmacyId = o.PharmacyId
        INNER JOIN OrderItems oi ON oi.OrderId    = o.OrderId
WHERE   o.CustomerId = @CustomerId
GROUP BY o.OrderId, o.OrderDate, ph.PharmacyName, o.ItemsTotal, o.DeliveryCharge,
         o.TotalAmount, o.PaymentMethod, o.Status
ORDER BY o.OrderDate DESC;
GO


/* -------------------------------------------------------------------------------------
   18.  Profile update, password change and prescription verification
   Form: MyProfileForm and the Verify Prescription dialog
  The password change verifies the current hash inside the same UPDATE, so a wrong
  current password simply updates no rows and the form reports failure without ever
  having read the stored hash into memory. The prescription statements are the Admin
  side of the same story: an order that still has a Pending prescription cannot be moved
  to Confirmed, which the last statement enforces with a NOT EXISTS clause rather than
  trusting the form.
   ------------------------------------------------------------------------------------- */
DECLARE @UserId INT = 6, @OldHash NVARCHAR(200) = 'HASH_Cust@123', @NewHash NVARCHAR(200) = 'HASH_Cust@456';
DECLARE @PrescriptionId INT = 3, @OrderId INT = 1005, @PharmacyId INT = 3;

-- update own profile  (requirements 17, 30)
UPDATE Users
SET    FullName = N'Rahim Uddin', Phone = '01811000006', Address = N'House 7, Road 3, Mirpur 10, Dhaka'
WHERE  UserId = @UserId;

-- change own password: succeeds only if the current password hash matches
UPDATE Users
SET    PasswordHash = @NewHash, PasswordSalt = 'NEWSALT'
WHERE  UserId = @UserId AND PasswordHash = @OldHash;

-- the customer uploads a prescription during checkout  (requirement 26)
INSERT INTO Prescriptions (OrderId, CustomerId, ImagePath, DoctorName)
VALUES (1005, 9, 'rx/2026/08/rx-1005b.jpg', N'Dr. Anisur Rahman, MBBS');

-- the pharmacy's prescription queue  (requirement 16)
SELECT  p.PrescriptionId, p.OrderId, u.FullName AS Customer, p.DoctorName, p.ImagePath, p.VerifyStatus
FROM    Prescriptions p
        INNER JOIN Orders o ON o.OrderId    = p.OrderId
        INNER JOIN Users  u ON u.UserId     = p.CustomerId
WHERE   o.PharmacyId = @PharmacyId AND p.VerifyStatus = 'Pending';

UPDATE Prescriptions SET VerifyStatus = 'Approved' WHERE PrescriptionId = @PrescriptionId;

-- an order cannot be confirmed while a prescription on it is still Pending
UPDATE Orders SET Status = 'Confirmed'
WHERE  OrderId = @OrderId
  AND  NOT EXISTS (SELECT 1 FROM Prescriptions p
                   WHERE p.OrderId = @OrderId AND p.VerifyStatus <> 'Approved');
GO


/* =====================================  END OF FILE  ================================= */
