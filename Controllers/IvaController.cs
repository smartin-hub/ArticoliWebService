using System.Collections.Generic;
using System.Threading.Tasks;
using ArticoliWebService.Dtos;
using ArticoliWebService.Services;
using Microsoft.AspNetCore.Mvc;

namespace ArticoliWebService.Controllers
{
    [ApiController]
    [Produces("application/json")]
    [Route("api/iva")]
    public class IvaController : Controller
    {
        private readonly IArticoliRepository articolirepository;

        public IvaController(IArticoliRepository articolirepository)
        {
            this.articolirepository = articolirepository;
        }

        [HttpGet]
        [ProducesResponseType(200, Type = typeof(IEnumerable<IvaDto>))]
        public async Task<IActionResult> GetIva()
        {
            var ivaDto = new List<IvaDto>();

            var iva = await this.articolirepository.SelIva();

            foreach(var Iva in iva)
            {
                ivaDto.Add(new IvaDto
                {
                    IdIva = Iva.IdIva,
                    Descrizione = Iva.Descrizione
                });
            }

            return Ok(ivaDto);

        }

        
    }
}