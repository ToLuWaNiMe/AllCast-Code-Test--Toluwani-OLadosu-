﻿using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using SimpleBlogAPI.DTOs;
using SimpleBlogAPI.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleBlogAPI.Controllers
{
    [Authorize(Policy = "RequireLoggedIn")]
    [ApiController]
    [Route("api/[controller]")]
    public class CommentsController : Controller
    {
        private readonly ICommentService _commentService;
        private readonly IMapper _mapper;

        public CommentsController(ICommentService commentService, IMapper mapper)
        {
            _commentService = commentService;
            _mapper = mapper;

        }

        [HttpGet("post/{postId}")]
        public async Task<ActionResult<IEnumerable<CommentDTO>>> GetCommentsByPostId(string postId)
        {
            if (string.IsNullOrEmpty(postId))
            {
                return BadRequest("Invalid postId provided.");
            }

            var comments = await _commentService.GetCommentsByPostIdAsync(postId);
            return Ok(comments);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CommentDTO>> GetComment(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("Invalid id provided.");
            }

            var comment = await _commentService.GetCommentByIdAsync(id);
            if (comment == null)
            {
                return NotFound();
            }
            return Ok(comment);
        }

        [Authorize(Policy = "RequireLoggedIn")]
        [HttpPost]
        public async Task<ActionResult<CommentDTO>> CreateComment([FromBody] CommentDTO commentDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                commentDto.Id = ObjectId.GenerateNewId().ToString();
                await _commentService.CreateCommentAsync(commentDto);
                return CreatedAtAction(nameof(GetComment), new { id = commentDto.Id }, commentDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [Authorize(Policy = "RequireLoggedIn")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateComment(string id, [FromBody] CommentDTO commentDto)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("Invalid id provided.");
            }

            if (commentDto == null)
            {
                return BadRequest("Comment data cannot be empty.");
            }

            var comment = await _commentService.GetCommentByIdAsync(id);
            if (comment == null)
            {
                return NotFound();
            }

            await _commentService.UpdateCommentAsync(id, commentDto);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteComment(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("Invalid id provided.");
            }

            var comment = await _commentService.GetCommentByIdAsync(id);
            if (comment == null)
            {
                return NotFound();
            }

            await _commentService.DeleteCommentAsync(id);
            return NoContent();
        }
    }
}
