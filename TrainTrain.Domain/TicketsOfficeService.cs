using System.Threading.Tasks;
using TrainTrain.Domain.Port;

namespace TrainTrain.Domain
{
    public class TicketsOfficeService : IProvideTickets
    {
        // Hexagon implementation
        // ======================

        private readonly IProvideBookedSeats _provideBookedSeats;
        private readonly IProvideBookingReference _provideBookingReference;
        private readonly IProvideTrainTopology _provideTrainTopology;

        // Hexagon constructor received all ports 
        public TicketsOfficeService(IProvideTrainTopology provideTrainTopology, IProvideBookedSeats provideBookedSeats,
            IProvideBookingReference provideBookingReference)
        {
            _provideTrainTopology = provideTrainTopology;
            _provideBookedSeats = provideBookedSeats;
            _provideBookingReference = provideBookingReference;
        }

        // The hexagon's design contains ports which are not "pure" by nature
        public async Task<Reservation>
            Reserve(TrainId trainId, SeatsRequested seatsRequested)
        {
            var train = await _provideTrainTopology.GetTrain(trainId);

            if (train.DoesNotExceedOverallCapacity(seatsRequested))
            {
                var reservationAttempt = train.BuildReservationAttempt(seatsRequested);

                if (reservationAttempt.IsFulFilled)
                    return await _provideBookedSeats.BookSeats(
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
            if (!train.DoesNotExceedOverallCapacity(seatsRequested)) return new Maybe<ReservationAttempt>();

            var reservationAttempt = train.BuildReservationAttempt(seatsRequested);

            return reservationAttempt.IsFulFilled
                ? new Maybe<ReservationAttempt>(reservationAttempt.AssignBookingReference(bookingReference))
                : new Maybe<ReservationAttempt>();
        }
    }
}