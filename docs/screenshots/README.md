# Screenshots

Save every capture in **this folder**, as PNG, using the exact file name in the
first column. [`../../README.md`](../../README.md) Section 15 links these names
literally — a typo means a broken image on GitHub.

> **These must be captures of the running application, not the design mockups
> from Section 8 of `Project_Report.pdf`.** The mockups were drawn during the
> design phase against different sample data and show controls the built app
> does not have. Presenting them as screenshots of the finished system is the
> kind of gap the viva is specifically checking for.

## Capture rules

- Run the app maximised at 1920×1080 if you can, and capture the **window only**
  (<kbd>Alt</kbd>+<kbd>PrtScn</kbd>), not the whole desktop.
- The database must have the sample data from `PharmaLinkDB_Setup.sql` loaded, so
  every grid has rows in it. An empty DataGridView is worth nothing to the examiner.
- No personal information, no real phone numbers, nothing from your desktop
  background or taskbar in frame.
- Where a screen exists to prove a rule (validation, low stock, data isolation),
  capture it **in the failing/alerting state**, not the empty default state.

## The 24 captures

### Shared entry

| File name | What must be visible |
|---|---|
| `01-login.png` | Login form with an inline validation message showing |
| `02-signup.png` | Sign Up with *Register as* set to Pharmacy Owner, licence number field filled |

### Super Admin

| File name | What must be visible |
|---|---|
| `03-superadmin-dashboard.png` | Tiles populated, pending queue and low-rated panel both non-empty |
| `04-manage-pharmacies.png` | Grid filtered to Pending, a row selected, Approve/Suspend enabled |
| `05-sales-report.png` | Result grid with the bold total row and the commission column |
| `06-moderate-reviews.png` | The 1–2 star moderation queue with a row selected |
| `07-low-rated-shops.png` | Dhanmondi Medico showing, red-tinted, with its average rating |
| `08-manage-categories.png` | Category list with Add/Edit/Deactivate visible |

### Pharmacy Owner (Admin)

| File name | What must be visible |
|---|---|
| `09-admin-dashboard.png` | Tiles, incoming order queue and the low-stock alert panel |
| `10-medicine-crud.png` | The medicine grid for **one** pharmacy only |
| `11-add-medicine-validation.png` | A negative unit price rejected, red error label under the field |
| `12-inventory.png` | Low-stock rows tinted red with the shortfall column |
| `13-earnings.png` | Four tiles: gross sales, commission, net earnings, units sold |
| `14-offers-admin.png` | An offer being created with a start and end date |
| `15-verify-prescription.png` | A pending prescription with the uploaded image loaded |

### Customer

| File name | What must be visible |
|---|---|
| `16-customer-home.png` | Keyword typed **and** at least two filters applied, with the result count |
| `17-medicine-details.png` | Struck-through original price beside the discounted price, plus reviews |
| `18-cart.png` | Lines from two different pharmacies and the per-pharmacy summary |
| `19-checkout.png` | Heading reading *Order 1 of 2*, bKash selected, Confirm disabled |
| `20-invoice.png` | Printable bill: order number, both addresses, line items, grand total |
| `21-order-history.png` | Several orders with different status pills |
| `22-give-rating.png` | Star selector and comment box, on a Delivered order |
| `23-customer-offers.png` | Offers grid with the calculated discounted price column |
| `24-my-profile.png` | Profile fields plus the Change Password panel |

## One extra worth taking

Not in the 24, but the single most convincing thing you can show at the viva:
log in as `kamrul@mitfordpharma.com`, screenshot his medicine list, then log in as
`shirin@dhanmondimedico.com` and screenshot hers. Two different lists from the same
form is the data-isolation requirement (No. 18) proven in one image pair.
