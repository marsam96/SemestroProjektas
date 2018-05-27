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
    [Route("/Noriu/Rodyk", "POST DELETE")]
    public class Paslaugoskurimas : IReturn<Paslaugoskurimas>
    {
        public string Veikla { get; set; }
        public string Vardas { get; set; }
        public string Kontaktai { get; set; }
        public string Kada { get; set; }
        public int Truksta { get; set; }
        public int Dalyvauja { get; set; }
        // kur tai https://www.google.com/maps/place/"kur" + Kaunas
        public string Gatve { get; set; }
        public string Miestas { get; set; }
        public int UserId { get; set; }
        [PrimaryKey]
        [Reference]
        public int PaslaugosId { get; set; }

        public Paslaugoskurimas()
        {
        }
        public Paslaugoskurimas(string Veikla, string Vardas, string Kontaktai, string Kada, int Truksta, int Dalyvauja, string Gatve, string Miestas, int UserId, int PaslaugosId)
        {
            this.Veikla = Veikla;
            this.Vardas = Vardas;
            this.Kontaktai = Kontaktai;
            this.Kada = Kada;
            this.Truksta = Truksta;
            this.Dalyvauja = Dalyvauja;
            this.Gatve = Gatve;
            this.Miestas = Miestas;
            this.UserId = UserId;
            this.PaslaugosId = PaslaugosId;
        }
    }
    public class NoriuServisas : Service // sukuriam servisa
    {
        public object Post(Paslaugoskurimas request)
        {
            var session = this.SessionAs<AuthUserSession>();
            var kiek = Db.Select<Paslaugoskurimas>().Where(x => x.UserId == int.Parse(session.UserAuthId));
            if (!kiek.Any()) {
                request.Gatve = Regex.Replace(request.Gatve, " ", "+");
                if (Db.Select<Paslaugoskurimas>().Any())
                {
                    request.PaslaugosId = Db.Select<Paslaugoskurimas>().Max(P => P.PaslaugosId) + 1;
                }
                else
                {
                    request.PaslaugosId = 1;
                }
                var paslauga = new Paslaugoskurimas
                {
                    Veikla = request.Veikla,
                    Vardas = session.DisplayName,
                    Kontaktai = request.Kontaktai,
                    Kada = request.Kada,
                    Truksta = request.Truksta,
                    Dalyvauja = request.Dalyvauja,
                    //Display link to googlemaps + Kur
                    Gatve = "https://www.google.com/maps/place/" + request.Gatve + "+" + request.Miestas,
                    Miestas = request.Miestas,
                    UserId = int.Parse(session.UserAuthId),
                    PaslaugosId = request.PaslaugosId
                };

                Db.Save(paslauga);
                return paslauga;
            }
            return "Jūs jau esate sukures paslauga";
        }
        public void Delete(Paslaugoskurimas request)
        {
            var session = this.SessionAs<AuthUserSession>();
            Db.Delete<Paslaugoskurimas>(x => x.UserId == int.Parse(session.UserAuthId));
            Db.Delete<Prisijungimokurimas>(x => x.Paslaugosid == request.PaslaugosId);
            Db.Delete<Patvirtinimas>(x => x.Paslaugosid == request.PaslaugosId);
            
        }
        
        public NoriuServisas ()
        {

            Db.CreateTableIfNotExists<Paslaugoskurimas>();
            if (!Db.ColumnExists<Paslaugoskurimas>(P => P.PaslaugosId))
                Db.AddColumn<Paslaugoskurimas>(P => P.PaslaugosId);
        }
        public object Get(KasPrisijunges request)
        {
            return Db.Select<UserAuth>().Select(u => u.UserName);
            
         }
        public object Get(VisosPaslaugos request)
        {
            return Db.Select<Paslaugoskurimas>();
        }
    }
    [Route("/Users", "GET")]
    public class KasPrisijunges
    {

    }


    public class Noriufunkcijos : Service
    {
        public List<Paslaugoskurimas> FindPaslaugos(string input)
        {
            // input - tai tarkim tekstas, įvestas paieškos laukelyje, pagal kurį ieškom paslaugų

            // Pirmiausia vykdom paiešką pagal veiklą
            var q = Db.From<Paslaugoskurimas>().Where(x => x.Veikla.ToLower().Contains(input.ToLower())).Select(x => x);
            List<Paslaugoskurimas> rows = Db.Select<Paslaugoskurimas>(q);
            if (rows.IsEmpty())
            {
                // Jei nerado pagal veiklą, toliau ieškosime pagal vardą
                q = Db.From<Paslaugoskurimas>().Where(x => x.Vardas.ToLower().Contains(input.ToLower())).Select(x => x);
                rows = Db.Select<Paslaugoskurimas>(q);
                if (rows.IsEmpty())
                {
                    // Jei nerado pagal vardą, toliau ieškosime pagal kontaktus
                    q = Db.From<Paslaugoskurimas>().Where(x => x.Kontaktai.ToLower().Contains(input.ToLower())).Select(x => x);
                    rows = Db.Select<Paslaugoskurimas>(q);
                    if (rows.IsEmpty())
                    {
                        // Jei nerado pagal kontaktus, toliau ieškosime pagal kada
                        q = Db.From<Paslaugoskurimas>().Where(x => x.Kada.ToLower().Contains(input.ToLower())).Select(x => x);
                        rows = Db.Select<Paslaugoskurimas>(q);
                        if (rows.IsEmpty())
                        {
                            // Jei nerado pagal kada, toliau ieškosime pagal gatvę
                            q = Db.From<Paslaugoskurimas>().Where(x => x.Gatve.ToLower().Contains(input.ToLower())).Select(x => x);
                            rows = Db.Select<Paslaugoskurimas>(q);
                        }
                    }
                }
            }
            return rows;
        }
        public List<Paslaugoskurimas> GetPaslaugos()
        {
            var q = Db.From<Paslaugoskurimas>().Select(x => x);
            List<Paslaugoskurimas> rows = Db.Select<Paslaugoskurimas>(q);
            return rows;
        }
        public int GetLatestPaslaugosId()
        {
            var q = Db.From<Paslaugoskurimas>().Select(x => x.PaslaugosId);
            List<int> rows = Db.Select<int>(q);
            return rows.Max();
        }
        public void AddPaslauga(Paslaugoskurimas paslauga)
        {
            Db.Insert(new Paslaugoskurimas { Veikla = paslauga.Veikla, Vardas = paslauga.Vardas, Kontaktai = paslauga.Kontaktai, Kada = paslauga.Kada, Truksta = paslauga.Truksta, Dalyvauja = paslauga.Dalyvauja, Gatve = paslauga.Gatve, UserId = paslauga.UserId, PaslaugosId = paslauga.PaslaugosId });
        }
        public void DeletePaslauga(Paslaugoskurimas paslauga)
        {
            Db.Delete<Paslaugoskurimas>(new { Veikla = paslauga.Veikla, Vardas = paslauga.Vardas, Kontaktai = paslauga.Kontaktai, Kada = paslauga.Kada, Truksta = paslauga.Truksta, Dalyvauja = paslauga.Dalyvauja, Gatve = paslauga.Gatve, UserId = paslauga.UserId, PaslaugosId = paslauga.PaslaugosId });
        }
        public void DeletePaslauga(int id)
        {
            Db.Delete<Paslaugoskurimas>(x => x.PaslaugosId == id);
        }
        public void ChangeVeikla(int id, string oldVeikla, string newVeikla)
        {
            Db.UpdateAdd(() => new Paslaugoskurimas { Veikla = newVeikla }, where: x => x.PaslaugosId == id && x.Veikla == oldVeikla);
        }
        public void ChangeVardas(int id, string oldVardas, string newVardas)
        {
            Db.UpdateAdd(() => new Paslaugoskurimas { Vardas = newVardas }, where: x => x.PaslaugosId == id && x.Vardas == oldVardas);
        }
        public void ChangeKontaktai(int id, string oldKontaktai, string newKontaktai)
        {
            Db.UpdateAdd(() => new Paslaugoskurimas { Kontaktai = newKontaktai }, where: x => x.PaslaugosId == id && x.Kontaktai == oldKontaktai);
        }
        public void ChangeKada(int id, string oldKada, string newKada)
        {
            Db.UpdateAdd(() => new Paslaugoskurimas { Kada = newKada }, where: x => x.PaslaugosId == id && x.Kada == oldKada);
        }
        public void ChangeTruksta(int id, int oldTruksta, int newTruksta)
        {
            Db.UpdateAdd(() => new Paslaugoskurimas { Truksta = newTruksta - oldTruksta }, where: x => x.PaslaugosId == id && x.Truksta == oldTruksta);
        }
        public void ChangeDalyvauja(int id, int oldDalyvauja, int newDalyvauja)
        {
            Db.UpdateAdd(() => new Paslaugoskurimas { Dalyvauja = newDalyvauja - oldDalyvauja }, where: x => x.PaslaugosId == id && x.Dalyvauja == oldDalyvauja);
        }
        public void ChangeGatve(int id, string oldGatve, string newGatve)
        {
            Db.UpdateAdd(() => new Paslaugoskurimas { Gatve = newGatve }, where: x => x.PaslaugosId == id && x.Gatve == oldGatve);
        }
        public List<Paslaugoskurimas> SortPaslaugosByVeikla()
        {
            var q = Db.From<Paslaugoskurimas>().OrderBy(x => x.Veikla).Select(x => x);
            List<Paslaugoskurimas> rows = Db.Select<Paslaugoskurimas>(q);
            return rows;
        }
        public List<Paslaugoskurimas> SortPaslaugosByVeiklaDescending()
        {
            var q = Db.From<Paslaugoskurimas>().OrderByDescending(x => x.Veikla).Select(x => x);
            List<Paslaugoskurimas> rows = Db.Select<Paslaugoskurimas>(q);
            return rows;
        }
        public List<Paslaugoskurimas> SortPaslaugosByVardas()
        {
            var q = Db.From<Paslaugoskurimas>().OrderBy(x => x.Vardas).Select(x => x);
            List<Paslaugoskurimas> rows = Db.Select<Paslaugoskurimas>(q);
            return rows;
        }
        public List<Paslaugoskurimas> SortPaslaugosByVardasDescending()
        {
            var q = Db.From<Paslaugoskurimas>().OrderByDescending(x => x.Vardas).Select(x => x);
            List<Paslaugoskurimas> rows = Db.Select<Paslaugoskurimas>(q);
            return rows;
        }
        public List<Paslaugoskurimas> SortPaslaugosByKontaktai()
        {
            var q = Db.From<Paslaugoskurimas>().OrderBy(x => x.Kontaktai).Select(x => x);
            List<Paslaugoskurimas> rows = Db.Select<Paslaugoskurimas>(q);
            return rows;
        }
        public List<Paslaugoskurimas> SortPaslaugosByKontaktaiDescending()
        {
            var q = Db.From<Paslaugoskurimas>().OrderByDescending(x => x.Kontaktai).Select(x => x);
            List<Paslaugoskurimas> rows = Db.Select<Paslaugoskurimas>(q);
            return rows;
        }
        public List<Paslaugoskurimas> SortPaslaugosByKada()
        {
            var q = Db.From<Paslaugoskurimas>().OrderBy(x => x.Kada).Select(x => x);
            List<Paslaugoskurimas> rows = Db.Select<Paslaugoskurimas>(q);
            return rows;
        }
        public List<Paslaugoskurimas> SortPaslaugosByKadaDescending()
        {
            var q = Db.From<Paslaugoskurimas>().OrderByDescending(x => x.Kada).Select(x => x);
            List<Paslaugoskurimas> rows = Db.Select<Paslaugoskurimas>(q);
            return rows;
        }
        public List<Paslaugoskurimas> SortPaslaugosByTruksta()
        {
            var q = Db.From<Paslaugoskurimas>().OrderBy(x => x.Truksta).Select(x => x);
            List<Paslaugoskurimas> rows = Db.Select<Paslaugoskurimas>(q);
            return rows;
        }
        public List<Paslaugoskurimas> SortPaslaugosByTrukstaDescending()
        {
            var q = Db.From<Paslaugoskurimas>().OrderByDescending(x => x.Truksta).Select(x => x);
            List<Paslaugoskurimas> rows = Db.Select<Paslaugoskurimas>(q);
            return rows;
        }
        public List<Paslaugoskurimas> SortPaslaugosByDalyvauja()
        {
            var q = Db.From<Paslaugoskurimas>().OrderBy(x => x.Dalyvauja).Select(x => x);
            List<Paslaugoskurimas> rows = Db.Select<Paslaugoskurimas>(q);
            return rows;
        }
        public List<Paslaugoskurimas> SortPaslaugosByDalyvaujaDescending()
        {
            var q = Db.From<Paslaugoskurimas>().OrderByDescending(x => x.Dalyvauja).Select(x => x);
            List<Paslaugoskurimas> rows = Db.Select<Paslaugoskurimas>(q);
            return rows;
        }
        public List<Paslaugoskurimas> SortPaslaugosByGatve()
        {
            var q = Db.From<Paslaugoskurimas>().OrderBy(x => x.Gatve).Select(x => x);
            List<Paslaugoskurimas> rows = Db.Select<Paslaugoskurimas>(q);
            return rows;
        }
        public List<Paslaugoskurimas> SortPaslaugosByGatveDescending()
        {
            var q = Db.From<Paslaugoskurimas>().OrderByDescending(x => x.Gatve).Select(x => x);
            List<Paslaugoskurimas> rows = Db.Select<Paslaugoskurimas>(q);
            return rows;
        }
        public void AddPlayer(Paslaugoskurimas paslauga)
        {
            if (paslauga.Truksta > 0)
            {
                ChangeTruksta(paslauga.PaslaugosId, paslauga.Truksta, paslauga.Truksta - 1);
                ChangeDalyvauja(paslauga.PaslaugosId, paslauga.Dalyvauja, paslauga.Dalyvauja + 1);
            }
        }
        public void RemovePlayer(Paslaugoskurimas paslauga)
        {
            if (paslauga.Dalyvauja > 0)
            {
                ChangeTruksta(paslauga.PaslaugosId, paslauga.Truksta, paslauga.Truksta + 1);
                ChangeDalyvauja(paslauga.PaslaugosId, paslauga.Dalyvauja, paslauga.Dalyvauja - 1);
            }
        }
    }
    // kito nario prisijungimas
    // priemimas kito nario
    // jei palieka


}
