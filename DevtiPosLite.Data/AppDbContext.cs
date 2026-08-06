using DevtiPosLite.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace DevtiPosLite.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public override int SaveChanges()
    {
        SetTimestamps();
        return base.SaveChanges();
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        SetTimestamps();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SetTimestamps();
        return await base.SaveChangesAsync(cancellationToken);
    }

    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        SetTimestamps();
        return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void SetTimestamps()
    {
        var now = DateTime.Now;
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = now;
                entry.Entity.UpdatedAt = now;
            }
            if (entry.State == EntityState.Modified)
                entry.Entity.UpdatedAt = now;
        }
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Denomination> Denominations => Set<Denomination>();
    public DbSet<Sale> Sales => Set<Sale>();
    public DbSet<SaleDetail> SaleDetails => Set<SaleDetail>();
    public DbSet<Return> Returns => Set<Return>();
    public DbSet<CashOpening> CashOpenings => Set<CashOpening>();
    public DbSet<CashoutHistory> CashoutHistories => Set<CashoutHistory>();
    public DbSet<StoreConfig> StoreConfigs => Set<StoreConfig>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(e =>
        {
            e.HasIndex(u => u.Email).IsUnique();
            e.HasOne(u => u.Role).WithMany(r => r.Users).HasForeignKey(u => u.RoleId);
        });

        modelBuilder.Entity<Role>(e =>
        {
            e.HasIndex(r => r.Name).IsUnique();
        });

        modelBuilder.Entity<Permission>(e =>
        {
            e.HasIndex(p => p.Name).IsUnique();
        });

        modelBuilder.Entity<RolePermission>(e =>
        {
            e.HasKey(rp => new { rp.RoleId, rp.PermissionId });
            e.HasOne(rp => rp.Role).WithMany(r => r.RolePermissions).HasForeignKey(rp => rp.RoleId);
            e.HasOne(rp => rp.Permission).WithMany(p => p.RolePermissions).HasForeignKey(rp => rp.PermissionId);
        });

        modelBuilder.Entity<Category>(e =>
        {
            e.HasIndex(c => c.Name).IsUnique();
        });

        modelBuilder.Entity<Product>(e =>
        {
            e.HasIndex(p => p.Barcode);
            e.HasOne(p => p.Category).WithMany(c => c.Products).HasForeignKey(p => p.CategoryId);
        });

        modelBuilder.Entity<Denomination>(e =>
        {
            e.HasIndex(d => d.Value).IsUnique();
        });

        modelBuilder.Entity<Sale>(e =>
        {
            e.HasOne(s => s.User).WithMany().HasForeignKey(s => s.UserId);
        });

        modelBuilder.Entity<SaleDetail>(e =>
        {
            e.HasOne(sd => sd.Sale).WithMany(s => s.Details).HasForeignKey(sd => sd.SaleId);
            e.HasOne(sd => sd.Product).WithMany().HasForeignKey(sd => sd.ProductId);
        });

        modelBuilder.Entity<Return>(e =>
        {
            e.HasOne(r => r.Sale).WithMany().HasForeignKey(r => r.SaleId);
            e.HasOne(r => r.Product).WithMany().HasForeignKey(r => r.ProductId);
            e.HasOne(r => r.User).WithMany().HasForeignKey(r => r.UserId);
            e.HasIndex(r => r.SaleReference);
        });

        modelBuilder.Entity<CashOpening>(e =>
        {
            e.HasOne(co => co.User).WithMany().HasForeignKey(co => co.UserId);
        });

        modelBuilder.Entity<CashoutHistory>(e =>
        {
            e.HasOne(ch => ch.CashOpening).WithOne(co => co.CashoutHistory).HasForeignKey<CashoutHistory>(ch => ch.CashOpeningId);
        });
    }

    public void CreateSchema()
    {
        Database.ExecuteSqlRaw(@"
CREATE TABLE IF NOT EXISTS Roles (Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,Name TEXT NOT NULL,Description TEXT,CreatedAt TEXT NOT NULL,UpdatedAt TEXT NOT NULL);
CREATE TABLE IF NOT EXISTS Permissions (Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,Name TEXT NOT NULL,CreatedAt TEXT NOT NULL,UpdatedAt TEXT NOT NULL);
CREATE TABLE IF NOT EXISTS RolePermissions (RoleId INTEGER NOT NULL,PermissionId INTEGER NOT NULL,PRIMARY KEY (RoleId, PermissionId),FOREIGN KEY (RoleId) REFERENCES Roles(Id) ON DELETE CASCADE,FOREIGN KEY (PermissionId) REFERENCES Permissions(Id) ON DELETE CASCADE);
CREATE TABLE IF NOT EXISTS Categories (Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,Name TEXT NOT NULL,Image TEXT,CreatedAt TEXT NOT NULL,UpdatedAt TEXT NOT NULL);
CREATE TABLE IF NOT EXISTS Denominations (Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,Type TEXT NOT NULL,Value REAL NOT NULL,Image TEXT,CreatedAt TEXT NOT NULL,UpdatedAt TEXT NOT NULL);
CREATE TABLE IF NOT EXISTS Users (Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,Name TEXT NOT NULL,Email TEXT NOT NULL,Phone TEXT,Status TEXT DEFAULT 'ACTIVE',Profile TEXT,Password TEXT NOT NULL,Image TEXT,RoleId INTEGER,CreatedAt TEXT NOT NULL,UpdatedAt TEXT NOT NULL,FOREIGN KEY (RoleId) REFERENCES Roles(Id));
CREATE TABLE IF NOT EXISTS Products (Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,Name TEXT NOT NULL,Barcode TEXT NOT NULL,Cost REAL NOT NULL,Price REAL NOT NULL,Stock INTEGER NOT NULL,Alerts INTEGER NOT NULL,Image TEXT,CategoryId INTEGER NOT NULL,CreatedAt TEXT NOT NULL,UpdatedAt TEXT NOT NULL,FOREIGN KEY (CategoryId) REFERENCES Categories(Id) ON DELETE CASCADE);
CREATE TABLE IF NOT EXISTS Sales (Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,Total REAL NOT NULL,Items INTEGER NOT NULL,Cash REAL NOT NULL,Change REAL NOT NULL,Status TEXT DEFAULT 'PAID',UserId INTEGER NOT NULL,CreatedAt TEXT NOT NULL,UpdatedAt TEXT NOT NULL,FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE);
CREATE TABLE IF NOT EXISTS SaleDetails (Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,Price REAL NOT NULL,Quantity INTEGER NOT NULL,ProductId INTEGER NOT NULL,SaleId INTEGER NOT NULL,CreatedAt TEXT NOT NULL,UpdatedAt TEXT NOT NULL,FOREIGN KEY (SaleId) REFERENCES Sales(Id) ON DELETE CASCADE,FOREIGN KEY (ProductId) REFERENCES Products(Id) ON DELETE CASCADE);
CREATE TABLE IF NOT EXISTS Returns (Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,SaleId INTEGER NOT NULL,ProductId INTEGER NOT NULL,UserId INTEGER NOT NULL,Quantity INTEGER NOT NULL,RefundAmount REAL NOT NULL,SaleReference TEXT NOT NULL,CreatedAt TEXT NOT NULL,UpdatedAt TEXT NOT NULL,FOREIGN KEY (SaleId) REFERENCES Sales(Id) ON DELETE CASCADE,FOREIGN KEY (ProductId) REFERENCES Products(Id) ON DELETE CASCADE,FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE);
CREATE TABLE IF NOT EXISTS CashOpenings (Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,UserId INTEGER NOT NULL,OpeningAmount REAL NOT NULL,ClosingAmount REAL,Status TEXT DEFAULT 'OPEN',Notes TEXT,CreatedAt TEXT NOT NULL,UpdatedAt TEXT NOT NULL,FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE);
CREATE TABLE IF NOT EXISTS CashoutHistories (Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,CashOpeningId INTEGER NOT NULL,TotalSales REAL NOT NULL,TotalCash REAL NOT NULL,DiscrepancyAmount REAL,Notes TEXT,CreatedAt TEXT NOT NULL,UpdatedAt TEXT NOT NULL,FOREIGN KEY (CashOpeningId) REFERENCES CashOpenings(Id) ON DELETE CASCADE);
CREATE TABLE IF NOT EXISTS StoreConfigs (Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,StoreName TEXT,BusinessName TEXT,Phone TEXT,RFC TEXT,Address TEXT,LogoPath TEXT,IVARate REAL NOT NULL DEFAULT 0.16,TicketHeader TEXT,TicketFooter TEXT,PrintTicket INTEGER NOT NULL DEFAULT 1,ShowIVABreakdown INTEGER NOT NULL DEFAULT 1,CreatedAt TEXT NOT NULL,UpdatedAt TEXT NOT NULL);
CREATE UNIQUE INDEX IF NOT EXISTS IX_Roles_Name ON Roles(Name); CREATE UNIQUE INDEX IF NOT EXISTS IX_Permissions_Name ON Permissions(Name); CREATE UNIQUE INDEX IF NOT EXISTS IX_Categories_Name ON Categories(Name); CREATE UNIQUE INDEX IF NOT EXISTS IX_Denominations_Value ON Denominations(Value); CREATE UNIQUE INDEX IF NOT EXISTS IX_Users_Name ON Users(Name); CREATE INDEX IF NOT EXISTS IX_Users_Email ON Users(Email); CREATE INDEX IF NOT EXISTS IX_Products_Barcode ON Products(Barcode); CREATE INDEX IF NOT EXISTS IX_Products_CategoryId ON Products(CategoryId); CREATE INDEX IF NOT EXISTS IX_Sales_UserId ON Sales(UserId); CREATE INDEX IF NOT EXISTS IX_SaleDetails_ProductId ON SaleDetails(SaleId); CREATE INDEX IF NOT EXISTS IX_SaleDetails_SaleId ON SaleDetails(SaleId); CREATE INDEX IF NOT EXISTS IX_Returns_SaleReference ON Returns(SaleReference);
");
        MigrateSchema();
    }

    public void MigrateSchema()
    {
        try { Database.ExecuteSqlRaw("ALTER TABLE Denominations ADD COLUMN Image TEXT"); } catch { }
        try { Database.ExecuteSqlRaw("CREATE UNIQUE INDEX IF NOT EXISTS IX_Users_Name ON Users(Name)"); } catch { }
        Database.ExecuteSqlRaw("CREATE TABLE IF NOT EXISTS StoreConfigs (Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,StoreName TEXT,BusinessName TEXT,Phone TEXT,RFC TEXT,Address TEXT,LogoPath TEXT,IVARate REAL NOT NULL DEFAULT 0.16,TicketHeader TEXT,TicketFooter TEXT,PrintTicket INTEGER NOT NULL DEFAULT 1,ShowIVABreakdown INTEGER NOT NULL DEFAULT 1,CreatedAt TEXT NOT NULL,UpdatedAt TEXT NOT NULL);");
        try { Database.ExecuteSqlRaw("UPDATE Sales SET CreatedAt = datetime('now') WHERE CreatedAt < '2024-01-01'"); } catch { }
        try { Database.ExecuteSqlRaw("ALTER TABLE StoreConfigs ADD COLUMN DefaultPrinter TEXT DEFAULT ''"); } catch { }
        try { Database.ExecuteSqlRaw("ALTER TABLE StoreConfigs ADD COLUMN AutoPrint INTEGER NOT NULL DEFAULT 0"); } catch { }
        try { Database.ExecuteSqlRaw("ALTER TABLE StoreConfigs ADD COLUMN PrintCopies INTEGER NOT NULL DEFAULT 1"); } catch { }
        try { Database.ExecuteSqlRaw("UPDATE StoreConfigs SET DefaultPrinter=COALESCE(DefaultPrinter,''),StoreName=COALESCE(StoreName,''),BusinessName=COALESCE(BusinessName,''),Phone=COALESCE(Phone,''),RFC=COALESCE(RFC,''),Address=COALESCE(Address,''),LogoPath=COALESCE(LogoPath,''),TicketHeader=COALESCE(TicketHeader,''),TicketFooter=COALESCE(TicketFooter,'')"); } catch { }
    }

    public async Task SeedAsync()
    {
        var allPermissionNames = new[]
        {
            "Category_index", "Products_index", "Coins_index",
            "Sales_index", "Reports_index", "Cashout_index",
            "Users_index", "Roles_index", "Permission_index",
            "Assing_index", "Config_index",
        };

        foreach (var name in allPermissionNames)
        {
            if (!await Permissions.AnyAsync(p => p.Name == name))
                Permissions.Add(new Permission { Name = name });
        }

        if (!await Roles.AnyAsync(r => r.Name == "Admin"))
            Roles.Add(new Role { Name = "Admin", Description = "Administrator" });

        if (!await Roles.AnyAsync(r => r.Name == "Cajero"))
            Roles.Add(new Role { Name = "Cajero", Description = "Cashier" });

        await SaveChangesAsync();

        var adminRole = await Roles.FirstAsync(r => r.Name == "Admin");
        var allPerms = await Permissions.ToListAsync();
        foreach (var p in allPerms)
        {
            if (!await RolePermissions.AnyAsync(rp => rp.RoleId == adminRole.Id && rp.PermissionId == p.Id))
                RolePermissions.Add(new RolePermission { RoleId = adminRole.Id, PermissionId = p.Id });
        }

        var cajeroRole = await Roles.FirstAsync(r => r.Name == "Cajero");
        var cajeroPerms = new[] { "Category_index", "Products_index", "Coins_index", "Sales_index", "Reports_index", "Cashout_index" };
        foreach (var name in cajeroPerms)
        {
            var perm = await Permissions.FirstAsync(p => p.Name == name);
            if (!await RolePermissions.AnyAsync(rp => rp.RoleId == cajeroRole.Id && rp.PermissionId == perm.Id))
                RolePermissions.Add(new RolePermission { RoleId = cajeroRole.Id, PermissionId = perm.Id });
        }

        if (!await Users.AnyAsync(u => u.Name == "Admin"))
        {
            var seedPassword = BCrypt.Net.BCrypt.HashPassword("admin123");
            Users.Add(new User
            {
                Name = "Admin",
                Email = "admin@devti.com",
                Password = seedPassword,
                RoleId = adminRole.Id,
                Status = "ACTIVE",
                Profile = "admin"
            });
        }

        if (!await Denominations.AnyAsync())
        {
            var denominations = new[]
            {
                new Denomination { Type = "BILLETE", Value = 1000m },
                new Denomination { Type = "BILLETE", Value = 500m },
                new Denomination { Type = "BILLETE", Value = 200m },
                new Denomination { Type = "BILLETE", Value = 100m },
                new Denomination { Type = "MONEDA", Value = 25m },
            };
            Denominations.AddRange(denominations);
        }

        await SaveChangesAsync();
    }
}
