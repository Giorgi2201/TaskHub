using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskHub.API.Data;
using TaskHub.API.DTOs;
using TaskHub.API.Models;

namespace TaskHub.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RequestsController : ControllerBase
    {
        private readonly TaskHubDbContext _context;

        public RequestsController(TaskHubDbContext context)
        {
            _context = context;
        }

        // GET: api/requests
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RequestDto>>> GetRequests()
        {
            var requests = await _context.Requests
                .Include(r => r.Category)
                .Include(r => r.Subcategory)
                .Include(r => r.Status)
                .Include(r => r.Initiator)
                .Select(r => new RequestDto
                {
                    RequestID = r.RequestID,
                    Category = r.Category.CategoryName,
                    Subcategory = r.Subcategory.SubcategoryName,
                    Description = r.Description,
                    StatusID = r.StatusID,
                    InitiatorID = r.InitiatorID,
                    InitiatorName = r.Initiator.Name,
                    CreatedAt = r.CreatedAt.ToString("M/d/yyyy")
                })
                .ToListAsync();

            return Ok(requests);
        }

        // GET: api/requests/5
        [HttpGet("{id}")]
        public async Task<ActionResult<RequestDetailDto>> GetRequest(int id)
        {
            var request = await _context.Requests
                .Include(r => r.Category)
                .Include(r => r.Subcategory)
                .Include(r => r.Status)
                .Include(r => r.Initiator)
                .Include(r => r.Participants)
                    .ThenInclude(p => p.User)
                .Include(r => r.Comments)
                    .ThenInclude(c => c.User)
                .FirstOrDefaultAsync(r => r.RequestID == id);

            if (request == null)
            {
                return NotFound();
            }

            var requestDetail = new RequestDetailDto
            {
                Id = request.RequestID.ToString("D4"),
                Category = request.Category.CategoryName,
                Subcategory = request.Subcategory.SubcategoryName,
                Description = request.Description,
                CreatedDate = request.CreatedAt.ToString("M/d/yyyy"),
                UpdatedDate = request.UpdatedAt.ToString("M/d/yyyy"),
                Status = request.Status.StatusName,
                StatusClass = request.Status.StatusClass,
                Submitter = new UserDto
                {
                    Name = request.Initiator.Name,
                    Role = "შემვსები",
                    Initials = request.Initiator.Initials,
                    RoleLabel = request.Initiator.Department,
                    AvatarClass = request.Initiator.AvatarClass
                },
                Participants = request.Participants.Select(p => new ParticipantDto
                {
                    Name = p.User.Name,
                    Role = p.Role,
                    Initials = p.User.Initials,
                    RoleLabel = p.User.Department,
                    AvatarClass = p.User.AvatarClass
                }).ToList(),
                Comments = request.Comments.Select(c => new CommentDto
                {
                    Author = c.User.Name,
                    Initials = c.User.Initials,
                    Date = c.CreatedAt.ToString("M/d/yyyy"),
                    Text = c.Text
                }).ToList()
            };

            return Ok(requestDetail);
        }

        // POST: api/requests
        [HttpPost]
        public async Task<ActionResult<RequestDto>> CreateRequest(CreateRequestDto createDto)
        {
            // Find category by name
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.CategoryName == createDto.Category);

            if (category == null)
            {
                return BadRequest("Invalid category");
            }

            // Find subcategory by name and category
            var subcategory = await _context.Subcategories
                .FirstOrDefaultAsync(s => s.SubcategoryName == createDto.Subcategory && s.CategoryID == category.CategoryID);

            if (subcategory == null)
            {
                return BadRequest("Invalid subcategory");
            }

            var request = new Request
            {
                CategoryID = category.CategoryID,
                SubcategoryID = subcategory.SubcategoryID,
                Description = createDto.Description,
                StatusID = createDto.StatusID,
                InitiatorID = createDto.InitiatorID,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _context.Requests.Add(request);
            await _context.SaveChangesAsync();

            // Add the initiator as a participant
            var participant = new RequestParticipant
            {
                RequestID = request.RequestID,
                UserID = createDto.InitiatorID,
                Role = "შემვსები"
            };

            _context.RequestParticipants.Add(participant);
            await _context.SaveChangesAsync();

            // Load the full request with related data
            var createdRequest = await _context.Requests
                .Include(r => r.Category)
                .Include(r => r.Subcategory)
                .FirstOrDefaultAsync(r => r.RequestID == request.RequestID);

            var result = new RequestDto
            {
                RequestID = createdRequest!.RequestID,
                Category = createdRequest.Category.CategoryName,
                Subcategory = createdRequest.Subcategory.SubcategoryName,
                Description = createdRequest.Description,
                StatusID = createdRequest.StatusID,
                InitiatorID = createdRequest.InitiatorID,
                CreatedAt = createdRequest.CreatedAt.ToString("M/d/yyyy")
            };

            return CreatedAtAction(nameof(GetRequest), new { id = request.RequestID }, result);
        }

        // PUT: api/requests/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRequest(int id, Request request)
        {
            if (id != request.RequestID)
            {
                return BadRequest();
            }

            request.UpdatedAt = DateTime.Now;
            _context.Entry(request).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RequestExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/requests/5/approve
        [HttpPost("{id}/approve")]
        public async Task<IActionResult> ApproveRequest(int id, [FromBody] ApproveRequestDto dto)
        {
            var request = await _context.Requests.FindAsync(id);
            if (request == null)
            {
                return NotFound();
            }

            request.StatusID = 3; // Approved
            request.UpdatedAt = DateTime.Now;

            // Add supervisor as participant if not already
            var existingParticipant = await _context.RequestParticipants
                .FirstOrDefaultAsync(rp => rp.RequestID == id && rp.UserID == dto.UserId);

            if (existingParticipant == null)
            {
                _context.RequestParticipants.Add(new RequestParticipant
                {
                    RequestID = id,
                    UserID = dto.UserId,
                    Role = "ხელმძღვანელი"
                });
            }

            // Add comment only if provided
            if (!string.IsNullOrWhiteSpace(dto.Comment))
            {
                _context.Comments.Add(new Comment
                {
                    RequestID = id,
                    UserID = dto.UserId,
                    Text = dto.Comment,
                    CreatedAt = DateTime.Now
                });
            }

            await _context.SaveChangesAsync();

            return Ok();
        }

        // POST: api/requests/5/reject
        [HttpPost("{id}/reject")]
        public async Task<IActionResult> RejectRequest(int id, [FromBody] RejectRequestDto dto)
        {
            var request = await _context.Requests.FindAsync(id);
            if (request == null)
            {
                return NotFound();
            }

            request.StatusID = 6; // Rejected
            request.UpdatedAt = DateTime.Now;

            // Add supervisor as participant if not already
            var existingParticipant = await _context.RequestParticipants
                .FirstOrDefaultAsync(rp => rp.RequestID == id && rp.UserID == dto.UserId);

            if (existingParticipant == null)
            {
                _context.RequestParticipants.Add(new RequestParticipant
                {
                    RequestID = id,
                    UserID = dto.UserId,
                    Role = "ხელმძღვანელი"
                });
            }

            // Add comment
            _context.Comments.Add(new Comment
            {
                RequestID = id,
                UserID = dto.UserId,
                Text = dto.Comment,
                CreatedAt = DateTime.Now
            });

            await _context.SaveChangesAsync();

            return Ok();
        }

        // DELETE: api/requests/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRequest(int id)
        {
            var request = await _context.Requests.FindAsync(id);
            if (request == null)
            {
                return NotFound();
            }

            _context.Requests.Remove(request);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // POST: api/requests/5/update
        [HttpPost("{id}/update")]
        public async Task<IActionResult> UpdateRequestStatus(int id, [FromBody] UpdateRequestDto dto)
        {
            var request = await _context.Requests.FindAsync(id);
            if (request == null)
            {
                return NotFound();
            }

            request.StatusID = dto.StatusId;
            request.UpdatedAt = DateTime.Now;

            // Add executor as participant if provided
            if (dto.ExecutorId.HasValue)
            {
                var existingExecutor = await _context.RequestParticipants
                    .FirstOrDefaultAsync(rp => rp.RequestID == id && rp.UserID == dto.ExecutorId.Value);

                if (existingExecutor == null)
                {
                    _context.RequestParticipants.Add(new RequestParticipant
                    {
                        RequestID = id,
                        UserID = dto.ExecutorId.Value,
                        Role = "შემსრულებელი"
                    });
                }
            }

            // Add supervisor as participant if not already
            var existingSuper = await _context.RequestParticipants
                .FirstOrDefaultAsync(rp => rp.RequestID == id && rp.UserID == dto.SupervisorId);

            if (existingSuper == null)
            {
                _context.RequestParticipants.Add(new RequestParticipant
                {
                    RequestID = id,
                    UserID = dto.SupervisorId,
                    Role = "ხელმძღვანელი"
                });
            }

            // Add comment if provided
            if (!string.IsNullOrWhiteSpace(dto.Comment))
            {
                _context.Comments.Add(new Comment
                {
                    RequestID = id,
                    UserID = dto.SupervisorId,
                    Text = dto.Comment,
                    CreatedAt = DateTime.Now
                });
            }

            await _context.SaveChangesAsync();

            return Ok();
        }

        // GET: api/requests/executors
        [HttpGet("executors")]
        public async Task<ActionResult<IEnumerable<UserListDto>>> GetExecutors()
        {
            var executors = await _context.Users
                .Where(u => u.Role == "შემსრულებელი")
                .Select(u => new UserListDto
                {
                    UserID = u.UserID,
                    Name = u.Name,
                    Initials = u.Initials,
                    Department = u.Department
                })
                .ToListAsync();

            return Ok(executors);
        }

        private bool RequestExists(int id)
        {
            return _context.Requests.Any(e => e.RequestID == id);
        }
    }
}
