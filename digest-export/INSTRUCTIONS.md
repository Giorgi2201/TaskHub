# Digest Feature – Integration Instructions

This folder contains everything needed to add the **Digest (დაიჯესტი)** feature to an Angular + ASP.NET Core project.

The feature has two parts:
- A **public Digest page** (`/digest`) that shows featured entries as circles and regular entries as a list.
- An **Admin panel tab** for creating, editing, and deleting digest entries.

---

## Assumptions about the target project

Before starting, confirm the target project has:

| Requirement | Notes |
|---|---|
| ASP.NET Core Web API | Any recent version (tested on .NET 9) |
| Entity Framework Core with SQL Server | Other DB providers need a regenerated migration |
| A `User` model with `int UserID` and `string Name` | Used as the author foreign key |
| A `DbContext` class with a `DbSet<User>` | The digest table links to users |
| Angular (standalone components) | Tested with Angular 17+ |
| `HttpClient` available in Angular | For API calls |
| An Angular service (e.g. `user.service.ts`) that already has `private apiUrl` and `HttpClient` | The digest API calls are added here |
| An admin component (`admin.component`) with `modalOpen`, `isEditMode`, `closeModal()`, and access to `authService.getCurrentUser()` | The digest admin tab slots into this |

---

## Step-by-step integration

### BACKEND

#### 1. Copy the model
Copy `backend/Models/DigestEntry.cs` into your project's `Models/` folder.

- Open the file and replace `YourProject.API` in the namespace with your actual project namespace.
- The model expects a `User` class with `int UserID` and `string Name`. If your user model uses different property names, update the `Author` navigation property and the controller's `.Select()` projections accordingly.

#### 2. Add navigation property to your User model
In your `User.cs` model, add this property:

```csharp
public ICollection<DigestEntry> DigestEntries { get; set; } = new List<DigestEntry>();
```

#### 3. Copy the DTOs
Copy `backend/DTOs/DigestDTOs.cs` into your project's `DTOs/` folder (or wherever you keep DTOs).

- Replace `YourProject.API` in the namespace.
- If your DTOs are all in one file, paste the three classes (`DigestEntryDto`, `CreateDigestEntryDto`, `UpdateDigestEntryDto`) into that file instead.

#### 4. Register the DbSet in your DbContext
In your `DbContext` class, add:

```csharp
public DbSet<DigestEntry> DigestEntries { get; set; }
```

No additional `OnModelCreating` configuration is needed — EF Core will use conventions. The foreign key to `User` will cascade delete by default.

#### 5. Copy the controller
Copy `backend/Controllers/DigestController.cs` into your `Controllers/` folder.

- Replace all occurrences of `YourProject.API` with your namespace.
- Replace `YourDbContext` with your actual DbContext class name.
- The controller is auto-discovered by ASP.NET Core — no manual registration needed as long as you call `builder.Services.AddControllers()` and `app.MapControllers()` in `Program.cs`.

#### 6. Run the migration

**Option A – Let EF Core generate it (recommended):**
```bash
dotnet ef migrations add AddDigestEntries
dotnet ef database update
```

**Option B – Use the provided migration file:**
Copy `backend/Migrations/AddDigestEntries_migration.cs` into your `Migrations/` folder.
- Replace the namespace.
- Adjust `principalTable: "Users"` and `principalColumn: "UserID"` if your user table/column names differ.
- Then run: `dotnet ef database update`

---

### FRONTEND

#### 7. Copy the Digest page component
Copy the entire `frontend/digest/` folder into your Angular `src/app/` directory.

The folder contains:
- `digest.component.ts`
- `digest.component.html`
- `digest.component.css`

Open `digest.component.ts` and check the import path for `UserService`:
```typescript
import { UserService } from '../user.service';
```
Adjust `'../user.service'` to match the actual path to your service file.

#### 8. Add the route
Open your `app.routes.ts` (or equivalent routing file) and follow the instructions in `patches/app.routes.addition.ts`.

In short: import `DigestComponent` and add `{ path: 'digest', component: DigestComponent }` to your routes array.

