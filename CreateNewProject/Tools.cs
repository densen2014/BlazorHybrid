// ********************************** 
// Densen Informatica 中讯科技 
// 作者：Alex Chow
// e-mail:zhouchuanglin@gmail.com 
// **********************************

using System.Linq;


public class Tools
{
    public static void CopyDirectory(string sourceDir, string destinationDir, bool recursive, string projectName, string[]? excludedFolders = null)
    {
        excludedFolders = excludedFolders ?? new[] { "bin", "obj", "packages" };

        // 获取有关源目录的信息
        var dir = new DirectoryInfo(sourceDir);

        // 检查源目录是否存在
        if (!dir.Exists)
        {
            //System.IO.Directory.CreateDirectory(sourceDir);
            throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");
        }
        if (Directory.Exists(destinationDir))
        {
            Console.WriteLine($"Destination directory exists: {destinationDir}, skip!");
            return;
        }

        // 在开始复制之前缓存目录
        DirectoryInfo[] dirs = dir.GetDirectories();

        // 创建目标目录
        Directory.CreateDirectory(destinationDir);

        // 获取源目录下的文件并复制到目标目录
        foreach (FileInfo file in dir.GetFiles())
        {
            var filename = file.Name;
            var extName = Path.GetExtension(filename);
            if (new[] { ".csproj", ".sln", }.Contains(extName))
            {
                filename = filename.Replace("BlazorHybrid", projectName);
            }
            var targetFilePath = Path.Combine(destinationDir, filename);
            file.CopyTo(targetFilePath);
            if (new[] { ".csproj", ".cshtml", ".html", ".cs", ".razor", ".xaml", ".md", ".xml", ".manifest", ".json" }.Contains(extName))
            {
                var content = File.ReadAllText(targetFilePath);
                var new_htmcontent = content.Replace("BlazorHybrid", projectName);
                File.WriteAllText(targetFilePath, new_htmcontent);
            }
        }

        // 如果递归并复制子目录，则递归调用该方法
        if (recursive)
        {
            foreach (DirectoryInfo subDir in dirs.Where(a => !excludedFolders.ToList().Contains(a.Name)))
            {
                var newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                CopyDirectory(subDir.FullName, newDestinationDir, true, projectName);
            }
        }
    }

    ////读取需要替换的文本
    //String pathTemp = System.Environment.CurrentDirectory + "\\temp.html";
    //String html = File.ReadAllText(@pathTemp);
    ////读取目录下所有需要替换的文件
    //String path = System.Environment.CurrentDirectory + "\\chapter_txt\\";
    //DirectoryInfo imagesfile = new DirectoryInfo(path);
    //FileInfo[] files = imagesfile.GetFiles("*.txt");
    //foreach (FileInfo file in files)//遍历需要替换的文件进行替换
    //{
    //    String content = File.ReadAllText(file.FullName);
    //    String new_html = html.Replace("#CONTENT", content);
    //    String new_path = path + file.Name.Replace("txt", "html");
    //    //重新写入文件
    //    StreamWriter sW = new StreamWriter(@new_path);
    //    sW.Write(new_html);
    //    sW.Close();
    //}
}
