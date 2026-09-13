-- =============================================================================
--  PharmaLink - Pharmacy Marketplace
--  In-place schema upgrade for an EXISTING PharmaLinkDB
--
--  Use this when PharmaLinkDB was created by an older PharmaLinkDB_Setup.sql
--  and already holds data you want to keep.  (For a brand new database just
--  run PharmaLinkDB_Setup.sql, which already contains everything below.)
--
--  The script is idempotent: every step first checks whether it has already
--  been applied, so running it twice changes nothing the second time.
--  Nothing is dropped except the two redundant indexes listed in step 6.
--
--  What it adds:
--   1. Users         FailedLoginCount, LockoutUntil          (login lockout)
--   2. Pharmacies    CK_Pharmacies_Status allows 'Rejected'
--   3. Orders        PaymentMobile, CK_Orders_Commission (CommissionAmount >= 0)
--   4. Reviews       IsReported, ReportReason, ReportedAt,
--                    FK_Reviews_OrderItem (OrderId, MedicineId) -> OrderItems
--   5. Prescriptions RejectReason
--   6. Indexes       drop IX_OrderItems_Order, IX_Cart_Customer;
--                    add IX_Reviews_Order, IX_Reviews_Customer,
--                        IX_Prescriptions_Order, IX_Pharmacies_Status
--   7. Users         MustChangePassword, PasswordResetRequestedAt
--                    (help-desk password reset and forced password change)
--   8. Pharmacies    WarningMessage, WarnedAt, WarningAcknowledgedAt
--                    (Super Admin warning shown to the owner)
--
--  Steps 7 and 8 only add columns: existing accounts start with
--  MustChangePassword = 0 and no reset request, and no pharmacy starts warned.
--
--  Passwords are NOT rewritten here.  Existing rows keep their legacy SHA-256
--  hashes; the application still accepts them and replaces each one with a
--  PBKDF2 hash the next time that user logs in successfully.
-- =============================================================================

USE PharmaLinkDB;
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;
GO


-- =============================================================================
--  1. Users: login lockout columns
-- =============================================================================
IF COL_LENGTH('dbo.Users', 'FailedLoginCount') IS NULL
BEGIN
    ALTER TABLE dbo.Users ADD FailedLoginCount INT NOT NULL
        CONSTRAINT DF_Users_FailedLogins DEFAULT (0);
    PRINT 'Users.FailedLoginCount added.';
END

IF COL_LENGTH('dbo.Users', 'LockoutUntil') IS NULL
BEGIN
    ALTER TABLE dbo.Users ADD LockoutUntil DATETIME2(0) NULL;
    PRINT 'Users.LockoutUntil added.';
END
GO


-- =============================================================================
--  2. Pharmacies: 'Rejected' status
--
--  The old application suspended a shop by setting Medicines.IsActive = 0 on
--  every medicine it listed, and reinstating set them all back to 1.  The new
--  application leaves IsActive alone (it means only "the owner listed it") and
--  hides a suspended shop through its Status instead.  So that reinstating a
--  shop suspended under the old rules still brings its medicines back, the
--  medicines of currently Suspended pharmacies are re-listed here - once, in
--  the same step that widens the constraint, so a second run does not repeat it.
-- =============================================================================
IF EXISTS (SELECT 1
           FROM   sys.check_constraints
           WHERE  name = 'CK_Pharmacies_Status'
             AND  parent_object_id = OBJECT_ID('dbo.Pharmacies')
             AND  definition NOT LIKE '%Rejected%')
BEGIN
    BEGIN TRANSACTION;

    ALTER TABLE dbo.Pharmacies DROP CONSTRAINT CK_Pharmacies_Status;
    ALTER TABLE dbo.Pharmacies WITH CHECK ADD CONSTRAINT CK_Pharmacies_Status
        CHECK (Status IN ('Pending', 'Approved', 'Suspended', 'Rejected'));

    UPDATE  m
    SET     m.IsActive = 1
    FROM    dbo.Medicines m
            INNER JOIN dbo.Pharmacies p ON p.PharmacyId = m.PharmacyId
    WHERE   p.Status = 'Suspended'
      AND   m.IsActive = 0;

    PRINT 'CK_Pharmacies_Status now allows Rejected; ' + CAST(@@ROWCOUNT AS VARCHAR(10)) +
          ' medicine(s) of suspended pharmacies re-listed (old suspension had delisted them).';

    COMMIT TRANSACTION;
END
ELSE IF NOT EXISTS (SELECT 1 FROM sys.check_constraints
                    WHERE name = 'CK_Pharmacies_Status' AND parent_object_id = OBJECT_ID('dbo.Pharmacies'))
