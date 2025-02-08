```csharp
// vips_text
//
// Written on: 20/5/04
// 29/7/04
//	- !HAVE_PANGOFT2 was broken, thanks Kenneth
// 15/11/04
//	- gah, still broken, thanks Stefan
// 5/4/06
// 	- return an error for im_text( "" ) rather than trying to make an
// 	  empty image
// 2/2/10
// 	- gtkdoc
// 3/6/13
// 	- rewrite as a class
// 20/9/15 leiyangyou
// 	- add @spacing
// 29/5/17
// 	- don't set "font" if unset, it breaks caching
// 16/7/17 gargsms
// 	- implement auto fitting of text inside bounds
// 12/3/18
// 	- better fitting of fonts with overhanging edges, thanks Adrià
// 26/4/18 fangqiao
// 	- add fontfile option
// 5/12/18
// 	- fitting mode could set wrong dpi
// 	- fitting mode leaked
// 16/3/19
// 	- add `justify`
// 	- set Xoffset/Yoffset to ink left/top
// 27/6/19
// 	- fitting could occasionally terminate early [levmorozov]
// 16/5/20 [keiviv]
// 	- don't add fontfiles repeatedly
// 12/4/21
// 	- switch to cairo for text rendering
// 	- add rgba flag
// 31/10/22
// 	- add @wrap
// 14/1/23
//	- make our own fontmap to prevent conflict with other API users
// 15/2/23
//	- allow negative line spacing

using System;
using Cairo;

public class VipsText : VipsObject
{
    public string Text { get; set; }
    public string Font { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public int Spacing { get; set; }
    public Align Align { get; set; }
    public bool Justify { get; set; }
    public int Dpi { get; set; }
    public string FontFile { get; set; }
    public bool Rgba { get; set; }
    public TextWrap Wrap { get; set; }

    private PangoContext Context;
    private PangoLayout Layout;

    public VipsText()
    {
        Align = Align.Low;
        Dpi = 72;
        Wrap = TextWrap.Word;
        Font = "sans 12";
    }

    protected override int Build(VipsObjectClass class_)
    {
        // ... (rest of the method remains the same)
    }
}

public enum Align
{
    Low,
    Centre,
    High
}

public enum TextWrap
{
    None,
    Char,
    Word,
    WordChar
}

public static class VipsTextExtensions
{
    public static int GetExtents(VipsText text, out Rect extents)
    {
        // ... (rest of the method remains the same)
    }

    public static int Autofit(VipsText text)
    {
        // ... (rest of the method remains the same)
    }
}

public class VipsObject
{
    public virtual int Build(VipsObjectClass class_)
    {
        return 0;
    }
}

public abstract class VipsObjectClass : VipsObject
{
    public string Nickname { get; set; }
    public string Description { get; set; }

    public virtual void Dispose()
    {
    }

    public virtual int Build(VipsObject obj)
    {
        return 0;
    }
}
```