// Program.cs — Timespinner WebAssembly host entry point.
//
// Key design principles:
//   • Use RunOneFrame() on every tick — never Tick() directly, so FNA properly
//     initializes gameTimer, DoInitialize(), and LoadContent() on frame 1.
//   • IsFixedTimeStep = false so FNA timing follows browser requestAnimationFrame.
//   • SynchronizeWithVerticalRetrace = false and SDL_GL_SetSwapInterval(0) prevent
//     the browser from decimating the refresh to 30 FPS.
//   • Main() returns cleanly after registering the per-frame loop callback with
//     simulate_infinite_loop = 0 to avoid Emscripten unwind exceptions.

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.JavaScript;
using Timespinner;

namespace ObjCRuntime
{
    [AttributeUsage(AttributeTargets.Method)]
    internal sealed class MonoPInvokeCallbackAttribute : Attribute
    {
        public MonoPInvokeCallbackAttribute(Type t) { }
    }
}

namespace Timespinner.Web
{
    public static partial class Program
    {
        private static TimespinnerGame s_game;
        private static bool s_firstFrameRendered = false;

        private delegate void MainLoopCallback();

        [JSImport("notifyGameReady", "main.js")]
        internal static partial void NotifyGameReady();

        [JSImport("setMainLoop", "main.js")]
        internal static partial void SetMainLoop([JSMarshalAs<JSType.Function>] Action cb);

        [DllImport("SDL2", EntryPoint = "emscripten_set_main_loop", CallingConvention = CallingConvention.Cdecl)]
        private static extern void emscripten_set_main_loop(
            MainLoopCallback cb, int fps, int simulateInfiniteLoop);

        [DllImport("__Native", EntryPoint = "emscripten_set_main_loop", CallingConvention = CallingConvention.Cdecl)]
        private static extern void emscripten_set_main_loop_native(
            MainLoopCallback cb, int fps, int simulateInfiniteLoop);

        [DllImport("SDL2", EntryPoint = "SDL_GL_SetSwapInterval", CallingConvention = CallingConvention.Cdecl)]
        private static extern int SDL_GL_SetSwapInterval(int interval);

        [ObjCRuntime.MonoPInvokeCallback(typeof(MainLoopCallback))]
        private static void LoopStep()
        {
            try
            {
                s_game?.RunOneFrame();
                if (!s_firstFrameRendered)
                {
                    s_firstFrameRendered = true;
                    Console.WriteLine("[Timespinner.Web] First frame successfully rendered!");
                    try { NotifyGameReady(); } catch { }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[Timespinner FATAL FRAME ERROR] " + ex);
                if (ex.InnerException != null)
                {
                    Console.WriteLine("[Timespinner INNER EXCEPTION] " + ex.InnerException);
                }
            }
        }

        public static void Main(string[] args)
        {
            Console.WriteLine("[Timespinner.Web] Main() started.");

            // Backend environment setup
            Environment.SetEnvironmentVariable("FNA_PLATFORM_BACKEND", "SDL2");
            Environment.SetEnvironmentVariable("FNA_GRAPHICS_SYNCHRONIZE_WITH_VERTICALRETRACE", "0");
            Environment.SetEnvironmentVariable("FNA_GAMEPAD_NUM_GAMEPADS", "4");
            Environment.SetEnvironmentVariable("XDG_DATA_HOME", "/save");

            try
            {
                Directory.CreateDirectory("/save");
                Directory.CreateDirectory("/save/Timespinner");
            }
            catch (Exception ex)
            {
                Console.WriteLine("[Timespinner.Web] Failed to create /save directory: " + ex.Message);
            }

            try
            {
                Console.WriteLine("[Timespinner.Web] Instantiating PlatformHelper and TimespinnerGame...");
                PlatformHelper platformHelper = new PlatformHelper();
                s_game = new TimespinnerGame(platformHelper);

                // Timing & Presentation Settings
                s_game.IsFixedTimeStep = false;
                s_game.TargetElapsedTime = TimeSpan.FromTicks(166667L);
                s_game.Graphics.PreferredBackBufferWidth = 1280;
                s_game.Graphics.PreferredBackBufferHeight = 720;
                s_game.Graphics.IsFullScreen = false;
                s_game.Graphics.SynchronizeWithVerticalRetrace = false;

                // Disable VSync at SDL level
                try { SDL_GL_SetSwapInterval(0); } catch { }

                bool loopSet = false;

                // Primary: JS requestAnimationFrame loop
                try
                {
                    SetMainLoop(LoopStep);
                    loopSet = true;
                    Console.WriteLine("[Timespinner.Web] JS SetMainLoop (rAF) registered successfully.");
                }
                catch (Exception jsEx)
                {
                    Console.WriteLine("[Timespinner.Web WARN] JS SetMainLoop failed: " + jsEx.Message);
                }

                // Fallback 1: SDL2 native
                if (!loopSet)
                {
                    try
                    {
                        emscripten_set_main_loop(LoopStep, 0, 0);
                        loopSet = true;
                        Console.WriteLine("[Timespinner.Web] emscripten_set_main_loop (SDL2) registered.");
                    }
                    catch (Exception pEx)
                    {
                        Console.WriteLine("[Timespinner.Web WARN] SDL2 emscripten_set_main_loop failed: " + pEx.Message);
                    }
                }

                // Fallback 2: __Native
                if (!loopSet)
                {
                    try
                    {
                        emscripten_set_main_loop_native(LoopStep, 0, 0);
                        loopSet = true;
                        Console.WriteLine("[Timespinner.Web] emscripten_set_main_loop (__Native) registered.");
                    }
                    catch (Exception nEx)
                    {
                        Console.WriteLine("[Timespinner.Web WARN] __Native emscripten_set_main_loop failed: " + nEx.Message);
                    }
                }

                Console.WriteLine("[Timespinner.Web] Main loop setup complete. Returning from Main().");
            }
            catch (Exception ex)
            {
                Console.WriteLine("[Timespinner FATAL ERROR in Main] " + ex);
                if (ex.InnerException != null)
                {
                    Console.WriteLine("[Timespinner INNER EXCEPTION] " + ex.InnerException);
                }
            }
        }
    }
}
