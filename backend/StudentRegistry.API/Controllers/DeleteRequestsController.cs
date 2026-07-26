using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentRegistry.Application.Constants;
using StudentRegistry.Application.DTOs;
using StudentRegistry.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StudentRegistry.API.Controllers
{
    // Editor-side: create a deletion request only. Editors can never delete a student directly —
    // approving/rejecting is exclusively an Admin action (see AdminReviewController).
    [ApiController]
    [Route("api/editor/deleterequests")]
    [Authorize(Roles = AuthConstants.RoleEditor)]
    public class DeleteRequestsController : ControllerBase
    {
        private readonly IDeleteRequestService _deleteRequestService;

        public DeleteRequestsController(IDeleteRequestService deleteRequestService)
        {
            _deleteRequestService = deleteRequestService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] DeleteRequestCreateDto createDto)
        {
            try
            {
                var result = await _deleteRequestService.CreateAsync(createDto);
                return Ok(new { status = "success", data = result });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { status = "error", message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { status = "error", message = ex.Message });
            }
        }

        [HttpGet("student/{studentId:int}")]
        public async Task<IActionResult> GetForStudent(int studentId)
        {
            var result = await _deleteRequestService.GetForStudentAsync(studentId);
            return Ok(new { status = "success", data = result });
        }
    }
}
