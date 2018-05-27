using ServiceStack;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using ServiceStack.OrmLite;
using ServiceStack.DataAnnotations;
using ServiceStack.Auth;
using System.Linq;

namespace Kursinis
{
    [Route("/Skelbimai/Rodyk", "POST")]
    public class RodykSkelbimus : IReturn<RodykSkelbimus>
    {
        public string Category { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int ExpirationInHours { get; set; }
        public int UserId { get; set; }
        [PrimaryKey]
        [Reference]
        public int SkelbimoId { get; set; }

        public RodykSkelbimus()
        {
        }
        public RodykSkelbimus(string Category, string Name, string Description, int ExpirationInHours, int UserId, int SkelbimoId)
        {
            this.Category = Category;
            this.Name = Name;
            this.Description = Description;
            this.ExpirationInHours = ExpirationInHours;
            this.UserId = UserId;
            this.SkelbimoId = SkelbimoId;
        }
    }
    public class SkelbimuServisas : Service
    {

        public object Post(RodykSkelbimus request)
        {
            var pasirinkimas = new List<string> { "Perku/parduodu", "Skelbiu", "Is A į B" };
         
            var session = this.SessionAs<AuthUserSession>();
            var Skelbimas = new RodykSkelbimus
            {
                Category = request.Category,
                Name = request.Name,
                Description = request.Description,//Db.Select<string>() new { "Perku/parduodu", "Skelbiu", "Is A į B" };
                ExpirationInHours = request.ExpirationInHours,
                UserId = int.Parse(session.UserAuthId),
                SkelbimoId = request.SkelbimoId
            };
            Db.Save(Skelbimas);
            return Skelbimas;
        }
        public SkelbimuServisas()
        {

            Db.CreateTableIfNotExists<RodykSkelbimus>();
            if (!Db.ColumnExists<RodykSkelbimus>(P => P.SkelbimoId))
                Db.AddColumn<RodykSkelbimus>(P => P.SkelbimoId);

        }
}
    public class SkelbimuRodymas : Service
    {
        public List<RodykSkelbimus> FindSkelbimai(string input)
        {
            // input - tai tarkim tekstas, įvestas paieškos laukelyje, pagal kurį ieškom skelbimų

            // Pirmiausia vykdom paiešką pagal kategoriją (Category)
            var q = Db.From<RodykSkelbimus>().Where(x => x.Category.ToLower().Contains(input.ToLower())).Select(x => x);
            List<RodykSkelbimus> rows = Db.Select<RodykSkelbimus>(q);
            if (rows.IsEmpty())
            {
                // Jei nerado pagal kategoriją (Category), toliau ieškosime pagal vardą (Name)
                q = Db.From<RodykSkelbimus>().Where(x => x.Name.ToLower().Contains(input.ToLower())).Select(x => x);
                rows = Db.Select<RodykSkelbimus>(q);
                if (rows.IsEmpty())
                {
                    // Jei nerado pagal vardą (Name), toliau ieškosime pagal aprašymą (Description)
                    q = Db.From<RodykSkelbimus>().Where(x => x.Description.ToLower().Contains(input.ToLower())).Select(x => x);
                    rows = Db.Select<RodykSkelbimus>(q);
                }
            }
            return rows;
        }
        public List<RodykSkelbimus> GetSkelbimai()
        {
            var q = Db.From<RodykSkelbimus>().Select(x => x);
            List<RodykSkelbimus> rows = Db.Select<RodykSkelbimus>(q);
            return rows;
        }
        public int GetLatestSkelbimoId()
        {
            var q = Db.From<RodykSkelbimus>().Select(x => x.SkelbimoId);
            List<int> rows = Db.Select<int>(q);
            return rows.Max();
        }
        /// <summary>
        /// KAŽKAIP REALIZUOTI :D
        /// </summary>
        public void UzpildytiSkelbimuSarasa()
        {
            // Gauname visus skelbimus iš duomenų bazės
            List<RodykSkelbimus> skelbimai = GetSkelbimai();

            // Tada visus skelbimus svetainėje kažkaip perkeliame į lentelę "Skelbimų sąrašas"...
        }
        /// <summary>
        /// KAŽKAIP REALIZUOTI :D
        /// </summary>
        public void PridetiSukurtaSkelbima()
        {
            var session = this.SessionAs<AuthUserSession>();

            // Reikšmė iš ComboBox
            string comboBoxValue = "";

            // Reikšmė iš laukelio "Įveskite skelbimo pavadinimą"
            string skelbimoPavadinimas = "";

            // Reikšmė iš laukelio "Įveskite skelbimo aprašymą"
            string skelbimoAprasymas = "";

            // Skelbimo galiojimo laikas valandomis (kol kas bet koks skaičius)
            int kolKasBetKoksSkaicius = 48;

            // Ar buvo paspaustas mygtukas "Sukurti skelbimą"?
            bool mygtukasBuvoPaspaustas = true;

            // Jei mygtukas "Sukurti skelbimą" buvo paspaustas, tada...
            if (mygtukasBuvoPaspaustas)
            {
                // Sukuriame naują skelbimą iš įvestų reikšmių
                RodykSkelbimus skelbimas = new RodykSkelbimus(comboBoxValue, skelbimoPavadinimas, skelbimoAprasymas, kolKasBetKoksSkaicius, int.Parse(session.UserAuthId), GetLatestSkelbimoId() + 1);

                // Pridedame naują skelbimą į duomenų bazę
                AddSkelbimas(skelbimas);

                // Iš naujo užpildomas skelbimų sąrašas
                UzpildytiSkelbimuSarasa();
            }
        }
        public void AddSkelbimas(RodykSkelbimus skelbimas)
        {
            Db.Insert(new RodykSkelbimus { UserId = skelbimas.UserId, SkelbimoId = skelbimas.SkelbimoId, Category = skelbimas.Category, Name = skelbimas.Name, Description = skelbimas.Description, ExpirationInHours = skelbimas.ExpirationInHours });
        }
        public void DeleteSkelbimas(RodykSkelbimus skelbimas)
        {
            Db.Delete<RodykSkelbimus>(new { UserId = skelbimas.UserId, SkelbimoId = skelbimas.SkelbimoId, Category = skelbimas.Category, Name = skelbimas.Name, Desciption = skelbimas.Description, ExpirationInHours = skelbimas.ExpirationInHours });
        }
        public void DeleteSkelbimas(int id)
        {
            Db.Delete<RodykSkelbimus>(x => x.SkelbimoId == id);
        }
        public void ChangeCategory(int id, string oldCategory, string newCategory)
        {
            Db.UpdateAdd(() => new RodykSkelbimus { Category = newCategory }, where: x => x.SkelbimoId == id && x.Category == oldCategory);
        }
        public void ChangeName(int id, string oldName, string newName)
        {
            Db.UpdateAdd(() => new RodykSkelbimus { Name = newName }, where: x => x.SkelbimoId == id && x.Name == oldName);
        }
        public void ChangeDescription(int id, string oldDescription, string newDescription)
        {
            Db.UpdateAdd(() => new RodykSkelbimus { Description = newDescription }, where: x => x.SkelbimoId == id && x.Description == oldDescription);
        }
        public void ChangeExpirationInHours(int id, int oldExpirationInHours, int newExpirationInHours)
        {
            Db.UpdateAdd(() => new RodykSkelbimus { ExpirationInHours = newExpirationInHours - oldExpirationInHours }, where: x => x.SkelbimoId == id && x.ExpirationInHours == oldExpirationInHours);
        }
        public List<RodykSkelbimus> SortSkelbimaiByCategory()
        {
            var q = Db.From<RodykSkelbimus>().OrderBy(x => x.Category).Select(x => x);
            List<RodykSkelbimus> rows = Db.Select<RodykSkelbimus>(q);
            return rows;
        }
        public List<RodykSkelbimus> SortSkelbimaiByCategoryDescending()
        {
            var q = Db.From<RodykSkelbimus>().OrderByDescending(x => x.Category).Select(x => x);
            List<RodykSkelbimus> rows = Db.Select<RodykSkelbimus>(q);
            return rows;
        }
        public List<RodykSkelbimus> SortSkelbimaiByName()
        {
            var q = Db.From<RodykSkelbimus>().OrderBy(x => x.Name).Select(x => x);
            List<RodykSkelbimus> rows = Db.Select<RodykSkelbimus>(q);
            return rows;
        }
        public List<RodykSkelbimus> SortSkelbimaiByNameDescending()
        {
            var q = Db.From<RodykSkelbimus>().OrderByDescending(x => x.Name).Select(x => x);
            List<RodykSkelbimus> rows = Db.Select<RodykSkelbimus>(q);
            return rows;
        }
        public List<RodykSkelbimus> SortSkelbimaiByDescription()
        {
            var q = Db.From<RodykSkelbimus>().OrderBy(x => x.Description).Select(x => x);
            List<RodykSkelbimus> rows = Db.Select<RodykSkelbimus>(q);
            return rows;
        }
        public List<RodykSkelbimus> SortSkelbimaiByDescriptionDescending()
        {
            var q = Db.From<RodykSkelbimus>().OrderByDescending(x => x.Description).Select(x => x);
            List<RodykSkelbimus> rows = Db.Select<RodykSkelbimus>(q);
            return rows;
        }
        public List<RodykSkelbimus> SortSkelbimaiByExpirationInHours()
        {
            var q = Db.From<RodykSkelbimus>().OrderBy(x => x.ExpirationInHours).Select(x => x);
            List<RodykSkelbimus> rows = Db.Select<RodykSkelbimus>(q);
            return rows;
        }
        public List<RodykSkelbimus> SortSkelbimaiByExpirationInHoursDescending()
        {
            var q = Db.From<RodykSkelbimus>().OrderByDescending(x => x.ExpirationInHours).Select(x => x);
            List<RodykSkelbimus> rows = Db.Select<RodykSkelbimus>(q);
            return rows;
        }
    }
}

//public class descriptions
//{
//    public string describ { get; set; }
//    //{"Perku/parduodu", "Skelbiu", "Is A į B"}
//    //base.Response.AddHeader("Parduodu/perku", request.Description);
//    //return new descriptions
//    //{
//    //    describ = request.Description;
//    //};

//}