# School Supplies POS

A point-of-sale system for a school supplies store, with **Manager** and **Staff** roles.
It comes in three versions that share the same features and rules.

**Live demo (runs in your browser):** https://reezerizz.github.io/school-supply-pos/

| Folder | What it is |
|---|---|
| `docs/` | Web version (single `index.html`, no build step). This is what GitHub Pages serves. |
| `windows-app/` | Windows Forms desktop version (C#, .NET 10). |
| `console-version/` | Original menu-driven console version (C#). |

## Features

**Both roles:** dashboard (today's sales, orders, average order value, low stock, revenue chart, live sales, top products), point of sale with cart / cash / change / receipts, inventory search, purchase records, void last sale, reprint receipt.

**Manager only:** add / remove items, update stock, change prices, void any sale, sales reports (sales by hour, revenue by category) and CSV export.

The demo starts with a week of sample sales so the charts are not empty. Use **Reset demo data** in the sidebar to restore it.
The web version saves its data in your browser (localStorage); the desktop versions keep data in memory only.

## Run the web version locally

Open `docs/index.html` in any browser.

## Run the Windows app

Requires the .NET 10 SDK and Windows.

```
cd windows-app
dotnet restore
dotnet run
```

(If you have a different SDK, change `TargetFramework` in `SchoolSupplyPOS.csproj`, e.g. `net8.0-windows`.)

## Run the console version

Create a console project and use `console-version/SchoolSupplyPOS.cs` as its `Program.cs`.