BEGIN
    ALTER TABLE dbo.Pharmacies WITH CHECK ADD CONSTRAINT CK_Pharmacies_Status
        CHECK (Status IN ('Pending', 'Approved', 'Suspended', 'Rejected'));
    PRINT 'CK_Pharmacies_Status created.';
END
GO


-- =============================================================================
--  3. Orders: wallet number and non-negative commission
-- =============================================================================
IF COL_LENGTH('dbo.Orders', 'PaymentMobile') IS NULL
BEGIN
    ALTER TABLE dbo.Orders ADD PaymentMobile NVARCHAR(20) NULL;
    PRINT 'Orders.PaymentMobile added.';
END

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints
               WHERE name = 'CK_Orders_Commission' AND parent_object_id = OBJECT_ID('dbo.Orders'))
BEGIN
    IF EXISTS (SELECT 1 FROM dbo.Orders WHERE CommissionAmount < 0)
    BEGIN
        ALTER TABLE dbo.Orders WITH NOCHECK ADD CONSTRAINT CK_Orders_Commission CHECK (CommissionAmount >= 0);
        PRINT 'WARNING: some existing orders have a negative CommissionAmount. CK_Orders_Commission was added '
            + 'WITH NOCHECK, so it protects new rows only. Fix those orders, then run '
            + 'ALTER TABLE dbo.Orders WITH CHECK CHECK CONSTRAINT CK_Orders_Commission;';
    END
    ELSE
    BEGIN
        ALTER TABLE dbo.Orders WITH CHECK ADD CONSTRAINT CK_Orders_Commission CHECK (CommissionAmount >= 0);
        PRINT 'CK_Orders_Commission added.';
    END
END
GO


-- =============================================================================
--  4. Reviews: report flag columns and the order line foreign key
-- =============================================================================
IF COL_LENGTH('dbo.Reviews', 'IsReported') IS NULL
BEGIN
    ALTER TABLE dbo.Reviews ADD IsReported BIT NOT NULL
        CONSTRAINT DF_Reviews_Reported DEFAULT (0);
    PRINT 'Reviews.IsReported added.';
END

IF COL_LENGTH('dbo.Reviews', 'ReportReason') IS NULL
BEGIN
    ALTER TABLE dbo.Reviews ADD ReportReason NVARCHAR(200) NULL;
    PRINT 'Reviews.ReportReason added.';
END

IF COL_LENGTH('dbo.Reviews', 'ReportedAt') IS NULL
BEGIN
    ALTER TABLE dbo.Reviews ADD ReportedAt DATETIME2(0) NULL;
    PRINT 'Reviews.ReportedAt added.';
END
GO

-- The composite foreign key needs a UNIQUE key on OrderItems(OrderId, MedicineId).
-- Every PharmaLinkDB_Setup.sql has created UQ_OrderItems_Line; this only
-- covers a database where it was removed by hand.
IF NOT EXISTS (SELECT 1 FROM sys.key_constraints
               WHERE name = 'UQ_OrderItems_Line' AND parent_object_id = OBJECT_ID('dbo.OrderItems'))
BEGIN
    ALTER TABLE dbo.OrderItems ADD CONSTRAINT UQ_OrderItems_Line UNIQUE (OrderId, MedicineId);
    PRINT 'UQ_OrderItems_Line added.';
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys
               WHERE name = 'FK_Reviews_OrderItem' AND parent_object_id = OBJECT_ID('dbo.Reviews'))
BEGIN
    IF EXISTS (SELECT 1
               FROM   dbo.Reviews r
               WHERE  NOT EXISTS (SELECT 1 FROM dbo.OrderItems oi
                                  WHERE  oi.OrderId = r.OrderId AND oi.MedicineId = r.MedicineId))
    BEGIN
        ALTER TABLE dbo.Reviews WITH NOCHECK ADD CONSTRAINT FK_Reviews_OrderItem
            FOREIGN KEY (OrderId, MedicineId) REFERENCES dbo.OrderItems (OrderId, MedicineId);
        PRINT 'WARNING: some existing reviews rate a medicine that is not a line on their order. '
            + 'FK_Reviews_OrderItem was added WITH NOCHECK, so it protects new reviews only. '
            + 'Find them with: SELECT r.* FROM Reviews r WHERE NOT EXISTS (SELECT 1 FROM OrderItems oi '
            + 'WHERE oi.OrderId = r.OrderId AND oi.MedicineId = r.MedicineId);';
    END
    ELSE
    BEGIN
        ALTER TABLE dbo.Reviews WITH CHECK ADD CONSTRAINT FK_Reviews_OrderItem
            FOREIGN KEY (OrderId, MedicineId) REFERENCES dbo.OrderItems (OrderId, MedicineId);
        PRINT 'FK_Reviews_OrderItem added.';
    END
