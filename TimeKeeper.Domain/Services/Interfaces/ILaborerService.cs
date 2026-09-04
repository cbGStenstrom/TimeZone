using TimeKeeper.Domain.Models;

namespace TimeKeeper.Domain.Services.Interfaces
{
    public interface ILaborerService
    {
        Task<Laborer?> CreateUser(Laborer model);
        Task<Laborer?> GetFromCredentials(string username, string password);
        Task<Laborer?> GetFromId(int laborerID);
        Task<bool> UpdatePassword(Laborer model);
        Task<bool> UpdateUser(Laborer model);
    }
}