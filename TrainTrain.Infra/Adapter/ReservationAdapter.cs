using System.Collections.Generic;
using System.Threading.Tasks;
using TrainTrain.Domain;
using TrainTrain.Domain.Port;

namespace TrainTrain.Infra.Adapter
{
    public class ReservationAdapter
    {
        private readonly IProvideTicket _ticketOffice;

        public ReservationAdapter(IProvideTicket ticketOffice)
        {
            _ticketOffice = ticketOffice;
        }

        public async Task<string> ReserveAsync(string trainId, int seatsRequestedCount)
        {
            return AdaptReservation(await _ticketOffice.Reserve(new TrainId(trainId), new SeatsRequested(seatsRequestedCount)));
        }

        public static string AdaptReservation(Reservation reservation)
        {
            return $"{{\"train_id\": \"{reservation.TrainId}\", \"booking_reference\": \"{reservation.BookingReference}\", \"seats\": {AdaptSeats(reservation.Seats)}}}";
        }

        public static string AdaptSeats(IEnumerable<Seat> seats)
        {
            return $"[{string.Join(", ", seats)}]";
        }
    }
}