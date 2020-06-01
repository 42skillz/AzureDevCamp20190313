using System.Threading.Tasks;

namespace TrainTrain
{
    public interface IProvideTrainTopology
    {
        Task<Train> GetTrain(string trainId);
        Task BookSeats(ReservationAttempt reservationAttempt);
    }
}