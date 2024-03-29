﻿using IAmTwo.Resources;
using OpenTK;
using OpenTK.Graphics;
using SM.Base.Controls;
using SM.Base.Window;
using SM2D.Controls;
using SM2D.Drawing;
using SM2D.Scene;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SM.Base.Drawing.Text;
using SM.Base;

namespace IAmTwo.Menu
{
    class CheckBox : ItemCollection
    {
        public static readonly Vector2 Size = new Vector2(25);

        private ulong lastUpdate;
        private DrawText _check;
        private DrawObject2D _border;

        public event Action Checking;
        public bool Checked { get; private set; }

        public CheckBox()
        {
            _border = new DrawObject2D()
            {
                Mesh = Models.CreateBackgroundPolygon(Size, 5),
                ForcedMeshType = OpenTK.Graphics.OpenGL4.PrimitiveType.LineLoop
            };
            _border.Transform.Size.Set(1);
            
            _check = new DrawText(Fonts.FontAwesome, "\uf00c")
            {
                Origin = TextOrigin.Center
            };
            _check.GenerateMatrixes();

            _check.Transform.Position.Set(-5, 0);

            Add(_border, _check);
        }

        public override void Update(UpdateContext context)
        {
            base.Update(context);

            if (lastUpdate == SMRenderer.CurrentFrame) return;
            lastUpdate = SMRenderer.CurrentFrame;

            if (_border.LastDrawingCamera == null) return;
            
            Vector2 mousePos = Mouse2D.InWorld(_border.LastDrawingCamera as Camera);
            if (Mouse2D.MouseOver(mousePos, _border))
            {
                _border.Color = Color4.AliceBlue;

                if (Controller.Actor.Get<bool>("g_click"))
                {
                    Checked = !Checked;
                    _check.RenderActive = Checked;

                    Checking?.Invoke();
                }
            }
            else _border.Color = Color4.Blue;
        }

        public void SetChecked(bool value)
        {
            Checked = value;
            _check.RenderActive = value;
        }
    }
}
