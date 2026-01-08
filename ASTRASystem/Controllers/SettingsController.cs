using ASTRASystem.Data;
using ASTRASystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ASTRASystem.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class SettingsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public SettingsController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        [HttpGet]
        public async Task<IActionResult> GetSettings()
        {
            var settings = await _context.SystemSettings.ToDictionaryAsync(s => s.Key, s => s.Value);
            return Ok(new { success = true, data = settings });
        }

        [HttpPut]
        public async Task<IActionResult> UpdateSettings([FromBody] Dictionary<string, string> settings)
        {
            foreach (var kvp in settings)
            {
                var setting = await _context.SystemSettings.FindAsync(kvp.Key);
                if (setting == null)
                {
                    setting = new SystemSetting { Key = kvp.Key };
                    _context.SystemSettings.Add(setting);
                }
                setting.Value = kvp.Value;
                setting.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return Ok(new { success = true, message = "Settings updated successfully" });
        }

        [HttpPost("logo")]
        public async Task<IActionResult> UploadLogo(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { success = false, message = "No file uploaded" });

            try
            {
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "settings");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = "company_logo_" + Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Update setting
                var logoUrl = $"/uploads/settings/{uniqueFileName}";
                var setting = await _context.SystemSettings.FindAsync("CompanyLogo");
                if (setting == null)
                {
                    setting = new SystemSetting { Key = "CompanyLogo" };
                    _context.SystemSettings.Add(setting);
                }
                setting.Value = logoUrl;
                setting.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(new { success = true, data = new { logoUrl } });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Internal server error: {ex.Message}" });
            }
        }
    }
}
