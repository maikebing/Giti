using System.IO;
using Giti.Code;
using Giti.Models;
using Giti.Services;
using LibGit2Sharp;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Giti.Controllers
{
    [Authorize]
    public class FileViewController : GitControllerBase
    {
        public FileViewController(
            GitRepositoryService repoService
        )
            : base(repoService)
        { }

        public IActionResult RedirectGitLink(string userName, string repoName)
        {
            return TryGetResult(repoName, () => RedirectToRoutePermanent("GetRepositoryHomeView", new { userName = userName, repoName = repoName }));
        }

        public IActionResult GetTreeView(string userName, string repoName, string id, string path)
        {
            return TryGetResult(repoName, () =>
            {
                repoName = Path.Combine(userName, repoName);
                LibGit2Sharp.Repository repo = RepositoryService.GetRepository(repoName);
                Commit commit = repo.Branches[id]?.Tip ?? repo.Lookup<Commit>(id);

                if (commit == null)
                    return View("Init");

                if (path == null)
                {
                    return View("Tree", new TreeModel(repo, "/", repoName, commit.Tree));
                }

                TreeEntry entry = commit[path];
                if (entry == null)
                    return NotFound();

                string parent = Path.GetDirectoryName(entry.Path).Replace(Path.DirectorySeparatorChar, '/');

                switch (entry.TargetType)
                {
                    case TreeEntryTargetType.Tree:
                        return View("Tree", new TreeModel(repo, entry.Path, entry.Name, (Tree)entry.Target, parent));

                    case TreeEntryTargetType.Blob:
                        return Redirect(Url.UnencodedRouteLink("GetBlobView", new { repoName = repoName, id = id, path = path }));

                    default:
                        return BadRequest();
                }
            });
        }

        public IActionResult GetBlobView(string userName, string repoName, string id, string path)
        {
            return TryGetResult(repoName, () =>
            {
                if (path == null)
                    return Redirect(Url.UnencodedRouteLink("GetTreeView", new { repoName = repoName, id = id, path = path }));

                repoName = Path.Combine(userName, repoName);
                LibGit2Sharp.Repository repo = RepositoryService.GetRepository(repoName);
                Commit commit = repo.Branches[id]?.Tip ?? repo.Lookup<Commit>(id);

                if (commit == null)
                    return NotFound();

                TreeEntry entry = commit[path];
                if (entry == null)
                    return NotFound();

                switch (entry.TargetType)
                {
                    case TreeEntryTargetType.Blob:
                        return View("Blob", new BlobModel(repo, entry.Path, entry.Name, (Blob)entry.Target));

                    case TreeEntryTargetType.Tree:
                        return Redirect(Url.UnencodedRouteLink("GetTreeView", new { repoName = repoName, id = id, path = path }));

                    default:
                        return BadRequest();
                }
            });
        }

        public IActionResult GetRawBlob(string userName, string repoName, string id, string path)
        {
            return TryGetResult(repoName, () =>
            {
                if (path == null)
                    return Redirect(Url.UnencodedRouteLink("GetTreeView", new { repoName = repoName, id = id, path = path }));

                repoName = Path.Combine(userName, repoName);
                LibGit2Sharp.Repository repo = RepositoryService.GetRepository(repoName);
                Commit commit = repo.Branches[id]?.Tip ?? repo.Lookup<Commit>(id);

                if (commit == null)
                    return NotFound();

                TreeEntry entry = commit[path.Replace('/', Path.DirectorySeparatorChar)];
                if (entry == null)
                    return NotFound();

                switch (entry.TargetType)
                {
                    case TreeEntryTargetType.Blob:
                        {
                            Blob blob = (Blob)entry.Target;

                            if (blob.IsBinary)
                                return File(blob.GetContentStream(), "application/octet-stream", entry.Name);
                            else
                                return File(blob.GetContentStream(), "text/plain");
                        }

                    case TreeEntryTargetType.Tree:
                        return Redirect(Url.UnencodedRouteLink("GetTreeView", new { repoName = repoName, id = id, path = path }));

                    default:
                        return BadRequest();
                }
            });
        }
    }
}