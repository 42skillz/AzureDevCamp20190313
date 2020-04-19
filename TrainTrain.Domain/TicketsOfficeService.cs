using System.Threading.Tasks;
using TrainTrain.Domain.Port;

namespace TrainTrain.Domain
{
    public class TicketsOfficeService : IProvideTickets
    {
        // Hexagon implementation
        // ======================

        private readonly IBookSeats _bookSeats;
        private readonly IProvideBookingReference _provideBookingReference;
        private readonly IProvideTrainTopology _provideTrainTopology;

        // Hexagon constructor received all ports 
        public TicketsOfficeService(IProvideTrainTopology provideTrainTopology, IBookSeats bookSeats,
            IProvideBookingReference provideBookingReference)
        {
            _provideTrainTopology = provideTrainTopology;
            _bookSeats = bookSeats;
            _provideBookingReference = provideBookingReference;
        }

        // The hexagon's design contains ports which are not "pure" by design
        public async Task<Reservation>
            Reserve(TrainId trainId, SeatsRequested seatsRequested)
        {
            var train = await _provideTrainTopology.GetTrain(trainId);

            if (train.DoesNotExceedOverallCapacity(seatsRequested))
            {
                var reservationAttempt = train.BuildReservationAttempt(seatsRequested);

                if (reservationAttempt.IsFulFilled)
                    return await _bookSeats.BookSeats(
                        reservationAttempt.AssignBookingReference(await _provideBookingReference
                            .GetBookingReference()));
            }

            return new ReservationFailure(trainId);
        }

        // Functional core implementation
        // ==============================

        // Functional core is called by an imperative shell
        public static Maybe<ReservationAttempt>
            TryReserve(Train train, SeatsRequested seatsRequested, BookingReference bookingReference)
        {
            // no async await, all stuff in train domain
            if (!train.DoesNotExceedOverallCapacity(seatsRequested))
            {
                return ReservationAttemptFailure();
            }

            var reservationAttempt = train.BuildReservationAttempt(seatsRequested);

            if (reservationAttempt.IsFulFilled)
            {
                return new Maybe<ReservationAttempt>(
                    reservationAttempt.AssignBookingReference(bookingReference));
            }
            
            return ReservationAttemptFailure();
        }

        private static Maybe<ReservationAttempt> ReservationAttemptFailure()
        {
            return new Maybe<ReservationAttempt>();
        }
    }
}