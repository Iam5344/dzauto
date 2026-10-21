using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectTracker.API.Data;
using ProjectTracker.API.DTOs;
using ProjectTracker.API.Models;

namespace ProjectTracker.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProjectsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public ProjectsController(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetProjects()
        {
            var projects = await _context.Projects
                .Where(p => !p.IsDeleted)
                .Include(p => p.Tasks.Where(t => !t.IsDeleted))
                .ToListAsync();

            return Ok(_mapper.Map<IEnumerable<ProjectReadDto>>(projects));
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetProjectById(Guid id)
        {
            var project = await _context.Projects
                .Where(p => p.Id == id && !p.IsDeleted)
                .Include(p => p.Tasks.Where(t => !t.IsDeleted))
                .FirstOrDefaultAsync();

            if (project == null) return NotFound("Проєкт не знайдено.");

            return Ok(_mapper.Map<ProjectReadDto>(project));
        }

        [HttpPost]
        public async Task<IActionResult> CreateProject([FromBody] ProjectCreateDto dto)
        {
            var project = _mapper.Map<Project>(dto);
            project.Id = Guid.NewGuid();
            project.CreatedAt = DateTime.UtcNow;

            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            var readDto = _mapper.Map<ProjectReadDto>(project);
            return CreatedAtAction(nameof(GetProjectById), new { id = readDto.Id }, readDto);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteProject(Guid id)
        {
            var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
            if (project == null) return NotFound();

            project.IsDeleted = true;
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
