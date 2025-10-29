using Domain;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Repository;
using Service;
using System.Net;
namespace performance_cache.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CepController : ControllerBase
    {
        private readonly ICepService cepService;
        private readonly ICepRepository cepRepository;
        private readonly ICacheService cacheService;
        private readonly ILogger<CepController> logger;
        private const string cacheKey = "ceps-cache";

        public CepController(
            ICepService cepService,
            ICepRepository cepRepository,
            ICacheService cacheService,
            ILogger<CepController> logger)
        {
            this.cepService = cepService;
            this.cepRepository = cepRepository;
            this.cacheService = cacheService;
            this.logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                logger.LogInformation("Iniciando busca de CEPs");

                try
                {
                    await cacheService.SetExpiryAsync(cacheKey, TimeSpan.FromMinutes(20));
                    string? cachedCeps = await cacheService.GetAsync(cacheKey);

                    if (!string.IsNullOrEmpty(cachedCeps)) 
                    {
                        logger.LogInformation("CEPs encontrados no cache Redis");
                        return Ok(cachedCeps);
                    }
                }
                catch (Exception redisEx)
                {
                    logger.LogWarning(redisEx, "Erro ao acessar cache Redis, continuando sem cache");
                }

                var cepList = await cepRepository.GetAllCepsAsync();

                if (cepList == null || !cepList.Any())
                {
                    logger.LogInformation("Nenhum CEP encontrado no banco de dados");
                    return Ok(new List<ViaCepResponse>());
                }
                try
                {
                    var cepListJson = JsonConvert.SerializeObject(cepList);
                    await cacheService.SetAsync(cacheKey, cepListJson, TimeSpan.FromMinutes(20));
                    logger.LogInformation("Dados salvos no cache Redis");
                }
                catch (Exception cacheEx)
                {
                    logger.LogWarning(cacheEx, "Erro ao salvar no cache Redis, mas dados foram retornados");
                }

                logger.LogInformation("Retornando {Count} CEPs", cepList.Count());
                return Ok(cepList);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro interno ao buscar CEPs");
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new { message = "Erro interno do servidor ao buscar CEPs", timestamp = DateTime.UtcNow });
            }
        }
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ViaCepResponse request)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.Cep))
                {
                    logger.LogWarning("Tentativa de consultar CEP com dados nulos ou inválidos");
                    return BadRequest(new { message = "O campo 'cep' é obrigatório", timestamp = DateTime.UtcNow });
                }

                string cepLimpo = request.Cep.Replace("-", "").Trim();
                if (cepLimpo.Length != 8 || !cepLimpo.All(char.IsDigit))
                {
                    logger.LogWarning("CEP inválido informado: {Cep}", request.Cep);
                    return BadRequest(new { message = "CEP inválido. Informe 8 dígitos numéricos.", timestamp = DateTime.UtcNow });
                }

                logger.LogInformation("Consultando CEP {Cep} via API ViaCEP", cepLimpo);

                var viaCepData = await cepService.ConsultarCepAsync(cepLimpo);

                if (viaCepData == null || viaCepData.Erro)
                {
                    logger.LogWarning("CEP {Cep} não encontrado no ViaCEP", cepLimpo);
                    return NotFound(new { message = "CEP não encontrado.", timestamp = DateTime.UtcNow });
                }

                await cepRepository.AddCepAsync(viaCepData);

             
   await InvalidateCache();

                logger.LogInformation("CEP {Cep} consultado e salvo com sucesso", viaCepData.CepCode);
                return CreatedAtAction(nameof(Get), new { cep = viaCepData.CepCode}, viaCepData);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro interno ao consultar ou salvar CEP");
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new { message = "Erro interno do servidor ao consultar CEP", timestamp = DateTime.UtcNow });
            }
        }
        private async Task InvalidateCache()
        {
            try
            {
                await cacheService.DeleteAsync(cacheKey);
                logger.LogInformation("Cache invalidado com sucesso");
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Erro ao invalidar cache Redis, mas operação continuará");
            }
        }
    }
}
