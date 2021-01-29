using System;
using Microsoft.AspNetCore.Mvc;

namespace Controllers
{
    [ApiController]
    [Route("api/saluti")]
    public class SalutiController
    {
        [HttpGet]
        public string getSaluti()
        {
            return "Saluti, sono il tuo primo web service creato con c#";
        }

        [HttpGet("{Nome}")]
        public string getSaluti2(string Nome) 
        {
            try
            {
                if (Nome == "Marco")
                    throw new Exception("\"Errore: L'utente Marco è disabilitato!\"");
                else 
                    return  string.Format("\"Saluti, {0} sono il tuo primo web service creato con c#\"", Nome);
            }
            catch (System.Exception ex)
            {
                return ex.Message;
            }
        }

        
    }
}