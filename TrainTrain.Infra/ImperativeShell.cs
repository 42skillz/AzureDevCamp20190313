using System.Threading.Tasks;
using TrainTrain.Domain;
using TrainTrain.Domain.Port;
using TrainTrain.Infra.Adapter;

namespace TrainTrain.Infra
{
    public static class ImperativeShell
    {
        public static async Task<string> ReserveSeat(
            IProvideTrainTopology provideTrainTopology,
            IProvideBookingReference provideBookingReference, 
            IProvideBookedSeats provideBookedSeats,
            string trainNumber, 
            int seatsRequestedCount)
        {
            // Adapt from infra to domain
            var seatsRequested = new SeatsRequested(seatsRequestedCount);
            var trainId = new TrainId(trainNumber);

            // Call functional core architecture
            var reservation = await TicketsOfficeService.TryReserve(
                    await provideTrainTopology.GetTrain(trainId),
                    seatsRequested,
                    await provideBookingReference.GetBookingReference())
                .Select(async reservationAttempt => await provideBookedSeats.BookSeats(reservationAttempt))
                .GetValueOrFallback(Task.FromResult(new Reservation(trainId)));
            
            // Adapt from domain to infra
            return ReservationAdapter.AdaptReservation(reservation);
        }
    }
}