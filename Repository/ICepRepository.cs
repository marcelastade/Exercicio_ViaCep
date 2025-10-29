using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain;

namespace Repository
{
    public interface ICepRepository
    {
        Task AddCepAsync(Cep cep);
        Task<IEnumerable<Cep>> GetAllCepsAsync();
        Task<Cep> GetCepByCodeAsync(string cepCode);
    }
}