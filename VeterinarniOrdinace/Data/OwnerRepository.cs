using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VeterinarniOrdinace.Models;


namespace VeterinarniOrdinace.Data
{
    public class OwnerRepository
    {
        private readonly string _cs = DatabaseInitializer.ConnectionString;

        public List<Owner> GetAll()
        {
            var result = new List<Owner>();

            using var con = new SqliteConnection(this._cs);
            con.Open();

            using var cmd = con.CreateCommand();
            cmd.CommandText = "SELECT Id, FullName, Phone, Email, Address FROM Owners ORDER BY Id;";

            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                result.Add(new Owner
                {
                    Id = r.GetInt32(0),
                    FullName = r.GetString(1),
                    Phone = r.IsDBNull(2) ? null : r.GetString(2),
                    Email = r.IsDBNull(3) ? null : r.GetString(3),
                    Address = r.IsDBNull(4) ? null : r.GetString(4),
                });
            }
            return result;
        }
        public int Insert(Owner o)
        {
            using var con = new SqliteConnection(this._cs);
            con.Open();

            using var cmd = con.CreateCommand();
            cmd.CommandText =
            """
            INSERT INTO Owners (FullName, Phone, Email, Address)
            VALUES ($name, $phone, $email, $address);
            SELECT last_insert_rowid();
            """;
            cmd.Parameters.AddWithValue("$name", o.FullName);
            cmd.Parameters.AddWithValue("$phone", (object?)o.Phone ?? DBNull.Value);
            cmd.Parameters.AddWithValue("$email", (object?)o.Email ?? DBNull.Value);
            cmd.Parameters.AddWithValue("$address", (object?)o.Address ?? DBNull.Value);

            var id = (long)cmd.ExecuteScalar()!;
            return (int)id;
        }

        public void Update(Owner o)
        {
            using var con = new SqliteConnection(this._cs);
            con.Open();

            using var cmd = con.CreateCommand();
            cmd.CommandText =
            """
            UPDATE Owners
            SET FullName=$name, Phone=$phone, Email=$email, Address=$address
            WHERE Id=$id;
            """;
            cmd.Parameters.AddWithValue("$id", o.Id);
            cmd.Parameters.AddWithValue("$name", o.FullName);
            cmd.Parameters.AddWithValue("$phone", (object?)o.Phone ?? DBNull.Value);
            cmd.Parameters.AddWithValue("$email", (object?)o.Email ?? DBNull.Value);
            cmd.Parameters.AddWithValue("$address", (object?)o.Address ?? DBNull.Value);

            cmd.ExecuteNonQuery();
        }

        public void Delete(int id)
        {
            using var con = new SqliteConnection(this._cs);
            con.Open();

            using var cmd = con.CreateCommand();
            cmd.CommandText = "DELETE FROM Owners WHERE Id=$id;";
            cmd.Parameters.AddWithValue("$id", id);
            cmd.ExecuteNonQuery();
        }
    }
}
