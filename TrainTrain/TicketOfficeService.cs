using System.Threading.Tasks;

namespace TrainTrain
{
    public class TicketOfficeService
    {
        private const string UriTrainDataService = "http://localhost:50680";
        private const string UriBookingReferenceService = "http://localhost:51691/";
        private readonly IProvideBookingReference _provideBookingReference;
        private readonly IProvideTrainTopology _provideTrainTopology;

        public TicketOfficeService() : this(new TrainDataServiceAdapter(UriTrainDataService),
            new BookingReferenceServiceAdapter(UriBookingReferenceService))
        {
        }

        public TicketOfficeService(IProvideTrainTopology provideProvideTrainTopology, IProvideBookingReference provideProvideBookingReference)
        {
            _provideTrainTopology = provideProvideTrainTopology;
            _provideBookingReference = provideProvideBookingReference;
        }

        public async Task<Reservation> Reserve(string trainId, int seatsRequestedCount)
        {
            var train = await _provideTrainTopology.GetTrain(trainId);

            if (train.DoesNotExceedOverallTrainCapacity(seatsRequestedCount))
            {
                var reservationAttempt = train.BuildReservationAttempt(seatsRequestedCount);

                if (reservationAttempt.IsFulFilled)
                {
                    var bookingReference = await _provideBookingReference.GetBookingReference();
                    var assignBookingReference = reservationAttempt.AssignBookingReference(bookingReference);
                    await _provideTrainTopology.BookSeats(assignBookingReference);
                    return assignBookingReference.Confirm();
                }
            }

            return new ReservationFailure(trainId);
        }
    }
}