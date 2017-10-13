using LibGit2Sharp;

namespace Giti.Models
{
	public class FileViewModel
	{
		private LibGit2Sharp.Repository _repository;
		private GitObject _object;
		private string _path;
		private string _name;

		protected LibGit2Sharp.Repository Repository => _repository;
		protected GitObject Object => _object;

		public string SHA1 => _object.Sha;
		public ObjectType Type => Repository.ObjectDatabase.RetrieveObjectMetadata(_object.Id).Type;
		public string Path => _path;
		public string Name => _name;

		protected internal FileViewModel(LibGit2Sharp.Repository repo, string path, string name, GitObject obj)
		{
			_repository = repo;
			_path = path;
			_name = name;
			_object = obj;
		}

		public static FileViewModel FromGitObject(LibGit2Sharp.Repository repo, string path, string name, GitObject obj)
		{
			switch(repo.ObjectDatabase.RetrieveObjectMetadata(obj.Id).Type)
			{
				case ObjectType.Blob:
					return new BlobModel(repo, path, name, (Blob)obj);

				case ObjectType.Tree:
					return new TreeModel(repo, path, name, (Tree)obj);

				default:
					return null;
			}
		}
	}

	public class FileViewModel<TObject> : FileViewModel where TObject : GitObject
    {
		protected new TObject Object => (TObject)base.Object;

		protected internal FileViewModel(LibGit2Sharp.Repository repo, string path, string name, TObject obj) : base(repo, path, name, obj) { }
	}
}
