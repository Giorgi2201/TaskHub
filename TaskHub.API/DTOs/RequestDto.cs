namespace TaskHub.API.DTOs
{
    public class RequestDto
    {
        public int RequestID { get; set; }
        public string Category { get; set; } = string.Empty;
        public string Subcategory { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int StatusID { get; set; }
        public int InitiatorID { get; set; }
        public string InitiatorName { get; set; } = string.Empty;
        public string CreatedAt { get; set; } = string.Empty;
    }

    public class RequestDetailDto
    {
        public string Id { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Subcategory { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string CreatedDate { get; set; } = string.Empty;
        public string UpdatedDate { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string StatusClass { get; set; } = string.Empty;
        public UserDto Submitter { get; set; } = new UserDto();
        public List<ParticipantDto> Participants { get; set; } = new List<ParticipantDto>();
        public List<CommentDto> Comments { get; set; } = new List<CommentDto>();
    }

    public class UserDto
    {
        public string Name { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Initials { get; set; } = string.Empty;
        public string RoleLabel { get; set; } = string.Empty;
        public string AvatarClass { get; set; } = string.Empty;
    }

    public class ParticipantDto
    {
        public string Name { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Initials { get; set; } = string.Empty;
        public string RoleLabel { get; set; } = string.Empty;
        public string AvatarClass { get; set; } = string.Empty;
    }

    public class CommentDto
    {
        public string Author { get; set; } = string.Empty;
        public string Initials { get; set; } = string.Empty;
        public string Date { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
    }

    public class CreateRequestDto
    {
        public string Category { get; set; } = string.Empty;
        public string Subcategory { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int StatusID { get; set; }
        public int InitiatorID { get; set; }
    }

    public class CategoryDto
    {
        public int CategoryID { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public List<SubcategoryDto> Subcategories { get; set; } = new List<SubcategoryDto>();
    }

    public class SubcategoryDto
    {
        public int SubcategoryID { get; set; }
        public string SubcategoryName { get; set; } = string.Empty;
    }

    public class ApproveRequestDto
    {
        public int UserId { get; set; }
        public string Comment { get; set; } = string.Empty;
    }

    public class RejectRequestDto
    {
        public int UserId { get; set; }
        public string Comment { get; set; } = string.Empty;
    }

    public class UpdateRequestDto
    {
        public int StatusId { get; set; }
        public int? ExecutorId { get; set; }
        public int SupervisorId { get; set; }
        public string Comment { get; set; } = string.Empty;
    }

    public class UserListDto
    {
        public int UserID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Initials { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
    }

    public class UserManagementDto
    {
        public int UserID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
    }

    public class CreateUserDto
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
    }

    public class UpdateUserDto
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
    }

    public class LoginDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class LoginResponseDto
    {
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Initials { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
    }

    public class UserWithPasswordDto
    {
        public int UserID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }

    // Read-only profile info shown on the profile page. Deliberately excludes
    // Password and anything editable — the frontend only ever displays these
    // as disabled fields.
    public class ProfileDto
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }

    public class ChangePasswordDto
    {
        public string OldPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }

    public class NewsDto
    {
        public int NewsID { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public string AuthorName { get; set; } = string.Empty;
        public string CreatedAt { get; set; } = string.Empty;
    }

    public class CreateNewsDto
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public int AuthorID { get; set; }
    }

    public class UpdateNewsDto
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
    }

    public class VacancyDto
    {
        public int VacancyID { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? Deadline { get; set; }
        public bool IsActive { get; set; }
        public string AuthorName { get; set; } = string.Empty;
        public string CreatedAt { get; set; } = string.Empty;
    }

    public class CreateVacancyDto
    {
        public string Title { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime? Deadline { get; set; }
        public bool IsActive { get; set; } = true;
        public int AuthorID { get; set; }
    }

    public class UpdateVacancyDto
    {
        public string Title { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime? Deadline { get; set; }
        public bool IsActive { get; set; }
    }

    public class DigestEntryDto
    {
        public int DigestEntryID { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public string SourceName { get; set; } = string.Empty;
        public string SourceUrl { get; set; } = string.Empty;
        public string PeriodFrom { get; set; } = string.Empty;
        public string PeriodTo { get; set; } = string.Empty;
        public bool IsFeatured { get; set; }
        public bool IsActive { get; set; }
        public string AuthorName { get; set; } = string.Empty;
        public string CreatedAt { get; set; } = string.Empty;
    }

    public class CreateDigestEntryDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public string SourceName { get; set; } = string.Empty;
        public string SourceUrl { get; set; } = string.Empty;
        public DateTime PeriodFrom { get; set; }
        public DateTime PeriodTo { get; set; }
        public bool IsFeatured { get; set; }
        public bool IsActive { get; set; } = true;
        public int AuthorID { get; set; }
    }

    public class UpdateDigestEntryDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public string SourceName { get; set; } = string.Empty;
        public string SourceUrl { get; set; } = string.Empty;
        public DateTime PeriodFrom { get; set; }
        public DateTime PeriodTo { get; set; }
        public bool IsFeatured { get; set; }
        public bool IsActive { get; set; }
    }

    // NOTE: the old DigestDraftDto/SaveDigestDraftDto (module-specific digest
    // draft DTOs) were replaced by the generic DraftDto/SaveDraftDto in
    // DTOs/DraftDto.cs, used by DraftsController for Digest, Vacancy, and News
    // drafts alike.
}
