using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventParking.API.Migrations
{
    /// <inheritdoc />
    public partial class BookingV2MultiSeatAndPaymentHardening : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Remove legacy foreign keys first, but preserve the columns
            // until their data has been copied into the new BRD v2 tables.
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_ParkingSlots_ParkingSlotId",
                table: "Bookings");

            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Seats_SeatId",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_Payments_BookingId",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_ParkingSlotId",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_SeatId",
                table: "Bookings");

            // Create BRD v2 multi-seat table before removing legacy SeatId.
            migrationBuilder.CreateTable(
                name: "BookingSeats",
                columns: table => new
                {
                    BookingId = table.Column<int>(
                        type: "int",
                        nullable: false),

                    SeatId = table.Column<int>(
                        type: "int",
                        nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "PK_BookingSeats",
                        x => new
                        {
                            x.BookingId,
                            x.SeatId
                        });

                    table.ForeignKey(
                        name: "FK_BookingSeats_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);

                    table.ForeignKey(
                        name: "FK_BookingSeats_Seats_SeatId",
                        column: x => x.SeatId,
                        principalTable: "Seats",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // Create BRD v2 parking reservation table before removing
            // legacy ParkingSlotId.
            migrationBuilder.CreateTable(
                name: "ParkingReservations",
                columns: table => new
                {
                    Id = table.Column<int>(
                            type: "int",
                            nullable: false)
                        .Annotation(
                            "SqlServer:Identity",
                            "1, 1"),

                    BookingId = table.Column<int>(
                        type: "int",
                        nullable: false),

                    ParkingSlotId = table.Column<int>(
                        type: "int",
                        nullable: false),

                    CreatedAt = table.Column<DateTime>(
                        type: "datetime2",
                        nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "PK_ParkingReservations",
                        x => x.Id);

                    table.ForeignKey(
                        name: "FK_ParkingReservations_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);

                    table.ForeignKey(
                        name: "FK_ParkingReservations_ParkingSlots_ParkingSlotId",
                        column: x => x.ParkingSlotId,
                        principalTable: "ParkingSlots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // Preserve every existing single-seat booking as the first
            // BookingSeats relationship in the new BRD v2 model.
            migrationBuilder.Sql(
                """
                INSERT INTO BookingSeats (BookingId, SeatId)
                SELECT Id, SeatId
                FROM Bookings
                WHERE SeatId IS NOT NULL;
                """);

            // Preserve every existing optional parking reservation.
            migrationBuilder.Sql(
                """
                INSERT INTO ParkingReservations
                    (BookingId, ParkingSlotId, CreatedAt)
                SELECT
                    Id,
                    ParkingSlotId,
                    CreatedAt
                FROM Bookings
                WHERE ParkingSlotId IS NOT NULL;
                """);

            // Legacy resource columns are now safe to remove.
            migrationBuilder.DropColumn(
                name: "ParkingSlotId",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "SeatId",
                table: "Bookings");

            // Database-level duplicate payment protection.
            migrationBuilder.CreateIndex(
                name: "IX_Payments_BookingId",
                table: "Payments",
                column: "BookingId",
                unique: true);

            // Booking numbers are externally visible identifiers and
            // must be unique.
            migrationBuilder.CreateIndex(
                name: "IX_Bookings_BookingNumber",
                table: "Bookings",
                column: "BookingNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BookingSeats_SeatId",
                table: "BookingSeats",
                column: "SeatId");

            migrationBuilder.CreateIndex(
                name: "IX_ParkingReservations_BookingId",
                table: "ParkingReservations",
                column: "BookingId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ParkingReservations_ParkingSlotId",
                table: "ParkingReservations",
                column: "ParkingSlotId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Restore legacy columns before removing the BRD v2 tables
            // so rollback can preserve compatible resource relationships.
            migrationBuilder.AddColumn<int>(
                name: "ParkingSlotId",
                table: "Bookings",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SeatId",
                table: "Bookings",
                type: "int",
                nullable: true);

            // The legacy schema supports only one seat per booking.
            // On rollback, retain the lowest SeatId as the compatible
            // legacy relationship.
            migrationBuilder.Sql(
                """
                UPDATE b
                SET b.SeatId = source.SeatId
                FROM Bookings b
                INNER JOIN
                (
                    SELECT BookingId, MIN(SeatId) AS SeatId
                    FROM BookingSeats
                    GROUP BY BookingId
                ) source
                    ON source.BookingId = b.Id;
                """);

            migrationBuilder.Sql(
                """
                UPDATE b
                SET b.ParkingSlotId = pr.ParkingSlotId
                FROM Bookings b
                INNER JOIN ParkingReservations pr
                    ON pr.BookingId = b.Id;
                """);

            migrationBuilder.DropTable(
                name: "BookingSeats");

            migrationBuilder.DropTable(
                name: "ParkingReservations");

            migrationBuilder.DropIndex(
                name: "IX_Payments_BookingId",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_BookingNumber",
                table: "Bookings");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_BookingId",
                table: "Payments",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_ParkingSlotId",
                table: "Bookings",
                column: "ParkingSlotId",
                unique: true,
                filter: "[ParkingSlotId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_SeatId",
                table: "Bookings",
                column: "SeatId",
                unique: true,
                filter: "[SeatId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_ParkingSlots_ParkingSlotId",
                table: "Bookings",
                column: "ParkingSlotId",
                principalTable: "ParkingSlots",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Seats_SeatId",
                table: "Bookings",
                column: "SeatId",
                principalTable: "Seats",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
