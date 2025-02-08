Here is the C# code equivalent of the provided C code:

```csharp
using System;

namespace Vips {
    public abstract class Connection : Object {
        private int descriptor;
        private int trackedDescriptor;
        private int closeDescriptor;
        private string filename;

        protected Connection() { }

        ~Connection() {
            if (trackedDescriptor >= 0) {
                TrackedClose();
            }
            if (closeDescriptor >= 0) {
                Close();
            }
        }

        public virtual void Finalize() {
            // empty implementation
        }

        public static void ClassInit(Type type) {
            ObjectClass gobjectClass = (ObjectClass)type;
            gobjectClass.Finalize += new FinalizeEventHandler(Finalize);
            gobjectClass.SetProperty += new SetPropertyHandler(SetProperty);
            gobjectClass.GetProperty += new GetPropertyHandler(GetProperty);

            ArgInt("descriptor", 1, "Descriptor", "File descriptor for read or write",
                ArgumentOption.OptionalInput, typeof(Connection), "descriptor", -1, 1000000000, 0);

            ArgString("filename", 2, "Filename", "Name of file to open",
                ArgumentOption.OptionalInput, typeof(Connection), "filename", null);
        }

        public static void Init() {
            descriptor = -1;
            trackedDescriptor = -1;
            closeDescriptor = -1;
        }

        public const string Filename(VipsConnection connection) {
            return connection.filename;
        }

        public const string Nick(VipsConnection connection) {
            return connection.filename != null ? connection.filename : ((VipsObject)connection).Nickname;
        }
    }

    public class VipsConnection : Connection {
        // empty implementation
    }
}
```

Note that this code uses the `System` namespace for basic types and does not include any types from the `System.Drawing` namespace. The `G_DEFINE_ABSTRACT_TYPE` macro is replaced with a manual implementation of the abstract class, as C# does not have a direct equivalent to this macro.