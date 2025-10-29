using Domain;
using Repository;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Service
{
    public class CepService : ICepService
    {
        private readonly ICepRepository _repository;

        public CepService(ICepRepository repository)
        {
            _repository = repository;
        }

        public async Task<Cep> ConsultarCepAsync(string cep)
        {
            cep = cep.Replace("-", "").Trim();

            if (cep.Length != 8 || !long.TryParse(cep, out _))
                throw new ArgumentException("CEP inválido");

            var viaCepData = await ConsultarViaCepAsync(cep);

            var novoCep = new Cep
            {
                CepCode = viaCepData.Cep.Replace("-", ""),
                Logradouro = viaCepData.Logradouro,
                Complemento = viaCepData.Complemento,
                Bairro = viaCepData.Bairro,
                Localidade = viaCepData.Localidade,
                Uf = viaCepData.Uf,
                Ibge = viaCepData.Ibge,
                Gia = viaCepData.Gia,
                Ddd = viaCepData.Ddd,
                Siafi = viaCepData.Siafi,
                Erro = viaCepData.Erro,
                DataConsulta = DateTime.Now
            };

            return novoCep;
        }

        public async Task<IEnumerable<Cep>> GetAllCepsAsync() =>
            await _repository.GetAllCepsAsync();

        private async Task<ViaCepResponse> ConsultarViaCepAsync(string cep)
        {
            using var httpClient = new HttpClient();
            var url = $"https://viacep.com.br/ws/{cep}/json/";

            var response = await httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<ViaCepResponse>(json);

            if (data == null || data.Erro)
                throw new Exception("CEP não encontrado");

            return data;
        }
    }

    public class ViaCepResponse
    {
        public string Cep { get; set; }
        public string? Logradouro { get; set; }
        public string? Complemento { get; set; }
        public string? Bairro { get; set; }
        public string? Localidade { get; set; }
        public string? Uf { get; set; }
        public string? Ibge { get; set; }
        public string? Gia { get; set; }
        public string? Ddd { get; set; }
        public string? Siafi { get; set; }
        public bool Erro { get; set; }
    }
}