END
GO


-- =============================================================================
--  5. Prescriptions: rejection reason
--  Several rows per order are allowed (a re-upload after a rejection adds a
--  row); the current prescription is the highest PrescriptionId.
-- =============================================================================
IF COL_LENGTH('dbo.Prescriptions', 'RejectReason') IS NULL
BEGIN
    ALTER TABLE dbo.Prescriptions ADD RejectReason NVARCHAR(200) NULL;
    PRINT 'Prescriptions.RejectReason added.';
END
GO


-- =============================================================================
--  6. Indexes
--  IX_OrderItems_Order and IX_Cart_Customer duplicate the leading column of
--  UQ_OrderItems_Line and UQ_Cart_Line, whose own indexes already serve those
--  lookups.
-- =============================================================================
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_OrderItems_Order' AND object_id = OBJECT_ID('dbo.OrderItems'))
BEGIN
    DROP INDEX IX_OrderItems_Order ON dbo.OrderItems;
    PRINT 'IX_OrderItems_Order dropped.';
END

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Cart_Customer' AND object_id = OBJECT_ID('dbo.Cart'))
BEGIN
    DROP INDEX IX_Cart_Customer ON dbo.Cart;
    PRINT 'IX_Cart_Customer dropped.';
END

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Reviews_Order' AND object_id = OBJECT_ID('dbo.Reviews'))
BEGIN
    CREATE INDEX IX_Reviews_Order ON dbo.Reviews(OrderId);
    PRINT 'IX_Reviews_Order created.';
END

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Reviews_Customer' AND object_id = OBJECT_ID('dbo.Reviews'))
BEGIN
    CREATE INDEX IX_Reviews_Customer ON dbo.Reviews(CustomerId);
    PRINT 'IX_Reviews_Customer created.';
END

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Prescriptions_Order' AND object_id = OBJECT_ID('dbo.Prescriptions'))
BEGIN
    CREATE INDEX IX_Prescriptions_Order ON dbo.Prescriptions(OrderId);
    PRINT 'IX_Prescriptions_Order created.';
END

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Pharmacies_Status' AND object_id = OBJECT_ID('dbo.Pharmacies'))
BEGIN
    CREATE INDEX IX_Pharmacies_Status ON dbo.Pharmacies(Status);
    PRINT 'IX_Pharmacies_Status created.';
END
GO


-- =============================================================================
--  7. Users: password reset request and forced password change
--  There is no email or SMS service.  "Forgot password?" stamps
--  PasswordResetRequestedAt; the Super Admin then issues a temporary password,
--  which sets MustChangePassword = 1 until the user picks a new one at login.
--  The NOT NULL column gets its DEFAULT in the same ALTER, so every existing
--  row is filled with 0 without a separate UPDATE.
-- =============================================================================
IF COL_LENGTH('dbo.Users', 'MustChangePassword') IS NULL
BEGIN
    ALTER TABLE dbo.Users ADD MustChangePassword BIT NOT NULL
        CONSTRAINT DF_Users_MustChangePw DEFAULT (0);
    PRINT 'Users.MustChangePassword added.';
END

IF COL_LENGTH('dbo.Users', 'PasswordResetRequestedAt') IS NULL
BEGIN
    ALTER TABLE dbo.Users ADD PasswordResetRequestedAt DATETIME2(0) NULL;
    PRINT 'Users.PasswordResetRequestedAt added.';
END
GO


-- =============================================================================
--  8. Pharmacies: Super Admin warning
--  The latest warning to the owner, when it was sent and when the owner
--  acknowledged it.  All NULL means the shop has never been warned.
-- =============================================================================
IF COL_LENGTH('dbo.Pharmacies', 'WarningMessage') IS NULL
BEGIN
    ALTER TABLE dbo.Pharmacies ADD WarningMessage NVARCHAR(500) NULL;
    PRINT 'Pharmacies.WarningMessage added.';
END

IF COL_LENGTH('dbo.Pharmacies', 'WarnedAt') IS NULL
BEGIN
    ALTER TABLE dbo.Pharmacies ADD WarnedAt DATETIME2(0) NULL;
    PRINT 'Pharmacies.WarnedAt added.';
END

