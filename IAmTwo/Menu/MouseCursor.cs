﻿using IAmTwo.Resources;
using OpenTK.Graphics.OpenGL4;
using SM.Base.Controls;
using SM.Base.Windows;
using SM2D.Controls;
using SM2D.Drawing;
using SM2D.Scene;

namespace IAmTwo.Menu
{
    public class MouseCursor : ItemCollection
    {
        public static MouseCursor Cursor = new MouseCursor();

        private MouseCursor()
        {
            Transform.Size.Set(.5f);

            DrawObject2D cursor = new DrawObject2D
            {
                Material = {Blending = true},
                Texture = Resource.RequestTexture(@".\Resources\Cursor.png", TextureMinFilter.Nearest,
                    TextureWrapMode.ClampToBorder),
            };
            cursor.Transform.ApplyTextureSize(cursor.Texture);

            cursor.Transform.Position.Set(cursor.Texture.Width / 2f, -cursor.Texture.Height / 2f);


            Add(cursor);
        }

        public override void Draw(DrawContext context)
        {
            Transform.Position.Set(Mouse2D.InWorld(context.UseCamera as Camera));

            base.Draw(context);
        }
    }
}