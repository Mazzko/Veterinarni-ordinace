using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VeterinarniOrdinace.Models;

namespace VeterinarniOrdinace.Data
{
    internal class AnimalRepository
    {
        private readonly string _cs = DatabaseInitializer.ConnectionString;

        public List<Animal> GetAll()
        {
            var result = new List<Animal>();

            using var con = new SqliteConnection(this._cs);
            con.Open();

            using var cmd = con.CreateCommand();
            cmd.CommandText = "SELECT Id, Name, Species, Breed, OwnerId FROM Animals ORDER BY Id;";

            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                result.Add(new Animal
                {
                    Id = r.GetInt32(0),
                    Name = r.GetString(1),
                    Species = r.GetString(2),
                    Breed = r.IsDBNull(3) ? null : r.GetString(3),
                    OwnerId = r.GetInt32(4)
                });
            }
            return result;
        }
        public int Insert(Animal a)
        {
            using var con = new SqliteConnection(this._cs);
            con.Open();

            using var cmd = con.CreateCommand();
            cmd.CommandText =
            """
            INSERT INTO Animals (Name, Species, Breed, OwnerId)
            VALUES ($name, $species, $breed, $ownerId);
            SELECT last_insert_rowid();
            """;

            cmd.Parameters.AddWithValue("$name", a.Name);
            cmd.Parameters.AddWithValue("$species", a.Species);
            cmd.Parameters.AddWithValue("$breed", (object?)a.Breed ?? DBNull.Value);
            cmd.Parameters.AddWithValue("$ownerId", a.OwnerId);

            var id = (long)cmd.ExecuteScalar()!;
            return (int)id;
        }

        public void Update(Animal a)
        {
            using var con = new SqliteConnection(this._cs);
            con.Open();

            using var cmd = con.CreateCommand();
            cmd.CommandText =
            """
            UPDATE Animals
            SET Name=$name, Species=$species, Breed=$breed, OwnerId=$ownerId
            WHERE Id=$id;
            """;

            cmd.Parameters.AddWithValue("$id", a.Id);
            cmd.Parameters.AddWithValue("$name", a.Name);
            cmd.Parameters.AddWithValue("$species", a.Species);
            cmd.Parameters.AddWithValue("$breed", (object?)a.Breed ?? DBNull.Value);
            cmd.Parameters.AddWithValue("$ownerId", a.OwnerId);

            cmd.ExecuteNonQuery();
        }

        public void Delete(int id)
        {
            using var con = new SqliteConnection(this._cs);
            con.Open();

            using var cmd = con.CreateCommand();
            cmd.CommandText = "DELETE FROM Animals WHERE Id=$id;";
            cmd.Parameters.AddWithValue("$id", id);
            cmd.ExecuteNonQuery();
        }
    }
}
