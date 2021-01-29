using System.Collections.Generic;
using System.Threading.Tasks;
using ArticoliWebService.Models;

namespace ArticoliWebService.Services
{
    public interface IArticoliRepository
    {
        Task<ICollection<Articoli>> SelArticoliByDescrizione(string Descrizione);
        Task<ICollection<Articoli>> SelArticoliByDescrizione(string Descrizione, string IdCat);
        Task<Articoli> SelArticoloByCodice(string Code);
        Articoli SelArticoloByCodice2(string Code);
        Task<Articoli> SelArticoloByEan(string Ean);
        Task<ICollection<Iva>> SelIva();
        Task<ICollection<FamAssort>> SelCat();

        bool InsArticoli(Articoli articolo);
        bool UpdArticoli(Articoli articolo);
        bool DelArticoli(Articoli articolo);
        bool Salva();

        Task<bool> ArticoloExists(string Code);
    }
}