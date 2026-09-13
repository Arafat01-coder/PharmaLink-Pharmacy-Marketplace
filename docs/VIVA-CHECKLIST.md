# Viva Preparation Checklist

Viva period: **3–14 September**. Everything below maps to a line in the official
announcement. Work top to bottom.

---

## Part 1 — Publishing the repository

The repository must be **public** and must show **meaningful individual commits**
from all four members. A single "initial commit" containing the whole project
fails that check, no matter how good the code is.

### If you have not pushed yet

```bash
cd PharmaLink-Pharmacy-Marketplace
git init
git branch -M main
```

Confirm the ignore rules are working *before* the first commit — `bin/`, `obj/`,
`.vs/` and `*.user` must never appear:

```bash
git add -A
git status --short          # scan this list; no bin/, obj/, .vs/ or .user files
```

Then create the repository on GitHub (public) and push:

```bash
git remote add origin https://github.com/<username>/PharmaLink-Pharmacy-Marketplace.git
git push -u origin main
```

### Making the commit history reflect who did what

Each member commits their own work, from their own machine, under their own
name and email:

```bash
git config user.name  "Your Name"
git config user.email "your-aiub-email@example.com"
```

Commit in slices that match the contribution table in Section 16 of the README —
database and checkout, Super Admin branch, Pharmacy Owner branch, Customer
branch — rather than one commit per person. Message style that reads well to an
examiner:

```
Add low-stock alert query filtered by PharmacyId
Fix commission double-counting when OrderItems join multiplies order rows
Enforce UNIQUE(CustomerId, MedicineId, OrderId) on reviews
```

Verify the history before the viva:

```bash
git log --pretty=format:"%h  %an  %ad  %s" --date=short
git shortlog -sn            # commit count per author — all four must appear
```

> If the project was genuinely written together on one machine, say so plainly at
> the viva. Do not backdate or fabricate commits — the announcement asks you to
> demonstrate that your claims are *supported* by the history, and a manufactured
> history is far worse than an honest explanation of how you actually worked.

---

## Part 2 — Repository contents

The announcement lists five things that must be in the repo. Tick each one off:

- [ ] **README** — `README.md` at the root
- [ ] **Project report PDF** — `docs/Project_Report.pdf`
- [ ] **Screenshots in the README** — Section 15, 24 images in `docs/screenshots/`
- [ ] **Video demonstration link in the README** — Quick Links table **and** Section 18
- [ ] **SQL file** — `PharmaLinkDB_Setup.sql` at the root

Two placeholders are deliberately left for you to fill in. Find them with:

```bash
grep -rn "REPLACE_WITH_YOUR_VIDEO_LINK" README.md
```

Both must be replaced before you submit the form.

---

## Part 3 — The report

`docs/Project_Report.pdf` is your submitted report: AIUB cover page, the CO2 and
CO3 rubric tables, table of contents, Chapters 1–10, the Chen ER diagram, the
schema diagram and the navigation diagrams.

Read `docs/REPORT-VS-CODE.md` before the viva. It lists the places where the
report and the built application currently disagree. Each one is a question an
examiner can ask, and each has a short honest answer — but only if you know it
is coming.

---

## Part 4 — What you must be able to answer, individually

Every member is examined separately. These are the questions the announcement
implies; prepare your own answer for each, pointing at code you actually wrote.

### On the database

- How many tables, and what does each one hold?
- Which table is the junction table, and what many-to-many relationship does it
  resolve? Why can't `Orders` just hold a list of medicines?
- Walk through one `JOIN` + `GROUP BY` query line by line. Query 8.10 is the
  strongest choice — it joins three tables, groups back up to the pharmacy, and
  uses `HAVING` because the condition is on the aggregate. Be ready to explain
  why `HAVING` and not `WHERE`.
- Why is `OrderItems.UnitPrice` stored instead of read from `Medicines`?
- What does `COMPUTED PERSISTED` mean on `TotalAmount` and `Subtotal`?
- Which queries did *you* write?

### On the UI

- How many Windows Forms, and which ones did you design and code?
- How is GUI consistency achieved? (`Helpers/UiTheme.cs` — one palette, one set
  of control styles.)
- Show a validation rule enforced twice: once in the form and once as a `CHECK`
  constraint. Explain why both.

### On the code

- Where does the connection string live, and which class reads it?
  (`App.config` lines 4–6; `Database/DbHelper.cs` lines 18–19.)
- Why is nothing concatenated into a query string? What attack does that prevent?
- Explain the checkout transaction: what are the five steps, and what breaks if
  any one of them runs without the others?
- How is data isolation enforced between two pharmacy owners? Point at the actual
  `WHERE` clause rather than describing the idea.
- How are passwords stored, and what happens on a password change if the current
  password is wrong?

### On integrity

- Confirm the approved project domain: an online pharmacy marketplace with three
  roles — platform operator, pharmacy owner, customer.
- Be ready to explain **any AI tool you used and how you used it.** Say what it
  did and what you did. Examiners generally accept AI assistance that you can
  explain and defend; what they will not accept is code you cannot read aloud
  and account for. If any part of this project came from a tool, know that part
  cold or be honest that you leaned on help there.

---

## Part 5 — On the day

- [ ] Laptop charged, project cloned fresh from GitHub and confirmed to build
- [ ] SQL Server running, `PharmaLinkDB` created from the script
- [ ] All three test logins verified working *that morning*
- [ ] Video link opens in a browser from a phone (not just from your own Drive)
- [ ] Repository opens publicly in a private/incognito window
- [ ] Submission form completed: https://forms.gle/3uQi5Wm1m6SucPyY9
- [ ] Whole group present together, each member ready to be examined alone

Book your slot as soon as the list above is complete. The deadline is
**14 September** and the announcement asks you not to leave it to the last day.
