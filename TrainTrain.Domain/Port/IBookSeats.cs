using System.Threading.Tasks;

namespace TrainTrain.Domain.Port
{
    public interface IBookSeats
    {
        Task<Reservation> BookSeats(ReservationAttempt reservationAttempt);
    }
}