```csharp
using System;

namespace Vips {
    public class Image {
        // ... (other methods and properties)
    }

    public class Vips {
        // ... (other methods)

        /// <summary>
        /// im_video_test:
        /// @im: write image here
        /// @brightness: brightness setting
        /// @error: set this to make the function return an error
        ///
        /// Make a test video image. Set @error to trigger an error.
        ///
        /// Returns: 0 on success, -1 on error
        /// </summary>
        public static int ImVideoTest(Image im, int brightness, out string error) {
            if (error != null) {
                throw new Exception("Error requested");
            }
            else {
                return Image.GaussNoise(im, 720, 576, brightness, 20);
            }
        }

        // ... (other methods)
    }
}
```