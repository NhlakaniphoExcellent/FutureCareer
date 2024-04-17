using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FutureCareer.Data;
using FutureCareer.Models;
using Microsoft.Identity.Client.Extensions.Msal;
using FutureCareer.Services;

namespace FutureCareer.Controllers
{
    public class StudentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IAzureStorage _storage;

        public StudentsController(ApplicationDbContext context, IAzureStorage storage)
        {
            _context = context;
            _storage = storage;
        }

        // GET: Students
        public async Task<IActionResult> Index(string searchString)
        {
            var students = await _context.Students.ToListAsync();
            if (!String.IsNullOrEmpty(searchString))
            {
                students = students.Where(n => n.Name.Contains(searchString) && n.EnrollmentStatus == true
                || n.Surname.Contains(searchString) && n.EnrollmentStatus == true ||
                n.Email.Contains(searchString) && n.EnrollmentStatus == true || n.Contact.Contains(searchString) && n.EnrollmentStatus == true).ToList();
            }

            return View(students);
        }

        // GET: Students/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Students
                .FirstOrDefaultAsync(m => m.StudentID == id);
            if (student == null)
            {
                return NotFound();
            }

            return View(student);
        }

        // GET: Students/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Students/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("StudentID,Name,Surname,Email,Contact,EnrollmentStatus")] Student student)
        {
            if (ModelState.IsValid)
            {
                student.StudentID = Guid.NewGuid();
                _context.Add(student);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(student);
        }

        // GET: Students/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Students.FindAsync(id);
            if (student == null)
            {
                return NotFound();
            }
            return View(student);
        }

        // POST: Students/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("StudentID,Name,Surname,Email,Contact,EnrollmentStatus")] Student student)
        {
            if (id != student.StudentID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(student);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StudentExists(student.StudentID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(student);
        }

        // GET: Students/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Students
                .FirstOrDefaultAsync(m => m.StudentID == id);
            if (student == null)
            {
                return NotFound();
            }

            return View(student);
        }

        // POST: Students/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student != null)
            {
                _context.Students.Remove(student);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool StudentExists(Guid id)
        {
            return _context.Students.Any(e => e.StudentID == id);
        }
        // GET: Learners/Select/5
        public async Task<IActionResult> Select(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Students.FindAsync(id);
            if (student == null)
            {
                return NotFound();
            }

            return View("Select", student);
        }
        // Other action methods...

        [HttpPost(nameof(Upload))]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file selected or empty file.");
            }

            try
            {
                BlobResponseDto? response = await _storage.UploadAsync(file);

                // Check if we got an error
                if (response.Error == true)
                {
                    // We got an error during upload, return an error with details to the client
                    return StatusCode(StatusCodes.Status500InternalServerError, response.Status);
                }
                else
                {
                    // Return a success message to the client about successful upload
                    return Ok(response);
                }
            }
            catch (Exception ex)
            {
                // Log the exception for further investigation
                // You might want to log the exception details for debugging or auditing purposes
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred during file upload.");
            }
        }
    }
}
