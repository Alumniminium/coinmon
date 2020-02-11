using System;
using System.IO;
using System.Reflection;

namespace CoinMon
{
    public static class Installer
    {
        public static void Install()
        {
            var name = Assembly.GetExecutingAssembly().GetName().ToString().ToLower().Split(',')[0];
            askagain:
            Console.WriteLine("Please chose a name for this application (default= " + name + " ):");
            var inputName = Console.ReadLine();

            if (inputName != "")
                name = inputName;

            usrCantReadLbl:
            Console.Write("Do you really want to use '" + name + "'? (Y/n)");

            var response = Console.ReadKey().Key.ToString().ToLower();

            if (response == "n")
                goto askagain;
            if (response != "y" && response != "enter")
                goto usrCantReadLbl;

            var path = Assembly.GetExecutingAssembly().Location;

            var script =
                "function " + name + "()" + Environment.NewLine +
                "{" + Environment.NewLine +
                "       dotnet " + path + " -installed-" + name + " $@" + Environment.NewLine +
                "}";

            var zshrcPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/.zshfunctions";
            if (!File.Exists(zshrcPath))
                File.Create(zshrcPath).Close();

            var zshrc = File.ReadAllText(zshrcPath);
            zshrc = script + Environment.NewLine + zshrc + Environment.NewLine;
            File.WriteAllText(zshrcPath, zshrc);
            Console.WriteLine($"{name} has been setup. Please run ' source ~/.zshrc ' before using {name}");
            Environment.Exit(0);
        }
    }
}