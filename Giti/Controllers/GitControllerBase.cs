using System;
using System.IO;
using Giti.Models;
using Giti.Services;
using LibGit2Sharp;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Giti.Controllers
{
	public class GitControllerBase : Controller
	{
		
		private GitRepositoryService _repoService;

		
		protected GitRepositoryService RepositoryService => _repoService;

		protected GitControllerBase(
			
			GitRepositoryService repoService
		)
		{			
			_repoService = repoService;
		}

		protected GitCommandResult GitCommand(string repoName, string service, bool advertiseRefs, bool endStreamWithNull = true)
		{
			return new GitCommandResult(new GitCommandOptions(
				RepositoryService.GetRepository(repoName),
				service,
				advertiseRefs,
				endStreamWithNull
			));
		}

		protected GitCommandResult GitUploadPack(string repoName) => GitCommand(repoName, "git-upload-pack", false, false);
		protected GitCommandResult GitReceivePack(string repoName) => GitCommand(repoName, "git-receive-pack", false);

		protected IActionResult TryGetResult(string repoName, Func<IActionResult> resFunc)
		{
			try
			{
				return resFunc();
			}
			catch (RepositoryNotFoundException)
			{
				return MakeError("Repository not found", repoName, 404);
			}
			catch (NotFoundException)
			{
				return MakeError("The requested file could not be found", repoName, 404);
			}
			catch (Exception e)
			{
				return MakeError(e, repoName);
			}
		}

		private IActionResult MakeError(string message, string repoName, int statusCode = 500)
		{
			ErrorModel model = new ErrorModel
			{
				StatusCode = statusCode,
				Message = message,
				Description = $"An error occured while accessing repository \"{repoName}\": {message}"
			};

			ViewResult viewResult = View("_Error", model);
			viewResult.StatusCode = statusCode;
			return viewResult;
		}

		private IActionResult MakeError(Exception error, string repoName, int statusCode = 500)
		{
			return MakeError(error.Message, repoName, statusCode);
		}
	}
}
