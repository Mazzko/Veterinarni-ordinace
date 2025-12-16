using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VeterinarniOrdinace.Data
{
    public static class DatabaseInitializer
    {
        private const string DbFileName = "veterina.db";
        public static string DbPath =>
            global::System.IO.Path.Combine(global::System.AppContext.BaseDirectory, "Data", DbFileName);

        public static string ConnectionString => $"Data Source={DbPath}";

        public static void Initialize()
        {
            var dir = global::System.IO.Path.GetDirectoryName(DbPath);
            if (!string.IsNullOrWhiteSpace(dir))
                global::System.IO.Directory.CreateDirectory(dir);

            using var connecition = new SqliteConnection(ConnectionString);
            connecition.Open();

            using var cmd = connecition.CreateCommand();
            cmd.CommandText =
            """
            PRAGMA foreign_keys = ON;

            CREATE TABLE IF NOT EXISTS Owners (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                FullName TEXT NOT NULL,
                Phone TEXT,
                Email TEXT,
                Address TEXT
            );

            CREATE TABLE IF NOT EXISTS Animals (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                Species TEXT NOT NULL,
                Breed TEXT,
                OwnerId INTEGER NOT NULL,
                FOREIGN KEY (OwnerId) REFERENCES Owners(Id) ON DELETE CASCADE
            );

            CREATE TABLE IF NOT EXISTS Visits (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                AnimalId INTEGER NOT NULL,
                Date TEXT NOT NULL,
                Reason TEXT,
                Diagnosis TEXT,
                Price REAL,
                Status TEXT,
                FOREIGN KEY (AnimalId) REFERENCES Animals(Id) ON DELETE CASCADE
            );
            """;
            cmd.ExecuteNonQuery();

        }
    }
}
