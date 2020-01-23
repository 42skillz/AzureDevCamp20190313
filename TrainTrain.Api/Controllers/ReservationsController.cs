using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TrainTrain.Infra.Adapter;

namespace TrainTrain.Api.Controllers
{
    [Route("api/[controller]")]
    public class ReservationsController : Controller
    {
        private readonly ReservationAdapter _reservationAdapter;

        public ReservationsController(ReservationAdapter reservationAdapter)
        {
            _reservationAdapter = reservationAdapter;
        }

        [HttpGet]
        public async Task<ActionResult<string>> Get([FromQuery(Name = "trainId")] string trainId,
            [FromQuery(Name = "numberOfSeats")] int numberOfSeats)
        {
            return await _reservationAdapter.ReserveAsync(trainId, numberOfSeats);
        }
    }
}