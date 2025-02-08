```csharp
// im_print(): print a string to stdout

using System;

public class Vips {
    // Print a string to stdout, with a "\n". Sometimes useful for debugging
    // language bindings.
    public static int im_print(string message) {
        Console.WriteLine(message);

        return 0;
    }
}
```