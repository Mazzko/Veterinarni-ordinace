using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VeterinarniOrdinace.Models;

namespace VeterinarniOrdinace.Data
{
    public static class DbSeed
    {
        public static void SeedIfEmpty()
        {
            var ownersRepo = new OwnerRepository();
            var animalsRepo = new AnimalRepository();
            var visitsRepo = new VisitRepository();

            if (ownersRepo.GetAll().Any()) return;

            var o1 = new Owner { FullName = "Jan Novák", Phone = "777 111 222", Email = "novak@email.cz", Address = "Brno" };
            var o2 = new Owner { FullName = "Petr Svoboda", Phone = "777 333 444", Email = "svoboda@email.cz", Address = "Praha" };
            var o3 = new Owner { FullName = "Eva Dvořáková", Phone = "777 555 666", Email = "eva@email.cz", Address = "Ostrava" };

            o1.Id = ownersRepo.Insert(o1);
            o2.Id = ownersRepo.Insert(o2);
            o3.Id = ownersRepo.Insert(o3);

            var a1 = new Animal { Name = "Rex", Species = "Pes", Breed = "Labrador", OwnerId = o1.Id };
            var a2 = new Animal { Name = "Micka", Species = "Kočka", Breed = "Evropská", OwnerId = o1.Id };
            var a3 = new Animal { Name = "Beny", Species = "Pes", Breed = "Border kolie", OwnerId = o2.Id };
            var a4 = new Animal { Name = "Koko", Species = "Papoušek", Breed = "Ara", OwnerId = o3.Id };

            a1.Id = animalsRepo.Insert(a1);
            a2.Id = animalsRepo.Insert(a2);
            a3.Id = animalsRepo.Insert(a3);
            a4.Id = animalsRepo.Insert(a4);

            var v1 = new Visit
            {
                AnimalId = a1.Id,
                Date = DateTime.Now.AddDays(-10).Date.AddHours(10).AddMinutes(30),
                Reason = "Očkování",
                Diagnosis = "OK",
                Price = 650,
                Status = "Dokončeno"
            };

            var v2 = new Visit
            {
                AnimalId = a2.Id,
                Date = DateTime.Now.AddDays(-3).Date.AddHours(16),
                Reason = "Kašel",
                Diagnosis = "Zánět horních cest dýchacích",
                Price = 900,
                Status = "Dokončeno"
            };

            var v3 = new Visit
            {
                AnimalId = a3.Id,
                Date = DateTime.Now.AddDays(2).Date.AddHours(9),
                Reason = "Kontrola",
                Diagnosis = null,
                Price = null, // pokud máš double? Price
                Status = "Naplánováno"
            };

            v1.Id = visitsRepo.Insert(v1);
            v2.Id = visitsRepo.Insert(v2);
            v3.Id = visitsRepo.Insert(v3);
        }
    }
}
