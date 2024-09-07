using SDL2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tinybuffer;

namespace tinydaw
{
    public class TinyDaw : Window
    {
        public TinyDaw() : base("tinydaw", 640, 360, true, false, false)
        {
            
        }
        FontRenderer fr;
        public override void OnClose()
        {

        }

        public override void OnEvent(SDL.SDL_Event e)
        {

        }
        public void SetPixel(int x, int y, byte r, byte g, byte b)
        {
            int pixelAddress = (y * width + x) * 3;
            if (buffer.Length <= pixelAddress + 2)
            {
                return;
            }
            buffer[pixelAddress] = r;
            buffer[pixelAddress + 1] = g;
            buffer[pixelAddress + 2] = b;
        }
        int frames;
        int fps;
        int frame = 0;
        DateTimeOffset last = DateTimeOffset.Now;
        public override void Render()
        {
            if (frame == 0)
            {
                fr = new FontRenderer("selawik.ttf");
            }
            frame++;
            frames++;
            if (DateTimeOffset.Now.ToUnixTimeMilliseconds() - last.ToUnixTimeMilliseconds() >= 1000l)
            {
                fps = frames;
                frames = 0;
                Console.WriteLine(fps);
                last = DateTimeOffset.Now;
            }
            if (!requiresRedraw)
            {
                return;
            }
            for (int x = 0; x < 50; x++)
            {
                for (int y = 0; y < 50; y++)
                {
                    SetPixel(x, y, 0, 255, 255);
                }
            }
            fr.DrawText(buffer, width, height, "tinydaw", 50, 100, 100, 255, 0, 0);
        }
    }
}
