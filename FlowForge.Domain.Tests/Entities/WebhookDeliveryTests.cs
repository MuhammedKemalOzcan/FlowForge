using FlowForge.Domain.Entities;
using FlowForge.Domain.Enums;
using FlowForge.Domain.ValueObjects;
using FluentAssertions;
using FluentAssertions.Execution;

namespace FlowForge.Domain.Tests.Entities
{
    public class WebhookDeliveryTests
    {
        [Fact]
        public void Create_WithValidParameters_ReturnsDeliveryInPendingStateWithEmptyAttempts()
        {
            //Arrange
            var tenantId = Guid.NewGuid();
            var endpointId = Guid.NewGuid();
            var eventType = EventType.Create("payment.succeeded");
            var payload = "{\"amount\": 1000}";
            var idempotencyKey = IdempotencyKey.Create("evt_abc123");
            var retryPolicy = RetryPolicy.Default();

            //Act
            var delivery = WebhookDelivery.Create(tenantId, endpointId, eventType, payload, idempotencyKey, retryPolicy);

            //assert
            //Scope içindeki tüm assertion'lar çalışır, hepsi kontrol edilir, sonunda kırık olanların hepsi birden raporlanır. Bu özellikle birden fazla property'yi aynı anda assert ettiğinde hayat kurtarıcı.
            using var scope = new AssertionScope();
            delivery.Should().NotBeNull();
            delivery.Id.Should().NotBe(Guid.Empty);
            delivery.TenantId.Should().Be(tenantId);
            delivery.EndpointId.Should().Be(endpointId);
            //çalışıyor çünkü EventType bir record ve record'lar value equality yapıyor.Eğer entity olsalardı (kimlik bazlı eşitlik) bu çalışmazdı.
            delivery.EventType.Should().Be(eventType);
            delivery.Payload.Should().Be(payload);
            delivery.IdempotencyKey.Should().Be(idempotencyKey);
            delivery.Status.Should().Be(DeliveryStatus.Pending);
            delivery.Attempts.Should().BeEmpty();
            delivery.NextRetryAt.Should().BeNull();
            delivery.FinalResultAt.Should().BeNull();
            //"şu anki zaman ± 1 saniye içinde olmalı". Time-based assertion'larda tolerans kritik.
            delivery.ReceivedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void Create_WithEmptyTenantId_ReturnsFailureResult()
        {
            //Arrange
            var tenantId = Guid.Empty;
            var endpointId = Guid.NewGuid();
            var eventType = EventType.Create("payment.succeeded");
            var payload = "{\"amount\": 1000}";
            var idempotencyKey = IdempotencyKey.Create("evt_abc123");
            var retryPolicy = RetryPolicy.Default();

            //Act
            //kodu hemen çalıştırmıyorsun. Sadece "bu kodu nasıl çalıştırırım" tarifini bir lambda'ya kaydediyorsun. Sonra act.Should().Throw<...>() ifadesi lambda'yı içeride güvenli biçimde çalıştırıyor
            Action act = () => WebhookDelivery.Create(tenantId, endpointId, eventType, payload, idempotencyKey, retryPolicy);

            //Assert
            //Test edeceğin kod exception fırlatıyorsa, onu doğrudan çağıramazsın — çünkü exception test metodunu kırar. Bunun yerine bir delegate (lambda) olarak sarmalıyorsun
            act.Should().Throw<ArgumentException>().WithMessage("*Tenant*");
        }

        [Fact]
        public void Create_WithEmptyEndPoint_ReturnsFailureResult()
        {
            //Arrange
            var tenantId = Guid.NewGuid();
            var endpointId = Guid.Empty;
            var eventType = EventType.Create("payment.succeeded");
            var payload = "{\"amount\": 1000}";
            var idempotencyKey = IdempotencyKey.Create("evt_abc123");
            var retryPolicy = RetryPolicy.Default();

            //Act
            Action act = () => WebhookDelivery.Create(tenantId, endpointId, eventType, payload, idempotencyKey, retryPolicy);

            //Assert
            act.Should().Throw<ArgumentException>().WithMessage("*Endpoint*");
        }

        [Fact]
        public void Create_WithEmptyEventType_ReturnsFailureResult()
        {
            //Arrange
            var tenantId = Guid.NewGuid();
            var endpointId = Guid.NewGuid();
            EventType eventType = null!;
            var payload = "{\"amount\": 1000}";
            var idempotencyKey = IdempotencyKey.Create("evt_abc123");
            var retryPolicy = RetryPolicy.Default();

            //Act
            Action act = () => WebhookDelivery.Create(tenantId, endpointId, eventType!, payload, idempotencyKey, retryPolicy);

            //Assert
            act.Should().Throw<ArgumentException>().WithMessage("*EventType*");
        }

        [Fact]
        public void Create_WithNullOrEmptyPayload_ReturnsFailureResult()
        {
            //Arrange
            var tenantId = Guid.NewGuid();
            var endpointId = Guid.NewGuid();
            var eventType = EventType.Create("payment.succeeded");
            var payload = "";
            var idempotencyKey = IdempotencyKey.Create("evt_abc123");
            var retryPolicy = RetryPolicy.Default();

            //Act
            Action act = () => WebhookDelivery.Create(tenantId, endpointId, eventType, payload, idempotencyKey, retryPolicy);

            //Assert
            act.Should().Throw<ArgumentException>().WithMessage("*payload*");
        }

        [Fact]
        public void Create_WithNullIdempotencyKey_ReturnsFailureResult()
        {
            //Arrange
            var tenantId = Guid.NewGuid();
            var endpointId = Guid.NewGuid();
            var eventType = EventType.Create("payment.succeeded");
            var payload = "{\"amount\": 1000}";
            IdempotencyKey idempotencyKey = null!;
            var retryPolicy = RetryPolicy.Default();

            //Act
            Action act = () => WebhookDelivery.Create(tenantId, endpointId, eventType, payload, idempotencyKey!, retryPolicy);

            //Assert
            act.Should().Throw<ArgumentException>().WithMessage("*IdempotencyKey*");
        }

        [Fact]
        public void Create_WithNullRetryPolicy_ReturnsFailureResult()
        {
            //Arrange
            var tenantId = Guid.NewGuid();
            var endpointId = Guid.NewGuid();
            var eventType = EventType.Create("payment.succeeded");
            var payload = "{\"amount\": 1000}";
            var idempotencyKey = IdempotencyKey.Create("evt_abc123");
            RetryPolicy retryPolicy = null!;

            //Act
            Action act = () => WebhookDelivery.Create(tenantId, endpointId, eventType, payload, idempotencyKey!, retryPolicy!);

            //Assert
            act.Should().Throw<ArgumentException>().WithMessage("*retryPolicy*");
        }

        //MarkInProgress() Testleri:

        [Fact]
        public void MarkInProgress_WhenStatusIsPending_ChangesStatusToInProgress()
        {
            //Arrange
            var delivery = CreateValidDelivery();

            //Act
            delivery.MarkInProgress();

            //Assert
            delivery.Status.Should().Be(DeliveryStatus.InProgress);
        }

        [Fact]
        public void MarkInProgress_WhenStatusIsDeadLettered_ThrowsInvalidOperationException()
        {
            //Arrange
            var delivery = CreateValidDelivery();
            for (int i = 0; i < 5; i++)
            {
                delivery.MarkInProgress();
                delivery.RecordFailedAttempt(150, System.Net.HttpStatusCode.InternalServerError, "Server Error", DateTime.UtcNow.AddMilliseconds(-150), DateTime.UtcNow);
            }

            //Act
            Action act = () => delivery.MarkInProgress();

            //Assert
            act.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void MarkInProgress_WhenStatusIsInProgress_ThrowsInvalidOperationException()

        {
            //Arrange
            var delivery = CreateValidDelivery();
            delivery.MarkInProgress();

            //Act
            Action act = () => delivery.MarkInProgress();

            //Assert
            act.Should().Throw<InvalidOperationException>().WithMessage("*InProgress*");
        }

        [Fact]
        public void MarkInProgress_WhenStatusIsSucceeded_ThrowsInvalidOperationException()
        {
            //Arrange
            var delivery = CreateValidDelivery();
            delivery.MarkInProgress();
            delivery.RecordSuccessfulAttempt(150, System.Net.HttpStatusCode.OK, "OK", DateTime.UtcNow.AddMilliseconds(-150), DateTime.UtcNow);

            //Act
            Action act = () => delivery.MarkInProgress();

            //Assert
            act.Should().Throw<InvalidOperationException>();
        }

        //Record Succesful Attempt Tests:

        [Fact]
        public void RecordSuccessfulAttempt_WhenStatusIsSucceeded_ThrowsInvalidOperationException()
        {
            //Arrange
            var delivery = CreateValidDelivery();
            delivery.MarkInProgress();
            delivery.RecordSuccessfulAttempt(150, System.Net.HttpStatusCode.OK, "OK", DateTime.UtcNow.AddMilliseconds(-150), DateTime.UtcNow);

            //Act
            Action act = () => delivery.RecordSuccessfulAttempt(150, System.Net.HttpStatusCode.OK, "OK", DateTime.UtcNow.AddMilliseconds(-150), DateTime.UtcNow);

            //Assert
            act.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void RecordSuccessfulAttempt_WhenStatusIsDeadLettered_ThrowsInvalidOperationException()
        {
            //Arrange
            var delivery = CreateValidDelivery();
            for (int i = 0; i < 5; i++)
            {
                delivery.MarkInProgress();
                delivery.RecordFailedAttempt(150, System.Net.HttpStatusCode.BadRequest, "Test", DateTime.UtcNow.AddMilliseconds(-150), DateTime.UtcNow);
            }

            //Act
            Action act = () => delivery.RecordSuccessfulAttempt(150, System.Net.HttpStatusCode.OK, "OK", DateTime.UtcNow.AddMilliseconds(-150), DateTime.UtcNow);

            //Assert
            act.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void RecordSuccessfulAttempt_WhenInProgress_AddsAttemptToList()
        {
            //Arrange
            var delivery = CreateValidDelivery();
            delivery.MarkInProgress();

            //Act
            delivery.RecordSuccessfulAttempt(150, System.Net.HttpStatusCode.OK, "OK", DateTime.UtcNow.AddMilliseconds(-150), DateTime.UtcNow);

            //Assert
            delivery.Attempts.Should().HaveCount(1);
            delivery.Attempts.First().AttemptNumber.Should().Be(1);
            delivery.Attempts.First().Outcome.Should().Be(OutcomeStatus.Succeeded);
        }

        [Fact]
        public void RecordSuccessfulAttempt_WhenInProgress_SetsStatusToSucceeded()
        {
            //Arrange
            var delivery = CreateValidDelivery();
            delivery.MarkInProgress();

            //Act
            delivery.RecordSuccessfulAttempt(150, System.Net.HttpStatusCode.OK, "OK", DateTime.UtcNow.AddMilliseconds(-150), DateTime.UtcNow);

            //Assert
            delivery.Status.Should().Be(DeliveryStatus.Succeeded);
            delivery.FinalResultAt.Should().NotBeNull();
            delivery.NextRetryAt.Should().BeNull();
        }

        [Fact]
        public void RecordSuccessfulAttempt_WhenNotInProgress_ThrowsInvalidOperation()
        {
            //Arrange
            var delivery = CreateValidDelivery();

            //Act
            Action act = () => delivery.RecordSuccessfulAttempt(150, System.Net.HttpStatusCode.OK, "OK", DateTime.UtcNow.AddMilliseconds(-150), DateTime.UtcNow);

            //Assert
            act.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void RecordSuccessfulAttempt_TruncatesLongResponseBodySnippet()
        {
            //Arrange
            var delivery = CreateValidDelivery();
            var longResponse = new string('A', 1000);
            delivery.MarkInProgress();

            //Act
            delivery.RecordSuccessfulAttempt(150, System.Net.HttpStatusCode.OK, longResponse, DateTime.UtcNow.AddMilliseconds(-150), DateTime.UtcNow);

            //Assert
            delivery.Attempts.First().ResponseBodySnippet.Should().NotBeNull();
            delivery.Attempts.First().ResponseBodySnippet!.Length.Should().Be(500);
        }

        //Record Failed Attempt

        [Fact]
        public void RecordFailedAttempt_WhenNotInProgress_ThrowsInvalidOperationException()
        {
            //Arrange
            var delivery = CreateValidDelivery();

            //Act
            Action act = () => delivery.RecordFailedAttempt(150, System.Net.HttpStatusCode.BadRequest, "Test", DateTime.UtcNow.AddMilliseconds(-150), DateTime.UtcNow);

            //Assert
            act.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void RecordFailedAttempt_WhenLastAttempt_SetsStatusToDeadLettered()
        {
            //Arrange
            var delivery = CreateValidDelivery();

            for (int i = 0; i < 4; i++)
            {
                delivery.MarkInProgress();
                delivery.RecordFailedAttempt(150, System.Net.HttpStatusCode.InternalServerError, "Server Error", DateTime.UtcNow.AddMilliseconds(-150), DateTime.UtcNow);
            }

            //Son aşama
            delivery.MarkInProgress();

            //Act
            delivery.RecordFailedAttempt(150, System.Net.HttpStatusCode.InternalServerError, "Server Error", DateTime.UtcNow.AddMilliseconds(-150), DateTime.UtcNow);

            //Assert
            delivery.Status.Should().Be(DeliveryStatus.DeadLettered);
            delivery.Attempts.Should().HaveCount(5);
            delivery.Attempts.Last().Outcome.Should().Be(OutcomeStatus.FailedFinal);
            delivery.FinalResultAt.Should().NotBeNull();
            delivery.NextRetryAt.Should().BeNull();
        }

        [Fact]
        public void RecordFailedAttempt_WhenNotLastAttempt_SetsStatusToPending()
        {
            //Arrange
            var delivery = CreateValidDelivery();
            delivery.MarkInProgress();

            //Act
            delivery.RecordFailedAttempt(150, System.Net.HttpStatusCode.InternalServerError, "Server Error", DateTime.UtcNow.AddMilliseconds(-150), DateTime.UtcNow);

            //Assert
            delivery.Status.Should().Be(DeliveryStatus.Pending);
            delivery.Attempts.Should().HaveCount(1);
            delivery.Attempts.Last().Outcome.Should().Be(OutcomeStatus.FailedWillRetry);
            delivery.FinalResultAt.Should().BeNull();
            delivery.NextRetryAt.Should().NotBeNull();
        }

        [Fact]
        public void RecordFailedAttempt_WhenNotLastAttempt_CalculatesNextRetryAtCorrectly()
        {
            //Arrange
            var delivery = CreateValidDelivery();
            delivery.MarkInProgress();
            var expectedDelay = delivery.RetryPolicy.CalculateDelayFor(1);
            var beforeAttempt = DateTime.UtcNow;

            //Act
            delivery.RecordFailedAttempt(150, System.Net.HttpStatusCode.InternalServerError, "Server Error", DateTime.UtcNow.AddMilliseconds(-150), DateTime.UtcNow);

            //Assert
            delivery.NextRetryAt.Should().NotBeNull();
            var expectedTime = beforeAttempt.Add(expectedDelay);

            delivery.NextRetryAt.Should().BeCloseTo(expectedTime, TimeSpan.FromMilliseconds(500));
        }

        [Fact]
        public void RecordFailedAttempt_MultipleAttempts_IncrementsAttemptNumber()
        {
            //Arrange
            var delivery = CreateValidDelivery();

            //Act
            for (int i = 0; i < 3; i++)
            {
                delivery.MarkInProgress();
                delivery.RecordFailedAttempt(150, System.Net.HttpStatusCode.InternalServerError, "Server Error", DateTime.UtcNow.AddMilliseconds(-150), DateTime.UtcNow);
            }

            //Assert
            delivery.Attempts.Count.Should().Be(3);
        }

        [Fact]
        public void RecordFailedAttempt_WithConnectionError_HandlesNullStatusCode()
        {
            //Arrange
            var delivery = CreateValidDelivery();
            delivery.MarkInProgress();

            //Act
            delivery.RecordFailedAttempt(150, null, "Server Error", DateTime.UtcNow.AddMilliseconds(-150), DateTime.UtcNow);
            //Assert
            delivery.Attempts.Last().StatusCode.Should().BeNull();
        }

        [Fact]
        public void RecordFailedAttempt_PersistsAttemptDataCorrectly()
        {
            //Arrange
            var delivery = CreateValidDelivery();
            delivery.MarkInProgress();

            //Act
            delivery.RecordFailedAttempt(150, System.Net.HttpStatusCode.InternalServerError, "Server Error", DateTime.UtcNow.AddMilliseconds(-150), DateTime.UtcNow);
            //Assert
            delivery.Attempts.First().DurationMs.Should().Be(150);
            delivery.Attempts.First().StatusCode.Should().Be(System.Net.HttpStatusCode.InternalServerError);
            delivery.Attempts.First().ErrorMessage.Should().Be("Server Error");
            delivery.Attempts.First().StartedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMilliseconds(200));
            delivery.Attempts.First().CompletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMilliseconds(100));
        }

        [Fact]
        public void Attempts_ShouldBeReadOnlyCollection()
        {
            // Arrange
            var delivery = CreateValidDelivery();

            // Act
            var attempts = delivery.Attempts;

            // Assert
            // Dışarıya açılan koleksiyonun gerçek tipinin ReadOnlyCollection olduğunu doğrular
            attempts.Should().BeAssignableTo<IReadOnlyCollection<DeliveryAttempt>>();

            // Yansıma (Reflection) ile ismini kontrol etmek istersen:
            attempts.GetType().Name.Should().Contain("ReadOnlyCollection");
        }

        private static WebhookDelivery CreateValidDelivery()
        {
            return WebhookDelivery.Create(
                tenantId: Guid.NewGuid(),
                endpointId: Guid.NewGuid(),
                eventType: EventType.Create("payment.succeeded"),
                payload: "{\"amount\": 1000}",
                idempotencyKey: IdempotencyKey.Create("evt_abc123"),
                retryPolicy: RetryPolicy.Default()
            );
        }
    }
}