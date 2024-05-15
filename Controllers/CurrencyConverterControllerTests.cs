using BambooCardCurrencyConverterAPI.Controllers;
using BambooCardCurrencyConverterAPI.Interfaces;
using BambooCardCurrencyConverterAPI.Models;
using BambooCardCurrencyConverterAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace BambooCardCurrencyConverterAPI.Tests.Controllers
{
    /// <summary>
    /// The currency converter controller tests.
    /// </summary>
    public class CurrencyConverterControllerTests
    {
        private readonly Mock<IFrankfurterService> _mockService;
        private readonly CurrencyConverterController _controller;

        /// <summary>
        /// Initializes a new instance of the <see cref="CurrencyConverterControllerTests"/> class.
        /// </summary>
        public CurrencyConverterControllerTests()
        {
            _mockService = new Mock<IFrankfurterService>();
            _controller = new CurrencyConverterController(_mockService.Object);
        }

        /// <summary>
        /// Gets the latest rates_ returns ok result.
        /// </summary>
        /// <returns>A Task.</returns>
        [Fact]
        public async Task GetLatestRates_ReturnsOkResult()
        {
            var rates = new ExchangeRate { Base = "EUR", Date = DateTime.Now, Rates = new Dictionary<string, decimal> { { "USD", 1.2M } } };
            _mockService.Setup(s => s.GetLatestRatesAsync(It.IsAny<string>())).ReturnsAsync(rates);

            var result = await _controller.GetLatestRates("EUR");

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(rates, okResult.Value);
        }

        /// <summary>
        /// Converts the currency_ excludes certain currencies.
        /// </summary>
        /// <returns>A Task.</returns>
        [Fact]
        public async Task ConvertCurrency_ExcludesCertainCurrencies()
        {
            var request = new ConversionRequest { Amount = 100, FromCurrency = "TRY", ToCurrency = "USD" };

            var result = await _controller.ConvertCurrency(request);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Currency conversion not supported for TRY, PLN, THB, and MXN.", badRequestResult.Value);
        }

        /// <summary>
        /// Gets the historical rates_ returns paginated results.
        /// </summary>
        /// <returns>A Task.</returns>
        [Fact]
        public async Task GetHistoricalRates_ReturnsPaginatedResults()
        {
            var rates = new ExchangeRate
            {
                Base = "EUR",
                Date = DateTime.Now,
                Rates = new Dictionary<string, decimal>
            {
                { "USD", 1.2M }, { "GBP", 0.85M }, { "JPY", 130M }
            }
            };

            _mockService.Setup(s => s.GetHistoricalRatesAsync(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>())).ReturnsAsync(rates);

            var request = new HistoricalRatesRequest
            {
                BaseCurrency = "EUR",
                StartDate = new DateTime(2020, 1, 1),
                EndDate = new DateTime(2020, 1, 31),
                Page = 1,
                PageSize = 2
            };

            var result = await _controller.GetHistoricalRates(request);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<HistoricalRatesResponse>(okResult.Value);

            Assert.Equal("EUR", value.Base);
            Assert.Equal(2, value.Rates.Count);
        }
    }
}
