using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using ServiceStack.OrmLite;
using ServiceStack.DataAnnotations;
using ServiceStack.Auth;
using System.Linq;
using ServiceStack;

namespace Kursinis
{
    [Route("/Patvirtinam", "POST")]
    public class Patvirtinimas : IReturn<Patvirtinimas>
    {
        public string Paslauga { get; set; }
        public bool patvirtinmas { get; set; }
        public string Vardas { get; set; }
        public int UserId { get; set; }
        public int Paslaugosid { get; set; }
        public bool Atliko { get; set; }
    }
    public class Patvirtinimo : Service
    {
        public object Post (Patvirtinimas request)
        {
            var session = this.SessionAs<AuthUserSession>();
            var pav = Db.Select<Paslaugoskurimas>().Where(P => P.UserId == int.Parse(session.UserAuthId)).Select(x => x.Veikla).First();
            if (Db.Select<Prisijungimokurimas>().Select(P => P.UserId).Contains(request.UserId))
            {
                var patvirtinimas = new Patvirtinimas
                {
                    Paslauga = pav, 
                    Paslaugosid = request.Paslaugosid,
                    Vardas = request.Vardas,//gauti is visupaslaugu per fronta
                    UserId = request.UserId,//gauti is visu paslaugu per fronta
                    patvirtinmas = request.patvirtinmas,
                    Atliko = true,
                };
                if(request.patvirtinmas)
                {
                    int oldTruksta = Db.Select<Paslaugoskurimas>().Where(x => x.PaslaugosId == request.Paslaugosid).Select(x => x.Truksta).First();
                    int olddalyvauja = Db.Select<Paslaugoskurimas>().Where(x => x.PaslaugosId == request.Paslaugosid).Select(x => x.Dalyvauja).First();
                    Db.UpdateAdd(() => new Paslaugoskurimas { Truksta = oldTruksta-1-oldTruksta }, where: x => x.PaslaugosId == request.Paslaugosid && x.Truksta == oldTruksta);
                    Db.UpdateAdd(() => new Paslaugoskurimas { Dalyvauja = olddalyvauja + 1 -olddalyvauja}, where: x => x.PaslaugosId == request.Paslaugosid && x.Dalyvauja == olddalyvauja);
                }
                Db.Save(patvirtinimas);
                return patvirtinimas;
             }
            return "";
        }
        public Patvirtinimo()
        {

            Db.CreateTableIfNotExists<Patvirtinimas>();
            if (!Db.ColumnExists<Patvirtinimas>(P => P.UserId))
                Db.AddColumn<Patvirtinimas>(P => P.UserId);
        }
    }
}
