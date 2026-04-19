using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FlowForge.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class mig_1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Tenants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Plan = table.Column<string>(type: "text", nullable: false),
                    TenantStatus = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PlanLimits_MaxEndpointsAllowed = table.Column<int>(type: "integer", nullable: false),
                    PlanLimits_MaxEventsPerMinute = table.Column<int>(type: "integer", nullable: false),
                    PlanLimits_MaxMembersAllowed = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenants", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExternalIdentityId_ExternalId = table.Column<string>(type: "text", nullable: false),
                    Email_Value = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    FullName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ApiKeys",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Key_Value = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Prefix = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastUsedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApiKeys", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApiKeys_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Membership",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Role = table.Column<string>(type: "text", nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Membership", x => new { x.TenantId, x.UserId });
                    table.ForeignKey(
                        name: "FK_Membership_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WebhookEndpoints",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name_Value = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TargetUrl_Value = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    SigningSecret_Value = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    RetryPolicy_MaxAttempts = table.Column<int>(type: "integer", nullable: false),
                    RetryPolicy_Strategy = table.Column<string>(type: "text", nullable: false),
                    RetryPolicy_InitialDelay = table.Column<TimeSpan>(type: "interval", nullable: false),
                    RetryPolicy_MaxDelay = table.Column<TimeSpan>(type: "interval", nullable: false),
                    RetryPolicy_TimeOut = table.Column<TimeSpan>(type: "interval", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebhookEndpoints", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WebhookEndpoints_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WebhookDeliveries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    EndpointId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventType_Value = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Payload = table.Column<string>(type: "character varying(50000)", maxLength: 50000, nullable: false),
                    IdempotencyKey_Value = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    RetryPolicy_MaxAttempts = table.Column<int>(type: "integer", nullable: false),
                    RetryPolicy_Strategy = table.Column<string>(type: "text", nullable: false),
                    RetryPolicy_InitialDelay = table.Column<TimeSpan>(type: "interval", nullable: false),
                    RetryPolicy_MaxDelay = table.Column<TimeSpan>(type: "interval", nullable: false),
                    RetryPolicy_TimeOut = table.Column<TimeSpan>(type: "interval", nullable: false),
                    ReceivedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    NextRetryAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FinalResultAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebhookDeliveries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WebhookDeliveries_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WebhookDeliveries_WebhookEndpoints_EndpointId",
                        column: x => x.EndpointId,
                        principalTable: "WebhookEndpoints",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WebhookEndpoints_SubscribedEventTypes",
                columns: table => new
                {
                    WebhookEndpointId = table.Column<Guid>(type: "uuid", nullable: false),
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Value = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebhookEndpoints_SubscribedEventTypes", x => new { x.WebhookEndpointId, x.Id });
                    table.ForeignKey(
                        name: "FK_WebhookEndpoints_SubscribedEventTypes_WebhookEndpoints_Webh~",
                        column: x => x.WebhookEndpointId,
                        principalTable: "WebhookEndpoints",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DeliveryAttempt",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AttemptNumber = table.Column<int>(type: "integer", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DurationMs = table.Column<long>(type: "bigint", nullable: false),
                    StatusCode = table.Column<string>(type: "text", nullable: true),
                    ResponseBodySnippet = table.Column<string>(type: "text", nullable: true),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true),
                    Outcome = table.Column<string>(type: "text", nullable: false),
                    WebhookDeliveryId = table.Column<Guid>(type: "uuid", nullable: true),
                    WebhookDeliveryId1 = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeliveryAttempt", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeliveryAttempt_WebhookDeliveries_WebhookDeliveryId",
                        column: x => x.WebhookDeliveryId,
                        principalTable: "WebhookDeliveries",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DeliveryAttempt_WebhookDeliveries_WebhookDeliveryId1",
                        column: x => x.WebhookDeliveryId1,
                        principalTable: "WebhookDeliveries",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApiKeys_Key_Value",
                table: "ApiKeys",
                column: "Key_Value",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ApiKeys_TenantId",
                table: "ApiKeys",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryAttempt_WebhookDeliveryId",
                table: "DeliveryAttempt",
                column: "WebhookDeliveryId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryAttempt_WebhookDeliveryId1",
                table: "DeliveryAttempt",
                column: "WebhookDeliveryId1");

            migrationBuilder.CreateIndex(
                name: "IX_Users_ExternalIdentityId_ExternalId",
                table: "Users",
                column: "ExternalIdentityId_ExternalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WebhookDeliveries_EndpointId",
                table: "WebhookDeliveries",
                column: "EndpointId");

            migrationBuilder.CreateIndex(
                name: "IX_WebhookDeliveries_IdempotencyKey_Value",
                table: "WebhookDeliveries",
                column: "IdempotencyKey_Value",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WebhookDeliveries_TenantId",
                table: "WebhookDeliveries",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_WebhookEndpoints_TenantId",
                table: "WebhookEndpoints",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApiKeys");

            migrationBuilder.DropTable(
                name: "DeliveryAttempt");

            migrationBuilder.DropTable(
                name: "Membership");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "WebhookEndpoints_SubscribedEventTypes");

            migrationBuilder.DropTable(
                name: "WebhookDeliveries");

            migrationBuilder.DropTable(
                name: "WebhookEndpoints");

            migrationBuilder.DropTable(
                name: "Tenants");
        }
    }
}
