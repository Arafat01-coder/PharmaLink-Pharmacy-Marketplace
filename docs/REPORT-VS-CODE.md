# Report vs Code — known discrepancies

`docs/Project_Report.pdf` was written during the **design phase**. The
application in this repository is the **implementation**, and the two have
drifted apart in the places listed below.

None of this is fatal. Design documents always drift, and an examiner who finds
a gap and hears *"yes — the design said X, we built Y, and here is why"* will
mark that far higher than a student who is surprised by their own report. What
loses marks is being caught not knowing.

Each item below gives the gap, where to see it, and the honest answer.

---

## 1. The report is written as if the code does not exist yet

**The biggest one.** Section 10 (Conclusion) and Section 10.1 (Future Work) are
in design-phase tense throughout:

- *"PharmaLink … ended as a complete blueprint for a three tier marketplace."*
- *"This submission is the design phase. The coding phase will implement the blueprint in the order below."*
- *"Choosing a junction table now means the checkout code will be simple later."*

Section 10.1 then lists as *future work* six things that are already built and
committed: login and role routing, the repository layer, Admin data isolation,
the transactional checkout, `CellFormatting` grid colouring, and the reporting
queries.

**What to do.** If you can still edit and re-export the report, rewrite Section
10 in the past tense and replace Section 10.1 with genuine future work — the
delivery rider role, SMS notifications, refill reminders, the real bKash
gateway, the Bangla toggle. Those five are already at the end of 10.1 and are
the only items there that have not been built.

**If you cannot re-export:** say so plainly. *"Section 10 was written at the
design submission. Everything in 10.1 items 1 to 6 is now implemented — here is
the checkout transaction in `OrderService.cs`."* Then show it.

---

## 2. The screenshots do not match the shipped sample data

The screenshots in Section 8 were made against a different data set from the one
`PharmaLinkDB_Setup.sql` creates. An examiner comparing the report to the
running app will see different names on every screen.

| In the report screenshots | In `PharmaLinkDB_Setup.sql` |
|---|---|
| Super Admin **Atik Bin Masud** | **PharmaLink Control** (`admin@pharmalink.com.bd`) |
| Mitford Pharma owner **Mohammad Rafiqul** | **Kamrul Hasan** |
| **Shahbagh Medicine Hub**, Shahbagh, owner Sultana Razia, 8.00% | **Dhanmondi Medico**, Dhanmondi, owner Shirin Sultana, 10.00% |
| Lazz Care Pharmacy — owner Kazi Nazmul Haque, Dhanmondi, 10.00% | owner Tanvir Ahmed, Mirpur, 8.00% |
| New Life Pharmacy — owner Farhana Yeasmin, Agrabad Ctg, `DGDA-CT-20015` | owner Imran Hossain, Uttara, `DGDA-DH-10099` |
| Customers **Karim Sheikh**, **Shakib Al Hasan** | Nusrat Jahan, Tanjila Akter, Sabbir Ahmed |
| Low-rated shop is **Lazz Care Pharmacy** (1.50★) | Low-rated shop is **Dhanmondi Medico** |

**What to do.** Take fresh captures from the running application against the
seeded data and use those in the README (see `docs/screenshots/README.md`). The
report screenshots can stay as they are — they are design mockups and the report
is a design document — but do not present them as evidence that the system runs.

---

## 3. Controls in the report screenshots that the application does not have

Searched across every `.cs` file in the repository; none of these strings appear
anywhere in the code:

| Shown in the report | Where | In the code |
|---|---|---|
| **Forgot password?** link, and a *Forgot Password* form | Login screenshot; also a box in the Entry and Role Decision diagram | No `ForgotPasswordForm`, no handler |
| **Remember me** checkbox | Login screenshot | Not present |
| **Reorder** button | Order History screenshot | Not present |
| **Save as PDF** button | Invoice screenshot | Not present (Print is, via `PrintDocument`) |
| **Warn Pharmacy**, **Open Pharmacy Record** | Moderate Reviews screenshot | Not present |

**What to do.** Either build them — *Forgot Password* and *Reorder* are each an
evening's work — or be ready to say which mockup controls were dropped during
implementation and why. Dropping scope is normal; not knowing you dropped it is
not.

---

## 4. The login query in the report is not the login query in the code

**Report, Section 7.1:**

