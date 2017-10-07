using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Giti.Code;
using Giti.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using LibGit2Sharp;

namespace Giti.Controllers
{
    [Authorize]
    public class RepoController : Controller
    {
		private readonly GitiContext _context;
		private readonly IHostingEnvironment _hostingEnvironment;

		public RepoController(
			IHostingEnvironment hostingEnvironment,
			GitiContext context
							 )
		{
			_hostingEnvironment = hostingEnvironment;
			_context = context;
		}

		public IActionResult Index()
		{
			var currentUserId = User.GetUserId();
            var model = _context.Repositories.Where(p => p.UserId == currentUserId).ToList();
			return View(model);
		}

		public IActionResult New()
		{
			return View();
		}


		[HttpPost]
		[ValidateAntiForgeryToken]
        public async Task<IActionResult> New(Models.Repository repo)
		{
			//ToDo: check for duplicate project names before save new repo in db
			ModelState.Remove("UserId");
			repo.UserId = User.GetUserId();
            repo.DefaultBranch = "master";
			if (ModelState.IsValid)
			{
				repo.Id = Guid.NewGuid();
				_context.Add(repo);
				await _context.SaveChangesAsync();
			}
			string contentRootPath = _hostingEnvironment.ContentRootPath;
			string currentUsername = User.Identity.Name;
            string projectRepoPath = $"{contentRootPath}/repos/{currentUsername}/{repo.Name.Trim()}";
            	
			
            //Create repo folder and execute git init
			Directory.CreateDirectory(projectRepoPath);
			//ToDo: check if project record saved in db then execute git init
			LibGit2Sharp.Repository.Init(projectRepoPath);
			return RedirectToAction(nameof(Index));
		}

		
    }
}
