using Dapper;
using Domain;
using MySqlConnector;

namespace Repository
{
    public class CepRepository : ICepRepository
    {
        private readonly string _connectionString;

        public CepRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task AddCepAsync(Cep cep)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            var command = new MySqlCommand(
                @"INSERT INTO Ceps (Cep, Logradouro, Complemento, Bairro, Localidade, Uf, Ibge, Gia, Ddd, Siafi, DataConsulta)
                  VALUES (@Cep, @Logradouro, @Complemento, @Bairro, @Localidade, @Uf, @Ibge, @Gia, @Ddd, @Siafi, @DataConsulta)",
                connection);

            command.Parameters.AddWithValue("@Cep", cep.CepCode);
            command.Parameters.AddWithValue("@Logradouro", cep.Logradouro);
            command.Parameters.AddWithValue("@Complemento", cep.Complemento);
            command.Parameters.AddWithValue("@Bairro", cep.Bairro);
            command.Parameters.AddWithValue("@Localidade", cep.Localidade);
            command.Parameters.AddWithValue("@Uf", cep.Uf);
            command.Parameters.AddWithValue("@Ibge", cep.Ibge);
            command.Parameters.AddWithValue("@Gia", cep.Gia);
            command.Parameters.AddWithValue("@Ddd", cep.Ddd);
            command.Parameters.AddWithValue("@Siafi", cep.Siafi);
            command.Parameters.AddWithValue("@DataConsulta", cep.DataConsulta);

            await command.ExecuteNonQueryAsync();
        }

        public async Task<IEnumerable<Cep>> GetAllCepsAsync()
        {
            var ceps = new List<Cep>();

            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            var command = new MySqlCommand("SELECT * FROM Ceps", connection);
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                ceps.Add(new Cep
                {
                    Id = reader.GetInt32("Id"),
                    CepCode = reader.GetString("Cep"),
                    Logradouro = reader.GetString("Logradouro"),
                    Complemento = reader.GetString("Complemento"),
                    Bairro = reader.GetString("Bairro"),
                    Localidade = reader.GetString("Localidade"),
                    Uf = reader.GetString("Uf"),
                    Ibge = reader.GetString("Ibge"),
                    Gia = reader.GetString("Gia"),
                    Ddd = reader.GetString("Ddd"),
                    Siafi = reader.GetString("Siafi"),
                    DataConsulta = reader.GetDateTime("DataConsulta")
                });
            }

            return ceps;
        }

        public async Task<Cep> GetCepByCodeAsync(string cepCode)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            var command = new MySqlCommand("SELECT * FROM Ceps WHERE Cep = @Cep", connection);
            command.Parameters.AddWithValue("@Cep", cepCode);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new Cep
                {
                    Id = reader.GetInt32("Id"),
                    CepCode = reader.GetString("Cep"),
                    Logradouro = reader.GetString("Logradouro"),
                    Complemento = reader.GetString("Complemento"),
                    Bairro = reader.GetString("Bairro"),
                    Localidade = reader.GetString("Localidade"),
                    Uf = reader.GetString("Uf"),
                    Ibge = reader.GetString("Ibge"),
                    Gia = reader.GetString("Gia"),
                    Ddd = reader.GetString("Ddd"),
                    Siafi = reader.GetString("Siafi"),
                    DataConsulta = reader.GetDateTime("DataConsulta")
                };
            }

            return null;
        }
    }
}
