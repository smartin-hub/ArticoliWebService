using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using ArticoliWebService.Dtos;
using ArticoliWebService.Models;
using ArticoliWebService.Resource;
using ArticoliWebService.Services;
using Dtos;
using Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace ArticoliWebService.Controllers
{
    [ApiController]
    [Produces("application/json")]
    [Route("api/articoli")]
    [Authorize(Roles = "ADMIN, USER")]
    public class ArticoliController : Controller
    {
        private readonly IArticoliRepository articolirepository;
        private readonly PriceWebApi priceWebApi;

        public ArticoliController(IArticoliRepository articolirepository, IOptions<PriceWebApi> priceWebApi)
        {
            this.articolirepository = articolirepository;
            this.priceWebApi = priceWebApi.Value;
        }

        [HttpGet("test")]
        [ProducesResponseType(200, Type = typeof(InfoMsg))]
        public IActionResult TestConnex() {
            return Ok (new InfoMsg(DateTime.Today, "Test connessione ok"));            
        }


        [HttpGet("cerca/descrizione/{filter}/{IdList?}")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ArticoliDto>))]
        public async Task<IActionResult> GetArticoliByDesc(string filter, string IdList,  [FromQuery] ArticoliParameters Parametri)
        {
            string accessToken = Request.Headers["Authorization"];

            IdList = (IdList == null) ? this.priceWebApi.Listino : IdList;

            var articoliDto = new List<ArticoliDto>();

            var articoli = await this.articolirepository.SelArticoliByDescrizione(filter, Parametri.IdCategoria);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (articoli.Count == 0)
            {
                return NotFound(new ErrMsg(string.Format("Non è stato trovato alcun articolo con il filtro '{0}'", filter), this.HttpContext.Response.StatusCode.ToString()));
            }

            foreach(var articolo in articoli)
            {                
                PrezziDTO prezzoDTO = await getPriceArtAsync(articolo.CodArt, IdList, accessToken);
                articoliDto.Add(this.CreateArticoloDTO(articolo, prezzoDTO)); 

            }

            if(Parametri.Prezzo > 0) {
                articoliDto = articoliDto.FindAll(a => a.Prezzo <= Parametri.Prezzo);
            }


            return Ok(articoliDto);
           
        }
        
