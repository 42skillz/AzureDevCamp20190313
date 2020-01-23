using System.Collections.Generic;
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
        public async Task 
            Reserve_seats_when_train_is_empty()
        {
            // Imperative shell
            var seatsExpected = new List<Seat> { new Seat("A", 1), new Seat("A", 2), new Seat("A", 3) };
            
            var (provideTrainTopology, provideBookingReference, provideReservation) =
                BuildInputAdapters(TrainTopologyGenerator.With_10_available_seats(), seatsExpected.ToArray());

            var seatsRequested = new SeatsRequested(3);

            // Call functional core architecture
            var reservation = await TicketOfficeService.TryReserve(await provideTrainTopology.GetTrain(_trainId), seatsRequested, await provideBookingReference.GetBookingReference()) 
                // Imperative shell
                .Select(async reservationAttempt => await provideReservation.BookSeats(reservationAttempt))
                .GetValueOrFallback(Task.FromResult(new Reservation(_trainId)));

            Check.That(ReservationAdapter.AdaptReservation(reservation))
                .IsEqualTo(
                    $"{{\"train_id\": \"{_trainId}\", \"booking_reference\": \"{_bookingReference}\", \"seats\": {ReservationAdapter.AdaptSeats(seatsExpected)}}}");
        }


        [Test]
        public async Task 
            Not_reserve_seats_when_it_exceed_max_capacity_threshold()
        {
            // Imperative shell
            var (provideTrainTopology, provideBookingReference, provideReservation) =
                BuildInputAdapters(TrainTopologyGenerator.With_10_seats_and_6_already_reserved());

            var seatsRequested = new SeatsRequested(3);

            // Call functional core architecture
            var reservation = await TicketOfficeService.TryReserve(await provideTrainTopology.GetTrain(_trainId), seatsRequested,
                    await provideBookingReference.GetBookingReference())
                // Imperative shell
                .Select(async reservationAttempt => await provideReservation.BookSeats(reservationAttempt))
                .GetValueOrFallback(Task.FromResult(new Reservation(_trainId)));

            Check.That(ReservationAdapter.AdaptReservation(reservation))
                .IsEqualTo($"{{\"train_id\": \"{_trainId}\", \"booking_reference\": \"\", \"seats\": []}}");
        }


        [Test]
        public async Task 
            Reserve_all_seats_in_the_same_coach()
        {
            // Imperative shell
            var seatsExpected = new List<Seat> { new Seat("B", 1), new Seat("B", 2) };

            var (provideTrainTopology, provideBookingReference, provideReservation) =
                BuildInputAdapters(TrainTopologyGenerator.With_2_coaches_and_9_seats_already_reserved_in_the_first_coach(), seatsExpected.ToArray());

            var seatsRequested = new SeatsRequested(2);

            // Call functional core architecture
            var reservation = await TicketOfficeService.TryReserve(await provideTrainTopology.GetTrain(_trainId), seatsRequested,
                    await provideBookingReference.GetBookingReference())
                // Imperative shell
                .Select(async reservationAttempt => await provideReservation.BookSeats(reservationAttempt))
                .GetValueOrFallback(Task.FromResult(new Reservation(_trainId)));

            Check.That(ReservationAdapter.AdaptReservation(reservation))
                .IsEqualTo(
                    $"{{\"train_id\": \"{_trainId}\", \"booking_reference\": \"{_bookingReference}\", \"seats\": {ReservationAdapter.AdaptSeats(seatsExpected)}}}");
        }

        [Test]
        public async Task 
            Reserve_at_least_seats_in_several_coaches()
        {
            // Imperative shell
            var seatsExpected = new List<Seat> { new Seat("A", 7), new Seat("A", 8), new Seat("A", 9), new Seat("A", 10), 
                                                 new Seat("B", 5), new Seat("B", 6) };

            var (provideTrainTopology, provideBookingReference, provideReservation) =
                BuildInputAdapters(TrainTopologyGenerator.With_3_coaches_and_6_then_4_seats_already_reserved(), seatsExpected.ToArray());

            var seatsRequested = new SeatsRequested(6);

            // Call functional core architecture
            var reservation = await TicketOfficeService.TryReserve(await provideTrainTopology.GetTrain(_trainId), seatsRequested,
                    await provideBookingReference.GetBookingReference())
                // Imperative shell
                .Select(async reservationAttempt => await provideReservation.BookSeats(reservationAttempt))
                .GetValueOrFallback(Task.FromResult(new Reservation(_trainId)));

            Check.That(ReservationAdapter.AdaptReservation(reservation))
                .IsEqualTo(
                    $"{{\"train_id\": \"{_trainId}\", \"booking_reference\": \"{_bookingReference}\", \"seats\": {ReservationAdapter.AdaptSeats(seatsExpected)}}}");
        }

        private static IProvideBookingReference 
            BuildBookingReferenceService(BookingReference bookingReference)
        {
            var bookingReferenceService = Substitute.For<IProvideBookingReference>();
            bookingReferenceService.GetBookingReference().Returns(Task.FromResult(bookingReference));
            return bookingReferenceService;
        }

        private static IProvideTrainTopology 
            BuildTrainTopology(TrainId trainId, string trainTopology)
        {
            var trainDataService = Substitute.For<IProvideTrainTopology>();
            trainDataService.GetTrain(trainId)
                .Returns(Task.FromResult(new Train(trainId,
                    TrainDataServiceAdapter.AdaptTrainTopology(trainTopology))));

            return trainDataService;
        }

        private static IProvideReservation 
            BuildReservation(TrainId trainId, BookingReference bookingReference, params Seat[] seats)
        {
            var trainDataService = Substitute.For<IProvideReservation>();

            trainDataService.BookSeats(Arg.Any<ReservationAttempt>())
                .Returns(Task.FromResult(new Reservation(trainId, bookingReference, seats)));

            return trainDataService;
        }

        private (IProvideTrainTopology, IProvideBookingReference, IProvideReservation) 
            BuildInputAdapters(string trainTopology, params Seat[] seats)
        {
            var provideTrainTopology = BuildTrainTopology(_trainId, trainTopology);
            var provideBookingReference = BuildBookingReferenceService(_bookingReference);
            var provideReservation =
                BuildReservation(_trainId, _bookingReference, seats);
            return (provideTrainTopology, provideBookingReference, provideReservation);
        }
    }
}