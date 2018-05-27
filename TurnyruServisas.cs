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
    [Route("/Turnyras/Rodyk", "POST")]
    public class RodykTurnyrus : IReturn<RodykTurnyrus>
    {
        public string Veikla { get; set; }
        public string UserName { get; set; }
        public string Kontaktai { get; set; }
        public string Prizas { get; set; }
        public int ZmoniuKiekis { get; set; }
        public Boolean ArKomandinis { get; set; }
        public string TurnyroTipas { get; set; }
        public int UserId { get; set; }
        [PrimaryKey]
        [Reference]
        public int TurnyroId { get; set; }

        public RodykTurnyrus()
        {
        }
        public RodykTurnyrus(string Veikla, string UserName, string Kontaktai, string Prizas, int ZmoniuKiekis, Boolean ArKomandinis, string TurnyroTipas, int UserId, int TurnyroId)
        {
            this.Veikla = Veikla;
            this.UserName = UserName;
            this.Kontaktai = Kontaktai;
            this.Prizas = Prizas;
            this.ZmoniuKiekis = ZmoniuKiekis;
            this.ArKomandinis = ArKomandinis;
            this.TurnyroTipas = TurnyroTipas;
            this.UserId = UserId;
            this.TurnyroId = TurnyroId;
        }
    }
    public class TurnyruServisas : Service
    {
        public object Post(RodykTurnyrus request)
        {
            var session = this.SessionAs<AuthUserSession>();
            var turnyrai = new RodykTurnyrus
            {
                Veikla = request.Veikla,
                UserName = session.DisplayName,
                Kontaktai = session.Email,
                Prizas = request.Prizas,
                ZmoniuKiekis = request.ZmoniuKiekis,
                ArKomandinis = request.ArKomandinis,
                TurnyroTipas = request.TurnyroTipas,
                UserId = int.Parse(session.UserAuthId),
                TurnyroId = request.TurnyroId
            };
            Db.Save(turnyrai);
            return turnyrai;
        }
        public TurnyruServisas ()
        {

            Db.CreateTableIfNotExists<RodykTurnyrus>();
            if (!Db.ColumnExists<RodykTurnyrus>(P => P.TurnyroId))
            {
                Db.AddColumn<RodykTurnyrus>(P => P.TurnyroId);
                 Db.AddColumn<RodykTurnyrus>(P => P.UserName);
                Db.AddColumn<RodykTurnyrus>(P => P.Kontaktai);
            }


        }
    }
    public class SukurtTurnyra : Service
    {
        public List<RodykTurnyrus> FindTurnyrai(string input)
        {
            // input - tai tarkim tekstas, įvestas paieškos laukelyje, pagal kurį ieškom turnyrų

            // Pirmiausia vykdom paiešką pagal veiklą
            var q = Db.From<RodykTurnyrus>().Where(x => x.Veikla.ToLower().Contains(input.ToLower())).Select(x => x);
            List<RodykTurnyrus> rows = Db.Select<RodykTurnyrus>(q);
            if (rows.IsEmpty())
            {
                // Jei nerado pagal veiklą, toliau ieškosime pagal UserName
                q = Db.From<RodykTurnyrus>().Where(x => x.UserName.ToLower().Contains(input.ToLower())).Select(x => x);
                rows = Db.Select<RodykTurnyrus>(q);
                if (rows.IsEmpty())
                {
                    // Jei nerado pagal UserName, toliau ieškosime pagal kontaktus
                    q = Db.From<RodykTurnyrus>().Where(x => x.Kontaktai.ToLower().Contains(input.ToLower())).Select(x => x);
                    rows = Db.Select<RodykTurnyrus>(q);
                    if (rows.IsEmpty())
                    {
                        // Jei nerado pagal kontaktus, toliau ieškosime pagal prizą
                        q = Db.From<RodykTurnyrus>().Where(x => x.Prizas.ToLower().Contains(input.ToLower())).Select(x => x);
                        rows = Db.Select<RodykTurnyrus>(q);
                        if (rows.IsEmpty())
                        {
                            // Jei nerado pagal prizą, toliau ieškosime pagal turnyro tipą
                            q = Db.From<RodykTurnyrus>().Where(x => x.TurnyroTipas.ToLower().Contains(input.ToLower())).Select(x => x);
                            rows = Db.Select<RodykTurnyrus>(q);
                        }
                    }
                }
            }
            return rows;
        }
        public List<RodykTurnyrus> GetTurnyrai()
        {
            var q = Db.From<RodykTurnyrus>().Select(x => x);
            List<RodykTurnyrus> rows = Db.Select<RodykTurnyrus>(q);
            return rows;
        }
        public int GetLatestTurnyroId()
        {
            var q = Db.From<RodykTurnyrus>().Select(x => x.TurnyroId);
            List<int> rows = Db.Select<int>(q);
            return rows.Max();
        }
        public void AddTurnyras(RodykTurnyrus turnyras)
        {
            Db.Insert(new RodykTurnyrus { Veikla = turnyras.Veikla, UserName = turnyras.UserName, Kontaktai = turnyras.Kontaktai, Prizas = turnyras.Prizas, ZmoniuKiekis = turnyras.ZmoniuKiekis, ArKomandinis = turnyras.ArKomandinis, TurnyroTipas = turnyras.TurnyroTipas, UserId = turnyras.UserId, TurnyroId = turnyras.TurnyroId });
        }
        public void DeleteTurnyras(RodykTurnyrus turnyras)
        {
            Db.Delete<RodykTurnyrus>(new { Veikla = turnyras.Veikla, UserName = turnyras.UserName, Kontaktai = turnyras.Kontaktai, Prizas = turnyras.Prizas, ZmoniuKiekis = turnyras.ZmoniuKiekis, ArKomandinis = turnyras.ArKomandinis, TurnyroTipas = turnyras.TurnyroTipas, UserId = turnyras.UserId, TurnyroId = turnyras.TurnyroId });
        }
        public void DeleteTurnyras(int id)
        {
            Db.Delete<RodykTurnyrus>(x => x.TurnyroId == id);
        }
        public void ChangeVeikla(int id, string oldVeikla, string newVeikla)
        {
            Db.UpdateAdd(() => new RodykTurnyrus { Veikla = newVeikla }, where: x => x.TurnyroId == id && x.Veikla == oldVeikla);
        }
        public void ChangeUserName(int id, string oldUserName, string newUserName)
        {
            Db.UpdateAdd(() => new RodykTurnyrus { UserName = newUserName }, where: x => x.TurnyroId == id && x.UserName == oldUserName);
        }
        public void ChangeKontaktai(int id, string oldKontaktai, string newKontaktai)
        {
            Db.UpdateAdd(() => new RodykTurnyrus { Kontaktai = newKontaktai }, where: x => x.TurnyroId == id && x.Kontaktai == oldKontaktai);
        }
        public void ChangePrizas(int id, string oldPrizas, string newPrizas)
        {
            Db.UpdateAdd(() => new RodykTurnyrus { Prizas = newPrizas }, where: x => x.TurnyroId == id && x.Prizas == oldPrizas);
        }
        public void ChangeZmoniuKiekis(int id, int oldZmoniuKiekis, int newZmoniuKiekis)
        {
            Db.UpdateAdd(() => new RodykTurnyrus { ZmoniuKiekis = newZmoniuKiekis - oldZmoniuKiekis }, where: x => x.TurnyroId == id && x.ZmoniuKiekis == oldZmoniuKiekis);
        }
        public void ChangeArKomandinis(int id, Boolean oldArKomandinis, Boolean newArKomandinis)
        {
            Db.UpdateAdd(() => new RodykTurnyrus { ArKomandinis = newArKomandinis }, where: x => x.TurnyroId == id && x.ArKomandinis == oldArKomandinis);
        }
        public void ChangeTurnyroTipas(int id, string oldTurnyroTipas, string newTurnyroTipas)
        {
            Db.UpdateAdd(() => new RodykTurnyrus { TurnyroTipas = newTurnyroTipas }, where: x => x.TurnyroId == id && x.TurnyroTipas == oldTurnyroTipas);
        }
        public List<RodykTurnyrus> SortTurnyraiByVeikla()
        {
            var q = Db.From<RodykTurnyrus>().OrderBy(x => x.Veikla).Select(x => x);
            List<RodykTurnyrus> rows = Db.Select<RodykTurnyrus>(q);
            return rows;
        }
        public List<RodykTurnyrus> SortTurnyraiByVeiklaDescending()
        {
            var q = Db.From<RodykTurnyrus>().OrderByDescending(x => x.Veikla).Select(x => x);
            List<RodykTurnyrus> rows = Db.Select<RodykTurnyrus>(q);
            return rows;
        }
        public List<RodykTurnyrus> SortTurnyraiByUserName()
        {
            var q = Db.From<RodykTurnyrus>().OrderBy(x => x.UserName).Select(x => x);
            List<RodykTurnyrus> rows = Db.Select<RodykTurnyrus>(q);
            return rows;
        }
        public List<RodykTurnyrus> SortTurnyraiByUserNameDescending()
        {
            var q = Db.From<RodykTurnyrus>().OrderByDescending(x => x.UserName).Select(x => x);
            List<RodykTurnyrus> rows = Db.Select<RodykTurnyrus>(q);
            return rows;
        }
        public List<RodykTurnyrus> SortTurnyraiByKontaktai()
        {
            var q = Db.From<RodykTurnyrus>().OrderBy(x => x.Kontaktai).Select(x => x);
            List<RodykTurnyrus> rows = Db.Select<RodykTurnyrus>(q);
            return rows;
        }
        public List<RodykTurnyrus> SortTurnyraiByKontaktaiDescending()
        {
            var q = Db.From<RodykTurnyrus>().OrderByDescending(x => x.Kontaktai).Select(x => x);
            List<RodykTurnyrus> rows = Db.Select<RodykTurnyrus>(q);
            return rows;
        }
        public List<RodykTurnyrus> SortTurnyraiByPrizas()
        {
            var q = Db.From<RodykTurnyrus>().OrderBy(x => x.Prizas).Select(x => x);
            List<RodykTurnyrus> rows = Db.Select<RodykTurnyrus>(q);
            return rows;
        }
        public List<RodykTurnyrus> SortTurnyraiByPrizasDescending()
        {
            var q = Db.From<RodykTurnyrus>().OrderByDescending(x => x.Prizas).Select(x => x);
            List<RodykTurnyrus> rows = Db.Select<RodykTurnyrus>(q);
            return rows;
        }
        public List<RodykTurnyrus> SortTurnyraiByZmoniuKiekis()
        {
            var q = Db.From<RodykTurnyrus>().OrderBy(x => x.ZmoniuKiekis).Select(x => x);
            List<RodykTurnyrus> rows = Db.Select<RodykTurnyrus>(q);
            return rows;
        }
        public List<RodykTurnyrus> SortTurnyraiByZmoniuKiekisDescending()
        {
            var q = Db.From<RodykTurnyrus>().OrderByDescending(x => x.ZmoniuKiekis).Select(x => x);
            List<RodykTurnyrus> rows = Db.Select<RodykTurnyrus>(q);
            return rows;
        }
        public List<RodykTurnyrus> SortTurnyraiByArKomandinis()
        {
            var q = Db.From<RodykTurnyrus>().OrderBy(x => x.ArKomandinis).Select(x => x);
            List<RodykTurnyrus> rows = Db.Select<RodykTurnyrus>(q);
            return rows;
        }
        public List<RodykTurnyrus> SortTurnyraiByArKomandinisDescending()
        {
            var q = Db.From<RodykTurnyrus>().OrderByDescending(x => x.ArKomandinis).Select(x => x);
            List<RodykTurnyrus> rows = Db.Select<RodykTurnyrus>(q);
            return rows;
        }
        public List<RodykTurnyrus> SortTurnyraiByTurnyroTipas()
        {
            var q = Db.From<RodykTurnyrus>().OrderBy(x => x.TurnyroTipas).Select(x => x);
            List<RodykTurnyrus> rows = Db.Select<RodykTurnyrus>(q);
            return rows;
        }
        public List<RodykTurnyrus> SortTurnyraiByTurnyroTipasDescending()
        {
            var q = Db.From<RodykTurnyrus>().OrderByDescending(x => x.TurnyroTipas).Select(x => x);
            List<RodykTurnyrus> rows = Db.Select<RodykTurnyrus>(q);
            return rows;
        }
    }
}
