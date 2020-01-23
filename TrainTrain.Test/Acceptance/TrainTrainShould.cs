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
    public class TrainTrainShould
    {
        private readonly TrainId _trainId = new TrainId("9043-2019-03-13");
        private readonly BookingReference _bookingReference = new BookingReference("341RTFA");

        [Test]
        public async Task Reserve_seats_when_train_is_empty()
        {
            var seatsRequestedCount = new SeatsRequested(3);
            
            var seatsExpected = new List<Seat> {new Seat("A", 1), new Seat("A", 2), new Seat("A", 3) };

            var provideTrainTopology = BuildTrainTopologyProvider(_trainId, TrainTopologyGenerator.With_10_available_seats());
            var provideBookingReference = BuildBookingReferenceProvider(_bookingReference);
            var provideReservation = BuildReservationProvider(_trainId, _bookingReference, seatsExpected.ToArray());

            // Hexagon
            IProvideTicket ticketOffice = new TicketOfficeService(provideTrainTopology, provideReservation, provideBookingReference);

            var seatsReservationAdapter = new ReservationAdapter(ticketOffice);
            var jsonReservation =  await seatsReservationAdapter.ReserveAsync(_trainId.Id, seatsRequestedCount.Count);

            Check.That(jsonReservation)
                .IsEqualTo(
                    $"{{\"train_id\": \"{_trainId}\", \"booking_reference\": \"{_bookingReference}\", \"seats\": {ReservationAdapter.AdaptSeats(seatsExpected)}}}");
        }

        [Test]
        public async Task Not_reserve_seats_when_it_exceed_max_capacity_threshold()
        {
            var seatsRequestedCount = new SeatsRequested(3);

            var provideTrainTopology = BuildTrainTopologyProvider(_trainId, TrainTopologyGenerator.With_10_seats_and_6_already_reserved());
            var provideBookingReference = BuildBookingReferenceProvider(_bookingReference);
            var provideReservation = BuildReservationProvider(_trainId, _bookingReference);

            // hexagon
            var ticketOffice = new TicketOfficeService(provideTrainTopology, provideReservation, provideBookingReference);
            
            var seatsReservationAdapter = new ReservationAdapter(ticketOffice);

            Check.That(await seatsReservationAdapter.ReserveAsync(_trainId.Id, seatsRequestedCount.Count))
                .IsEqualTo($"{{\"train_id\": \"{_trainId}\", \"booking_reference\": \"\", \"seats\": []}}");
        }

        [Test]
        public async Task Reserve_all_seats_in_the_same_coach()
        {
            var seatsRequestedCount = new SeatsRequested(2);

            var seatsExpected = new List<Seat> { new Seat("B", 1), new Seat("B", 2) };

            var provideTrainTopology = BuildTrainTopologyProvider(_trainId, TrainTopologyGenerator.With_2_coaches_and_9_seats_already_reserved_in_the_first_coach());
            var provideReservation = BuildReservationProvider(_trainId, _bookingReference, seatsExpected.ToArray());
            var bookingReferenceService = BuildBookingReferenceProvider(_bookingReference);

            // hexagon
            var ticketOffice = new TicketOfficeService(provideTrainTopology, provideReservation, bookingReferenceService);
            
            var seatsReservationAdapter = new ReservationAdapter(ticketOffice);

            Check.That(await seatsReservationAdapter.ReserveAsync(_trainId.Id, seatsRequestedCount.Count))
                .IsEqualTo(
                    $"{{\"train_id\": \"{_trainId}\", \"booking_reference\": \"{_bookingReference}\", \"seats\": {ReservationAdapter.AdaptSeats(seatsExpected)}}}");
        }


        [Test]
        public async Task Reserve_at_least_seats_in_several_coaches()
        {
            var seatsExpected = new List<Seat>
            {
                new Seat("A", 7), new Seat("A", 8), new Seat("A", 9), new Seat("A", 10), 
                new Seat("B", 5), new Seat("B", 6)
            };

            var seatsRequestedCount = new SeatsRequested(6);

            var provideTrainTopology = BuildTrainTopologyProvider(_trainId, TrainTopologyGenerator.With_3_coaches_and_6_then_4_seats_already_reserved());
            var provideReservation = BuildReservationProvider(_trainId, _bookingReference, seatsExpected.ToArray());
            var provideBookingReference = BuildBookingReferenceProvider(_bookingReference);

            // hexagon
            var ticketOffice = new TicketOfficeService(provideTrainTopology, provideReservation, provideBookingReference);
            
            var seatsReservationAdapter = new ReservationAdapter(ticketOffice);

            Check.That(await seatsReservationAdapter.ReserveAsync(_trainId.Id, seatsRequestedCount.Count))
                .IsEqualTo(
                    $"{{\"train_id\": \"{_trainId}\", \"booking_reference\": \"{_bookingReference}\", \"seats\": {ReservationAdapter.AdaptSeats(seatsExpected)}}}");
        }

        private static IProvideBookingReference BuildBookingReferenceProvider(BookingReference bookingReference)
        {
            var bookingReferenceService = Substitute.For<IProvideBookingReference>();
            bookingReferenceService.GetBookingReference().Returns(Task.FromResult(bookingReference));
            return bookingReferenceService;
        }

        private static IProvideTrainTopology BuildTrainTopologyProvider(TrainId trainId, string trainTopology)
        {
            var trainDataService = Substitute.For<IProvideTrainTopology>();
            trainDataService.GetTrain(trainId)
                .Returns(Task.FromResult(new Train(trainId,
                    TrainDataServiceAdapter.AdaptTrainTopology(trainTopology))));

            return trainDataService;
        }

        private static IProvideReservation BuildReservationProvider(TrainId trainId,
            BookingReference bookingReference, params Seat[] seats)
        {
            var trainDataService = Substitute.For<IProvideReservation>();

            trainDataService.BookSeats(Arg.Any<ReservationAttempt>())
                .Returns(Task.FromResult(new Reservation(trainId, bookingReference, seats)));

            return trainDataService;
        }
    }
}