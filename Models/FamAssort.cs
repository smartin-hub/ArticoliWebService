using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ArticoliWebService.Models
{
    public class FamAssort
    {
        [Key]
        public int Id { get; set; }
        public string Descrizione { get; set; }

        public virtual ICollection<Articoli> Articoli { get; set; }
    }
}