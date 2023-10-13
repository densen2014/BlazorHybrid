// ********************************** 
// Densen Informatica 中讯科技 
// 作者：Alex Chow
// e-mail:zhouchuanglin@gmail.com 
// **********************************

using System.Linq;

Console.WriteLine("输入新工程名称,默认为 HybirdAppTest");
var projectName = Console.ReadLine();
if (string.IsNullOrWhiteSpace(projectName)) projectName = "HybirdAppTest";
Directory.CreateDirectory(projectName);

var dirs = new string[] { "Core", "Maui", "Shared", "Maui.Shared", "SSR" };

var batFile_content = "";
foreach (var dir in dirs)
{
    Tools.CopyDirectory($@"..\..\..\..\BlazorHybrid.{dir}", $@".\{projectName}.{dir}", true, projectName);

    batFile_content += $"dotnet sln add {projectName}.{dir}/{projectName}.{dir}.csproj\r\n";
}

File.WriteAllText($@".\{projectName}.bat", batFile_content);
