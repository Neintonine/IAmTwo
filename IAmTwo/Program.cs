﻿using System;
using System.IO;
using IAmTwo.Game;
using IAmTwo.LevelObjects;
using IAmTwo.Shaders;
using OpenTK;
using SM.Base.Window;
using SM2D;
using SM2D.Types;
using System.Linq;
using IAmTwo.LevelObjects.Objects;
using SM.Utils.Controls;
using SM.Base;
using SM.Base.Utility;

namespace IAmTwo
{
    class Program   
    {
        [STAThread]
        static void Main(string[] args)
        {
            Log.SetLogFile("log.dat");

            Controller.Actor = GameKeybindActor.CreateKeyboardActor();
            Transformation.ZIndexPercision = 50;
            GameController.GlobalDeadband = .5f;

            LevelSet.Load();

            GLWindow window = new GLWindow(1600, 900, "I am two - DevBuild", WindowFlags.Window, VSyncMode.Off)
            {
                Icon = new System.Drawing.Icon(AssemblyUtility.GetAssemblyStream("IAmTwo.Resources.icon.ico"))
            };
            window.ApplySetup(new Window2DSetup());
            window.UpdateFrame += Controller.MouseCursor;
            window.Loaded += (a) => UserSettings.Load();


            window.TargetUpdateFrequency = 60;
            window.SetRenderPipeline(new GameRenderPipeline());

            if (args.Any(a => a == "--testLevel"))
                using (FileStream stream = new FileStream(@".\Levels\_test.iatl", FileMode.Open))
                {
                    window.SetScene(new PlayScene(LevelConstructor.Load(stream)));
                }
            else window.SetScene(new SplashScreen());
            //window.SetScene(new CreditsScene(LevelSet.LevelSets.First().Value[0]));
            //window.SetScene(new GameScene(new LevelConstructor() { Size = 650 }));
            window.RunFixedUpdate(100);
            window.Run();
        }
    }
}
