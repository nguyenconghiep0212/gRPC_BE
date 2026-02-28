using IotGrpcLearning.Classes;
using IotGrpcLearning.Models;
using Microsoft.Data.Sqlite;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace IotGrpcLearning.Infrastructure
{
	public sealed class Seeder
	{
		private readonly ISqliteConnectionFactory _dbFactory;

		public Seeder(ISqliteConnectionFactory dbFactory)
		{
			_dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
		}

		private sealed record MachineSeed(
			string Name,
			string Details,
			string VendorName,
			decimal PurchasePrice,
			DateTime PurchaseDate,
			string SiteName,
			string? Alias = null
		);

		/// <summary>
		/// Delete all rows from known application tables so seeding can start from a clean state.
		/// Uses a transaction and temporarily disables foreign key enforcement.
		/// WARNING: destructive operation — execute only in non-production environments.
		/// </summary>
		public async Task ClearAllDataAsync(CancellationToken ct = default)
		{
			// Order chosen to avoid FK constraint violations when deleting rows:
			var tables = new[]
			{
				"MachineStatus",
				"Machines",
				"Sites",
				"Divisions",
				"Roles",
				"Customers",
				"Vendors"
			};

			using var conn = _dbFactory.CreateConnection();
			await conn.OpenAsync(ct);

			using var tx = conn.BeginTransaction();
			using var cmd = conn.CreateCommand();
			cmd.Transaction = tx;

			// Temporarily disable foreign key checks so deletes succeed regardless of FK order
			cmd.CommandText = "PRAGMA foreign_keys = OFF;";
			await cmd.ExecuteNonQueryAsync(ct);

			foreach (var table in tables)
			{
				ct.ThrowIfCancellationRequested();

				// Delete all rows
				cmd.CommandText = $"DELETE FROM \"{table}\";";
				await cmd.ExecuteNonQueryAsync(ct);

				// Reset AUTOINCREMENT counter for the table if present
				cmd.CommandText = $"DELETE FROM sqlite_sequence WHERE name = '{table}';";
				try
				{
					await cmd.ExecuteNonQueryAsync(ct);
				}
				catch
				{
					// ignore - sqlite_sequence may not exist or table may not use AUTOINCREMENT
				}
			}

			// Re-enable foreign key enforcement
			cmd.CommandText = "PRAGMA foreign_keys = ON;";
			await cmd.ExecuteNonQueryAsync(ct);

			tx.Commit();
		}


		/// <summary>
		/// Idempotently inserts a small set of sample into tables.
		/// Uses a transaction and parameterized SQL to avoid SQL injection.
		/// </summary>
		public async Task SeedVendorAsync(CancellationToken ct = default)
		{
			var samples = new[]
			{
				"Semiki",
				"HCL",
				"MicroTest",
				"Axxon",
				"NextPCB"
			};

			using var conn = _dbFactory.CreateConnection();
			await conn.OpenAsync(ct);

			using var tx = conn.BeginTransaction();
			using var cmd = conn.CreateCommand();
			cmd.Transaction = tx;

			// Insert only if that name does not already exist (idempotent).
			cmd.CommandText = @"
                INSERT INTO Vendors (name)
                SELECT @name
                WHERE NOT EXISTS (SELECT 1 FROM Vendors WHERE name = @name LIMIT 1);
            ";

			var p = cmd.CreateParameter();
			p.ParameterName = "@name";
			cmd.Parameters.Add(p);

			foreach (var name in samples)
			{
				ct.ThrowIfCancellationRequested();
				p.Value = name;
				await cmd.ExecuteNonQueryAsync(ct);
			}

			tx.Commit();
		}

		public async Task SeedCustomerAsync(CancellationToken ct = default)
		{
			var samples = new[]
			{
				"Samsung",
				"Apple",
				"LG",
				"Asus",
			};

			using var conn = _dbFactory.CreateConnection();
			await conn.OpenAsync(ct);

			using var tx = conn.BeginTransaction();
			using var cmd = conn.CreateCommand();
			cmd.Transaction = tx;

			// Insert only if that name does not already exist (idempotent).
			cmd.CommandText = @"
                INSERT INTO Customers (name)
                SELECT @name
                WHERE NOT EXISTS (SELECT 1 FROM Customers WHERE name = @name LIMIT 1);
            ";

			var p = cmd.CreateParameter();
			p.ParameterName = "@name";
			cmd.Parameters.Add(p);

			foreach (var name in samples)
			{
				ct.ThrowIfCancellationRequested();
				p.Value = name;
				await cmd.ExecuteNonQueryAsync(ct);
			}

			tx.Commit();
		}

		public async Task SeedRolesAsync(CancellationToken ct = default)
		{
			var samples = new[]
			{
				"Operator",
				"Project Manager",
				"Failure Analysis",
				"Software Engineer",
				"Line Manager",
				"Automation Engineer",
				"Division Director",
				"Director"
			};

			using var conn = _dbFactory.CreateConnection();
			await conn.OpenAsync(ct);

			using var tx = conn.BeginTransaction();
			using var cmd = conn.CreateCommand();
			cmd.Transaction = tx;

			// Insert only if that name does not already exist (idempotent).
			cmd.CommandText = @"
                INSERT INTO Roles (name)
                SELECT @name
                WHERE NOT EXISTS (SELECT 1 FROM Roles WHERE name = @name LIMIT 1);
            ";

			var p = cmd.CreateParameter();
			p.ParameterName = "@name";
			cmd.Parameters.Add(p);

			foreach (var name in samples)
			{
				ct.ThrowIfCancellationRequested();
				p.Value = name;
				await cmd.ExecuteNonQueryAsync(ct);
			}

			tx.Commit();
		}

		public async Task SeedDivisionAsync(CancellationToken ct = default)
		{
			var samples = new[]
			{
				"Software Divions",
				"Electronic Division",
				"Failure Analysis Division",
				"Engineering Division",
				"Manufactoring Division",
				"Business Division"
			};

			using var conn = _dbFactory.CreateConnection();
			await conn.OpenAsync(ct);

			using var tx = conn.BeginTransaction();
			using var cmd = conn.CreateCommand();
			cmd.Transaction = tx;

			// Insert only if that name does not already exist (idempotent).
			cmd.CommandText = @"
                INSERT INTO Divisions (name)
                SELECT @name
                WHERE NOT EXISTS (SELECT 1 FROM Divisions WHERE name = @name LIMIT 1);
            ";

			var p = cmd.CreateParameter();
			p.ParameterName = "@name";
			cmd.Parameters.Add(p);

			foreach (var name in samples)
			{
				ct.ThrowIfCancellationRequested();
				p.Value = name;
				await cmd.ExecuteNonQueryAsync(ct);
			}

			tx.Commit();
		}

		public async Task SeedSiteAsync(CancellationToken ct = default)
		{
			var samples = new[]
			{
				new { Name = "Korea", Location = "{lat:37.5665,long:126.9780}", Address = "Sejong-daero 209, Jongno-gu, Seoul" },          // Seoul
				new { Name = "China", Location = "{lat:39.9042,long:116.4074}", Address = "No. 1, East Chang'an Avenue, Dongcheng, Beijing" }, // Beijing
				new { Name = "Vietnam", Location = "{lat:21.0278,long:105.8342}", Address = "Ba Dinh District, Hanoi" },                  // Hanoi
				new { Name = "Indonesia", Location = "{lat:-6.2088,long:106.8456}", Address = "Jl. Medan Merdeka Utara, Jakarta" },       // Jakarta
				new { Name = "Thailand", Location = "{lat:13.7563,long:100.5018}", Address = "Krung Kasem Road, Phra Nakhon, Bangkok" },  // Bangkok
				new { Name = "India", Location = "{lat:28.6139,long:77.2090}", Address = "Rajpath Marg, New Delhi" },                    // New Delhi
				new { Name = "Mexico", Location = "{lat:19.4326,long:-99.1332}", Address = "Plaza de la Constitución, Mexico City" },    // Mexico City
				new { Name = "Brazil", Location = "{lat:-15.8267,long:-47.9218}", Address = "Praça dos Três Poderes, Brasília" },         // Brasília
				new { Name = "UK", Location = "{lat:51.5074,long:-0.1278}", Address = "10 Downing Street, London" },                     // London
				new { Name = "USA", Location = "{lat:38.8977,long:-77.0365}", Address = "1600 Pennsylvania Avenue NW, Washington, DC" }, // Washington, D.C.
				new { Name = "Poland", Location = "{lat:52.2297,long:21.0122}", Address = "Krakowskie Przedmieście, Warsaw" }             // Warsaw
			};

			using var conn = _dbFactory.CreateConnection();
			await conn.OpenAsync(ct);

			using var tx = conn.BeginTransaction();
			using var cmd = conn.CreateCommand();
			cmd.Transaction = tx;

			// Insert only if that name does not already exist (idempotent).
			cmd.CommandText = @"
                INSERT INTO Sites (name, location, address)
                SELECT @name, @location, @address
                WHERE NOT EXISTS (SELECT 1 FROM Sites WHERE name = @name LIMIT 1);
           ";

			var pName = cmd.CreateParameter(); pName.ParameterName = "@name"; cmd.Parameters.Add(pName);
			var pLocation = cmd.CreateParameter(); pLocation.ParameterName = "@location"; cmd.Parameters.Add(pLocation);
			var pAddress = cmd.CreateParameter(); pAddress.ParameterName = "@address"; cmd.Parameters.Add(pAddress);

			foreach (var s in samples)
			{
				ct.ThrowIfCancellationRequested();
				pName.Value = s.Name;
				pLocation.Value = s.Location;
				pAddress.Value = s.Address;
				await cmd.ExecuteNonQueryAsync(ct);
			}

			tx.Commit();
		}

		readonly MachineSeed[] samples =
				[
					new MachineSeed("Line-01","Primary assembly line","Semiki",120000.00m,DateTime.UtcNow.AddYears(-2),"Korea","Line-01"),
					new MachineSeed("Line-02","Secondary assembly line","Semiki",95000.50m,DateTime.UtcNow.AddYears(-1).AddMonths(-3),"Korea","Line-02"),
					new MachineSeed("Furnace-1","High-temp reflow furnace","HCL",45000.75m,DateTime.UtcNow.AddYears(-3),"China","Furnace-1"),
					new MachineSeed("Tester-XL","Automated inspection tester","MicroTest",30000.00m,DateTime.UtcNow.AddYears(-4).AddMonths(6),"Vietnam","Tester-XL"),
					new MachineSeed("RobotArm-7","Pick-and-place robotic arm","Axxon",75000.00m,DateTime.UtcNow.AddYears(-1),"Indonesia","RobotArm-7")
				];
		public async Task SeedMachineAsync(CancellationToken ct = default)
		{

			using var conn = _dbFactory.CreateConnection();
			await conn.OpenAsync(ct);
			using var tx = conn.BeginTransaction();
			using var cmd = conn.CreateCommand();
			cmd.Transaction = tx;

			cmd.CommandText = @"
                INSERT INTO Machines ( 
                    name,
					alias,
                    details,
                    vendor,
                    purchase_price,
                    purchase_date, 
                    site
                )
                SELECT 
                    @name,
					@alias,
                    @details,
                    @vendor,
                    @purchase_price,
                    @purchase_date, 
                    @site
                WHERE NOT EXISTS (SELECT 1 FROM Machines WHERE name = @name LIMIT 1);
            ";

			var pName = cmd.CreateParameter(); pName.ParameterName = "@name"; cmd.Parameters.Add(pName);
			var pAlias = cmd.CreateParameter(); pAlias.ParameterName = "@alias"; cmd.Parameters.Add(pAlias);
			var pDetails = cmd.CreateParameter(); pDetails.ParameterName = "@details"; cmd.Parameters.Add(pDetails);
			var pVendor = cmd.CreateParameter(); pVendor.ParameterName = "@vendor"; cmd.Parameters.Add(pVendor);
			var pPurchasePrice = cmd.CreateParameter(); pPurchasePrice.ParameterName = "@purchase_price"; cmd.Parameters.Add(pPurchasePrice);
			var pPurchaseDate = cmd.CreateParameter(); pPurchaseDate.ParameterName = "@purchase_date"; cmd.Parameters.Add(pPurchaseDate);
			var pSite = cmd.CreateParameter(); pSite.ParameterName = "@site"; cmd.Parameters.Add(pSite);


			foreach (var sample in samples)
			{
				ct.ThrowIfCancellationRequested();
				// ensure vendor and site exist and get their ids
				var vendorId = await EnsureLookupIdAsync(conn, tx, ct, "Vendors", sample.VendorName, "", null);
				var siteId = await EnsureLookupIdAsync(conn, tx, ct, "Sites", sample.SiteName, "location, address", "'', ''");

				pName.Value = sample.Name;
				pAlias.Value = sample.Name;
				pDetails.Value = sample.Details ?? (object)DBNull.Value;
				pVendor.Value = vendorId;
				// SQLite accepts REAL for decimal; convert to double
				pPurchasePrice.Value = Convert.ToDouble(sample.PurchasePrice);
				pPurchaseDate.Value = DateTime.SpecifyKind(sample.PurchaseDate, DateTimeKind.Utc);
				pSite.Value = siteId;

				await cmd.ExecuteNonQueryAsync(ct);
			}

			tx.Commit();
			await SeedMachineStatus(ct);
		}

		public async Task SeedMachineStatus(CancellationToken ct = default)
		{
			using var conn = _dbFactory.CreateConnection();
			await conn.OpenAsync(ct);
			using var tx = conn.BeginTransaction();
			using var cmd = conn.CreateCommand();
			cmd.Transaction = tx;

			cmd.CommandText = @"
                INSERT INTO MachineStatus ( 
                    machine,
					health,
                    is_online,
                    last_online
                )
                SELECT 
                    @machine,
					@health,
                    @is_online,
                    @last_online
                WHERE NOT EXISTS (SELECT 1 FROM MachineStatus WHERE machine = @machine LIMIT 1);
            ";
			var pMachine = cmd.CreateParameter(); pMachine.ParameterName = "@machine"; cmd.Parameters.Add(pMachine);
			var pHealth = cmd.CreateParameter(); pHealth.ParameterName = "@health"; cmd.Parameters.Add(pHealth);
			var pIsOnline = cmd.CreateParameter(); pIsOnline.ParameterName = "@is_online"; cmd.Parameters.Add(pIsOnline);
			var pLastOnline = cmd.CreateParameter(); pLastOnline.ParameterName = "@last_online"; cmd.Parameters.Add(pLastOnline);


			foreach (var sample in samples)
			{
				ct.ThrowIfCancellationRequested();
				// ensure vendor and site exist and get their ids
				var MachineId = await EnsureLookupIdAsync(conn, tx, ct, "Machines", sample.Name, "", null);

				pMachine.Value = MachineId;
				pHealth.Value = MachineHealth.Unavailable;
				pIsOnline.Value = MachineState.Offline;
				pLastOnline.Value = DateTime.Now;

				await cmd.ExecuteNonQueryAsync(ct);
			}

			tx.Commit();
		}

		#region ==== Helper ====
		// helper that ensures a lookup row exists and returns its id (in same transaction)
		async Task<int> EnsureLookupIdAsync(SqliteConnection conn, SqliteTransaction tx, CancellationToken ct, string table, string name, string additionalInsertColumns = "", object? additionalInsertValues = null)
		{
			// try select
			using var sel = conn.CreateCommand();
			sel.Transaction = tx;
			sel.CommandText = $"SELECT id FROM \"{table}\" WHERE name = @name LIMIT 1;";
			var pSel = sel.CreateParameter();
			pSel.ParameterName = "@name";
			pSel.Value = name;
			sel.Parameters.Add(pSel);

			var scalar = await sel.ExecuteScalarAsync(ct);
			if (scalar != null && scalar != DBNull.Value)
				return Convert.ToInt32(scalar);

			// not found -> insert minimal row
			using var ins = conn.CreateCommand();
			ins.Transaction = tx;

			if (string.IsNullOrEmpty(additionalInsertColumns))
			{
				ins.CommandText = $"INSERT INTO \"{table}\" (name) VALUES (@name); SELECT last_insert_rowid();";
			}
			else
			{
				ins.CommandText = $"INSERT INTO \"{table}\" (name, {additionalInsertColumns}) VALUES (@name, {additionalInsertValues}); SELECT last_insert_rowid();";
			}

			var pIns = ins.CreateParameter();
			pIns.ParameterName = "@name";
			pIns.Value = name;
			ins.Parameters.Add(pIns);

			var newId = await ins.ExecuteScalarAsync(ct);
			return Convert.ToInt32(newId);
		}
		#endregion
	}
}