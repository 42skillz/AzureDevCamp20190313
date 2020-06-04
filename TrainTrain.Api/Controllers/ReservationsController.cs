using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace TrainTrain.Api.Controllers
{
    [Route("api/[controller]")]
    public class ReservationsController : Controller
    {
        [HttpGet]
        public async Task<ActionResult<string>> Get(
            [FromQuery(Name = "trainId")] string trainId,
            [FromQuery(Name = "seatsRequestedCount")] int seatsRequestedCount)
        {
            var ticketOfficeService = new TicketOfficeService();
            return SeatsReservationAdapter
                .AdaptReservation(await ticketOfficeService.Reserve(trainId, seatsRequestedCount));
        }
    }
}
