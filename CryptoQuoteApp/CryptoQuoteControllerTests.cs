using Microsoft.AspNetCore.Mvc;
using Moq;
using OkalaTask.Controllers;
using OkalaTask.Interface;
using OkalaTask.Models;

public class CryptoQuoteControllerTests
{
    private readonly Mock<ICryptoQuoteService> _serviceMock;
    private readonly CryptoQuoteController _controller;

    public CryptoQuoteControllerTests()
    {
        _serviceMock = new Mock<ICryptoQuoteService>();

        _controller = new CryptoQuoteController(_serviceMock.Object);
    }

    [Fact]
    public async Task GetQuote_ReturnsOk_WhenValidCryptoCodeIsProvided()
    {
        // Arrange
        var request = new CryptoQuoteRequest { CryptoCode = "BTC" };
        var response = new Dictionary<string, decimal>
        {
            { "USD", 50000.0m },
            { "EUR", 45000.0m }
        };

        _serviceMock
            .Setup(service => service.GetCryptoQuoteAsync(It.IsAny<string>()))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.GetQuote(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedData = Assert.IsType<CryptoQuoteResponse>(okResult.Value);
        Assert.Equal("BTC", returnedData.CryptoCode);
        Assert.Equal(response, returnedData.Rates);
    }

    [Fact]
    public async Task GetQuote_ReturnsBadRequest_WhenCryptoCodeIsMissing()
    {
        // Arrange
        var request = new CryptoQuoteRequest { CryptoCode = "" };

        // Act
        var result = await _controller.GetQuote(request);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task GetQuote_ReturnsNotFound_WhenServiceReturnsNull()
    {
        // Arrange
        var request = new CryptoQuoteRequest { CryptoCode = "BTC" };

        _serviceMock
            .Setup(service => service.GetCryptoQuoteAsync(It.IsAny<string>()))
            .ReturnsAsync((Dictionary<string, decimal>)null);

        // Act
        var result = await _controller.GetQuote(request);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }
}
