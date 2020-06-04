using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TrainTrain.Api.Models;

namespace TrainTrain.Api.Controllers
{
    [Route("api/[controller]")]
    public class ReservationsController : Controller
    {
        [HttpGet]
        public async Task<ActionResult<string>> Get([FromQuery(Name = "trainId")] string trainId,
            [FromQuery(Name = "numberOfSeats")] int numberOfSeats)
        {
            var manager = new TicketOfficeService();
            return SeatsReservationAdapter.AdaptReservation(await manager.Reserve(trainId, numberOfSeats));
        }
    }
}
