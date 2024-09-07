using System;
using System.Runtime.InteropServices;
using FreeTypeSharp;

namespace tinybuffer
{
    public unsafe class FontRenderer : IDisposable
    {
        private FreeTypeLibrary library;
        // private  face;
        FT_FaceRec_* fontface;
        private IntPtr fontPathPtr;

        public FontRenderer(string fontPath)
        {
            FT_FaceRec_* face;
            library = new FreeTypeLibrary();
            fontPathPtr = Marshal.StringToHGlobalAnsi(fontPath);
            FT.FT_New_Face(library.Native, (byte*)fontPathPtr, 0, &face);
            fontface = face;
        }

        public void DrawText(byte[] buffer, int width, int height, string text, int size, int x, int y, byte r, byte g, byte b)
        {
            FT.FT_Set_Char_Size(fontface, 0, size * 64, 72, 0);

            int startX = x;

            foreach (char c in text)
            {
                FT.FT_Load_Char(fontface, c, FT_LOAD.FT_LOAD_RENDER);

                var bitmap = fontface->glyph->bitmap;

                int bitmapTop = fontface->glyph->bitmap_top;
                int bitmapLeft = fontface->glyph->bitmap_left;

                for (int row = 0; row < bitmap.rows; row++)
                {
                    for (int col = 0; col < bitmap.width; col++)
                    {
                        byte pixelValue = (byte) (bitmap.buffer[row * bitmap.pitch + col]);
                        int bufferIndex = ((y - bitmapTop + row) * width + (startX + bitmapLeft + col)) * 3;
                        if (bufferIndex >= 0 && bufferIndex < buffer.Length - 3)
                        {
                            if (pixelValue == 255)
                            {
                                buffer[bufferIndex] = r;
                                buffer[bufferIndex + 1] = g;
                                buffer[bufferIndex + 2] = b;
                            } else if (pixelValue != 0)
                            {
                                float alpha = pixelValue / 255.0f;
                                float invAlpha = 1.0f - alpha;
                                int oldR = buffer[bufferIndex];
                                byte blendedR = (byte)((oldR * invAlpha) + (r * alpha));
                                int oldG = buffer[bufferIndex + 1];
                                byte blendedG = (byte)((oldG * invAlpha) + (g * alpha));
                                int oldB = buffer[bufferIndex + 2];
                                byte blendedB = (byte)((oldB * invAlpha) + (b * alpha));
                                buffer[bufferIndex] = blendedR;
                                buffer[bufferIndex + 1] = blendedG;
                                buffer[bufferIndex + 2] = blendedB;
                            }
                            
                        }
                    }
                }

                startX += (int)(fontface->glyph->advance.x >> 6);
            }
        }

        public void Dispose()
        {
            FT.FT_Done_Face(fontface);
            library.Dispose();
            Marshal.FreeHGlobal(fontPathPtr);
        }
    }
}
