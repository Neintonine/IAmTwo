﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using IAmTwo.Game;
using IAmTwo.Shaders;
using OpenTK;
using SM.Base.Windows;
using SM.Game.Controls;
using SM2D;
using SM2D.Pipelines;

namespace IAmTwo
{
    class Program
    {
        static void Main(string[] args)
        {
            GLWindow window = new GLWindow(1600,900, "I am two - DevBuild", GameWindowFlags.Default);
            window.ApplySetup(new Window2DSetup()
            {
                WorldScale = new Vector2(0, 650)
            });
            window.SetRenderPipeline(new GameRenderPipeline());
            window.SetScene(new GameScene());
            window.Run();
        }
    }
}
