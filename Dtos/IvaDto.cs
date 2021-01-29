using System;

namespace ArticoliWebService.Dtos
{
    public class IvaDto
    {

        public int IdIva { get; set; }
        public string Descrizione { get; set; }
        public Int16 Aliquota {get;set;}
    }
}