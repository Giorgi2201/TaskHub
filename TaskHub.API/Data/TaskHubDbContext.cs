using Microsoft.EntityFrameworkCore;
using TaskHub.API.Models;

namespace TaskHub.API.Data
{
    public class TaskHubDbContext : DbContext
    {
        public TaskHubDbContext(DbContextOptions<TaskHubDbContext> options) : base(options)
        {
        }

        public DbSet<Request> Requests { get; set; }
        public DbSet<Status> Statuses { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Subcategory> Subcategories { get; set; }
        public DbSet<RequestParticipant> RequestParticipants { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<News> News { get; set; }
        public DbSet<Vacancy> Vacancies { get; set; }
        public DbSet<DigestEntry> DigestEntries { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure relationships
            modelBuilder.Entity<Request>()
                .HasOne(r => r.Status)
                .WithMany(s => s.Requests)
                .HasForeignKey(r => r.StatusID);

            modelBuilder.Entity<Request>()
                .HasOne(r => r.Initiator)
                .WithMany(u => u.InitiatedRequests)
                .HasForeignKey(r => r.InitiatorID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Request>()
                .HasOne(r => r.Category)
                .WithMany(c => c.Requests)
                .HasForeignKey(r => r.CategoryID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Request>()
                .HasOne(r => r.Subcategory)
                .WithMany(sc => sc.Requests)
                .HasForeignKey(r => r.SubcategoryID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Subcategory>()
                .HasOne(sc => sc.Category)
                .WithMany(c => c.Subcategories)
                .HasForeignKey(sc => sc.CategoryID);

            modelBuilder.Entity<RequestParticipant>()
                .HasOne(rp => rp.Request)
                .WithMany(r => r.Participants)
                .HasForeignKey(rp => rp.RequestID);

            modelBuilder.Entity<RequestParticipant>()
                .HasOne(rp => rp.User)
                .WithMany(u => u.Participations)
                .HasForeignKey(rp => rp.UserID);

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.Request)
                .WithMany(r => r.Comments)
                .HasForeignKey(c => c.RequestID);

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.User)
                .WithMany(u => u.Comments)
                .HasForeignKey(c => c.UserID);

            modelBuilder.Entity<News>()
                .HasOne(n => n.Author)
                .WithMany()
                .HasForeignKey(n => n.AuthorID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Vacancy>()
                .HasOne(v => v.Author)
                .WithMany()
                .HasForeignKey(v => v.AuthorID)
                .OnDelete(DeleteBehavior.Restrict);

            // Seed data
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed Statuses
            modelBuilder.Entity<Status>().HasData(
                new Status { StatusID = 1, StatusName = "ახალი", StatusClass = "new" },
                new Status { StatusID = 2, StatusName = "დამტკიცების მოლოდინში", StatusClass = "pending" },
                new Status { StatusID = 3, StatusName = "დამტკიცებული", StatusClass = "approved" },
                new Status { StatusID = 4, StatusName = "შესრულების პროცესში", StatusClass = "inprogress" },
                new Status { StatusID = 5, StatusName = "შესრულებული", StatusClass = "completed" },
                new Status { StatusID = 6, StatusName = "უარყოფილი", StatusClass = "rejected" }
            );

            // Seed Users (using simple passwords for demo purposes)
            modelBuilder.Entity<User>().HasData(
                new User { 
                    UserID = 1, 
                    Name = "გიორგი მაისურაძე", 
                    Email = "g.maisuradze@railway.ge",
                    Password = "password1",
                    Phone = "+995 599 123 456",
                    Role = "შემვსები",
                    Title = "პროექტის მენეჯერი",
                    Initials = "გმ", 
                    Department = "IT დეპარტამენტი", 
                    AvatarClass = "avatar-blue" 
                },
                new User { 
                    UserID = 2, 
                    Name = "ნინო ბერიძე", 
                    Email = "n.beridze@railway.ge",
                    Password = "password2",
                    Phone = "+995 577 234 567",
                    Role = "ხელმძღვანელი",
                    Title = "დეპარტამენტის ხელმძღვანელი",
                    Initials = "ნბ", 
                    Department = "IT დეპარტამენტი", 
                    AvatarClass = "avatar-green" 
                },
                new User { 
                    UserID = 3, 
                    Name = "დავით კვარაცხელია", 
                    Email = "d.kvaratskhelia@railway.ge",
                    Password = "password3",
                    Phone = "+995 555 345 678",
                    Role = "შემსრულებელი",
                    Title = "ტექნიკური სპეციალისტი",
                    Initials = "დკ", 
                    Department = "ტექნიკური სამსახური", 
                    AvatarClass = "avatar-orange" 
                },
                new User { 
                    UserID = 4, 
                    Name = "მარიამ გელაშვილი", 
                    Email = "m.gelashvili@railway.ge",
                    Password = "admin123",
                    Phone = "+995 591 456 789",
                    Role = "ადმინისტრატორი",
                    Title = "სისტემის ადმინისტრატორი",
                    Initials = "მგ", 
                    Department = "ადმინისტრაცია", 
                    AvatarClass = "avatar-red" 
                }
            );

            // Seed Categories
            modelBuilder.Entity<Category>().HasData(
                new Category { CategoryID = 1, CategoryName = "კომპიუტერული ტექნიკა" },
                new Category { CategoryID = 2, CategoryName = "პრინტერი" },
                new Category { CategoryID = 3, CategoryName = "ქსელი" },
                new Category { CategoryID = 4, CategoryName = "პროგრამული უზრუნველყოფა" },
                new Category { CategoryID = 5, CategoryName = "ტელეფონი" },
                new Category { CategoryID = 6, CategoryName = "სხვა" }
            );

            // Seed Subcategories
            modelBuilder.Entity<Subcategory>().HasData(
                // კომპიუტერული ტექნიკა
                new Subcategory { SubcategoryID = 1, SubcategoryName = "ლეპტოპი", CategoryID = 1 },
                new Subcategory { SubcategoryID = 2, SubcategoryName = "მონიტორი", CategoryID = 1 },
                new Subcategory { SubcategoryID = 3, SubcategoryName = "კლავიატურა/მაუსი", CategoryID = 1 },
                // პრინტერი
                new Subcategory { SubcategoryID = 4, SubcategoryName = "კარტრიჯის შეცვლა", CategoryID = 2 },
                new Subcategory { SubcategoryID = 5, SubcategoryName = "შეკეთება", CategoryID = 2 },
                new Subcategory { SubcategoryID = 6, SubcategoryName = "ახალი პრინტერი", CategoryID = 2 },
                // ქსელი
                new Subcategory { SubcategoryID = 7, SubcategoryName = "ინტერნეტ კავშირი", CategoryID = 3 },
                new Subcategory { SubcategoryID = 8, SubcategoryName = "WiFi პრობლემა", CategoryID = 3 },
                new Subcategory { SubcategoryID = 9, SubcategoryName = "ქსელის კონფიგურაცია", CategoryID = 3 },
                // პროგრამული უზრუნველყოფა
                new Subcategory { SubcategoryID = 10, SubcategoryName = "ინსტალაცია", CategoryID = 4 },
                new Subcategory { SubcategoryID = 11, SubcategoryName = "განახლება", CategoryID = 4 },
                new Subcategory { SubcategoryID = 12, SubcategoryName = "ლიცენზია", CategoryID = 4 },
                // ტელეფონი
                new Subcategory { SubcategoryID = 13, SubcategoryName = "ახალი ტელეფონი", CategoryID = 5 },
                new Subcategory { SubcategoryID = 14, SubcategoryName = "შეკეთება", CategoryID = 5 },
                new Subcategory { SubcategoryID = 15, SubcategoryName = "SIM ბარათი", CategoryID = 5 },
                // სხვა
                new Subcategory { SubcategoryID = 16, SubcategoryName = "სხვა", CategoryID = 6 }
            );

            // Seed Requests (from the mock data)
            modelBuilder.Entity<Request>().HasData(
                new Request
                {
                    RequestID = 1,
                    CategoryID = 1,
                    SubcategoryID = 1,
                    Description = "საჭიროა ახალი ლეპტოპი პროექტის მენეჯერისთვის",
                    StatusID = 2,
                    InitiatorID = 1,
                    CreatedAt = new DateTime(2025, 1, 10),
                    UpdatedAt = new DateTime(2025, 1, 10)
                },
                new Request
                {
                    RequestID = 2,
                    CategoryID = 2,
                    SubcategoryID = 4,
                    Description = "პრინტერის კარტრიჯი ამოიწურა, საჭიროა შეცვლა",
                    StatusID = 3,
                    InitiatorID = 1,
                    CreatedAt = new DateTime(2025, 1, 8),
                    UpdatedAt = new DateTime(2025, 1, 9)
                },
                new Request
                {
                    RequestID = 3,
                    CategoryID = 3,
                    SubcategoryID = 7,
                    Description = "ინტერნეტ კავშირი არ მუშაობს მე-3 სართულზე",
                    StatusID = 5,
                    InitiatorID = 1,
                    CreatedAt = new DateTime(2025, 1, 5),
                    UpdatedAt = new DateTime(2025, 1, 7)
                },
                new Request
                {
                    RequestID = 4,
                    CategoryID = 4,
                    SubcategoryID = 10,
                    Description = "Microsoft Office-ის ინსტალაცია ახალ კომპიუტერზე",
                    StatusID = 4,
                    InitiatorID = 1,
                    CreatedAt = new DateTime(2025, 1, 3),
                    UpdatedAt = new DateTime(2025, 1, 4)
                },
                new Request
                {
                    RequestID = 5,
                    CategoryID = 1,
                    SubcategoryID = 2,
                    Description = "დამატებითი მონიტორი",
                    StatusID = 6,
                    InitiatorID = 1,
                    CreatedAt = new DateTime(2025, 1, 2),
                    UpdatedAt = new DateTime(2025, 1, 2)
                }
            );

            // Seed Request Participants
            modelBuilder.Entity<RequestParticipant>().HasData(
                // Request 1
                new RequestParticipant { RequestParticipantID = 1, RequestID = 1, UserID = 1, Role = "შემვსები" },
                // Request 2
                new RequestParticipant { RequestParticipantID = 2, RequestID = 2, UserID = 1, Role = "შემვსები" },
                new RequestParticipant { RequestParticipantID = 3, RequestID = 2, UserID = 2, Role = "ხელმძღვანელი" },
                new RequestParticipant { RequestParticipantID = 4, RequestID = 2, UserID = 3, Role = "შემსრულებელი" },
                // Request 3
                new RequestParticipant { RequestParticipantID = 5, RequestID = 3, UserID = 1, Role = "შემვსები" },
                new RequestParticipant { RequestParticipantID = 6, RequestID = 3, UserID = 2, Role = "ხელმძღვანელი" },
                new RequestParticipant { RequestParticipantID = 7, RequestID = 3, UserID = 3, Role = "შემსრულებელი" },
                // Request 4
                new RequestParticipant { RequestParticipantID = 8, RequestID = 4, UserID = 1, Role = "შემვსები" },
                new RequestParticipant { RequestParticipantID = 9, RequestID = 4, UserID = 2, Role = "ხელმძღვანელი" },
                new RequestParticipant { RequestParticipantID = 10, RequestID = 4, UserID = 3, Role = "შემსრულებელი" },
                // Request 5
                new RequestParticipant { RequestParticipantID = 11, RequestID = 5, UserID = 1, Role = "შემვსები" },
                new RequestParticipant { RequestParticipantID = 12, RequestID = 5, UserID = 2, Role = "ხელმძღვანელი" }
            );

            // Seed Comments
            modelBuilder.Entity<Comment>().HasData(
                new Comment
                {
                    CommentID = 1,
                    RequestID = 2,
                    UserID = 2,
                    Text = "დამტკიცებულია, გადადეცა ტექნიკურ სამსახურს",
                    CreatedAt = new DateTime(2025, 1, 9)
                },
                new Comment
                {
                    CommentID = 2,
                    RequestID = 3,
                    UserID = 3,
                    Text = "პრობლემა მოგვარებულია - როუტერი გადაიტვირთა",
                    CreatedAt = new DateTime(2025, 1, 7)
                },
                new Comment
                {
                    CommentID = 3,
                    RequestID = 5,
                    UserID = 2,
                    Text = "ბიუჯეტი ამჟამად არ იძლევა საშუალებას",
                    CreatedAt = new DateTime(2025, 1, 2)
                }
            );

            // Seed News
            modelBuilder.Entity<News>().HasData(
                new News
                {
                    NewsID = 1,
                    Title = "ახალი მატარებლების დიეზა",
                    Content = "საქართველოს რკინიგზა შეიძენს 10 ახალ თანამედროვე მატარებელს. ყველა მატარებელი აღჭურვილი იქნება თანამედროვე კომფორტის სისტემებით.",
                    AuthorID = 4,
                    CreatedAt = new DateTime(2026, 1, 23)
                },
                new News
                {
                    NewsID = 2,
                    Title = "რკინიგზის მოდერნიზაცია დასრულდა",
                    Content = "თბილისი-ბათუმის მიმართულებით რკინიგზის მოდერნიზაციის სამუშაოები წარმატებით დასრულდა. განახლდა ლიანდაგი 150 კილომეტრის სიგრძეზე, რაც მნიშვნელოვნად გააუმჯობესებს მატარებლების მოძრაობის უსაფრთხოებას და კომფორტს. პროექტში ჩართული იყო 200-ზე მეტი სპეციალისტი და დასრულდა დაგეგმილ ვადებში. ახალი ლიანდაგი საშუალებას იძლევა მატარებლებმა განავითარონ უფრო მაღალი სიჩქარე უსაფრთხო პირობებში.",
                    AuthorID = 4,
                    CreatedAt = new DateTime(2026, 1, 22)
                },
                new News
                {
                    NewsID = 3,
                    Title = "ახალი დისპეტჩერული სისტემა",
                    Content = "დანერგილია უახლესი ციფრული დისპეტჩერული სისტემა, რომელიც რეალურ დროში აკონტროლებს ყველა მატარებლის მდებარეობას.",
                    AuthorID = 2,
                    CreatedAt = new DateTime(2026, 1, 21)
                },
                new News
                {
                    NewsID = 4,
                    Title = "უსაფრთხოების ახალი სტანდარტები",
                    Content = "საქართველოს რკინიგზამ დანერგა საერთაშორისო უსაფრთხოების სტანდარტები, რომელიც მოიცავს ახალ სიგნალიზაციის სისტემებს, გაუმჯობესებულ აპარატურას და თანამშრომელთა რეგულარულ ტრენინგებს. ამ ცვლილებებით მნიშვნელოვნად შემცირდება ავარიების რისკი და გაიზრდება მგზავრთა უსაფრთხოება. სისტემა შემუშავებულია ევროპის წამყვან კომპანიებთან თანამშრობით.",
                    AuthorID = 4,
                    CreatedAt = new DateTime(2026, 1, 20)
                },
                new News
                {
                    NewsID = 5,
                    Title = "ელექტრო მატარებლები გზაზე",
                    Content = "2026 წლის ზაფხულიდან დაიწყება ელექტრო მატარებლების ექსპლუატაცია თბილისის მიმართულებით.",
                    AuthorID = 2,
                    CreatedAt = new DateTime(2026, 1, 19)
                },
                new News
                {
                    NewsID = 6,
                    Title = "რკინიგზის მუშაკთა ტრენინგი",
                    Content = "ჩატარდა საერთაშორისო ტრენინგი რკინიგზის 150 თანამშრომლისთვის. ტრენინგი მოიცავდა უსაფრთხოების ახალ პროტოკოლებს, თანამედროვე ტექნოლოგიების გამოყენებას და საავარიო სიტუაციებში მოქმედების გზებს. ლექტორები იყვნენ გერმანიისა და საფრანგეთის წამყვანი სპეციალისტები.",
                    AuthorID = 4,
                    CreatedAt = new DateTime(2026, 1, 18)
                },
                new News
                {
                    NewsID = 7,
                    Title = "ახალი ვაგონები",
                    Content = "შევიდა 20 ახალი თანამედროვე სამგზავრო ვაგონი ევროპული სტანდარტებით.",
                    AuthorID = 2,
                    CreatedAt = new DateTime(2026, 1, 17)
                },
                new News
                {
                    NewsID = 8,
                    Title = "სადგურების რეკონსტრუქცია",
                    Content = "დაიწყო მთავარი სადგურების რეკონსტრუქცია. პროექტი მოიცავს თბილისის, ბათუმისა და ქუთაისის სადგურების სრულ განახლებას. დამონტაჟდება თანამედროვე ინფორმაციული ტაბლოები, განახლდება მოლოდინის დარბაზები და გაუმჯობესდება ინფრასტრუქტურა. რეკონსტრუქცია დასრულდება 2026 წლის ბოლოსთვის.",
                    AuthorID = 4,
                    CreatedAt = new DateTime(2026, 1, 16)
                },
                new News
                {
                    NewsID = 9,
                    Title = "WiFi ყველა მატარებელში",
                    Content = "ყველა საერთაშორისო და საშინაო მიმართულების მატარებელს დაემატება უფასო WiFi.",
                    AuthorID = 2,
                    CreatedAt = new DateTime(2026, 1, 15)
                },
                new News
                {
                    NewsID = 10,
                    Title = "ახალი მარშრუტი - თბილისი-მესტია",
                    Content = "დაემატება ახალი სეზონური მარშრუტი თბილისი-მესტია, რომელიც იმუშავებს ზამთრის სეზონში. მარშრუტი გაივლის ცხინვალის რეგიონს და იქნება ერთ-ერთი ულამაზესი სარკინიგზო მიმართულება საქართველოში. მატარებელი იქნება აღჭურვილი თანამედროვე კომფორტის ყველა საშუალებით.",
                    AuthorID = 4,
                    CreatedAt = new DateTime(2026, 1, 14)
                },
                new News
                {
                    NewsID = 11,
                    Title = "ეკოლოგიური ინიციატივა",
                    Content = "რკინიგზა იწყებს მწვანე ინიციატივას - ნახშირბადის გამონაბოლქვის შემცირების პროგრამას.",
                    AuthorID = 2,
                    CreatedAt = new DateTime(2026, 1, 13)
                },
                new News
                {
                    NewsID = 12,
                    Title = "ახალი ბილეთების სისტემა",
                    Content = "დაინერგა მობილური აპლიკაცია ბილეთების შესაძენად. აპლიკაცია საშუალებას აძლევს მომხმარებლებს დაჯავშნონ ადგილები, გადაიხადონ ონლაინ და მიიღონ ციფრული ბილეთები. ასევე ხელმისაწვდომია ისტორია და ბონუსების სისტემა ლოიალური მომხმარებლებისთვის. სისტემა მუშაობს როგორც iOS, ასევე Android პლატფორმებზე.",
                    AuthorID = 4,
                    CreatedAt = new DateTime(2026, 1, 12)
                },
                new News
                {
                    NewsID = 13,
                    Title = "საზაფხულო განრიგის ცვლილება",
                    Content = "აპრილიდან ამოქმედდება საზაფხულო განრიგი გაზრდილი რეისებით პოპულარულ მიმართულებებზე.",
                    AuthorID = 2,
                    CreatedAt = new DateTime(2026, 1, 11)
                },
                new News
                {
                    NewsID = 14,
                    Title = "საერთაშორისო თანამშრომლობა",
                    Content = "ხელი მოეწერა მემორანდუმს თურქეთის და აზერბაიჯანის რკინიგზებთან რეგიონული სატრანსპორტო დერეფნის განსავითარებლად. თანამშრომლობა მოიცავს ტექნოლოგიების გაცვლას, ერთობლივ პროექტებს და საერთაშორისო მარშრუტების გაფართოებას.",
                    AuthorID = 4,
                    CreatedAt = new DateTime(2026, 1, 10)
                },
                new News
                {
                    NewsID = 15,
                    Title = "შეღავათები სტუდენტებისთვის",
                    Content = "დაინერგა ახალი შეღავათიანი სისტემა სტუდენტებისთვის - 50% ფასდაკლება სემესტრის განმავლობაში.",
                    AuthorID = 2,
                    CreatedAt = new DateTime(2026, 1, 9)
                },
                new News
                {
                    NewsID = 16,
                    Title = "ახალი სამშენებლო პროექტი",
                    Content = "დაიწყო ახალი სარკინიგზო ხიდის მშენებლობა მტკვარზე. პროექტი განხორციელდება თანამედროვე ინჟინერიული გადაწყვეტილებებით და უზრუნველყოფს უფრო სწრაფ და უსაფრთხო მიმოსვლას. ხიდი გამოირჩევა უნიკალური არქიტექტურული დიზაინით და გახდება თბილისის ახალი ღირსშესანიშნაობა. მშენებლობა დასრულდება 2027 წლის გაზაფხულზე.",
                    AuthorID = 4,
                    CreatedAt = new DateTime(2026, 1, 8)
                },
                new News
                {
                    NewsID = 17,
                    Title = "საშობაო აქცია",
                    Content = "დეკემბრის განმავლობაში მოქმედებს სპეციალური საშობაო ფასები ოჯახური ბილეთებისთვის.",
                    AuthorID = 2,
                    CreatedAt = new DateTime(2026, 1, 7)
                },
                new News
                {
                    NewsID = 18,
                    Title = "რკინიგზის ცენტრალური დიაგნოსტიკის ცენტრი",
                    Content = "გაიხსნა ულტრათანამედროვე დიაგნოსტიკის ცენტრი, სადაც რეგულარულად ტარდება მატარებლების ტექნიკური შემოწმება და მოვლა. ცენტრი აღჭურვილია უახლესი ტექნოლოგიით და საშუალებას იძლევა დროულად გამოვლინდეს ნებისმიერი ტექნიკური პრობლემა.",
                    AuthorID = 4,
                    CreatedAt = new DateTime(2026, 1, 6)
                },
                new News
                {
                    NewsID = 19,
                    Title = "ახალი სატვირთო მარშრუტები",
                    Content = "გაიხსნა ახალი სატვირთო მიმართულებები რეგიონში, რაც გააძლიერებს ეკონომიკურ კავშირებს.",
                    AuthorID = 2,
                    CreatedAt = new DateTime(2026, 1, 5)
                },
                new News
                {
                    NewsID = 20,
                    Title = "70 წლის იუბილე",
                    Content = "საქართველოს რკინიგზა აღნიშნავს 70 წლის იუბილეს. ამ პერიოდში გავლილია გრძელი და საინტერესო გზა - საბჭოთა პერიოდის რკინიგზიდან დღევანდელ თანამედროვე ევროპულ სტანდარტებამდე. იუბილესთან დაკავშირებით დაგეგმილია სპეციალური ღონისძიებები და გამოფენები, სადაც წარმოდგენილი იქნება რკინიგზის ისტორია და მომავლის გეგმები. მოხდება ვეტერანების დაჯილდოება და გაიხსნება ახალი რკინიგზის მუზეუმი.",
                    AuthorID = 4,
                    CreatedAt = new DateTime(2026, 1, 4)
                }
            );
        }
    }
}
