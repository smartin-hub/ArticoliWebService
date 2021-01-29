using System.Collections.Generic;
using System.Threading.Tasks;
using ArticoliWebService.Dtos;
using ArticoliWebService.Services;
using Microsoft.AspNetCore.Mvc;

namespace ArticoliWebService.Controllers
{
    [ApiController]
    [Produces("application/json")]
    [Route("api/cat")]
    public class CatController : Controller
    {
        private readonly IArticoliRepository articolirepository;

        public CatController(IArticoliRepository articolirepository)
        {
            this.articolirepository = articolirepository;
        }

        [HttpGet]
        [ProducesResponseType(200, Type = typeof(IEnumerable<CategoriaDto>))]
        public async Task<IActionResult> GetIva()
        {
            var catDto = new List<CategoriaDto>();

            var iva = await this.articolirepository.SelCat();

            foreach(var Iva in iva)
            {
                catDto.Add(new CategoriaDto
                {
                    Id = Iva.Id,
                    Descrizione = Iva.Descrizione
                });
            }

            return Ok(catDto);

        }
    }
}