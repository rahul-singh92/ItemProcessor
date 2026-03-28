# Item Processor — Full Stack Assessment

A .NET MVC web application for processing items with parent-child hierarchy,
built with ASP.NET Core 8, SQL Server, and Selenium test automation.

---

## Tech Stack

- Backend:  ASP.NET Core 8 MVC (C#)
- Database: SQL Server 2022 (via Docker)
- Frontend: Bootstrap 5, Razor Views
- Testing:  Python 3 + Selenium + pytest

---

## Prerequisites

Install the following before running:

| Tool            | Download / Command                              |
|-----------------|-------------------------------------------------|
| .NET 8 SDK      | brew install --cask dotnet-sdk                  |
| Docker Desktop  | brew install --cask docker                      |
| DBeaver         | brew install --cask dbeaver-community           |
| VS Code         | brew install --cask visual-studio-code          |
| Python 3        | brew install python                             |
| Google Chrome   | brew install --cask google-chrome               |

---

## Step 1 — Start SQL Server via Docker

Open Docker Desktop first, then run:
```bash
docker run -e "ACCEPT_EULA=Y" \
  -e "MSSQL_SA_PASSWORD=YourStrong@Passw0rd" \
  -p 1433:1433 \
  --name sqlserver \
  -d mcr.microsoft.com/mssql/server:2022-latest
```

If the container already exists from a previous run:
```bash
docker start sqlserver
```

Verify it is running:
```bash
docker ps
```

You should see `sqlserver` in the list.

---

## Step 2 — Set Up the Database

Open DBeaver and connect with these settings:
```
Host:           localhost
Port:           1433
Database:       master
Authentication: SQL Server Authentication
Username:       sa
Password:       YourStrong@Passw0rd
```

Then open the SQL editor and run the complete script from:
```
database_script.sql
```

This creates the database, all tables, indexes, and sample data.

---

## Step 3 — Configure the Application

Open `appsettings.json` and verify the connection string:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=ItemProcessorDB;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True;"
  }
}
```

---

## Step 4 — Run the Application
```bash
cd ItemProcessor
dotnet run
```

Open your browser and go to:
```
http://localhost:5000
```

---

## Step 5 — Login

Use the default admin account:
```
Email:    admin@test.com
Password: Admin@123
```

Or register a new account from the Login page.

---

## Step 6 — Using the Application

| Feature          | How to use                                        |
|------------------|---------------------------------------------------|
| Add Item         | Items page → click + Add Item                     |
| Edit Item        | Items page → click Edit on any row                |
| Delete Item      | Items page → click Delete (soft delete)           |
| Search Items     | Items page → type in search box → click Search    |
| Process Item     | Items page → click Process on any item            |
| View Processed   | Click Processed in navbar                         |
| View Tree        | Click Tree View in navbar or Tree on any item     |

---

## Step 7 — Run Selenium Tests (Optional)

Install Python dependencies:
```bash
cd ItemProcessor
python3 -m venv venv
source venv/bin/activate
pip install selenium webdriver-manager pytest pytest-html
```

Make sure the app is running in another terminal, then:
```bash
# Run auth tests separately
pytest Tests/test_auth_special.py -v

# Run all other tests
pytest Tests/test_login.py Tests/test_items.py \
       Tests/test_process.py Tests/test_tree.py \
       -v --html=TestReport.html --self-contained-html
```

Open `TestReport.html` in your browser to see the full test report.

---

## Project Structure
```
ItemProcessor/
├── Controllers/
│   ├── AccountController.cs    ← Login, Register, Logout
│   ├── ItemController.cs       ← CRUD + Search
│   └── ProcessController.cs    ← Processing + Tree View
├── Models/
│   ├── User.cs
│   ├── Item.cs
│   ├── ProcessedItem.cs
│   ├── ApplicationDbContext.cs
│   └── ViewModels/
│       ├── LoginViewModel.cs
│       ├── ProcessItemViewModel.cs
│       └── ItemTreeViewModel.cs
├── Views/
│   ├── Account/
│   │   ├── Login.cshtml
│   │   └── Register.cshtml
│   ├── Item/
│   │   ├── Index.cshtml
│   │   ├── Create.cshtml
│   │   └── Edit.cshtml
│   └── Process/
│       ├── Index.cshtml
│       ├── Create.cshtml
│       ├── Tree.cshtml
│       ├── AllTrees.cshtml
│       └── _TreeNode.cshtml
├── Tests/
│   ├── conftest.py
│   ├── test_login.py
│   ├── test_auth_special.py
│   ├── test_items.py
│   ├── test_process.py
│   └── test_tree.py
├── appsettings.json
├── Program.cs
├── database_script.sql         ← Run this first
├── TestReport.html             ← Generated after running tests
└── README.md                 
```

---

## Default Credentials
```
Email:    admin@test.com
Password: Admin@123
```

---

## Notes

- Items are soft deleted — they are never permanently removed from the database
- Output weight of child items cannot exceed the parent item weight
- The tree view supports unlimited depth of parent-child hierarchy
- All passwords are hashed using SHA256 before storing
