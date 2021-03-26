﻿using System;
using IAmTwo.LevelObjects;
using IAmTwo.Shaders;
using OpenTK;
using SM.Base.Window;
using SM2D;
using SM2D.Types;

namespace IAmTwo
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Transformation.ZIndexPercision = 50;

            GLWindow window = new GLWindow(1600,900, "I am two - DevBuild", WindowFlags.Window, VSyncMode.Off);
            window.ApplySetup(new Window2DSetup());
            window.CursorVisible = false;

            window.TargetUpdateFrequency = 60;
            window.SetRenderPipeline(new GameRenderPipeline());
            window.SetScene(MainMenu.Menu);
            //window.SetScene(new GameScene(new LevelConstructor() { Size = 650 }));
            window.Run();
        }
    }
}
