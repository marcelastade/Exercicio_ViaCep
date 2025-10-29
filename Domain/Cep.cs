using System;

namespace Domain
{
    public class Cep 
    {  
        public int Id { get; set; }
        public string CepCode { get; set; }
        public string Logradouro { get; set; }
        public string Complemento { get; set; }
        public string Bairro { get; set; }
        public string Localidade { get; set; }
        public string Uf { get; set; }
        public string Ibge { get; set; }
        public string Gia { get; set; }
        public string Ddd { get; set; }
        public string Siafi { get; set; }
        public bool Erro { get; set; }
        public DateTime DataConsulta { get; set; } = DateTime.Now;
    }
}
