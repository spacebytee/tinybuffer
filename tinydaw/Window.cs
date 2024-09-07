using SDL2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static SDL2.SDL;

namespace tinybuffer
{
    public abstract class Window
    {
        private string name;
        public int width;
        public int height;
        private bool resizable;
        protected byte[] buffer;
        public nint sdlWindow;
        public nint renderer;
        private byte defaultR = 255;
        private byte defaultG = 255;
        private byte defaultB = 255;
        protected bool requiresRedraw = true;
        private bool running = true;
        private bool vsync;
        private bool accelerated;
        public Window(string name, int width, int height, bool resizable, bool vsync, bool accelerated)
        {
            this.name = name;
            this.width = width; 
            this.height = height;   
            this.resizable = resizable;
            this.vsync = vsync;
            this.accelerated = accelerated;
            if (!Init())
            {
                throw new Exception("Failed to init.");
            }
            while (running)
            {
                PollEvents();
                Render();
                PostRender();
            }
            SDL.SDL_DestroyRenderer(renderer);
            SDL.SDL_DestroyWindow(sdlWindow);
            SDL.SDL_Quit();
            OnClose();
        }
        private void PollEvents()
        {
            while (SDL.SDL_PollEvent(out SDL.SDL_Event e) == 1)
            {
                switch (e.type)
                {
                    case SDL.SDL_EventType.SDL_QUIT:
                        OnEvent(e);
                        running = false;
                        break;
                    case SDL.SDL_EventType.SDL_WINDOWEVENT:
                        OnEvent(e);
                        if (e.window.windowEvent == SDL.SDL_WindowEventID.SDL_WINDOWEVENT_RESIZED)
                        {
                            width = e.window.data1;
                            height = e.window.data2;
                            InitBuffer();
                        }
                        break;
                    default:
                        OnEvent(e);
                        break;
                }
            }
        }
        public abstract void OnEvent(SDL_Event e);
        public abstract void Render();
        public abstract void OnClose();
        IntPtr texture = IntPtr.Zero;
        GCHandle handle;
        IntPtr pixelPtr;
        private void InitializeTexture()
        {
            texture = SDL.SDL_CreateTexture(renderer, SDL.SDL_PIXELFORMAT_RGB24, 0, width, height);
            handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            pixelPtr = handle.AddrOfPinnedObject();
        }
        private void PostRender()
        {
            SDL.SDL_UpdateTexture(texture, IntPtr.Zero, pixelPtr, width * 3);
            SDL.SDL_RenderCopy(renderer, texture, IntPtr.Zero, IntPtr.Zero);
            SDL.SDL_RenderPresent(renderer);
            requiresRedraw = false;
        }
        public void SetDefaults(byte r, byte g, byte b)
        {
            this.defaultR = r;
            this.defaultG = g;
            this.defaultB = b;
        }
        public void InitBuffer()
        {
            buffer = new byte[width * height * 3];
            for (int i = 0; i < buffer.Length; i += 3)
            {
                buffer[i] = defaultR; 
                buffer[i + 1] = defaultG;
                buffer[i + 2] = defaultB;
            }
            if (texture != IntPtr.Zero)
            {
                SDL.SDL_DestroyTexture(texture);
            }
            InitializeTexture();
            requiresRedraw = true;
        }
        public bool Init()
        {
            if (SDL.SDL_Init(SDL.SDL_INIT_VIDEO) < 0)
            {
                return false;
            }
            SDL.SDL_WindowFlags flag = SDL.SDL_WindowFlags.SDL_WINDOW_SHOWN;
            if (resizable)
            {
                flag = flag | SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE;
            }
            sdlWindow = SDL.SDL_CreateWindow(name, SDL.SDL_WINDOWPOS_UNDEFINED, SDL.SDL_WINDOWPOS_UNDEFINED, width, height, flag);

            if (sdlWindow == IntPtr.Zero)
            {
                return false;
            }
            SDL.SDL_RendererFlags flags = SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED;
            if (!accelerated)
            {
                flags = SDL.SDL_RendererFlags.SDL_RENDERER_SOFTWARE;
            }
            if (vsync)
            {
                flags = flags | SDL.SDL_RendererFlags.SDL_RENDERER_PRESENTVSYNC;
            }
            renderer = SDL.SDL_CreateRenderer(sdlWindow, -1, flags);
            if (renderer == IntPtr.Zero)
            {
                return false;
            }

            if (SDL_image.IMG_Init(SDL_image.IMG_InitFlags.IMG_INIT_PNG) == 0)
            {
                return false;
            }
            InitBuffer();
            return true;
        }
    }
}