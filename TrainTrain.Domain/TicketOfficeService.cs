using System.Threading.Tasks;
using TrainTrain.Domain.Port;

namespace TrainTrain.Domain
{
    public class TicketOfficeService : IProvideTicket
    {
        // Functional core
        public static Maybe<ReservationAttempt> 
            TryReserve(Train train, SeatsRequested seatsRequested, BookingReference bookingReference)
        {
            if (!train.DoesNotExceedOverallCapacity(seatsRequested)) return new Maybe<ReservationAttempt>();

            var reservationAttempt = train.BuildReservationAttempt(seatsRequested);

            return reservationAttempt.IsFulFilled
                ? new Maybe<ReservationAttempt>(reservationAttempt.AssignBookingReference(bookingReference))
                : new Maybe<ReservationAttempt>();
        }

        // Hexagon
        public async Task<Reservation> 
            Reserve(TrainId trainId, SeatsRequested seatsRequested)
        {
            var train = await _provideTrainTopology.GetTrain(trainId);

            if (train.DoesNotExceedOverallCapacity(seatsRequested))
            {
                var reservationAttempt = train.BuildReservationAttempt(seatsRequested);

                if (reservationAttempt.IsFulFilled)
                {
                    return await _provideReservation.BookSeats(
                        reservationAttempt.AssignBookingReference(await _provideBookingReference
                            .GetBookingReference()));
                }
            }

            return new ReservationFailure(trainId);
        }

        public TicketOfficeService(IProvideTrainTopology provideTrainTopology, IProvideReservation provideReservation,
            IProvideBookingReference provideBookingReference)
        {
            _provideTrainTopology = provideTrainTopology;
            _provideReservation = provideReservation;
            _provideBookingReference = provideBookingReference;
        }

        private readonly IProvideBookingReference _provideBookingReference;
        private readonly IProvideReservation _provideReservation;
        private readonly IProvideTrainTopology _provideTrainTopology;
    }
}