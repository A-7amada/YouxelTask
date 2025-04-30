using Api.Models;
using FileStorage.Application.Dtos;
using FileStorage.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
	[ApiController]
	[Route("api/files")]
	[Authorize]
	public class FilesController : ControllerBase
	{
		private readonly IFileService _fileService;
		private readonly ILogger<FilesController> _logger;

		public FilesController(
			IFileService fileService,
			ILogger<FilesController> logger)
		{
			_fileService = fileService;
			_logger = logger;
		}

		[HttpPost("upload")]
		public async Task<IActionResult> UploadFile([FromForm] UploadFileRequest request)
		{
			try
			{
				if (request.File == null || request.File.Length == 0)
				{
					return BadRequest("File is required");
				}

				var uploadedBy = User.Identity.IsAuthenticated ? User.Identity.Name : "anonymous";
				var fileUploadDto = new FileUploadDto
				{
					Content = request.File.OpenReadStream(),
					FileName = request.File.FileName,
					ContentType = request.File.ContentType,
					Size = request.File.Length,
				};

				var fileDto = await _fileService.UploadFileAsync(fileUploadDto, uploadedBy);

				return Ok(fileDto);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error uploading file");
				return StatusCode(500, "Internal server error");
			}
		}

		[HttpGet("{fileId}/download")]
		public async Task<IActionResult> DownloadFile(Guid fileId)
		{
			try
			{
				var (fileStream, contentType, fileName) = await _fileService.DownloadFileAsync(fileId);
				return File(fileStream, contentType, fileName);
			}
			catch (FileNotFoundException)
			{
				return NotFound();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error downloading file");
				return StatusCode(500, "Internal server error");
			}
		}

		[HttpGet("{fileId}")]
		public async Task<IActionResult> GetFileInfo(Guid fileId)
		{
			try
			{
				var fileDto = await _fileService.GetFileInfoAsync(fileId);
				return Ok(fileDto);
			}
			catch (FileNotFoundException)
			{
				return NotFound();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error getting file info");
				return StatusCode(500, "Internal server error");
			}
		}

		[HttpDelete("{fileId}")]
		public async Task<IActionResult> DeleteFile(Guid fileId)
		{
			try
			{
				await _fileService.DeleteFileAsync(fileId);
				return NoContent();
			}
			catch (FileNotFoundException)
			{
				return NotFound();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error deleting file");
				return StatusCode(500, "Internal server error");
			}
		}

		[HttpGet("search")]
		public async Task<IActionResult> SearchFiles([FromQuery] string tenantId, [FromQuery] string searchTerm)
		{
			try
			{
				if (string.IsNullOrEmpty(tenantId))
				{
					return BadRequest("TenantId is required");
				}

				var files = await _fileService.SearchFilesAsync(tenantId, searchTerm);
				return Ok(files);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error searching files");
				return StatusCode(500, "Internal server error");
			}
		}
	}
}
