using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace depiR5gp.Migrations
{
    /// <inheritdoc />
    public partial class InitialDatabaseSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppUsers",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    first_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    last_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    role = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    is_suspended = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    is_deleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    points_balance = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    phone = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppUsers", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Cuisines",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cuisines", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AppUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AppUsers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AppUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AppUsers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AppUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AppUsers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AssistantConversations",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssistantConversations", x => x.id);
                    table.ForeignKey(
                        name: "FK_AssistantConversations_AppUsers_user_id",
                        column: x => x.user_id,
                        principalTable: "AppUsers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AssistantUsages",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    usage_date = table.Column<DateOnly>(type: "date", nullable: false),
                    request_count = table.Column<int>(type: "int", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssistantUsages", x => x.id);
                    table.ForeignKey(
                        name: "FK_AssistantUsages_AppUsers_user_id",
                        column: x => x.user_id,
                        principalTable: "AppUsers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChefProfiles",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    kitchen_name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    bio = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    photo_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    address = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "Pending"),
                    approval_notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    rejection_reason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    has_accepted_terms = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    commission_rate = table.Column<decimal>(type: "decimal(5,4)", precision: 5, scale: 4, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChefProfiles", x => x.id);
                    table.ForeignKey(
                        name: "FK_ChefProfiles_AppUsers_user_id",
                        column: x => x.user_id,
                        principalTable: "AppUsers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Faqs",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    updated_by_user_id = table.Column<int>(type: "int", nullable: true),
                    question = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    answer = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Faqs", x => x.id);
                    table.ForeignKey(
                        name: "FK_Faqs_AppUsers_updated_by_user_id",
                        column: x => x.updated_by_user_id,
                        principalTable: "AppUsers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    message = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    link = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    is_read = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.id);
                    table.ForeignKey(
                        name: "FK_Notifications_AppUsers_user_id",
                        column: x => x.user_id,
                        principalTable: "AppUsers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlatformSettings",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    updated_by_user_id = table.Column<int>(type: "int", nullable: true),
                    commission_rate = table.Column<decimal>(type: "decimal(5,4)", precision: 5, scale: 4, nullable: false, defaultValue: 0.1500m),
                    refund_window_hours = table.Column<int>(type: "int", nullable: false, defaultValue: 24),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlatformSettings", x => x.id);
                    table.ForeignKey(
                        name: "FK_PlatformSettings_AppUsers_updated_by_user_id",
                        column: x => x.updated_by_user_id,
                        principalTable: "AppUsers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    RoleId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AppUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AppUsers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AssistantMessages",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    assistant_conversation_id = table.Column<int>(type: "int", nullable: false),
                    role = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssistantMessages", x => x.id);
                    table.ForeignKey(
                        name: "FK_AssistantMessages_AssistantConversations_assistant_conversation_id",
                        column: x => x.assistant_conversation_id,
                        principalTable: "AssistantConversations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Carts",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: true),
                    chef_profile_id = table.Column<int>(type: "int", nullable: false),
                    guest_token = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Carts", x => x.id);
                    table.ForeignKey(
                        name: "FK_Carts_AppUsers_user_id",
                        column: x => x.user_id,
                        principalTable: "AppUsers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Carts_ChefProfiles_chef_profile_id",
                        column: x => x.chef_profile_id,
                        principalTable: "ChefProfiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ChefDocuments",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    chef_profile_id = table.Column<int>(type: "int", nullable: false),
                    doc_type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    file_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    uploaded_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChefDocuments", x => x.id);
                    table.ForeignKey(
                        name: "FK_ChefDocuments_ChefProfiles_chef_profile_id",
                        column: x => x.chef_profile_id,
                        principalTable: "ChefProfiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChefWorkingHours",
                columns: table => new
                {
                    chef_profile_id = table.Column<int>(type: "int", nullable: false),
                    day_of_week = table.Column<byte>(type: "tinyint", nullable: false),
                    open_time = table.Column<TimeOnly>(type: "time", nullable: false),
                    close_time = table.Column<TimeOnly>(type: "time", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChefWorkingHours", x => new { x.chef_profile_id, x.day_of_week });
                    table.ForeignKey(
                        name: "FK_ChefWorkingHours_ChefProfiles_chef_profile_id",
                        column: x => x.chef_profile_id,
                        principalTable: "ChefProfiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Conversations",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    customer_user_id = table.Column<int>(type: "int", nullable: false),
                    chef_profile_id = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Conversations", x => x.id);
                    table.ForeignKey(
                        name: "FK_Conversations_AppUsers_customer_user_id",
                        column: x => x.customer_user_id,
                        principalTable: "AppUsers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Conversations_ChefProfiles_chef_profile_id",
                        column: x => x.chef_profile_id,
                        principalTable: "ChefProfiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Dishes",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    chef_profile_id = table.Column<int>(type: "int", nullable: false),
                    category_id = table.Column<int>(type: "int", nullable: false),
                    cuisine_id = table.Column<int>(type: "int", nullable: false),
                    name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    price = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    image_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    is_available = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    is_archived = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dishes", x => x.id);
                    table.ForeignKey(
                        name: "FK_Dishes_Categories_category_id",
                        column: x => x.category_id,
                        principalTable: "Categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Dishes_ChefProfiles_chef_profile_id",
                        column: x => x.chef_profile_id,
                        principalTable: "ChefProfiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Dishes_Cuisines_cuisine_id",
                        column: x => x.cuisine_id,
                        principalTable: "Cuisines",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Favorites",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    chef_profile_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Favorites", x => x.id);
                    table.ForeignKey(
                        name: "FK_Favorites_AppUsers_user_id",
                        column: x => x.user_id,
                        principalTable: "AppUsers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Favorites_ChefProfiles_chef_profile_id",
                        column: x => x.chef_profile_id,
                        principalTable: "ChefProfiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    chef_profile_id = table.Column<int>(type: "int", nullable: false),
                    status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "Pending"),
                    payment_method = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "Cash"),
                    payment_status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "Pending"),
                    reference = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    submission_token = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    subtotal = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    points_discount = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false, defaultValue: 0.00m),
                    total = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    commission_rate = table.Column<decimal>(type: "decimal(5,4)", precision: 5, scale: 4, nullable: false, defaultValue: 0.1500m),
                    commission_amount = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false, defaultValue: 0.00m),
                    delivery_address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    cancellation_reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.id);
                    table.ForeignKey(
                        name: "FK_Orders_AppUsers_user_id",
                        column: x => x.user_id,
                        principalTable: "AppUsers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Orders_ChefProfiles_chef_profile_id",
                        column: x => x.chef_profile_id,
                        principalTable: "ChefProfiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Payouts",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    chef_profile_id = table.Column<int>(type: "int", nullable: false),
                    amount = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "Pending"),
                    reference = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    idempotency_key = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    is_simulated = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payouts", x => x.id);
                    table.ForeignKey(
                        name: "FK_Payouts_ChefProfiles_chef_profile_id",
                        column: x => x.chef_profile_id,
                        principalTable: "ChefProfiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Messages",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    conversation_id = table.Column<int>(type: "int", nullable: false),
                    sender_user_id = table.Column<int>(type: "int", nullable: false),
                    text = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    sent_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    is_read = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Messages", x => x.id);
                    table.ForeignKey(
                        name: "FK_Messages_AppUsers_sender_user_id",
                        column: x => x.sender_user_id,
                        principalTable: "AppUsers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Messages_Conversations_conversation_id",
                        column: x => x.conversation_id,
                        principalTable: "Conversations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CartItems",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    cart_id = table.Column<int>(type: "int", nullable: false),
                    dish_id = table.Column<int>(type: "int", nullable: false),
                    quantity = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CartItems", x => x.id);
                    table.ForeignKey(
                        name: "FK_CartItems_Carts_cart_id",
                        column: x => x.cart_id,
                        principalTable: "Carts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CartItems_Dishes_dish_id",
                        column: x => x.dish_id,
                        principalTable: "Dishes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DishOptions",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    dish_id = table.Column<int>(type: "int", nullable: false),
                    group_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    extra_price = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false, defaultValue: 0.00m),
                    is_required = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DishOptions", x => x.id);
                    table.ForeignKey(
                        name: "FK_DishOptions_Dishes_dish_id",
                        column: x => x.dish_id,
                        principalTable: "Dishes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderItems",
                columns: table => new
                {
                    order_id = table.Column<int>(type: "int", nullable: false),
                    line_no = table.Column<int>(type: "int", nullable: false),
                    dish_id = table.Column<int>(type: "int", nullable: false),
                    dish_name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    unit_price = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    quantity = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItems", x => new { x.order_id, x.line_no });
                    table.ForeignKey(
                        name: "FK_OrderItems_Dishes_dish_id",
                        column: x => x.dish_id,
                        principalTable: "Dishes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrderItems_Orders_order_id",
                        column: x => x.order_id,
                        principalTable: "Orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderStatusHistories",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    order_id = table.Column<int>(type: "int", nullable: false),
                    changed_by_user_id = table.Column<int>(type: "int", nullable: true),
                    status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    changed_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderStatusHistories", x => x.id);
                    table.ForeignKey(
                        name: "FK_OrderStatusHistories_AppUsers_changed_by_user_id",
                        column: x => x.changed_by_user_id,
                        principalTable: "AppUsers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_OrderStatusHistories_Orders_order_id",
                        column: x => x.order_id,
                        principalTable: "Orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    order_id = table.Column<int>(type: "int", nullable: false),
                    amount = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "Pending"),
                    stripe_reference = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    unsettled_amount = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false, defaultValue: 0.00m)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.id);
                    table.ForeignKey(
                        name: "FK_Payments_Orders_order_id",
                        column: x => x.order_id,
                        principalTable: "Orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PointsLedgers",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    order_id = table.Column<int>(type: "int", nullable: true),
                    points = table.Column<int>(type: "int", nullable: false),
                    type = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PointsLedgers", x => x.id);
                    table.ForeignKey(
                        name: "FK_PointsLedgers_AppUsers_user_id",
                        column: x => x.user_id,
                        principalTable: "AppUsers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PointsLedgers_Orders_order_id",
                        column: x => x.order_id,
                        principalTable: "Orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RefundRequests",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    order_id = table.Column<int>(type: "int", nullable: false),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    reason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "Requested"),
                    amount = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    requested_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    decision = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    decision_reason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    decided_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    stripe_refund_ref = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefundRequests", x => x.id);
                    table.ForeignKey(
                        name: "FK_RefundRequests_AppUsers_user_id",
                        column: x => x.user_id,
                        principalTable: "AppUsers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RefundRequests_Orders_order_id",
                        column: x => x.order_id,
                        principalTable: "Orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Reviews",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    order_id = table.Column<int>(type: "int", nullable: false),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    chef_profile_id = table.Column<int>(type: "int", nullable: false),
                    rating = table.Column<byte>(type: "tinyint", nullable: false),
                    comment = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    moderation_status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "Pending"),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reviews", x => x.id);
                    table.ForeignKey(
                        name: "FK_Reviews_AppUsers_user_id",
                        column: x => x.user_id,
                        principalTable: "AppUsers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Reviews_ChefProfiles_chef_profile_id",
                        column: x => x.chef_profile_id,
                        principalTable: "ChefProfiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Reviews_Orders_order_id",
                        column: x => x.order_id,
                        principalTable: "Orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SupportCases",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    order_id = table.Column<int>(type: "int", nullable: true),
                    subject = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "Open"),
                    requester_role = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    awaiting_support = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    last_activity_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    closed_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupportCases", x => x.id);
                    table.ForeignKey(
                        name: "FK_SupportCases_AppUsers_user_id",
                        column: x => x.user_id,
                        principalTable: "AppUsers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SupportCases_Orders_order_id",
                        column: x => x.order_id,
                        principalTable: "Orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "CartItemOptions",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    cart_item_id = table.Column<int>(type: "int", nullable: false),
                    dish_option_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CartItemOptions", x => x.id);
                    table.ForeignKey(
                        name: "FK_CartItemOptions_CartItems_cart_item_id",
                        column: x => x.cart_item_id,
                        principalTable: "CartItems",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CartItemOptions_DishOptions_dish_option_id",
                        column: x => x.dish_option_id,
                        principalTable: "DishOptions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrderItemOptions",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    order_id = table.Column<int>(type: "int", nullable: false),
                    order_item_line_no = table.Column<int>(type: "int", nullable: false),
                    group_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    option_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    extra_price = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false, defaultValue: 0.00m)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItemOptions", x => x.id);
                    table.ForeignKey(
                        name: "FK_OrderItemOptions_OrderItems_order_id_order_item_line_no",
                        columns: x => new { x.order_id, x.order_item_line_no },
                        principalTable: "OrderItems",
                        principalColumns: new[] { "order_id", "line_no" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LedgerEntries",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    chef_profile_id = table.Column<int>(type: "int", nullable: false),
                    order_id = table.Column<int>(type: "int", nullable: true),
                    payout_id = table.Column<int>(type: "int", nullable: true),
                    refund_request_id = table.Column<int>(type: "int", nullable: true),
                    type = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    amount = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LedgerEntries", x => x.id);
                    table.ForeignKey(
                        name: "FK_LedgerEntries_ChefProfiles_chef_profile_id",
                        column: x => x.chef_profile_id,
                        principalTable: "ChefProfiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LedgerEntries_Orders_order_id",
                        column: x => x.order_id,
                        principalTable: "Orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LedgerEntries_Payouts_payout_id",
                        column: x => x.payout_id,
                        principalTable: "Payouts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LedgerEntries_RefundRequests_refund_request_id",
                        column: x => x.refund_request_id,
                        principalTable: "RefundRequests",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CaseMessages",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    support_case_id = table.Column<int>(type: "int", nullable: false),
                    author_user_id = table.Column<int>(type: "int", nullable: false),
                    author_role = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    text = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CaseMessages", x => x.id);
                    table.ForeignKey(
                        name: "FK_CaseMessages_AppUsers_author_user_id",
                        column: x => x.author_user_id,
                        principalTable: "AppUsers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CaseMessages_SupportCases_support_case_id",
                        column: x => x.support_case_id,
                        principalTable: "SupportCases",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AppUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_AppUsers_email",
                table: "AppUsers",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AppUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_AssistantConversations_user_id",
                table: "AssistantConversations",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_AssistantMessages_assistant_conversation_id",
                table: "AssistantMessages",
                column: "assistant_conversation_id");

            migrationBuilder.CreateIndex(
                name: "IX_AssistantUsages_user_id_usage_date",
                table: "AssistantUsages",
                columns: new[] { "user_id", "usage_date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CartItemOptions_cart_item_id",
                table: "CartItemOptions",
                column: "cart_item_id");

            migrationBuilder.CreateIndex(
                name: "IX_CartItemOptions_dish_option_id",
                table: "CartItemOptions",
                column: "dish_option_id");

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_cart_id",
                table: "CartItems",
                column: "cart_id");

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_dish_id",
                table: "CartItems",
                column: "dish_id");

            migrationBuilder.CreateIndex(
                name: "IX_Carts_chef_profile_id",
                table: "Carts",
                column: "chef_profile_id");

            migrationBuilder.CreateIndex(
                name: "IX_Carts_user_id",
                table: "Carts",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_CaseMessages_author_user_id",
                table: "CaseMessages",
                column: "author_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_CaseMessages_support_case_id",
                table: "CaseMessages",
                column: "support_case_id");

            migrationBuilder.CreateIndex(
                name: "IX_ChefDocuments_chef_profile_id",
                table: "ChefDocuments",
                column: "chef_profile_id");

            migrationBuilder.CreateIndex(
                name: "IX_ChefProfiles_user_id",
                table: "ChefProfiles",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_chef_profile_id",
                table: "Conversations",
                column: "chef_profile_id");

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_customer_user_id_chef_profile_id",
                table: "Conversations",
                columns: new[] { "customer_user_id", "chef_profile_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Dishes_category_id",
                table: "Dishes",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_Dishes_chef_profile_id",
                table: "Dishes",
                column: "chef_profile_id");

            migrationBuilder.CreateIndex(
                name: "IX_Dishes_cuisine_id",
                table: "Dishes",
                column: "cuisine_id");

            migrationBuilder.CreateIndex(
                name: "IX_DishOptions_dish_id",
                table: "DishOptions",
                column: "dish_id");

            migrationBuilder.CreateIndex(
                name: "IX_Faqs_updated_by_user_id",
                table: "Faqs",
                column: "updated_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_Favorites_chef_profile_id",
                table: "Favorites",
                column: "chef_profile_id");

            migrationBuilder.CreateIndex(
                name: "IX_Favorites_user_id_chef_profile_id",
                table: "Favorites",
                columns: new[] { "user_id", "chef_profile_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LedgerEntries_chef_profile_id",
                table: "LedgerEntries",
                column: "chef_profile_id");

            migrationBuilder.CreateIndex(
                name: "IX_LedgerEntries_order_id",
                table: "LedgerEntries",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "IX_LedgerEntries_payout_id",
                table: "LedgerEntries",
                column: "payout_id");

            migrationBuilder.CreateIndex(
                name: "IX_LedgerEntries_refund_request_id",
                table: "LedgerEntries",
                column: "refund_request_id");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_conversation_id",
                table: "Messages",
                column: "conversation_id");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_sender_user_id",
                table: "Messages",
                column: "sender_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_user_id",
                table: "Notifications",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItemOptions_order_id_order_item_line_no",
                table: "OrderItemOptions",
                columns: new[] { "order_id", "order_item_line_no" });

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_dish_id",
                table: "OrderItems",
                column: "dish_id");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_chef_profile_id",
                table: "Orders",
                column: "chef_profile_id");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_reference",
                table: "Orders",
                column: "reference",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_submission_token",
                table: "Orders",
                column: "submission_token",
                unique: true,
                filter: "[submission_token] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_user_id",
                table: "Orders",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_OrderStatusHistories_changed_by_user_id",
                table: "OrderStatusHistories",
                column: "changed_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_OrderStatusHistories_order_id",
                table: "OrderStatusHistories",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_order_id",
                table: "Payments",
                column: "order_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Payouts_chef_profile_id",
                table: "Payouts",
                column: "chef_profile_id");

            migrationBuilder.CreateIndex(
                name: "IX_Payouts_idempotency_key",
                table: "Payouts",
                column: "idempotency_key",
                unique: true,
                filter: "[idempotency_key] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PlatformSettings_updated_by_user_id",
                table: "PlatformSettings",
                column: "updated_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_PointsLedgers_order_id",
                table: "PointsLedgers",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "IX_PointsLedgers_user_id",
                table: "PointsLedgers",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_RefundRequests_order_id",
                table: "RefundRequests",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "IX_RefundRequests_user_id",
                table: "RefundRequests",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_chef_profile_id",
                table: "Reviews",
                column: "chef_profile_id");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_order_id",
                table: "Reviews",
                column: "order_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_user_id",
                table: "Reviews",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_SupportCases_order_id",
                table: "SupportCases",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "IX_SupportCases_user_id",
                table: "SupportCases",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "AssistantMessages");

            migrationBuilder.DropTable(
                name: "AssistantUsages");

            migrationBuilder.DropTable(
                name: "CartItemOptions");

            migrationBuilder.DropTable(
                name: "CaseMessages");

            migrationBuilder.DropTable(
                name: "ChefDocuments");

            migrationBuilder.DropTable(
                name: "ChefWorkingHours");

            migrationBuilder.DropTable(
                name: "Faqs");

            migrationBuilder.DropTable(
                name: "Favorites");

            migrationBuilder.DropTable(
                name: "LedgerEntries");

            migrationBuilder.DropTable(
                name: "Messages");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "OrderItemOptions");

            migrationBuilder.DropTable(
                name: "OrderStatusHistories");

            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropTable(
                name: "PlatformSettings");

            migrationBuilder.DropTable(
                name: "PointsLedgers");

            migrationBuilder.DropTable(
                name: "Reviews");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AssistantConversations");

            migrationBuilder.DropTable(
                name: "CartItems");

            migrationBuilder.DropTable(
                name: "DishOptions");

            migrationBuilder.DropTable(
                name: "SupportCases");

            migrationBuilder.DropTable(
                name: "Payouts");

            migrationBuilder.DropTable(
                name: "RefundRequests");

            migrationBuilder.DropTable(
                name: "Conversations");

            migrationBuilder.DropTable(
                name: "OrderItems");

            migrationBuilder.DropTable(
                name: "Carts");

            migrationBuilder.DropTable(
                name: "Dishes");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "Cuisines");

            migrationBuilder.DropTable(
                name: "ChefProfiles");

            migrationBuilder.DropTable(
                name: "AppUsers");
        }
    }
}
