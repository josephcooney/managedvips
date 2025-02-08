Here is the converted C# code:

```csharp
using System;
using VipsDotNet;

// vips__correl:
// @ref: reference image
// @sec: secondary image
// @xref: position in reference image
// @yref: position in reference image
// @xsec: position in secondary image
// @ysec: position in secondary image
// @hwindowsize: half window size
// @hsearchsize: half search size
// @correlation: return detected correlation
// @x: return found position
// @y: return found position

public static int VipsCorrel(VipsImage ref, VipsImage sec,
	int xref, int yref, int xsec, int ysec,
	int hwindowsize, int hsearchsize,
	double[] correlation, ref int x, ref int y)
{
	VipsImage surface = new VipsImage();
	VipsImage[] t = (VipsImage[])VipsObject.LocalArray(surface, 5);

	VipsRect refr, secr;
	VipsRect winr, srhr;
	VipsRect wincr, srhcr;

	// Find position of window and search area, and clip against image size.
	refr.Left = 0;
	refr.Top = 0;
	refr.Width = ref.Xsize;
	refr.Height = ref.Ysize;
	winr.Left = xref - hwindowsize;
	winr.Top = yref - hwindowsize;
	winr.Width = hwindowsize * 2 + 1;
	winr.Height = hwindowsize * 2 + 1;
	VipsRect.Intersect(refr, winr, wincr);

	secr.Left = 0;
	secr.Top = 0;
	secr.Width = sec.Xsize;
	secr.Height = sec.Ysize;
	srhr.Left = xsec - hsearchsize;
	srhr.Top = ysec - hsearchsize;
	srhr.Width = hsearchsize * 2 + 1;
	srhr.Height = hsearchsize * 2 + 1;
	VipsRect.Intersect(secr, srhr, srhcr);

	// Extract window and search area.
	if (VipsExtractArea(ref, t[0],
			wincr.Left, wincr.Top, wincr.Width, wincr.Height,
			null) ||
		VipsExtractArea(sec, t[1],
			srhcr.Left, srhcr.Top, srhcr.Width, srhcr.Height,
			null))
	{
		GObject.Unref(surface);
		return -1;
	}
	ref = t[0];
	sec = t[1];

	// Make sure we have just one band. From vips_*mosaic() we will, but
	// from vips_match() etc. we may not.
	if (ref.Bands != 1)
	{
		if (VipsExtractBand(ref, t[2], 0, null))
		{
			GObject.Unref(surface);
			return -1;
		}
		ref = t[2];
	}
	if (sec.Bands != 1)
	{
		if (VipsExtractBand(sec, t[3], 0, null))
		{
			GObject.Unref(surface);
			return -1;
		}
		sec = t[3];
	}

	// Search!
	if (VipsSpcor(sec, ref, t[4], null))
	{
		GObject.Unref(surface);
		return -1;
	}

	// Find maximum of correlation surface.
	if (VipsMax(t[4], correlation, "x", x, "y", y, null))
	{
		GObject.Unref(surface);
		return -1;
	}
	GObject.Unref(surface);

	// Translate back to position within sec.
	x += srhcr.Left;
	y += srhcr.Top;

	return 0;
}

public static int VipsChkpair(VipsImage ref, VipsImage sec, TiePoints points)
{
	int i;
	int x, y;
	double correlation;

	const int hcor = points.HalfCorsize;
	const int harea = points.HalfAreasize;

	// Check images.
	if (VipsImage.WioInput(ref) || VipsImage.WioInput(sec))
		return -1;
	if (ref.Bands != sec.Bands || ref.BandFmt != sec.BandFmt ||
		ref.Coding != sec.Coding)
	{
		VipsError("vips_chkpair", "%s", "inputs incompatible");
		return -1;
	}
	if (ref.Bands != 1 || ref.BandFmt != VipsFormat.UChar)
	{
		VipsError("vips_chkpair", "%s", "help!");
		return -1;
	}

	for (i = 0; i < points.Nopoints; i++)
	{
		// Find correlation point.
		if (VipsCorrel(ref, sec,
				points.XReference[i], points.YReference[i],
				points.XReference[i], points.YReference[i],
				hcor, harea,
				ref double[] correlation, ref int x, ref int y))
			return -1;

		// And note in x_secondary.
		points.XSecondary[i] = x;
		points.YSecondary[i] = y;
		points.Correlation[i] = correlation;

		// Note each dx, dy too.
		points.Dx[i] =
			points.XSecondary[i] - points.XReference[i];
		points.Dy[i] =
			points.YSecondary[i] - points.YReference[i];
	}

	return 0;
}
```