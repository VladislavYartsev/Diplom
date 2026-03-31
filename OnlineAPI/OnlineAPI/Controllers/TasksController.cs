using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OnlineAPI.Entities;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace OnlineAPI.Controllers
{

    [Authorize]
    public class TasksController : Controller
        {
            private readonly AppContext _context;

            public TasksController(AppContext context)
            {
                _context = context;
            }


        [HttpGet]
        public async Task<IActionResult> Index(int projectId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var hasAccess = await _context.ProjectMembers
                .AnyAsync(pm => pm.ProjectId == projectId && pm.UserId == userId);

            if (!hasAccess)
            {
                TempData["ErrorMessage"] = "У вас нет доступа к этому проекту";
                return RedirectToAction("Index", "Projects");
            }

            var project = await _context.Projects.FindAsync(projectId);
            var tasks = await _context.Tasks.Where(t => t.ProjectId == projectId).ToListAsync();

            ViewBag.ProjectId = projectId;
            return View(tasks);
        }
        
        [HttpGet]
        public async Task<IActionResult> Create(int projectId)
        {
            var project = _context.Projects.Find(projectId);
            if (project == null)
            {
                return NotFound();
            }
            ViewBag.ProjectId = projectId;
            ViewBag.Members = await GetProjectMembers(projectId);

            var model = new TaskCreateViewModel
            {
                ProjectID = projectId,
                AvailableUsers = _context.ProjectMembers
    .Where(pm => pm.ProjectId == projectId)
    .Select(pm => new SelectListItem
    {
        Value = pm.UserId.ToString(),
        Text = pm.UserName
    })
    .ToList()

            };

            return View(model);


        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TaskCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {

                model.AvailableUsers = _context.ProjectMembers
                    .Where(pm => pm.ProjectId == model.ProjectID)
                    .Select(pm => new SelectListItem
                    {
                        Value = pm.UserId.ToString(),
                        Text = pm.UserName
                    }).ToList();

                ViewBag.ProjectId = model.ProjectID;
                return View(model);
            }

            var lastTask = await _context.Tasks.OrderByDescending(t => t.Id).FirstOrDefaultAsync();
            var nextId = lastTask?.Id + 1 ?? 1;

            var task = new Entities.Task
            {
                Title = model.Title,
                Description = model.Description,
                Priority = model.Priority,
                ProjectId = model.ProjectID,
                TaskCode = $"TASK-{nextId}",
                Status = Entities.TaskStatus.ToDo,

                Assignee = model.SelectedUsers.Any() ? model.SelectedUsers.ToArray() : new string[] { }
            };

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { projectId = model.ProjectID });
        }

        // GET: Tasks/Edit/5
        public async Task<IActionResult> Edit(int? id)
            {
                if (id == null)
                {
                    return NotFound();
                }

                var task = await _context.Tasks.FindAsync(id);
                if (task == null)
                {
                    return NotFound();
                }
                return View(task);
            }

            // POST: Tasks/Edit/5
            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> Edit(int id, Entities.Task task)
            {
                if (id != task.Id)
                {
                    return NotFound();
                }

                ModelState.Remove("CreatedDate");
                ModelState.Remove("TaskCode");

            if (ModelState.IsValid)
                {

                    try
                    {
                        var existingTask = await _context.Tasks.FindAsync(id);
                        if (existingTask == null)
                        {
                            return NotFound();
                        }

                        existingTask.Description = task.Description;
                        existingTask.Status = task.Status;
                        existingTask.Priority = task.Priority;
                        existingTask.Assignee = task.Assignee;
                        existingTask.UpdatedDate = DateTime.UtcNow;

                        Console.WriteLine($"Trying save data with created time {task.CreatedDate} TASK-{task.Id}");
                        task.UpdatedDate = DateTime.UtcNow;
                        task.TaskCode = $"TASK-{task.Id}";
                        _context.Update(existingTask);
                        await _context.SaveChangesAsync();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!TaskExists(task.Id))
                        {
                            return NotFound();
                        }
                        else
                        {
                            throw;
                        }
                    }
                    return RedirectToAction(nameof(Index), new { projectId = task.ProjectId });
                }
            ViewBag.Members = await GetProjectMembers(task.ProjectId);
            return View(task);
            }

            // POST: Tasks/Move
            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> Move([FromForm] TaskMoveRequest moveRequest)
            {
                var task = await _context.Tasks.FindAsync(moveRequest.TaskId);
                if (task == null)
                {
                    return NotFound();
                }

                task.Status = moveRequest.NewStatus;
                task.UpdatedDate = DateTime.UtcNow;
                

                _context.Update(task);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index), new { projectId = task.ProjectId });
            }

            // GET: Tasks/Delete/5 
            public async Task<IActionResult> Delete(int? id)
            {
                if (id == null)
                {
                    return NotFound();
                }

                var task = await _context.Tasks
                    .FirstOrDefaultAsync(m => m.Id == id);
                if (task == null)
                {
                    return NotFound();
                }

                return View(task);
            }

            // POST: Tasks/Delete/5 
            [HttpPost, ActionName("Delete")]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> DeleteConfirmed(int id)
            {
                var task = await _context.Tasks.FindAsync(id);
                _context.Tasks.Remove(task);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index), new { projectId = task.ProjectId });
            }

            private bool TaskExists(int id)
            {
                return _context.Tasks.Any(e => e.Id == id);
            }
        private async Task<List<SelectListItem>> GetProjectMembers(int projectId)
        {
            return await _context.ProjectMembers
                .Where(m => m.ProjectId == projectId)
                .Select(m => new SelectListItem
                {
                    Value = m.Id.ToString(),
                    Text = m.UserName
                })
                .ToListAsync();
        }


    }
}