```sql
SELECT u.UserId, u.FullName, u.UserType, u.Status, p.PharmacyId
FROM   Users u LEFT JOIN Pharmacies p ON p.OwnerId = u.UserId
WHERE  u.Email = @Email
  AND  u.PasswordHash = @PasswordHash
  AND  u.Status = 'Active';
```

**Code, `Services/AuthService.cs` lines 39–44:**

```sql
SELECT u.UserId, u.FullName, u.Email, u.PasswordHash, u.PasswordSalt,
       u.Phone, u.Address, u.UserType, u.Status, u.CreatedAt,
       p.PharmacyId, p.PharmacyName, p.Status AS PharmacyStatus
FROM   Users u LEFT JOIN Pharmacies p ON p.OwnerId = u.UserId
WHERE  u.Email = @Email;
```

The hash and the status are then checked in C#.

**The code is the better version, and you should say so.** The salt is
per-user, so the hash cannot be computed before the row is read — matching
`PasswordHash` in a `WHERE` clause only works if every account shares one salt,
which defeats the point of salting. Keeping the comparison in memory also avoids
leaking information through query timing.

This is the single most likely question in the whole viva ("walk me through
login"). Know this answer cold.

---

## 5. Form and file names

| In the report | In the repository |
|---|---|
| `AdminProfileForm` (traceability table, requirement 17) | `MyProfileForm` and `PharmacyProfileForm` — no `AdminProfileForm` exists |
| *"The complete INSERT section is in `database/schema.sql`"* (Section 7.1) | `PharmaLinkDB_Setup.sql`, in the repository root. There is no `database/` folder |
| *"eighteen consistent form designs"* (Section 10) | 28 Windows Forms |

---

## 6. Small internal inconsistencies inside the report

Worth a glance so nothing catches you off guard:

- **Orders primary key.** Section 6 describes `OrderId` as `INT IDENTITY(1,1)`;
  the `CREATE TABLE` in Section 7 says `IDENTITY(1001,1)`. The code and the
  shipped script use **1001**, so invoice numbers start at 1001. Section 6 is
  the one that is wrong.
- **Duplicate section numbers.** Section 7 runs *7.1 Sample Data*, *7.2 Feature
  Queries*, and then restarts at *7.1 Login and role routing* through *7.18*.
  The traceability table in Section 3 points at the second set.
- **Unnumbered chapter.** *Functional Requirements & User Stories* sits between
  chapters 3 and 4 without a number of its own, so the table of contents jumps.
- **Delivery charge.** Section 7.5 hardcodes `60.00 AS DeliveryCharge`; the
  application reads it from `App.config` (`DeliveryCharge`, currently 60).
- **"Entries shown in red are repeats"** (Section 5, Finalization). Open the PDF
  and confirm the red actually renders — if the colour was lost on export, that
  sentence points at nothing.

---

## 7. What the report gets right, and should be defended

Do not let the list above make you defensive. These claims in the report are
backed by the code, and you can prove each one on the spot:

| Report claim | Proof in the repository |
|---|---|
| Ten tables, third normal form, `OrderItems` as the junction table | `PharmaLinkDB_Setup.sql` — 10 `CREATE TABLE`, 16 foreign keys, 9 `UNIQUE`, 20 `CHECK`, 14 indexes (counted from `sys.foreign_keys`, `sys.key_constraints`, `sys.check_constraints` and `sys.indexes` after running the script) |
| Data isolation via `WHERE PharmacyId = @PharmacyId` | `MedicineService.cs`, `ReportService.cs`, `OrderService.cs` |
| Checkout is one transaction | `OrderService.cs` |
| Commission frozen on the order row | `Orders.CommissionAmount`, written once at checkout |
| Nothing concatenated into a query string | `Database/DbHelper.cs` — every method takes `SqlParameter[]` |
| Passwords salted and SHA-256 hashed | `Helpers/PasswordHelper.cs` |
| `TotalAmount` and `Subtotal` are computed persisted columns | `PharmaLinkDB_Setup.sql` |
| Review verified by `OrderId`, one per purchase | `UQ_Reviews_OneEach UNIQUE (CustomerId, MedicineId, OrderId)` |

The strongest single demonstration is still the isolation one: run the same
Medicines screen as `kamrul@mitfordpharma.com` and then as
`shirin@dhanmondimedico.com`, and show two different lists coming out of one
form and one query.
