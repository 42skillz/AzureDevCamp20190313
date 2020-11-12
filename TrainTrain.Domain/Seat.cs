namespace TrainTrain.Domain
{
    public sealed record Seat
    {
        public string CoachName { get; }
        public int SeatNumber { get; }
        private BookingReference BookingReference { get; }

        public Seat(string coachName, int seatNumber) : this(coachName, seatNumber, new BookingReference())
        {
        }

        public Seat(string coachName, int seatNumber, BookingReference bookingReference)
        {
            CoachName = coachName;
            SeatNumber = seatNumber;
            BookingReference = bookingReference;
        }

        public bool IsAvailable()
        {
            return !BookingReference.IsValid();
        }

        public override string ToString()
        {
            return $"\"{SeatNumber}{CoachName}\"";
        }
    }
}