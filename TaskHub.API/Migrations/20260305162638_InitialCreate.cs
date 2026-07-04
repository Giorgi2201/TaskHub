using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TaskHub.API.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    CategoryID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.CategoryID);
                });

            migrationBuilder.CreateTable(
                name: "Statuses",
                columns: table => new
                {
                    StatusID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StatusName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StatusClass = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Statuses", x => x.StatusID);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Initials = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Department = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AvatarClass = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserID);
                });

            migrationBuilder.CreateTable(
                name: "Subcategories",
                columns: table => new
                {
                    SubcategoryID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubcategoryName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CategoryID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subcategories", x => x.SubcategoryID);
                    table.ForeignKey(
                        name: "FK_Subcategories_Categories_CategoryID",
                        column: x => x.CategoryID,
                        principalTable: "Categories",
                        principalColumn: "CategoryID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "News",
                columns: table => new
                {
                    NewsID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AuthorID = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_News", x => x.NewsID);
                    table.ForeignKey(
                        name: "FK_News_Users_AuthorID",
                        column: x => x.AuthorID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Vacancies",
                columns: table => new
                {
                    VacancyID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Department = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Location = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Deadline = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    AuthorID = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vacancies", x => x.VacancyID);
                    table.ForeignKey(
                        name: "FK_Vacancies_Users_AuthorID",
                        column: x => x.AuthorID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Requests",
                columns: table => new
                {
                    RequestID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryID = table.Column<int>(type: "int", nullable: false),
                    SubcategoryID = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StatusID = table.Column<int>(type: "int", nullable: false),
                    InitiatorID = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Requests", x => x.RequestID);
                    table.ForeignKey(
                        name: "FK_Requests_Categories_CategoryID",
                        column: x => x.CategoryID,
                        principalTable: "Categories",
                        principalColumn: "CategoryID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Requests_Statuses_StatusID",
                        column: x => x.StatusID,
                        principalTable: "Statuses",
                        principalColumn: "StatusID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Requests_Subcategories_SubcategoryID",
                        column: x => x.SubcategoryID,
                        principalTable: "Subcategories",
                        principalColumn: "SubcategoryID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Requests_Users_InitiatorID",
                        column: x => x.InitiatorID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Comments",
                columns: table => new
                {
                    CommentID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RequestID = table.Column<int>(type: "int", nullable: false),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comments", x => x.CommentID);
                    table.ForeignKey(
                        name: "FK_Comments_Requests_RequestID",
                        column: x => x.RequestID,
                        principalTable: "Requests",
                        principalColumn: "RequestID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Comments_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RequestParticipants",
                columns: table => new
                {
                    RequestParticipantID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RequestID = table.Column<int>(type: "int", nullable: false),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequestParticipants", x => x.RequestParticipantID);
                    table.ForeignKey(
                        name: "FK_RequestParticipants_Requests_RequestID",
                        column: x => x.RequestID,
                        principalTable: "Requests",
                        principalColumn: "RequestID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RequestParticipants_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "CategoryID", "CategoryName" },
                values: new object[,]
                {
                    { 1, "კომპიუტერული ტექნიკა" },
                    { 2, "პრინტერი" },
                    { 3, "ქსელი" },
                    { 4, "პროგრამული უზრუნველყოფა" },
                    { 5, "ტელეფონი" },
                    { 6, "სხვა" }
                });

            migrationBuilder.InsertData(
                table: "Statuses",
                columns: new[] { "StatusID", "StatusClass", "StatusName" },
                values: new object[,]
                {
                    { 1, "new", "ახალი" },
                    { 2, "pending", "დამტკიცების მოლოდინში" },
                    { 3, "approved", "დამტკიცებული" },
                    { 4, "inprogress", "შესრულების პროცესში" },
                    { 5, "completed", "შესრულებული" },
                    { 6, "rejected", "უარყოფილი" }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserID", "AvatarClass", "Department", "Email", "Initials", "Name", "Password", "Phone", "Role", "Title" },
                values: new object[,]
                {
                    { 1, "avatar-blue", "IT დეპარტამენტი", "g.maisuradze@railway.ge", "გმ", "გიორგი მაისურაძე", "password1", "+995 599 123 456", "შემვსები", "პროექტის მენეჯერი" },
                    { 2, "avatar-green", "IT დეპარტამენტი", "n.beridze@railway.ge", "ნბ", "ნინო ბერიძე", "password2", "+995 577 234 567", "ხელმძღვანელი", "დეპარტამენტის ხელმძღვანელი" },
                    { 3, "avatar-orange", "ტექნიკური სამსახური", "d.kvaratskhelia@railway.ge", "დკ", "დავით კვარაცხელია", "password3", "+995 555 345 678", "შემსრულებელი", "ტექნიკური სპეციალისტი" },
                    { 4, "avatar-red", "ადმინისტრაცია", "m.gelashvili@railway.ge", "მგ", "მარიამ გელაშვილი", "admin123", "+995 591 456 789", "ადმინისტრატორი", "სისტემის ადმინისტრატორი" }
                });

            migrationBuilder.InsertData(
                table: "News",
                columns: new[] { "NewsID", "AuthorID", "Content", "CreatedAt", "ImageUrl", "Title" },
                values: new object[,]
                {
                    { 1, 4, "საქართველოს რკინიგზა შეიძენს 10 ახალ თანამედროვე მატარებელს. ყველა მატარებელი აღჭურვილი იქნება თანამედროვე კომფორტის სისტემებით.", new DateTime(2026, 1, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "ახალი მატარებლების დიეზა" },
                    { 2, 4, "თბილისი-ბათუმის მიმართულებით რკინიგზის მოდერნიზაციის სამუშაოები წარმატებით დასრულდა. განახლდა ლიანდაგი 150 კილომეტრის სიგრძეზე, რაც მნიშვნელოვნად გააუმჯობესებს მატარებლების მოძრაობის უსაფრთხოებას და კომფორტს. პროექტში ჩართული იყო 200-ზე მეტი სპეციალისტი და დასრულდა დაგეგმილ ვადებში. ახალი ლიანდაგი საშუალებას იძლევა მატარებლებმა განავითარონ უფრო მაღალი სიჩქარე უსაფრთხო პირობებში.", new DateTime(2026, 1, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "რკინიგზის მოდერნიზაცია დასრულდა" },
                    { 3, 2, "დანერგილია უახლესი ციფრული დისპეტჩერული სისტემა, რომელიც რეალურ დროში აკონტროლებს ყველა მატარებლის მდებარეობას.", new DateTime(2026, 1, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "ახალი დისპეტჩერული სისტემა" },
                    { 4, 4, "საქართველოს რკინიგზამ დანერგა საერთაშორისო უსაფრთხოების სტანდარტები, რომელიც მოიცავს ახალ სიგნალიზაციის სისტემებს, გაუმჯობესებულ აპარატურას და თანამშრომელთა რეგულარულ ტრენინგებს. ამ ცვლილებებით მნიშვნელოვნად შემცირდება ავარიების რისკი და გაიზრდება მგზავრთა უსაფრთხოება. სისტემა შემუშავებულია ევროპის წამყვან კომპანიებთან თანამშრობით.", new DateTime(2026, 1, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "უსაფრთხოების ახალი სტანდარტები" },
                    { 5, 2, "2026 წლის ზაფხულიდან დაიწყება ელექტრო მატარებლების ექსპლუატაცია თბილისის მიმართულებით.", new DateTime(2026, 1, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "ელექტრო მატარებლები გზაზე" },
                    { 6, 4, "ჩატარდა საერთაშორისო ტრენინგი რკინიგზის 150 თანამშრომლისთვის. ტრენინგი მოიცავდა უსაფრთხოების ახალ პროტოკოლებს, თანამედროვე ტექნოლოგიების გამოყენებას და საავარიო სიტუაციებში მოქმედების გზებს. ლექტორები იყვნენ გერმანიისა და საფრანგეთის წამყვანი სპეციალისტები.", new DateTime(2026, 1, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "რკინიგზის მუშაკთა ტრენინგი" },
                    { 7, 2, "შევიდა 20 ახალი თანამედროვე სამგზავრო ვაგონი ევროპული სტანდარტებით.", new DateTime(2026, 1, 17, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "ახალი ვაგონები" },
                    { 8, 4, "დაიწყო მთავარი სადგურების რეკონსტრუქცია. პროექტი მოიცავს თბილისის, ბათუმისა და ქუთაისის სადგურების სრულ განახლებას. დამონტაჟდება თანამედროვე ინფორმაციული ტაბლოები, განახლდება მოლოდინის დარბაზები და გაუმჯობესდება ინფრასტრუქტურა. რეკონსტრუქცია დასრულდება 2026 წლის ბოლოსთვის.", new DateTime(2026, 1, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "სადგურების რეკონსტრუქცია" },
                    { 9, 2, "ყველა საერთაშორისო და საშინაო მიმართულების მატარებელს დაემატება უფასო WiFi.", new DateTime(2026, 1, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "WiFi ყველა მატარებელში" },
                    { 10, 4, "დაემატება ახალი სეზონური მარშრუტი თბილისი-მესტია, რომელიც იმუშავებს ზამთრის სეზონში. მარშრუტი გაივლის ცხინვალის რეგიონს და იქნება ერთ-ერთი ულამაზესი სარკინიგზო მიმართულება საქართველოში. მატარებელი იქნება აღჭურვილი თანამედროვე კომფორტის ყველა საშუალებით.", new DateTime(2026, 1, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "ახალი მარშრუტი - თბილისი-მესტია" },
                    { 11, 2, "რკინიგზა იწყებს მწვანე ინიციატივას - ნახშირბადის გამონაბოლქვის შემცირების პროგრამას.", new DateTime(2026, 1, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "ეკოლოგიური ინიციატივა" },
                    { 12, 4, "დაინერგა მობილური აპლიკაცია ბილეთების შესაძენად. აპლიკაცია საშუალებას აძლევს მომხმარებლებს დაჯავშნონ ადგილები, გადაიხადონ ონლაინ და მიიღონ ციფრული ბილეთები. ასევე ხელმისაწვდომია ისტორია და ბონუსების სისტემა ლოიალური მომხმარებლებისთვის. სისტემა მუშაობს როგორც iOS, ასევე Android პლატფორმებზე.", new DateTime(2026, 1, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "ახალი ბილეთების სისტემა" },
                    { 13, 2, "აპრილიდან ამოქმედდება საზაფხულო განრიგი გაზრდილი რეისებით პოპულარულ მიმართულებებზე.", new DateTime(2026, 1, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "საზაფხულო განრიგის ცვლილება" },
                    { 14, 4, "ხელი მოეწერა მემორანდუმს თურქეთის და აზერბაიჯანის რკინიგზებთან რეგიონული სატრანსპორტო დერეფნის განსავითარებლად. თანამშრომლობა მოიცავს ტექნოლოგიების გაცვლას, ერთობლივ პროექტებს და საერთაშორისო მარშრუტების გაფართოებას.", new DateTime(2026, 1, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "საერთაშორისო თანამშრომლობა" },
                    { 15, 2, "დაინერგა ახალი შეღავათიანი სისტემა სტუდენტებისთვის - 50% ფასდაკლება სემესტრის განმავლობაში.", new DateTime(2026, 1, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "შეღავათები სტუდენტებისთვის" },
                    { 16, 4, "დაიწყო ახალი სარკინიგზო ხიდის მშენებლობა მტკვარზე. პროექტი განხორციელდება თანამედროვე ინჟინერიული გადაწყვეტილებებით და უზრუნველყოფს უფრო სწრაფ და უსაფრთხო მიმოსვლას. ხიდი გამოირჩევა უნიკალური არქიტექტურული დიზაინით და გახდება თბილისის ახალი ღირსშესანიშნაობა. მშენებლობა დასრულდება 2027 წლის გაზაფხულზე.", new DateTime(2026, 1, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "ახალი სამშენებლო პროექტი" },
                    { 17, 2, "დეკემბრის განმავლობაში მოქმედებს სპეციალური საშობაო ფასები ოჯახური ბილეთებისთვის.", new DateTime(2026, 1, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "საშობაო აქცია" },
                    { 18, 4, "გაიხსნა ულტრათანამედროვე დიაგნოსტიკის ცენტრი, სადაც რეგულარულად ტარდება მატარებლების ტექნიკური შემოწმება და მოვლა. ცენტრი აღჭურვილია უახლესი ტექნოლოგიით და საშუალებას იძლევა დროულად გამოვლინდეს ნებისმიერი ტექნიკური პრობლემა.", new DateTime(2026, 1, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "რკინიგზის ცენტრალური დიაგნოსტიკის ცენტრი" },
                    { 19, 2, "გაიხსნა ახალი სატვირთო მიმართულებები რეგიონში, რაც გააძლიერებს ეკონომიკურ კავშირებს.", new DateTime(2026, 1, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "ახალი სატვირთო მარშრუტები" },
                    { 20, 4, "საქართველოს რკინიგზა აღნიშნავს 70 წლის იუბილეს. ამ პერიოდში გავლილია გრძელი და საინტერესო გზა - საბჭოთა პერიოდის რკინიგზიდან დღევანდელ თანამედროვე ევროპულ სტანდარტებამდე. იუბილესთან დაკავშირებით დაგეგმილია სპეციალური ღონისძიებები და გამოფენები, სადაც წარმოდგენილი იქნება რკინიგზის ისტორია და მომავლის გეგმები. მოხდება ვეტერანების დაჯილდოება და გაიხსნება ახალი რკინიგზის მუზეუმი.", new DateTime(2026, 1, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "70 წლის იუბილე" }
                });

            migrationBuilder.InsertData(
                table: "Subcategories",
                columns: new[] { "SubcategoryID", "CategoryID", "SubcategoryName" },
                values: new object[,]
                {
                    { 1, 1, "ლეპტოპი" },
                    { 2, 1, "მონიტორი" },
                    { 3, 1, "კლავიატურა/მაუსი" },
                    { 4, 2, "კარტრიჯის შეცვლა" },
                    { 5, 2, "შეკეთება" },
                    { 6, 2, "ახალი პრინტერი" },
                    { 7, 3, "ინტერნეტ კავშირი" },
                    { 8, 3, "WiFi პრობლემა" },
                    { 9, 3, "ქსელის კონფიგურაცია" },
                    { 10, 4, "ინსტალაცია" },
                    { 11, 4, "განახლება" },
                    { 12, 4, "ლიცენზია" },
                    { 13, 5, "ახალი ტელეფონი" },
                    { 14, 5, "შეკეთება" },
                    { 15, 5, "SIM ბარათი" },
                    { 16, 6, "სხვა" }
                });

            migrationBuilder.InsertData(
                table: "Requests",
                columns: new[] { "RequestID", "CategoryID", "CreatedAt", "Description", "InitiatorID", "StatusID", "SubcategoryID", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, 1, new DateTime(2025, 1, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "საჭიროა ახალი ლეპტოპი პროექტის მენეჯერისთვის", 1, 2, 1, new DateTime(2025, 1, 10, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 2, 2, new DateTime(2025, 1, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), "პრინტერის კარტრიჯი ამოიწურა, საჭიროა შეცვლა", 1, 3, 4, new DateTime(2025, 1, 9, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 3, 3, new DateTime(2025, 1, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), "ინტერნეტ კავშირი არ მუშაობს მე-3 სართულზე", 1, 5, 7, new DateTime(2025, 1, 7, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 4, 4, new DateTime(2025, 1, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "Microsoft Office-ის ინსტალაცია ახალ კომპიუტერზე", 1, 4, 10, new DateTime(2025, 1, 4, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 5, 1, new DateTime(2025, 1, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), "დამატებითი მონიტორი", 1, 6, 2, new DateTime(2025, 1, 2, 0, 0, 0, 0, DateTimeKind.Unspecified) }
                });

            migrationBuilder.InsertData(
                table: "Comments",
                columns: new[] { "CommentID", "CreatedAt", "RequestID", "Text", "UserID" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 1, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), 2, "დამტკიცებულია, გადადეცა ტექნიკურ სამსახურს", 2 },
                    { 2, new DateTime(2025, 1, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), 3, "პრობლემა მოგვარებულია - როუტერი გადაიტვირთა", 3 },
                    { 3, new DateTime(2025, 1, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), 5, "ბიუჯეტი ამჟამად არ იძლევა საშუალებას", 2 }
                });

            migrationBuilder.InsertData(
                table: "RequestParticipants",
                columns: new[] { "RequestParticipantID", "RequestID", "Role", "UserID" },
                values: new object[,]
                {
                    { 1, 1, "შემვსები", 1 },
                    { 2, 2, "შემვსები", 1 },
                    { 3, 2, "ხელმძღვანელი", 2 },
                    { 4, 2, "შემსრულებელი", 3 },
                    { 5, 3, "შემვსები", 1 },
                    { 6, 3, "ხელმძღვანელი", 2 },
                    { 7, 3, "შემსრულებელი", 3 },
                    { 8, 4, "შემვსები", 1 },
                    { 9, 4, "ხელმძღვანელი", 2 },
                    { 10, 4, "შემსრულებელი", 3 },
                    { 11, 5, "შემვსები", 1 },
                    { 12, 5, "ხელმძღვანელი", 2 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Comments_RequestID",
                table: "Comments",
                column: "RequestID");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_UserID",
                table: "Comments",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_News_AuthorID",
                table: "News",
                column: "AuthorID");

            migrationBuilder.CreateIndex(
                name: "IX_RequestParticipants_RequestID",
                table: "RequestParticipants",
                column: "RequestID");

            migrationBuilder.CreateIndex(
                name: "IX_RequestParticipants_UserID",
                table: "RequestParticipants",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_Requests_CategoryID",
                table: "Requests",
                column: "CategoryID");

            migrationBuilder.CreateIndex(
                name: "IX_Requests_InitiatorID",
                table: "Requests",
                column: "InitiatorID");

            migrationBuilder.CreateIndex(
                name: "IX_Requests_StatusID",
                table: "Requests",
                column: "StatusID");

            migrationBuilder.CreateIndex(
                name: "IX_Requests_SubcategoryID",
                table: "Requests",
                column: "SubcategoryID");

            migrationBuilder.CreateIndex(
                name: "IX_Subcategories_CategoryID",
                table: "Subcategories",
                column: "CategoryID");

            migrationBuilder.CreateIndex(
                name: "IX_Vacancies_AuthorID",
                table: "Vacancies",
                column: "AuthorID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Comments");

            migrationBuilder.DropTable(
                name: "News");

            migrationBuilder.DropTable(
                name: "RequestParticipants");

            migrationBuilder.DropTable(
                name: "Vacancies");

            migrationBuilder.DropTable(
                name: "Requests");

            migrationBuilder.DropTable(
                name: "Statuses");

            migrationBuilder.DropTable(
                name: "Subcategories");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Categories");
        }
    }
}
