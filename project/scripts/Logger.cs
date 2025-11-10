using Godot;
using System.Diagnostics;

public partial class Logger : Node
{
    public static void Log(string message)
    {
        if (Debugger.IsAttached)
        {
            Debugger.Log(2, "Info", message + "\n");
        }
        else
        {
            GD.Print(message);
        }
    }

    public static void LogError(string message)
    {
        if (Debugger.IsAttached)
        {
            Debugger.Log(2, "Error", message + "\n");
        }
        else
        {
            GD.PushError(message);
        }
    }
}
