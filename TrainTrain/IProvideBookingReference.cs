using System.Threading.Tasks;

namespace TrainTrain
{
    public interface IProvideBookingReference
    {
        Task<string> GetBookingReference();
    }
}