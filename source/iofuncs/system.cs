```csharp
// vips_system(): run a command on an image
// 
// 7/3/00 JC
//	- hacked it in
// 21/10/02 JC
//	- use mktemp() if mkstemp() is not available
// 10/3/03 JC
//	- out can be NULL
// 23/12/04
//	- use g_mkstemp()
// 8/9/09
// 	- add .v suffix (thanks Roland)
// 	- use vipsbuf
// 	- rewrite to make it simpler
// 2/2/10
// 	- gtkdoc
// 4/6/13
// 	- redo as a class
// 	- input and output images are now optional
// 3/5/14
// 	- switch to g_spawn_command_line_sync() from popen() ... helps stop
// 	  stray command-windows on Windows
// 27/3/16
// 	- allow [options] in out_format

using System;
using System.IO;
using System.Diagnostics;

public class VipsSystem : VipsOperation
{
    public VipsArrayImage In { get; set; }
    public VipsImage Out { get; set; }
    public string CmdFormat { get; set; }
    public string InFormat { get; set; }
    public string OutFormat { get; set; }
    public string Log { get; set; }

    // Array of names we wrote the input images to.
    private string[] in_name;

    // Output name without any options, so /tmp/vips-weifh.svg, for
    // example.
    private string out_name;

    // Output name with any options, so /tmp/vips-weifh.svg[scale=2], for
    // example.
    private string out_name_options;

    public VipsSystem()
    {
        in = new VipsArrayImage();
        Out = null;
        CmdFormat = "";
        InFormat = "%s.tif";
        OutFormat = "";
        Log = "";
    }

    protected override int Build(VipsObject object)
    {
        // Write the input images to files. We must always make copies of the
        // files, even if this image is a disc file already, in case the
        // command needs a different format.
        if (In != null)
        {
            string[] in_array = new string[In.Count];
            for (int i = 0; i < In.Count; i++)
                in_array[i] = VipsImage.GetFileName(In[i]);

            if (!(in_name = new string[in_array.Length]))
                return -1;
            Array.Copy(in_array, in_name, in_array.Length);
        }

        // Make the output filename.
        if (OutFormat != null)
        {
            string[] parts = OutFormat.Split(new char[] { '[' }, StringSplitOptions.RemoveEmptyEntries);
            out_name = VipsImage.GetFileName(parts[0]);
            if (parts.Length > 1)
                out_name_options = out_name + "[" + parts[1].TrimEnd(']') + "]";
            else
                out_name_options = out_name;
        }

        // Make the command string to run.
        string cmd = CmdFormat;
        if (In != null)
        {
            for (int i = 0; i < in_array.Length; i++)
                cmd = cmd.Replace("%" + (i + 1).ToString(), in_name[i]);
        }
        if (OutFormat != null && out_name_options != null)
            cmd = cmd.Replace("%" + (in_array.Length + 1).ToString(), out_name_options);

        // Swap all "%%" in the string for a single "%". We need this for
        // compatibility with older printf-based vips_system()s which
        // needed a double %%.
        for (int i = 0; i < cmd.Length - 1; i++)
            if (cmd[i] == '%' && cmd[i + 1] == '%')
                cmd = cmd.Remove(i, 2).Insert(i, "%");

        try
        {
            Process process = new Process();
            process.StartInfo.FileName = "cmd.exe";
            process.StartInfo.Arguments = "/c " + cmd;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.Start();

            string std_output = process.StandardOutput.ReadToEnd();
            string std_error = process.StandardError.ReadToEnd();
            int result = process.ExitCode;

            if (result != 0)
            {
                throw new Exception("Command failed with exit code " + result);
            }

            Log = std_output;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return -1;
        }

        // If out_format is set, open the output image and return it.
        if (OutFormat != null && out_name_options != null)
        {
            VipsImage out_image = new VipsImage();
            try
            {
                out_image.FromFile(out_name_options);
                Out = out_image;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return -1;
            }
        }

        // Finally the input images are deleted.
        if (In != null)
        {
            foreach (VipsImage image in In)
                image.Delete();
        }

        return 0;
    }
}

public class VipsSystemClass : VipsOperationClass
{
    public static void Register(VipsObjectBox box)
    {
        // Commands can have side-effects, so don't cache them.
        box.AddProperty("in", typeof(VipsArrayImage), "Input");
        box.AddProperty("out", typeof(VipsImage), "Output");
        box.AddProperty("cmd_format", typeof(string), "Command");
        box.AddProperty("in_format", typeof(string), "Input format");
        box.AddProperty("out_format", typeof(string), "Output format");
        box.AddProperty("log", typeof(string), "Log");
    }
}

public class VipsSystemFactory : VipsOperationFactory
{
    public override VipsObject Create()
    {
        return new VipsSystem();
    }

    public override void Register(VipsObjectBox box)
    {
        VipsSystemClass.Register(box);
    }
}
```