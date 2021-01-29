using System.ComponentModel.DataAnnotations;

namespace ArticoliWebService.Models
{
    public class Ingredienti
    {
        [Key]
        public string CodArt { get; set; }
        public string Info { get; set; }
        
        public virtual Articoli articolo { get; set; }
    }
}