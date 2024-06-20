namespace RGSSGui;

public class TreeFileNode : TreeNode
{
    public TreeFileNode(string text, string path, bool isFile) : base(text)
    {
        Path = path;
        IsFile = isFile;
    }

    public string Path { get; }
    
    public bool IsFile { get; }
}
