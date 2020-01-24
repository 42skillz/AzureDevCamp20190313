using System.Threading.Tasks;

namespace TrainTrain.Domain.Port
{
    public interface IProvideBookedSeats
    {
        Task<Reservation> BookSeats(ReservationAttempt reservationAttempt);
    }
}