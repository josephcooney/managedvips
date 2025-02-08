Here is the C# code equivalent to the provided C code:

```csharp
// save to nifti
//
// 5/7/18
//	- from fitssave.c
// 9/9/19
//	- use double for all floating point scalar metadata, like other loaders

using System;
using VipsDotNet;

namespace VipsForeignSaveNifti
{
    public class NiftiSave : ForeignSave
    {
        // Filename for save.
        public string filename { get; set; }

        nifti_image nim;

        public override int Build()
        {
            if (base.Build())
                return -1;

            // This could be an image (indirectly) from niftiload, or something
            // like OME_TIFF, which does not have all the "nifti-ndim" fields.
            //
            // If it doesn't look like a nifti, try to make a nifti header from
            // what we have.
            if (Image.Get(typeof(string), "nifti-ndim") != null)
            {
                if (HeaderNifti(nim, Image))
                    return -1;
            }
            else
            {
                if (HeaderVips(nim, Image))
                    return -1;
            }

            // set ext, plus other stuff

            if (nifti_set_filenames(nim, filename, false, true))
            {
                throw new Exception("unable to set nifti filename");
            }

            if (!(nim.data = Image.WriteToMemory(null)))
                return -1;

            // No return code!??!?!!
            nim.data = null;

            return 0;
        }

        public override int HeaderVips(VipsImage image)
        {
            VipsObjectClass class_ = (VipsObjectClass)typeof(NiftiSave).GetBaseType();
            int[] dims = new int[8];
            int datatype;
            int i;

            // Most nifti images have this defaulted as 1.
            for (i = 0; i < 8; i++)
                dims[i] = 1;

            dims[0] = 2;
            dims[1] = image.Xsize;
            dims[2] = VipsImage.GetPageHeight(image);

            // Multipage image?
            if (dims[2] < image.Ysize)
            {
                dims[0] = 3;
                dims[3] = image.Ysize / dims[2];
            }

            datatype = ForeignNifti.BandFmt2datatype(image.BandFmt);
            if (datatype == -1)
            {
                throw new Exception("unsupported libvips image type");
            }

            if (image.Bands > 1)
            {
                if (image.BandFmt != VipsFormat.UChar)
                {
                    throw new Exception("8-bit colour images only");
                }

                if (image.Bands == 3)
                    datatype = nifti_dt_RGB;
                else if (image.Bands == 4)
                    datatype = nifti_dt_RGBA32;
                else
                {
                    throw new Exception("3 or 4 band colour images only");
                }
            }

            if (!(nim = nifti_make_new_nim(dims, datatype, false)))
                return -1;

            nim.dx = 1.0 / image.Xres;
            nim.dy = 1.0 / image.Yres;
            nim.dz = 1.0 / image.Yres;
            nim.xyz_units = NIFTI_UNITS_MM;

            string descrip = $"libvips-{VipsVersion}";
            nifti_copy_string(nim.descrip, sizeof(nim.descrip), descrip);

            // All other fields can stay at their default value.

            return 0;
        }

        public override int HeaderNifti(VipsImage image)
        {
            VipsObjectClass class_ = (VipsObjectClass)typeof(NiftiSave).GetBaseType();
            int[] dims = new int[8];
            int datatype;
            guint height;
            int i;

            // Most nifti images have this defaulted as 1.
            for (i = 0; i < 8; i++)
                dims[i] = 1;

            VipsNdimInfo info = new VipsNdimInfo();
            info.image = image;
            info.dims = dims;
            info.n = 0;
            if (ForeignNifti.Map(info, null))
                return -1;

            // page-height overrides ny if it makes sense. This might not be
            // correct :(
            dims[2] = VipsImage.GetPageHeight(image);

            // Multipage image?
            if (dims[2] < image.Ysize)
            {
                dims[0] = 3;
                dims[3] = image.Ysize / dims[2];
            }

            height = 1;
            for (i = 2; i < 8 && i < dims[0] + 1; i++)
                if (!guint_checked_mul(ref height, height, dims[i]))
                {
                    throw new Exception("dimension overflow");
                }
            if (image.Xsize != dims[1] || image.Ysize != height)
            {
                throw new Exception("bad image dimensions");
            }

            datatype = ForeignNifti.BandFmt2datatype(image.BandFmt);
            if (datatype == -1)
            {
                throw new Exception("unsupported libvips image type");
            }

            if (!(nim = nifti_make_new_nim(dims, datatype, false)))
                return -1;

            info.image = image;
            info.nim = nim;
            info.n = 0;
            if (ForeignNifti.Map(info, null))
                return -1;

            // Attach any ext blocks.
            if (image.Map((VipsImageMapFn)ForeignSaveNiftiExt, nim))
                return -1;

            return 0;
        }

        public override int Build(VipsObject obj)
        {
            VipsObjectClass class_ = (VipsObjectClass)typeof(NiftiSave).GetBaseType();
            if (base.Build(obj))
                return -1;

            // This could be an image (indirectly) from niftiload, or something
            // like OME_TIFF, which does not have all the "nifti-ndim" fields.
            //
            // If it doesn't look like a nifti, try to make a nifti header from
            // what we have.
            if (Image.Get(typeof(string), "nifti-ndim") != null)
            {
                if (HeaderNifti(nim, Image))
                    return -1;
            }
            else
            {
                if (HeaderVips(nim, Image))
                    return -1;
            }

            // set ext, plus other stuff

            if (nifti_set_filenames(nim, filename, false, true))
            {
                throw new Exception("unable to set nifti filename");
            }

            if (!(nim.data = Image.WriteToMemory(null)))
                return -1;

            // No return code!??!?!!
            nim.data = null;

            return 0;
        }
    }
}
```

Note that I've assumed the `VipsDotNet` namespace is available, and used it to access VIPS functionality. You may need to adjust this depending on your specific setup.

Also note that some parts of the code have been simplified or modified for C# compatibility. If you need exact equivalence with the original C code, please let me know and I'll do my best to accommodate your request.