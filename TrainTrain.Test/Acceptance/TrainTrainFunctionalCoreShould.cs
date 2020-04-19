using System.Collections.Generic;
using System.Threading.Tasks;
using NFluent;
using NSubstitute;
using NUnit.Framework;
using TrainTrain.Domain;
using TrainTrain.Domain.Port;
using TrainTrain.Infra;
using TrainTrain.Infra.Adapter;

namespace TrainTrain.Test.Acceptance
{
    public class TrainTrainFunctionalCoreShould
    {
        [Test]
        public async Task 
            Reserve_seats_when_train_is_empty()
        {
            const string bookingReference = "341RTA";
            const string trainNumber = "9043-2019-03-13";
            const int seatsRequestedCount = 3;

            var seatsExpected = new List<Seat> { new Seat("A", 1), new Seat("A", 2), new Seat("A", 3) };

            var (provideTrainTopology, provideBookingReference, provideReservation) =
                BuildInputAdapters(TrainTopologyGenerator.With_10_available_seats(), trainNumber, bookingReference,  seatsExpected.ToArray());

            // Call imperative Shell
            var reservation = await TrainReservation
                .ReserveSeats(trainNumber, seatsRequestedCount, provideTrainTopology, provideBookingReference, provideReservation);

            Check.That(reservation)
                .IsEqualTo(
                    $"{{\"train_id\": \"{trainNumber}\", \"booking_reference\": \"{bookingReference}\", \"seats\": {ReservationAdapter.AdaptSeats(seatsExpected)}}}");
        }


        [Test]
        public async Task 
            Not_reserve_seats_when_it_exceed_max_capacity_threshold()
        {
            var bookingReference = "341RTA";
            var trainNumber = "9043-2019-03-13";
            var seatsRequestedCount = 3;

            var (provideTrainTopology, provideBookingReference, provideReservation) =
                BuildInputAdapters(
                    TrainTopologyGenerator.With_10_seats_and_6_already_reserved(), 
                    trainNumber, 
                    bookingReference);


            var reservation = await TrainReservation.ReserveSeats(trainNumber, 
                seatsRequestedCount, provideTrainTopology, provideBookingReference, provideReservation);

            Check.That(reservation)
                .IsEqualTo($"{{\"train_id\": \"{trainNumber}\", \"booking_reference\": \"\", \"seats\": []}}");
        }


        [Test]
        public async Task 
            Reserve_all_seats_in_the_same_coach()
        {
            var bookingReference = "341RTA";
            var trainNumber = "9043-2019-03-13";
            var seatsRequestedCount = 2;

            var seatsExpected = new List<Seat> { new Seat("B", 1), new Seat("B", 2) };

            var (provideTrainTopology, provideBookingReference, provideReservation) =
                BuildInputAdapters(
                    TrainTopologyGenerator.With_2_coaches_and_9_seats_already_reserved_in_the_first_coach(),
                    trainNumber,
                    bookingReference,
                    seatsExpected.ToArray());

            var reservation = await TrainReservation.ReserveSeats(trainNumber, 
                seatsRequestedCount, provideTrainTopology, provideBookingReference, provideReservation);

            Check.That(reservation)
                .IsEqualTo(
                    $"{{\"train_id\": \"{trainNumber}\", \"booking_reference\": \"{bookingReference}\", \"seats\": {ReservationAdapter.AdaptSeats(seatsExpected)}}}");
        }

        [Test]
        public async Task 
            Reserve_at_least_seats_in_several_coaches()
        {
            var bookingReference = "341RTA";
            var trainNumber = "9043-2019-03-13";
            var seatsRequestedCount = 6;

            var seatsExpected = new List<Seat> { new Seat("A", 7), new Seat("A", 8), new Seat("A", 9), new Seat("A", 10), 
                                                 new Seat("B", 5), new Seat("B", 6) };

            var (provideTrainTopology, provideBookingReference, provideReservation) =
                BuildInputAdapters(
                    TrainTopologyGenerator.With_3_coaches_and_6_then_4_seats_already_reserved(),
                    trainNumber,
                    bookingReference,
                    seatsExpected.ToArray());

            var reservation = await TrainReservation.ReserveSeats(trainNumber, 
                seatsRequestedCount, provideTrainTopology, provideBookingReference, provideReservation);

            Check.That(reservation)
                .IsEqualTo(
                    $"{{\"train_id\": \"{trainNumber}\", \"booking_reference\": \"{bookingReference}\", \"seats\": {ReservationAdapter.AdaptSeats(seatsExpected)}}}");
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

        private static IBookSeats 
            BuildReservation(TrainId trainId, BookingReference bookingReference, params Seat[] seats)
        {
            var trainDataService = Substitute.For<IBookSeats>();

            trainDataService.BookSeats(Arg.Any<ReservationAttempt>())
                .Returns(Task.FromResult(new Reservation(trainId, bookingReference, seats)));

            return trainDataService;
        }

        private (IProvideTrainTopology, IProvideBookingReference, IBookSeats) 
            BuildInputAdapters(string trainTopology, string trainNumber, string bookingRef, params Seat[] seats)
        {
            var trainId = new TrainId(trainNumber);
            var bookingReference = new BookingReference(bookingRef);
            var provideTrainTopology = BuildTrainTopology(trainId, trainTopology);
            var provideBookingReference = BuildBookingReferenceService(bookingReference);
            var provideReservation =
                BuildReservation(trainId, bookingReference, seats);
            return (provideTrainTopology, provideBookingReference, provideReservation);
        }
    }
}