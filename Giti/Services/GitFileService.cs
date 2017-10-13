using System;
using System.Collections.Generic;
using System.IO;
using LibGit2Sharp;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;

namespace Giti.Services
{
	public class GitFileService : GitServiceBase
	{
		private readonly IHostingEnvironment _hostingEnvironment;

		

		public GitFileService(IHostingEnvironment hostingEnvironment) : base(hostingEnvironment)
		{
            _hostingEnvironment = hostingEnvironment;
		}

		public Tree GetFileTree(string repoName, string branch = null)
		{
			return GetLatestCommit(repoName, branch).Tree;			
		}

		public TreeEntry GetFileTreeEntry(string repoName, string path, string branch = null) => GetFileTree(repoName, branch)[path];

		public IEnumerable<string> GetFiles(string repoName, string branch = null)
		{
			Tree tree = GetFileTree(repoName);
			Stack<TreeEntry> nodes = new Stack<TreeEntry>(tree);
			
			while(nodes.Count > 0)
			{
				TreeEntry entry = nodes.Pop();
				switch(entry.TargetType)
				{
					case TreeEntryTargetType.Blob:
						yield return entry.Path.Replace(Path.DirectorySeparatorChar, '/');
						break;
					case TreeEntryTargetType.Tree:
						foreach(TreeEntry e in ((Tree)entry.Target))
						{
							nodes.Push(e);
						}
						break;
				}
			}
		}

		public string GetFileContents(string repoName, string filePath)
		{
			Tree fileTree = GetFileTree(repoName);
			TreeEntry entry = fileTree[filePath];

			if (entry == null)
				throw new GitException($"No file found at path \"{filePath}\"", null);

			if (entry.TargetType != TreeEntryTargetType.Blob)
				throw new GitException("Path doesn't point to a file", null);

			return ((Blob)entry.Target).GetContentText();
		}
	}
}
