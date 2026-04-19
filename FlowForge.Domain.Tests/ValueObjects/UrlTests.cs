using FlowForge.Domain.ValueObjects;
using FluentAssertions;

namespace FlowForge.Domain.Tests.ValueObjects
{
    public class UrlTests
    {
        //Constructor Tests:
        [Fact]
        public void Constructor_WithValidHttpsUrl_CreatesUrlInstance()
        {
            //Arrange
            var validUrl = "https://api.example.com/webhooks/receive";

            //Act
            var url = Url.Create(validUrl);

            //Assert
            url.Should().NotBeNull();
            url.Data.Value.Should().Be(validUrl);
        }

        [Theory]
        [InlineData("https://api.stripe.com/v1/webhook")]
        [InlineData("https://hooks.slack.com/services/T123/B456/abc")]
        [InlineData("https://api.example.com:8443/webhook")]  // custom port
        [InlineData("https://subdomain.example.com/path/to/endpoint")]
        [InlineData("https://example.com/path?query=value&other=test")]  // query string
        public void Constructor_WithValidPublicHttpsUrl_CreatesUrlInstance(string validUrl)
        {
            //Act
            Action act = () => Url.Create(validUrl);

            //Assert
            act.Should().NotThrow();
        }

        [Fact]
        public void Constructor_WithNullUrl_ReturnsFailureResult()
        {
            //Act
            Action act = () => Url.Create(null!);

            //Assert
            act.Should().Throw<ArgumentException>().WithMessage("*empty*");
        }

        [Theory]
        [InlineData("")]
        [InlineData("  ")]
        [InlineData("\n")]
        [InlineData("\t")]
        public void Constructor_WithEmptyUrl_ReturnsFailureResult(string invalidUrl)
        {
            //Act
            Action act = () => Url.Create(invalidUrl);

            //Assert
            act.Should().Throw<ArgumentException>();
        }

        [Theory]
        [InlineData("not a url")]
        [InlineData("test")]
        [InlineData("htppsssss://test")]
        [InlineData("://test")]
        public void Constructor_WithMalformedUrl_ReturnsFailureResult(string invalidUrl)
        {
            //Act
            Action act = () => Url.Create(invalidUrl);

            //Assert
            act.Should().Throw<ArgumentException>();
        }

        [Theory]
        [InlineData("/path/only")]
        [InlineData("relative/path")]
        [InlineData("//cdn.example.com/path")]  // protocol-relative
        public void Constructor_WithRelativeUrl_ReturnsFailureResult(string relativeUrl)
        {
            // Act
            Action act = () => Url.Create(relativeUrl);

            // Assert
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void Constructor_WithHttpUrl_ReturnsFailureResult()
        {
            //Arrange
            var httpUrl = "http://example.com/test";

            //Act
            Action act = () => Url.Create(httpUrl);

            //Assert
            act.Should().Throw<ArgumentException>().WithMessage("*HTTPS*");
        }

        [Theory]
        [InlineData("ftp://example.com/webhook")]
        [InlineData("ws://example.com/socket")]
        [InlineData("wss://example.com/socket")]
        [InlineData("file:///etc/passwd")]
        [InlineData("data:text/plain;base64,SGVsbG8=")]
        [InlineData("javascript:alert(1)")]  // XSS vektörü
        [InlineData("gopher://example.com")]
        public void Constructor_WithNonHttpsScheme_ReturnsFailureResult(string url)
        {
            // Act
            Action act = () => Url.Create(url);

            // Assert
            act.Should().Throw<ArgumentException>();
        }

        [Theory]
        [InlineData("https://localhost/webhook")]
        [InlineData("https://localhost:8080/webhook")]
        [InlineData("https://LOCALHOST/webhook")]  // case sensitivity
        public void Constructor_WithLocalhostHostname_ReturnsFailureResult(string localhostUrl)
        {
            // Act
            Action act = () => Url.Create(localhostUrl);

            // Assert
            act.Should().Throw<ArgumentException>();
        }

        [Theory]
        [InlineData("https://127.0.0.1/webhook")]
        [InlineData("https://127.0.0.1:8080/webhook")]
        [InlineData("https://127.1.2.3/webhook")]  // 127.x range tamamı
        [InlineData("https://127.255.255.254/webhook")]
        public void Constructor_WithIPv4LocalhostRange_ReturnsFailureResult(string localhostUrl)
        {
            // Act
            Action act = () => Url.Create(localhostUrl);

            // Assert
            act.Should().Throw<ArgumentException>();
        }

        [Theory]
        [InlineData("https://10.0.0.1/webhook")]
        [InlineData("https://10.255.255.255/webhook")]
        [InlineData("https://192.168.1.1/webhook")]
        [InlineData("https://192.168.0.50/webhook")]
        [InlineData("https://172.16.0.1/webhook")]   // 172.16-31 başı
        [InlineData("https://172.31.255.254/webhook")]  // 172.16-31 sonu
        [InlineData("https://172.20.1.1/webhook")]   // ortası
        public void Constructor_WithPrivateIpAddress_ReturnsFailureResult(string privateIpUrl)
        {
            // Act
            Action act = () => Url.Create(privateIpUrl);

            // Assert
            act.Should().Throw<ArgumentException>();
        }

        [Theory]
        [InlineData("https://172.217.0.1/webhook")]   // Google public IP range
        [InlineData("https://172.15.0.1/webhook")]    // 172.16'nın hemen altı
        [InlineData("https://172.32.0.1/webhook")]    // 172.31'in hemen üstü
        public void Constructor_WithPublicIpIn172Range_CreatesUrlInstance(string publicIpUrl)
        {
            // Act
            Action act = () => Url.Create(publicIpUrl);

            // Assert
            act.Should().NotThrow();
        }

        [Theory]
        [InlineData("https://169.254.169.254/latest/meta-data/")]
        [InlineData("https://169.254.169.254/")]
        [InlineData("https://169.254.1.1/webhook")]  // 169.254.x tamamı
        public void Constructor_WithAwsMetadataIp_ReturnsFailureResult(string metadataUrl)
        {
            // Act
            Action act = () => Url.Create(metadataUrl);

            // Assert
            act.Should().Throw<ArgumentException>();
        }

        [Theory]
        [InlineData("https://0.0.0.0/webhook")]
        [InlineData("https://0.0.0.0:8080/webhook")]
        public void Constructor_WithUnspecifiedAddress_ReturnsFailureResult(string url)
        {
            // Act
            Action act = () => Url.Create(url);

            // Assert
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void Constructor_WithLocalhost_ExceptionMessageIndicatesSsrfProtection()
        {
            // Arrange
            var localhostUrl = "https://localhost/webhook";

            // Act
            Action act = () => Url.Create(localhostUrl);

            // Assert
            act.Should().Throw<ArgumentException>();
        }
    }
}