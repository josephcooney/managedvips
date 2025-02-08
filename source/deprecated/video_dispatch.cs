```csharp
// video_v4l1_vec function converted from C
public static int VideoV4L1Vec(IMAGE out, string device, int channel, int brightness, int colour, int contrast, int hue, int ngrabs)
{
    return Vips.VideoV4L1(out, device, channel, brightness, colour, contrast, hue, ngrabs);
}

// video_test_vec function converted from C
public static int VideoTestVec(IMAGE out, int brightness, int error)
{
    return Vips.VideoTest(out, brightness, error);
}

// im_arg_desc structs for video_v4l1 and video_test functions
public class VideoV4L1ArgDesc : IMArgDesc
{
    public string Name { get; set; }
    public string Description { get; set; }

    public VideoV4L1ArgDesc(string name, string description)
    {
        Name = name;
        Description = description;
    }
}

public class VideoTestArgDesc : IMArgDesc
{
    public string Name { get; set; }
    public string Description { get; set; }

    public VideoTestArgDesc(string name, string description)
    {
        Name = name;
        Description = description;
    }
}

// im_function structs for video_v4l1 and video_test functions
public class VideoV4L1Function : IMFunction
{
    public string Name { get; set; }
    public string Description { get; set; }
    public int Flags { get; set; }
    public Func<IMObject[], int> DispatchFunction { get; set; }
    public int ArgListSize { get; set; }
    public IMArgDesc[] ArgList { get; set; }

    public VideoV4L1Function(string name, string description, int flags, Func<IMObject[], int> dispatchFunction, int argListSize, IMArgDesc[] argList)
    {
        Name = name;
        Description = description;
        Flags = flags;
        DispatchFunction = dispatchFunction;
        ArgListSize = argListSize;
        ArgList = argList;
    }
}

public class VideoTestFunction : IMFunction
{
    public string Name { get; set; }
    public string Description { get; set; }
    public int Flags { get; set; }
    public Func<IMObject[], int> DispatchFunction { get; set; }
    public int ArgListSize { get; set; }
    public IMArgDesc[] ArgList { get; set; }

    public VideoTestFunction(string name, string description, int flags, Func<IMObject[], int> dispatchFunction, int argListSize, IMArgDesc[] argList)
    {
        Name = name;
        Description = description;
        Flags = flags;
        DispatchFunction = dispatchFunction;
        ArgListSize = argListSize;
        ArgList = argList;
    }
}

// im_package struct for video functions
public class VideoPackage : IMPackage
{
    public string PackageName { get; set; }
    public IMFunction[] FunctionList { get; set; }

    public VideoPackage(string packageName, IMFunction[] functionList)
    {
        PackageName = packageName;
        FunctionList = functionList;
    }
}

// Dispatch table for video functions
public static class VideoFunctions
{
    private static readonly VideoV4L1Function _videoV4L1Desc = new VideoV4L1Function("im_video_v4l1", "grab a video frame with v4l1", IM_FN_NOCACHE, VideoV4L1Vec, 8, new[] { new VideoV4L1ArgDesc("out", "output image"), new VideoV4L1ArgDesc("device", "video device"), new VideoV4L1ArgDesc("channel", "video channel"), new VideoV4L1ArgDesc("brightness", "video brightness"), new VideoV4L1ArgDesc("colour", "video colour"), new VideoV4L1ArgDesc("contrast", "video contrast"), new VideoV4L1ArgDesc("hue", "video hue"), new VideoV4L1ArgDesc("ngrabs", "number of video grabs") });
    private static readonly VideoTestFunction _videoTestDesc = new VideoTestFunction("im_video_test", "test video grabber", IM_FN_NOCACHE, VideoTestVec, 3, new[] { new VideoTestArgDesc("out", "output image"), new VideoTestArgDesc("brightness", "video brightness"), new VideoTestArgDesc("error", "video error") });
    private static readonly IMFunction[] _videoList = new[] { _videoTestDesc, _videoV4L1Desc };

    public static readonly VideoPackage Im__Video = new VideoPackage("video", _videoList);
}
```