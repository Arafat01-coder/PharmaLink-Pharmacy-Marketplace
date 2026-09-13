# Report vs Code

`docs/Project_Report.pdf` (editable version: `docs/Project_Report.docx`) was
rewritten on **14 September 2026** so that it describes the finished application
rather than the design phase. Every technical claim in it was checked against
the code in this repository.

Read this before the viva: the first part tells each member what changed in the
report, the second lists the few places where something still differs.

---

## 1. What changed in the report

| Area | Before | Now |
|---|---|---|
| Tense | Written as a design ("the coding phase will implement…") | Describes the built system; Section 10.1 lists only genuine future work |
| Requirements | 30 | 37 — requirements 31 to 37 were added during implementation: login lockout, forgot password with a temporary password and forced change, remember my email, pharmacy warnings, reported reviews, confirm / deliver / cancel orders, reorder and save invoice as PDF |
| Traceability | Pointed at non-existent forms such as `AdminProfileForm` | Real form class names, query numbers 7.3.1–7.3.21 |
| Passwords | "Salted SHA-256", hash compared in the SQL `WHERE` clause | PBKDF2-HMAC-SHA256, 100,000 iterations; the row is read by email and the hash, lockout and status are checked in C# (`Services/AuthService.cs`) |
| Schema (6.1) | Design-time columns, `Orders.OrderId IDENTITY(1,1)` | Regenerated from `PharmaLinkDB_Setup.sql`: every column, constraint and all 14 indexes, `IDENTITY(1001,1)` |
| Normalization | Single-key relations split under "2NF" | Real 2NF work only where the key is composite (Holds, Order Items); the other splits are under 3NF |
| Queries (7) | Numbered 7.1 twice, several did not match the code or failed on the sample data | 7.1 script, 7.2 sample data, 7.3.1–7.3.21 feature queries copied from `Services/*.cs`, example values valid against the seed data |
| Checkout | A batch with no error handling | The real transaction: `ReadCommitted`, row locks and re-checks, prescription row inside the transaction, delivery charge from `App.config` |
| Suspension | "Three UPDATE statements", medicines delisted | Two tables (`Pharmacies`, `Users`) in one transaction; medicines disappear because catalogue queries require an Approved pharmacy |
| Earnings | "Commission recomputed from the pharmacy rate" | Sums the frozen `Orders.CommissionAmount` |
| Transitions (8) | Chains of screens that do not open each other | Transition tables built from the click handlers, four new navigation diagrams |
| Screenshots | Design mockups with different sample data | The 24 captures in `docs/screenshots/` |

---

## 2. What still differs

1. **The two design diagrams.** Figure 4.1 (ER diagram) and Figure 6.1 (SQL
   schema diagram) are the drawings made at design time. They do not show the
   twelve columns added during implementation: `Users.FailedLoginCount`,
   `LockoutUntil`, `MustChangePassword`, `PasswordResetRequestedAt`;
   `Pharmacies.WarningMessage`, `WarnedAt`, `WarningAcknowledgedAt`;
   `Orders.PaymentMobile`; `Reviews.IsReported`, `ReportReason`, `ReportedAt`;
   `Prescriptions.RejectReason`. The report says so under each figure, and
   Section 6.1 lists every column.
   *If asked:* "The diagrams are the design; 6.1 is generated from the final
   script, and each added column depends only on its table's key, so the tables
   are still in 3NF."

2. **A stale code comment.** The summary comment at the top of
   `Forms/MyProfileForm.cs` still describes the old password check ("a wrong
   entry updates no rows"). The code itself verifies the current password in
   memory and guards the `UPDATE` against a concurrent change, which is what
   Section 7.3.18 describes.

3. **Service methods with no screen.** `ReportService.GetRevenueByArea`
   (Section 7.3.13) and `CategoryService.Delete` exist but no form calls them.
   The report states this rather than claiming a screen for them.

---

## 3. The strongest single demonstration

Data isolation: open the Medicines screen as `kamrul@mitfordpharma.com`, then as
`shirin@dhanmondimedico.com`. One form and one query produce two different
lists, because every Pharmacy Owner query carries `WHERE PharmacyId = @PharmacyId`
(Section 7.3.15).
