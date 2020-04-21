using System.Threading.Tasks;
using TrainTrain.Domain;
using TrainTrain.Domain.Port;
using TrainTrain.Infra.Adapter;

namespace TrainTrain.Infra
{
    // Imperative Shell (mutable)
    public static class TrainReservation
    {
        public static async Task<string> ReserveSeats(string trainNumber, int seatsRequestedCount, 
            IProvideTrainTopology provideTrainTopology,
            IProvideBookingReference provideBookingReference,
            IBookSeats bookSeats)
        {   // Adapt from infra to domain
            var seatsRequested = new SeatsRequested(seatsRequestedCount);
            var trainId = new TrainId(trainNumber);

            var train = await provideTrainTopology.GetTrain(trainId);
            var bookingReference = await provideBookingReference.GetBookingReference();

            var reservation = await TicketsOfficeService
                // Call functional core
                .TryReserve(train, seatsRequested, bookingReference)
                    // Call right adapter BookedSeats to book seats 
                    .Select(async reservationAttempt => await bookSeats.BookSeats(reservationAttempt))
                        .GetValueOrFallback(Task.FromResult((Reservation) new ReservationFailure(trainId)));
            
            // Adapt from domain to infra
            return ReservationAdapter.AdaptReservation(reservation);
        }
    }
}