using Giti.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.IO;
using Giti.Controllers;
using Giti.Code;

namespace Giti.Controllers
{
    [Authorize(AuthenticationSchemes = BasicAuthenticationDefaults.AuthenticationScheme)]
	public class GitController : GitControllerBase
    {
        public GitController(
            GitRepositoryService repoService

        )
            : base(repoService)
        { }

        [Route("{userName}/{repoName}.git/git-upload-pack")]
        public IActionResult ExecuteUploadPack(string userName, string repoName) => TryGetResult(repoName, () => GitUploadPack(Path.Combine(userName, repoName)));

        [Route("{userName}/{repoName}.git/git-receive-pack")]
        public IActionResult ExecuteReceivePack(string userName, string repoName) => TryGetResult(repoName, () => GitReceivePack(Path.Combine(userName, repoName)));

        [Route("{userName}/{repoName}.git/info/refs")]
        //ToDo: check EndStreamWithNull option again
        public IActionResult GetInfoRefs(string userName, string repoName, string service) => TryGetResult(repoName, () => GitCommand(Path.Combine(userName, repoName), service, false));
    }
}