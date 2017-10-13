using System.IO;
using System.Linq;
using LibGit2Sharp;
using Microsoft.AspNetCore.Hosting;
using Giti.Code;

namespace Giti.Services
{
    public abstract class GitServiceBase
    {		
        private readonly IHostingEnvironment _hostingEnvironment;

		protected GitServiceBase(IHostingEnvironment hostingEnvironment)
		{
            _hostingEnvironment = hostingEnvironment;
		}

		public LibGit2Sharp.Repository GetRepository(string name)
        => new Repository(Path.Combine(_hostingEnvironment.GetReposBaseFolderPath(), name));

		protected Commit GetLatestCommit(string repoName, string branch = null)
		{
			LibGit2Sharp.Repository repo = GetRepository(repoName);

			Branch b;
			if (branch == null)
				b = repo.Head;
			else
				b = repo.Branches.First(d => d.CanonicalName == branch);

			return b.Tip;
		}
    }
}
