using System.Text;
using CatConfig;
using CatResource.Processor;

namespace CatResource;

public class TextProvider : IResourceProvider
{
    private const string type = "raw";
    private const string format = "txt";

    private NoValue noValue = new();
    private Dictionary<string, IDataResolver> resolvers = new(StringComparer.OrdinalIgnoreCase);

    public string DataType => type;
    public string DataFormat => format;

    public IUnit GetResource(int id, string path)
    {
        int index = 0;
        string name = PathHelpers.GetPathNode(path, ref index);

        if (path.Length >= index && resolvers.TryGetValue(name, out var resolver))
        {
            var bytes = resolver.LocateResource(path[index..]);
            if (bytes.Length == 0)
                return noValue;

            string result = UTF8Encoding.UTF8.GetString(bytes);
            return new UnitValue(id, result);
        }

        return noValue;
    }
}


public interface IDataResolver
{
    string Name { get; }
    byte[] LocateResource(string path);
}


public class FileSystemRetriever : IDataRetriever
{
    private readonly IFileSystem fs;

    public FileSystemRetriever(string name, IFileSystem fs)
    {
        Name = name;
        this.fs = fs;
        
    }

    public string Name { get; }

    public bool TryRetrieveData(string path, out byte[] data)
    {
        string localPath = getLocalPath(path);

        data = fs.Exists(localPath) ?
            fs.GetFileAtPath(localPath) : [];

        return data.Length > 0;
    }

    private string getLocalPath(string path)
    {
        throw new NotImplementedException();
    }
}

public interface IDataRetriever
{
    string Name { get; }
    bool TryRetrieveData(string path, out byte[] data);
}


public interface IFileSystem
{
    byte[] GetFileAtPath(string path);
    bool Exists(string path);

}

public class FileSystem : IFileSystem
{
    private readonly string basePath;

    public FileSystem(string basePath)
    {
        this.basePath = basePath;
    }

    public bool Exists(string path)
    {
        return Path.Exists(Path.Combine(basePath, path));
    }

    public byte[] GetFileAtPath(string path)
    {
        if (Path.Exists(path))
            return File.ReadAllBytes(path);

        return [];
    }
}

public class KvStore : IKvStore
{
    private readonly IUnitRecord record;

    public KvStore(IUnit unit)
    {
        this.record = unit is IUnitRecord record ?
              record :
              new NoRecord();

        this.Value = unit is IUnitValue v ?
            v.Value : "";

        this.Value = unit is IUnitArray a ?
            string.Join(',', a.Elements.Select(e => e is IUnitValue v ? v.Value : "")) :
            this.Value;
    }

    public string Value { get; }

    public IKvStore Retrieve(string key) =>
        new KvStore(record[key]);

}



public interface IKvStore
{
    IKvStore Retrieve(string key);
    string Value { get; }
}