IF COL_LENGTH('dbo.Pharmacies', 'WarningAcknowledgedAt') IS NULL
BEGIN
    ALTER TABLE dbo.Pharmacies ADD WarningAcknowledgedAt DATETIME2(0) NULL;
    PRINT 'Pharmacies.WarningAcknowledgedAt added.';
END
GO


-- =============================================================================
--  VERIFY
--  Every row should say OK.
-- =============================================================================
SELECT Item, CASE WHEN Present = 1 THEN 'OK' ELSE 'MISSING' END AS Result
FROM (
    SELECT 'Users.FailedLoginCount' AS Item, CASE WHEN COL_LENGTH('dbo.Users', 'FailedLoginCount') IS NULL THEN 0 ELSE 1 END AS Present
    UNION ALL SELECT 'Users.LockoutUntil',          CASE WHEN COL_LENGTH('dbo.Users', 'LockoutUntil') IS NULL THEN 0 ELSE 1 END
    UNION ALL SELECT 'Orders.PaymentMobile',        CASE WHEN COL_LENGTH('dbo.Orders', 'PaymentMobile') IS NULL THEN 0 ELSE 1 END
    UNION ALL SELECT 'Reviews.IsReported',          CASE WHEN COL_LENGTH('dbo.Reviews', 'IsReported') IS NULL THEN 0 ELSE 1 END
    UNION ALL SELECT 'Reviews.ReportReason',        CASE WHEN COL_LENGTH('dbo.Reviews', 'ReportReason') IS NULL THEN 0 ELSE 1 END
    UNION ALL SELECT 'Reviews.ReportedAt',          CASE WHEN COL_LENGTH('dbo.Reviews', 'ReportedAt') IS NULL THEN 0 ELSE 1 END
    UNION ALL SELECT 'Prescriptions.RejectReason',  CASE WHEN COL_LENGTH('dbo.Prescriptions', 'RejectReason') IS NULL THEN 0 ELSE 1 END
    UNION ALL SELECT 'Users.MustChangePassword',    CASE WHEN COL_LENGTH('dbo.Users', 'MustChangePassword') IS NULL THEN 0 ELSE 1 END
    UNION ALL SELECT 'Users.PasswordResetRequestedAt', CASE WHEN COL_LENGTH('dbo.Users', 'PasswordResetRequestedAt') IS NULL THEN 0 ELSE 1 END
    UNION ALL SELECT 'Pharmacies.WarningMessage',   CASE WHEN COL_LENGTH('dbo.Pharmacies', 'WarningMessage') IS NULL THEN 0 ELSE 1 END
    UNION ALL SELECT 'Pharmacies.WarnedAt',         CASE WHEN COL_LENGTH('dbo.Pharmacies', 'WarnedAt') IS NULL THEN 0 ELSE 1 END
    UNION ALL SELECT 'Pharmacies.WarningAcknowledgedAt', CASE WHEN COL_LENGTH('dbo.Pharmacies', 'WarningAcknowledgedAt') IS NULL THEN 0 ELSE 1 END
    UNION ALL SELECT 'CK_Pharmacies_Status allows Rejected',
              CASE WHEN EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_Pharmacies_Status' AND definition LIKE '%Rejected%') THEN 1 ELSE 0 END
    UNION ALL SELECT 'CK_Orders_Commission',
              CASE WHEN EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_Orders_Commission') THEN 1 ELSE 0 END
    UNION ALL SELECT 'FK_Reviews_OrderItem',
              CASE WHEN EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Reviews_OrderItem') THEN 1 ELSE 0 END
    UNION ALL SELECT 'IX_OrderItems_Order removed',
              CASE WHEN EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_OrderItems_Order') THEN 0 ELSE 1 END
    UNION ALL SELECT 'IX_Cart_Customer removed',
              CASE WHEN EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Cart_Customer') THEN 0 ELSE 1 END
    UNION ALL SELECT 'IX_Reviews_Order',       CASE WHEN EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Reviews_Order') THEN 1 ELSE 0 END
    UNION ALL SELECT 'IX_Reviews_Customer',    CASE WHEN EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Reviews_Customer') THEN 1 ELSE 0 END
    UNION ALL SELECT 'IX_Prescriptions_Order', CASE WHEN EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Prescriptions_Order') THEN 1 ELSE 0 END
    UNION ALL SELECT 'IX_Pharmacies_Status',   CASE WHEN EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Pharmacies_Status') THEN 1 ELSE 0 END
) checks;
GO

PRINT 'PharmaLinkDB upgrade finished.';
GO
