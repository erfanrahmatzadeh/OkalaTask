using Microsoft.AspNetCore.Mvc;
using OkalaTask.Interface;
using OkalaTask.Models;

namespace OkalaTask.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CryptoQuoteController : ControllerBase
    {
        private readonly ICryptoQuoteService _cryptoQuoteService;

        public CryptoQuoteController(ICryptoQuoteService cryptoQuoteService) => _cryptoQuoteService = cryptoQuoteService;

        /// <summary>
        /// Get cryptocurrency quote in specified currencies.
        /// </summary>
        /// <param name="request">The cryptocurrency code request.</param>
        /// <returns>Exchange rates for the given cryptocurrency code.</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetQuote([FromBody] CryptoQuoteRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.CryptoCode))
                return BadRequest("Crypto code is required.");

            var result = await _cryptoQuoteService.GetCryptoQuoteAsync(request.CryptoCode.ToUpper());
            if (result == null)
                return NotFound("Unable to retrieve data for the given crypto code.");

            return Ok(new CryptoQuoteResponse
            {
                CryptoCode = request.CryptoCode,
                Rates = result
            });
        }
    }
}