#### 9. Add the nav link
**Do NOT add the Digest link to the top/header navigation.**
Instead, place it in the **left sidebar panel of the main portal homepage**. The target project has a left-side navigation panel on its homepage — find that component and add the link there alongside the other sidebar menu items. Use the link markup from `patches/nav.html.addition.html` as a reference for the icon and label, but match the styling conventions of the existing sidebar links in that panel.

#### 10. Add API methods to your service
Open your Angular HTTP service (e.g. `user.service.ts`) and paste the 4 methods from `patches/user.service.additions.ts` into the class body.

Make sure the `apiUrl` base matches your backend port (e.g. `http://localhost:5250/api`).

#### 11. Add the Admin tab
The digest tab inside the admin panel requires 3 additions:

**HTML** (`admin.component.html`) — follow `patches/admin.digest-tab.html`:
  - Add the tab button inside the `.tab-switch` div.
  - Add the tab content panel alongside your other tab panels.
  - Add the modal at the bottom of the file, alongside other modals.

**TypeScript** (`admin.component.ts`) — follow `patches/admin.digest-tab.ts`:
  - Add the `DigestItem` interface.
  - Extend `activeTab` union type to include `'digest'`.
  - Add the properties (`digestEntries`, `digestFormData`, `digestPhotoMode`, `digestUploadedFile`).
  - Call `this.loadDigestEntries()` inside `ngOnInit()`.
  - Add the reset block inside `closeModal()`.
  - Add the 6 methods (`loadDigestEntries`, `openAddDigestModal`, `openEditDigestModal`, `onDigestFileChange`, `saveDigestEntry`, `deleteDigestEntry`).

**CSS** (`admin.component.css`) — append the contents of `patches/admin.digest-tab.css` to the end of your admin CSS file.

  > **Note:** The admin tab requires that toggle-switch styles (`toggle-group`, `toggle-switch`, `toggle-slider`, `toggle-label`, `toggle-description`, `form-row`) already exist in your admin CSS. These are standard styles shared with other admin tabs. If they are missing, copy them from the vacancies section of your admin CSS.

---

## File map

```
digest-export/
├── INSTRUCTIONS.md                        ← You are here
│
├── backend/
│   ├── Models/
│   │   └── DigestEntry.cs                 ← Copy to your Models/ folder
│   ├── DTOs/
│   │   └── DigestDTOs.cs                  ← Copy to your DTOs/ folder (or merge into existing DTOs file)
│   ├── Controllers/
│   │   └── DigestController.cs            ← Copy to your Controllers/ folder
│   └── Migrations/
│       └── AddDigestEntries_migration.cs  ← Copy to Migrations/ OR just run `dotnet ef migrations add`
│
├── frontend/
│   └── digest/
│       ├── digest.component.ts            ← Copy to src/app/digest/
│       ├── digest.component.html          ← Copy to src/app/digest/
│       └── digest.component.css           ← Copy to src/app/digest/
│
└── patches/                               ← Instructions for modifying existing files
    ├── user.service.additions.ts          ← Paste methods into your HTTP service
    ├── app.routes.addition.ts             ← Add route entry + import
    ├── nav.html.addition.html             ← Add nav link to header
    ├── admin.digest-tab.html              ← Add tab button, panel, and modal to admin HTML
    ├── admin.digest-tab.ts                ← Add interface, properties, and methods to admin TS
    └── admin.digest-tab.css               ← Append to admin CSS
```

---

## How the feature works

- **Admin creates entries** via the admin panel tab. Each entry has:
  - Title, description, optional photo (URL or file upload)
  - Source name + source URL (clickable link shown in the table and on the public page)
  - Period From / Period To (activation/deactivation dates)
  - **"Main Event" toggle** (`isFeatured`): if on, entry appears as a circle at the top of the digest page (max 3)
  - **"Active Entry" toggle** (`isActive`): only active entries within the date range appear on the public page

- **Public Digest page** (`/digest`):
  - Fetches only active, in-date-range entries from `GET /api/digest?activeOnly=true`
  - Featured entries (up to 3) render as circular image cards with hover animation
  - Non-featured entries render as horizontal list cards below a divider
  - If no entries are active, an empty state message is shown
  - The date pill in the header auto-calculates the min/max period across all active entries
