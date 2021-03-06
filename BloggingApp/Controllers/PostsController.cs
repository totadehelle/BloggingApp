﻿using BloggingApp.Models;
using BloggingApp.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace BloggingApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PostsController : ControllerBase
    {
        private readonly BloggingContext _context;
        private TimeSpan _periodWhenChangesAllowed = new TimeSpan(1, 0, 0);

        public PostsController(BloggingContext context)
        {
            _context = context;
        }

        // GET: api/Posts
        [HttpGet]
        public IEnumerable<Post> GetPosts()
        {
            return _context.Posts;
        }

        // GET: api/Posts/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPost([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var post = await _context.Posts.FindAsync(id);

            if (post == null)
            {
                return NotFound();
            }

            return Ok(post);
        }

        // PUT: api/Posts/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePost([FromRoute] int id, [FromBody] Post post)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != post.Id)
            {
                return BadRequest();
            }

            var dbPost = _context.Posts.AsNoTracking().FirstAsync(p => p.Id == id).Result;
            var dbUser = _context.Users.AsNoTracking().FirstAsync(u => u.Id == dbPost.UserId).Result;

            if (User.Identity.Name != dbUser.Email)
            {
                return Unauthorized();
            }

            if (DateTime.Now - dbPost.CreationDate > _periodWhenChangesAllowed)
            {
                return Content("Sorry, the time limit for editing this has expired.");
            }

            post.CreationDate = dbPost.CreationDate;
            _context.Entry(post).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PostExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Ok();
        }

        // POST: api/Posts
        [HttpPost]
        public async Task<IActionResult> AddPost([FromBody] Post post)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var dbUser = _context.Users.AsNoTracking().FirstAsync(u => u.Id == post.UserId).Result;

            if (User.Identity.Name != dbUser.Email)
            {
                return Unauthorized();
            }

            post.CreationDate = DateTime.Now;
            _context.Posts.Add(post);
            await _context.SaveChangesAsync();
            return CreatedAtAction("GetPost", new { id = post.Id }, post);
        }

        // DELETE: api/Posts/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePost([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var post = await _context.Posts.FindAsync(id);

            if (post == null)
            {
                return NotFound();
            }

            var dbUser = _context.Users.AsNoTracking().FirstAsync(u => u.Id == post.UserId).Result;

            if (User.Identity.Name != dbUser.Email)
            {
                return Unauthorized();
            }

            if (DateTime.Now - post.CreationDate > _periodWhenChangesAllowed)
            {
                return Content("Sorry, the time limit for editing this has expired.");
            }

            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();

            return Ok(post);
        }

        // GET: api/Posts/5/comments
        [HttpGet("{id}/comments")]
        public async Task<IActionResult> GetPostComments([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var post = await _context.Posts.FindAsync(id);

            if (post == null)
            {
                return NotFound();
            }
            var comments = _context.Comments.Where(c => c.PostId == id);
            return Ok(comments);
        }

        private bool PostExists(int id)
        {
            return _context.Posts.Any(e => e.Id == id);
        }
    }
}