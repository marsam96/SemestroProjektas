using ServiceStack;
using ServiceStack.OrmLite;
using System.Linq;

namespace Kursinis
{
    [Route("/Paslaugos", "GET")]
    public class VisosPaslaugos
    {

    }
    [Route("/Prisijungimas", "POST GET")]
    public class Prisijungimokurimas : IReturn<Prisijungimokurimas>
    {
        public string Paslauga { get; set; }
        public bool Prisijungimas { get; set; }
        public string Vardas { get; set; }
        public int UserId { get; set; }
        public int Paslaugosid { get; set; }
        public string Arprijungtas { get; set; }
    }
    public class Prisijungimas : Service
    {
        public object Post(Prisijungimokurimas request)
        {
            var session = this.SessionAs<AuthUserSession>();
           
            var prisijungti = new Prisijungimokurimas
            {
                Paslauga = request.Paslauga,  // nurodyt frontuj
                Paslaugosid = request.Paslaugosid, // reikia nurodyt frontuj
                Prisijungimas = request.Prisijungimas,
                Vardas = session.DisplayName,
                UserId = int.Parse(session.UserAuthId),
                Arprijungtas = "Nepatvirtinta",
            };
            var sk = Db.Select<Paslaugoskurimas>().Where(p => p.PaslaugosId == request.Paslaugosid);
            if(Db.Select<Paslaugoskurimas>().Select(P=>P.PaslaugosId).Contains(request.Paslaugosid))
            {
                if (sk.Select(x => x.Truksta).First() > 0)
                {
                    Db.Save(prisijungti);
                    return prisijungti;
                }
                return "surinkti visi reikiami zaidėjai";
            }
            return "Nera tokios Paslaugos";
        }
        public Prisijungimas()
        { 

            Db.CreateTableIfNotExists<Prisijungimokurimas>();
            if (!Db.ColumnExists<Prisijungimokurimas>(P => P.UserId))
                Db.AddColumn<Prisijungimokurimas>(P => P.UserId);
        }
        public object Get(Prisijungimokurimas request)
        {

            var eiltue = Db.Select<Patvirtinimas>().Where(P => P.Atliko).Where(P => P.UserId == request.UserId).Where(P => P.Paslaugosid == request.Paslaugosid);
            if (eiltue.Any())
                if (eiltue.Select(P => P.patvirtinmas).First())
                {
                    Db.UpdateAdd(() => new Prisijungimokurimas { Arprijungtas = "Priimtas" }, where: x => x.Paslaugosid == request.Paslaugosid);
                    request.Arprijungtas = "Priimtas";
                    return request.Arprijungtas;
                }
                else
                {
                    Db.UpdateAdd(() => new Prisijungimokurimas { Arprijungtas = "Nepriimtas" }, where: x => x.Paslaugosid == request.Paslaugosid);
                    request.Arprijungtas = "Nepriimtas";
                    return request.Arprijungtas;
                }
            return "Nepatvirtinta";
        }
    }
}
