using System.Threading.Tasks;
using NFluent;
using NSubstitute;
using NUnit.Framework;
using TrainTrain.Domain;
using TrainTrain.Domain.Port;
using TrainTrain.Infra.Adapter;

namespace TrainTrain.Test.Acceptance
{
    public class TrainTrainFunctionalCoreShould
    {
        private readonly BookingReference _bookingReference = new BookingReference("341RTFA");
        private readonly TrainId _trainId = new TrainId("9043-2019-03-13");

        [Test]
        public async Task Reserve_seats_when_train_is_empty()
        {
            var (provideTrainTopology, bookingReferenceService, provideReservation) =
                BuildInputAdapters(TrainTopologyGenerator.With_10_available_seats(), new Seat("A", 1), new Seat("A", 2),
                    new Seat("A", 3));

            var seatsRequested = new SeatsRequested(3);

            var reservation = await new TicketOfficeService()
                .TryReserve(await provideTrainTopology.GetTrain(_trainId), seatsRequested,
                    await bookingReferenceService.GetBookingReference())
                .Select(async reservationAttempt => await provideReservation.BookSeats(reservationAttempt))
                .GetValueOrFallback(Task.FromResult(new Reservation(_trainId)));

            Check.That(SeatsReservationAdapter.AdaptReservation(reservation))
                .IsEqualTo(
                    $"{{\"train_id\": \"{_trainId}\", \"booking_reference\": \"{_bookingReference}\", \"seats\": [\"1A\", \"2A\", \"3A\"]}}");
        }


        [Test]
        public async Task Not_reserve_seats_when_it_exceed_max_capacity_threshold()
        {
            var (provideTrainTopology, bookingReferenceService, provideReservation) =
                BuildInputAdapters(TrainTopologyGenerator.With_10_seats_and_6_already_reserved());

            var seatsRequested = new SeatsRequested(3);

            var reservation = await new TicketOfficeService()
                .TryReserve(await provideTrainTopology.GetTrain(_trainId), seatsRequested,
                    await bookingReferenceService.GetBookingReference())
                .Select(async reservationAttempt => await provideReservation.BookSeats(reservationAttempt))
                .GetValueOrFallback(Task.FromResult(new Reservation(_trainId)));

            Check.That(SeatsReservationAdapter.AdaptReservation(reservation))
                .IsEqualTo($"{{\"train_id\": \"{_trainId}\", \"booking_reference\": \"\", \"seats\": []}}");
        }


        [Test]
        public async Task Reserve_all_seats_in_the_same_coach()
        {
            var (provideTrainTopology, bookingReferenceService, provideReservation) =
                BuildInputAdapters(
                    TrainTopologyGenerator.With_2_coaches_and_9_seats_already_reserved_in_the_first_coach(),
                    new Seat("B", 1), new Seat("B", 2));

            var seatsRequested = new SeatsRequested(2);

            var reservation = await new TicketOfficeService()
                .TryReserve(await provideTrainTopology.GetTrain(_trainId), seatsRequested,
                    await bookingReferenceService.GetBookingReference())
                .Select(async reservationAttempt => await provideReservation.BookSeats(reservationAttempt))
                .GetValueOrFallback(Task.FromResult(new Reservation(_trainId)));

            Check.That(SeatsReservationAdapter.AdaptReservation(reservation))
                .IsEqualTo(
                    $"{{\"train_id\": \"{_trainId}\", \"booking_reference\": \"{_bookingReference}\", \"seats\": [\"1B\", \"2B\"]}}");
        }

        [Test]
        public async Task Reserve_at_least_seats_in_several_coaches()
        {
            var (provideTrainTopology, bookingReferenceService, provideReservation) =
                BuildInputAdapters(TrainTopologyGenerator.With_3_coaches_and_6_then_4_seats_already_reserved(),
                    new Seat("A", 7), new Seat("A", 8), new Seat("A", 9), new Seat("A", 10), new Seat("B", 5),
                    new Seat("B", 6));

            var seatsRequested = new SeatsRequested(6);

            var reservation = await new TicketOfficeService()
                .TryReserve(await provideTrainTopology.GetTrain(_trainId), seatsRequested,
                    await bookingReferenceService.GetBookingReference())
                .Select(async reservationAttempt => await provideReservation.BookSeats(reservationAttempt))
                .GetValueOrFallback(Task.FromResult(new Reservation(_trainId)));

            Check.That(SeatsReservationAdapter.AdaptReservation(reservation))
                .IsEqualTo(
                    $"{{\"train_id\": \"{_trainId}\", \"booking_reference\": \"{_bookingReference}\", \"seats\": [\"7A\", \"8A\", \"9A\", \"10A\", \"5B\", \"6B\"]}}");
        }

        private static IProvideBookingReference BuildBookingReferenceService(BookingReference bookingReference)
        {
            var bookingReferenceService = Substitute.For<IProvideBookingReference>();
            bookingReferenceService.GetBookingReference().Returns(Task.FromResult(bookingReference));
            return bookingReferenceService;
        }

        private static IProvideTrainTopology BuildTrainTopology(TrainId trainId, string trainTopology)
        {
            var trainDataService = Substitute.For<IProvideTrainTopology>();
            trainDataService.GetTrain(trainId)
                .Returns(Task.FromResult(new Train(trainId,
                    TrainDataServiceAdapter.AdaptTrainTopology(trainTopology))));

            return trainDataService;
        }

        private static IProvideReservation BuildMakeReservation(TrainId trainId,
            BookingReference bookingReference, params Seat[] seats)
        {
            var trainDataService = Substitute.For<IProvideReservation>();

            trainDataService.BookSeats(Arg.Any<ReservationAttempt>())
                .Returns(Task.FromResult(new Reservation(trainId, bookingReference, seats)));

            return trainDataService;
        }

        private (IProvideTrainTopology, IProvideBookingReference, IProvideReservation) BuildInputAdapters(
            string trainTopology, params Seat[] seats)
        {
            var provideTrainTopology = BuildTrainTopology(_trainId, trainTopology);
            var bookingReferenceService = BuildBookingReferenceService(_bookingReference);
            var provideReservation =
                BuildMakeReservation(_trainId, _bookingReference, seats);
            return (provideTrainTopology, bookingReferenceService, provideReservation);
        }
    }
}