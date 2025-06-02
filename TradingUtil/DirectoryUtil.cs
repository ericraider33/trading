using pnyx.net.util;

namespace trading.util;

public static class DirectoryUtil
{
    public static void changeToTradingDirectory()
    {
        string currentDirectoryPath = Directory.GetCurrentDirectory();
        DirectoryInfo? parentDirectory = new DirectoryInfo(currentDirectoryPath);
        while (parentDirectory != null && !TextUtil.isEqualsIgnoreCase(parentDirectory.Name, "trading"))
        {
            parentDirectory = parentDirectory.Parent;
        }
        
        if (parentDirectory != null)
            Directory.SetCurrentDirectory(parentDirectory.FullName);
        else
            throw new DirectoryNotFoundException("Could not find `trading` directory");
    }
}