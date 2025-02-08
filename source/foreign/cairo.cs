```csharp
// vips__premultiplied_bgra2rgba
void vips__premultiplied_bgra2rgba(uint[] p, int n)
{
    for (int x = 0; x < n; x++)
    {
        uint bgra = BitConverter.ToUInt32(BitConverter.GetBytes(p[x]), 0);
        byte a = (byte)(bgra & 0xff);

        uint rgba;

        if (a == 0 || a == 255)
            rgba =
                ((bgra & 0x00ff00ff) |
                 (bgra & 0x0000ff00) << 16 |
                 (bgra & 0xff000000) >> 16);
        else
            /* Undo premultiplication.
             */
            rgba =
                (((255 * ((bgra >> 8) & 0xff)) / a) << 24) |
                (((255 * ((bgra >> 16) & 0xff)) / a) << 16) |
                (((255 * ((bgra >> 24) & 0xff)) / a) << 8) |
                a;

        p[x] = BitConverter.ToUInt32(BitConverter.GetBytes(rgba), 0);
    }
}

// vips__rgba2bgra_premultiplied
void vips__rgba2bgra_premultiplied(uint[] p, int n)
{
    for (int x = 0; x < n; x++)
    {
        uint rgba = BitConverter.ToUInt32(BitConverter.GetBytes(p[x]), 0);
        byte a = (byte)(rgba & 0xff);

        uint bgra;

        if (a == 0)
            bgra = 0;
        else if (a == 255)
            bgra =
                ((rgba & 0x00ff00ff) |
                 (rgba & 0x0000ff00) << 16 |
                 (rgba & 0xff000000) >> 16);
        else
        {
            int r = (int)((rgba >> 24) & 0xff);
            int g = (int)((rgba >> 16) & 0xff);
            int b = (int)((rgba >> 8) & 0xff);

            r = ((r * a + 128) >> 8);
            g = ((g * a + 128) >> 8);
            b = ((b * a + 128) >> 8);

            bgra = (b << 24) | (g << 16) | (r << 8) | a;
        }

        p[x] = BitConverter.ToUInt32(BitConverter.GetBytes(bgra), 0);
    }
}

// vips__bgra2rgba
void vips__bgra2rgba(uint[] p, int n)
{
    for (int x = 0; x < n; x++)
    {
        uint bgra = BitConverter.ToUInt32(BitConverter.GetBytes(p[x]), 0);

        uint rgba;

        /* Leave G and A, swap R and B.
         */
        rgba =
            ((bgra & 0x00ff00ff) |
             (bgra & 0x0000ff00) << 16 |
             (bgra & 0xff000000) >> 16);

        p[x] = BitConverter.ToUInt32(BitConverter.GetBytes(rgba), 0);
    }
}
```