/*
        [HttpGet("cerca/descrizione/{filter}")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ArticoliDto>))]
        public async Task<IActionResult> GetArticoliByDesc(string filter, [FromQuery(Name="cat")] string idCat)
        {
            string accessToken = Request.Headers["Authorization"];
        
            var articoliDto = new List<ArticoliDto>();

            var articoli = await this.articolirepository.SelArticoliByDescrizione(filter, idCat);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (articoli.Count == 0)
            {
                return NotFound(new ErrMsg(string.Format("Non è stato trovato alcun articolo con il filtro '{0}'", filter), this.HttpContext.Response.StatusCode.ToString()));
            }

            foreach(var articolo in articoli)
            {                
                PrezziDTO prezzoDTO = await getPriceArtAsync(articolo.CodArt, this.priceWebApi.Listino, accessToken);
                articoliDto.Add(this.CreateArticoloDTO(articolo, prezzoDTO));
                
            }

            return Ok(articoliDto);
           
        }
*/
        private ArticoliDto CreateArticoloDTO(Articoli articolo, PrezziDTO prezzoDto)
        {
            //Console.WriteLine($"Codice: {articolo.CodArt}");

            var barcodeDto = new List<BarcodeDto>();
            
            foreach(var ean in articolo.barcode)
            {
                barcodeDto.Add(new BarcodeDto
                {
                    Barcode = ean.Barcode,
                    Tipo = ean.IdTipoArt
                });
            }

            IvaDto ivaDto = new IvaDto();
            if(articolo.iva != null){
                ivaDto.Descrizione =  articolo.iva.Descrizione;
                ivaDto.IdIva = articolo.iva.IdIva;
                ivaDto.Aliquota = articolo.iva.Aliquota;
            }

            var articoliDto = new ArticoliDto
            {
                CodArt = articolo.CodArt,
                Descrizione = articolo.Descrizione,
                Um = (articolo.Um != null) ? articolo.Um.Trim() : "",
                CodStat = (articolo.CodStat != null) ? articolo.CodStat.Trim() : "", 
                PzCart = articolo.PzCart,
                PesoNetto = articolo.PesoNetto,
                DataCreazione = articolo.DataCreazione,
                Ean = barcodeDto,
                IdFamAss = articolo.IdFamAss,
                IdStatoArt = (articolo.IdStatoArt != null) ? articolo.IdStatoArt.Trim() : "",
                Iva = ivaDto,
                Categoria = (articolo.famAssort != null) ? articolo.famAssort.Descrizione : "Non Definito",
                Prezzo = prezzoDto.Prezzo
            };

            return articoliDto;
        }

        private async Task<PrezziDTO> getPriceArtAsync(string CodArt, string IdList, string Token)
        {
            PrezziDTO prezzo = null;

            using (var client = new HttpClient())
            {
                Token = Token.Replace("Bearer ","");

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);

                //string EndPointPrezzo = "http://localhost:5071/api/prezzi/";
                //var result = await client.GetAsync(EndPointPrezzo + CodArt + "/" + IdList);
                var result = await client.GetAsync(this.priceWebApi.EndPointPrezzo + CodArt + "/" + IdList);
                var response = await result.Content.ReadAsStringAsync();
                prezzo = JsonConvert.DeserializeObject<PrezziDTO>(response);
            }

            return prezzo;
        }

        [HttpGet("cerca/codice/{CodArt}/{IdList?}", Name = "GetArticoli")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(ArticoliDto))]
        [AllowAnonymous]
        public async Task<IActionResult> GetArticoloByCode(string CodArt, string IdList)
        {
            string accessToken = Request.Headers["Authorization"];

            IdList = (IdList == null) ? this.priceWebApi.Listino : IdList;

            if (!await this.articolirepository.ArticoloExists(CodArt))
            {
                return NotFound(new ErrMsg(string.Format("Non è stato trovato alcun articolo con il codice '{0}'", CodArt), this.HttpContext.Response.StatusCode.ToString()));
                
            }

            var articolo = await this.articolirepository.SelArticoloByCodice(CodArt);
            PrezziDTO prezzoDTO = await getPriceArtAsync(articolo.CodArt, IdList, accessToken);
            return Ok(CreateArticoloDTO(articolo, prezzoDTO));
        }

        [HttpGet("cerca/ean/{Ean}/{IdList?}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(ArticoliDto))]
        public async Task<IActionResult> GetArticoloByEan(string Ean, string IdList)
        {

            string accessToken = Request.Headers["Authorization"];

            IdList = (IdList == null) ? this.priceWebApi.Listino : IdList;
            var articolo = await this.articolirepository.SelArticoloByEan(Ean);

            if (articolo == null)
            {
                return NotFound(new ErrMsg(string.Format("Non è stato trovato alcun articolo con il barcode '{0}'", Ean), this.HttpContext.Response.StatusCode.ToString()));                                
            }

            PrezziDTO prezzoDTO = await getPriceArtAsync(articolo.CodArt, IdList, accessToken);

	        return Ok(CreateArticoloDTO(articolo, prezzoDTO));
            
        }

        [HttpPost("inserisci")]
        [ProducesResponseType(201, Type = typeof(Articoli))]
        [ProducesResponseType(400)]
        [ProducesResponseType(422)]
        [ProducesResponseType(500)]
        [Authorize(Roles="ADMIN")]
        public IActionResult SaveArticoli([FromBody] Articoli articolo)
        {
            if (articolo == null)
            {
                return BadRequest(ModelState);
            }

             //Verifichiamo che i dati siano corretti
            if (!ModelState.IsValid)
            {
                string ErrVal = "";

                foreach (var modelState in ModelState.Values) 
                {
                    foreach (var modelError in modelState.Errors) 
                    {
                        ErrVal += modelError.ErrorMessage + " - "; 
                    }
                }

                return BadRequest(new InfoMsg(DateTime.Today, ErrVal));
            }

            //Contolliamo se l'articolo è presente
            var isPresent = articolirepository.SelArticoloByCodice2(articolo.CodArt);

            if (isPresent != null)
            {
                //ModelState.AddModelError("", $"Articolo {articolo.CodArt} presente in anagrafica! Impossibile utilizzare il metodo POST!");
                return StatusCode(422, new InfoMsg(DateTime.Today, $"Articolo {articolo.CodArt} presente in anagrafica! Impossibile utilizzare il metodo POST!"));
            }

            
            articolo.DataCreazione = DateTime.Today;
            
            //verifichiamo che i dati siano stati regolarmente inseriti nel database
            if (!articolirepository.InsArticoli(articolo))
            {
                //ModelState.AddModelError("", $"Ci sono stati problemi nell'inserimento dell'Articolo {articolo.CodArt}.  ");
                return StatusCode(500, new InfoMsg(DateTime.Today, $"Ci sono stati problemi nell'inserimento dell'Articolo {articolo.CodArt}."));
            }

            //return CreatedAtRoute("GetArticoli", new {codart = articolo.CodArt}, CreateArticoloDTO(articolo));

            return Ok(new InfoMsg(DateTime.Today, $"Inserimento articolo {articolo.CodArt} eseguita con successo!"));
        }

        [HttpPut("modifica")]
        [ProducesResponseType(201, Type = typeof(InfoMsg))]
        [ProducesResponseType(400)]
        [ProducesResponseType(422)]
        [ProducesResponseType(500)]
        [Authorize(Roles="ADMIN")]
        public IActionResult UpdateArticoli([FromBody] Articoli articolo)
        {
            if (articolo == null)
            {
                return BadRequest(ModelState);
            }

             //Verifichiamo che i dati siano corretti
            if (!ModelState.IsValid)
            {
                string ErrVal = "";

                foreach (var modelState in ModelState.Values) 
                {
                    foreach (var modelError in modelState.Errors) 
                    {
                        ErrVal += modelError.ErrorMessage + " - "; 
                    }
                }

                return BadRequest(new InfoMsg(DateTime.Today, ErrVal));
            }

            //Contolliamo se l'articolo è presente (Usare il metodo senza Traking)
            var isPresent = articolirepository.SelArticoloByCodice2(articolo.CodArt);

            if (isPresent == null)
            {
                //ModelState.AddModelError("", $"Articolo {articolo.CodArt} NON presente in anagrafica! Impossibile utilizzare il metodo PUT!");
                return StatusCode(422, new InfoMsg(DateTime.Today, $"Articolo {articolo.CodArt} NON presente in anagrafica! Impossibile utilizzare il metodo PUT!"));
            }
            

            //verifichiamo che i dati siano stati regolarmente inseriti nel database
            if (!articolirepository.UpdArticoli(articolo))
            {
                //ModelState.AddModelError("", $"Ci sono stati problemi nella modifica dell'Articolo {articolo.CodArt}.  ");
                return StatusCode(500, new InfoMsg(DateTime.Today, $"Ci sono stati problemi nella modifica dell'Articolo {articolo.CodArt}.  "));
            }

            return Ok(new InfoMsg(DateTime.Today, $"Modifica articolo {articolo.CodArt} eseguita con successo!"));

        }

        [HttpDelete("elimina/{codart}")]
        [ProducesResponseType(201, Type = typeof(InfoMsg))]
        [ProducesResponseType(400, Type = typeof(InfoMsg))]
        [ProducesResponseType(422, Type = typeof(InfoMsg))]
        [ProducesResponseType(500)]
        [Authorize(Roles="ADMIN")]
        public IActionResult DeleteArticoli(string codart)
        {
            if (codart == "")
            {
                return BadRequest(new InfoMsg(DateTime.Today, $"E' necessario inserire il codice dell'articolo da eliminare!"));
            }

            //Contolliamo se l'articolo è presente (Usare il metodo senza Traking)
            var articolo = articolirepository.SelArticoloByCodice2(codart);

            if (articolo == null)
            {
                return StatusCode(422, new InfoMsg(DateTime.Today, $"Articolo {codart} NON presente in anagrafica! Impossibile Eliminare!"));
            }

            //verifichiamo che i dati siano stati regolarmente eliminati dal database
            if (!articolirepository.DelArticoli(articolo))
            {
                //ModelState.AddModelError("", $"Ci sono stati problemi nella eliminazione dell'Articolo {articolo.CodArt}.  ");
                return StatusCode(500, new InfoMsg(DateTime.Today, $"Ci sono stati problemi nella eliminazione dell'Articolo {articolo.CodArt}.  "));
            }

            return Ok(new InfoMsg(DateTime.Today, $"Eliminazione articolo {codart} eseguita con successo!"));

        }






    }
}