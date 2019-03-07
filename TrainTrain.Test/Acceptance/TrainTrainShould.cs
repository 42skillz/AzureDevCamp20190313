﻿using System.Threading.Tasks;
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
        public void Reserve_seats_when_train_is_empty()
        {
            var seatsRequestedCount = new SeatsRequested(3);

            var provideTrainTopology = BuildTrainTopology(_trainId, TrainTopologyGenerator.With_10_available_seats());
            var bookingReferenceService = BuildBookingReferenceService(_bookingReference);
            var provideReservation = BuildMakeReservation(_trainId, _bookingReference, new Seat[] { new Seat("A", 1), new Seat("A", 2), new Seat("A", 3)});

            IProvideTicket ticketOffice = new TicketOffice(provideTrainTopology, provideReservation, bookingReferenceService);
            var seatsReservationAdapter = new SeatsReservationAdapter(ticketOffice);
            var jsonReservation = seatsReservationAdapter.ReserveAsync(_trainId.Id, seatsRequestedCount.Count).Result;

            Check.That(jsonReservation)
                .IsEqualTo(
                    $"{{\"train_id\": \"{_trainId}\", \"booking_reference\": \"{_bookingReference}\", \"seats\": [\"1A\", \"2A\", \"3A\"]}}");
        }

        [Test]
        public void Not_reserve_seats_when_it_exceed_max_capacity_threshold()
        {
            var seatsRequestedCount = new SeatsRequested(3);

            var provideTrainTopology =
                BuildTrainTopology(_trainId, TrainTopologyGenerator.With_10_seats_and_6_already_reserved());
            var bookingReferenceService = BuildBookingReferenceService(_bookingReference);
            var provideReservation = BuildMakeReservation(_trainId, _bookingReference);


            var ticketOffice = new TicketOffice(provideTrainTopology, provideReservation, bookingReferenceService);
            var reservation = ticketOffice.Reserve(_trainId, seatsRequestedCount).Result;

            Check.That(SeatsReservationAdapter.AdaptReservation(reservation))
                .IsEqualTo($"{{\"train_id\": \"{_trainId}\", \"booking_reference\": \"\", \"seats\": []}}");
        }

        [Test]
        public void Reserve_all_seats_in_the_same_coach()
        {
            var seatsRequestedCount = new SeatsRequested(2);

            var provideTrainTopology = BuildTrainTopology(_trainId,
                TrainTopologyGenerator.With_2_coaches_and_9_seats_already_reserved_in_the_first_coach());

            var provideReservation = BuildMakeReservation(_trainId, _bookingReference, new Seat[] {new Seat("B", 1), new Seat("B", 2)});

            var bookingReferenceService = BuildBookingReferenceService(_bookingReference);

            var ticketOffice = new TicketOffice(provideTrainTopology, provideReservation, bookingReferenceService);
            var reservation = ticketOffice.Reserve(_trainId, seatsRequestedCount).Result;

            Check.That(SeatsReservationAdapter.AdaptReservation(reservation))
                .IsEqualTo(
                    $"{{\"train_id\": \"{_trainId}\", \"booking_reference\": \"{_bookingReference}\", \"seats\": [\"1B\", \"2B\"]}}");
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
    }
}