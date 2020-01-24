using System.Threading.Tasks;

namespace TrainTrain.Domain.Port
{
    public interface IProvideTickets
    {
        Task<Reservation> Reserve(TrainId trainId, SeatsRequested seatsRequested);
    }
}