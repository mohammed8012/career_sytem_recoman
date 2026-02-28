using career_sytem_recoman.Models.DTOs.Course;
using career_sytem_recoman.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace career_sytem_recoman.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class CoursesController(ICourseService courseService) : ControllerBase
{
    private readonly ICourseService _courseService = courseService ?? throw new ArgumentNullException(nameof(courseService));

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var id) ? id : 0;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetCourses([FromQuery] CourseFilterDto filter)
    {
        var courses = await _courseService.GetCoursesAsync(filter);
        return Ok(courses);
    }

    [HttpGet("{courseId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetCourse(int courseId)
    {
        var course = await _courseService.GetCourseAsync(courseId);
        return Ok(course);
    }

    [HttpPost]
    public async Task<IActionResult> CreateCourse([FromBody] CreateCourseDto dto)
    {
        var course = await _courseService.CreateCourseAsync(dto);
        return Ok(course);
    }

    [HttpPut("{courseId}")]
    public async Task<IActionResult> UpdateCourse(int courseId, [FromBody] UpdateCourseDto dto)
    {
        var course = await _courseService.UpdateCourseAsync(courseId, dto);
        return Ok(course);
    }

    [HttpDelete("{courseId}")]
    public async Task<IActionResult> DeleteCourse(int courseId)
    {
        await _courseService.DeleteCourseAsync(courseId);
        return NoContent();
    }

    [HttpPost("{courseId}/enroll")]
    public async Task<IActionResult> Enroll(int courseId)
    {
        var userId = GetCurrentUserId();
        var result = await _courseService.EnrollAsync(courseId, userId);
        return Ok(result);
    }

    [HttpPost("{courseId}/save")]
    public async Task<IActionResult> SaveCourse(int courseId)
    {
        var userId = GetCurrentUserId();
        var result = await _courseService.SaveCourseAsync(courseId, userId);
        return Ok(result);
    }

    [HttpDelete("{courseId}/save")]
    public async Task<IActionResult> UnsaveCourse(int courseId)
    {
        var userId = GetCurrentUserId();
        await _courseService.UnsaveCourseAsync(courseId, userId);
        return NoContent();
    }

    [HttpGet("~/api/users/{userId}/saved-courses")]
    [AllowAnonymous]
    public async Task<IActionResult> GetSavedCourses(int userId)
    {
        var saved = await _courseService.GetSavedCoursesAsync(userId);
        return Ok(saved);
    }
}