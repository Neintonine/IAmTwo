﻿using System;
using System.Collections.Generic;
using System.Linq;
using IAmTwo.Game;
using IAmTwo.LevelObjects;
using IAmTwo.LevelObjects.Objects;
using IAmTwo.Menu;
using IAmTwo.Resources;
using KWEngine.Hitbox;
using OpenTK;
using OpenTK.Input;
using SM.Base.Scene;
using SM.Base.Window;
using SM2D.Controls;
using SM2D.Drawing;
using SM2D.Scene;
using SM2D.Types;
using Keyboard = SM.Base.Controls.Keyboard;
using Mouse = SM.Base.Controls.Mouse;

namespace IAmTwo.LevelEditor
{
    public class PropertyObjectControl : ItemCollection
    {
        private Button connectButton;
        private Button _resolveCollisions;

        private bool _connectMode = false;
        private List<IPlaceableObject> _connectSelectList;
        private IConnectable _connectAsked;

        private IPlaceableObject _object;

        public PropertyObjectControl()
        {
        }

        public override void Update(UpdateContext context)
        {
            base.Update(context);
        }

        public void Reload(IPlaceableObject obj)
        {
            this.Clear();
            _object = obj;

            DrawText txt = new DrawText(Fonts.Button, _object.ToString());

            Add(txt);
            float yPos = 50;

            _resolveCollisions = new Button("Resolve Collisions\n [X]", 200);
            _resolveCollisions.Transform.Position.Set(20, -yPos);
            _resolveCollisions.Click += () => ResolveCollisions(obj);
            Add(_resolveCollisions);

            yPos += 50;

            if (_object is IConnectable connectable)
            {
                Tuple<IShowTransformItem<Transformation>, float> d = GenerateConnectable(connectable);
                d.Item1.Transform.Position.Set(0, -yPos);
                Add(d.Item1);

                yPos += d.Item2 + 10f;
            }

            if (_object is IPlayerDependent playerDependent)
            {
                DrawText t = new DrawText(Fonts.Button, "Mirror: ");
                t.Transform.Position.Set(0, -yPos);

                CheckBox box = new CheckBox();
                box.SetChecked(playerDependent.Mirror);
                box.Checking += () => playerDependent.Mirror = box.Checked;
                box.Transform.Position.Set(100, -yPos);
                Add(t, box);

                yPos += 25 + 10;
            }
        }

        public void ExecuteKeybinds()
        {
            if (_connectMode)
            {
                if (Controller.Actor.Get<bool>("g_click"))
                {
                    if (Mouse2D.MouseOver(Mouse2D.InWorld(LevelEditor.CurrentEditor.Camera),
                        out IPlaceableObject clicked,
                        _connectSelectList))
                    {
                        if (_connectAsked.ConnectedTo != null) _connectAsked.Disconnect();
                        if (clicked is IConnectable c && c.ConnectedTo != null) c.Disconnect();

                        _connectAsked.Connect(clicked);
                        ExitConnectMode();
                    }
                }

                if (Keyboard.IsDown(Key.Escape, true))
                {
                    ExitConnectMode();
                }
            }
            else if (Keyboard.IsDown(Key.C, true))
            {
                if (_object is IConnectable) connectButton.TriggerClick();
            } else if (Keyboard.IsDown(Key.X, true) && !Keyboard.IsDown(Key.ControlLeft))
            {
                _resolveCollisions.TriggerClick();
            }
        }

        private void ResolveCollisions(IPlaceableObject o)
        {
            o.Hitbox.Update(o.Transform.GetMatrix(), o.Transform.Rotation);
            foreach (Hitbox _hitbox in LevelEditor.CurrentEditor.Hitboxes.ToArray())
            {
                if (_hitbox == o.Hitbox) continue;

                _hitbox.Update(_hitbox.PhysicsObject.Transform.GetMatrix(), _hitbox.PhysicsObject.Transform.Rotation);

                Vector2 mtv;
                if (Hitbox.TestIntersection(o.Hitbox, _hitbox, out mtv))
                {
                    o.Transform.Position.Add(mtv);
                }
            }
        }

        private Tuple<IShowTransformItem<Transformation>, float> GenerateConnectable(IConnectable obj)
        {
            if (obj.ConnectedTo == null)
            {
                ItemCollection nConnected = new ItemCollection();

                DrawText header = new DrawText(Fonts.Text, "Not connected");
                connectButton = new Button("Connect [C]", 100);
                connectButton.Transform.Position.Set(10, -30);
                connectButton.Click += () => EnterConnectMode(obj);

                nConnected.Add(header, connectButton);

                return new Tuple<IShowTransformItem<Transformation>, float>(nConnected, 60f);
            }

            return new Tuple<IShowTransformItem<Transformation>, float>(new DrawText(Fonts.Text, "Connected to:\n" + obj.ConnectedTo), 40f);
        }

        private void EnterConnectMode(IConnectable connectable)
        {
            LevelEditor.CurrentEditor.DisableInput = true;
            LevelEditor.CurrentEditor.Background.Active = false;
            LevelEditor.CurrentEditor.HUD.RenderActive = false;

            _connectMode = true;
            _connectAsked = connectable;
            _connectSelectList = new List<IPlaceableObject>();
            foreach (IShowItem item in LevelEditor.CurrentEditor.Objects)
            {
                if (item is GameObject obj)
                {
                    if (LevelEditor.CurrentEditor.Walls.Contains(obj)) continue;
                }

                if (item == connectable) continue;


                if ((item.GetType().IsAssignableFrom(connectable.ConnectTo) || connectable.ConnectTo.IsInterface && item.GetType().GetInterfaces().Contains(connectable.ConnectTo)) && item is IPlaceableObject placeable)
                {
                    _connectSelectList.Add(placeable);
                    continue;
                }
                
                item.Active = false;
            }
        }

        private void ExitConnectMode()
        {
            LevelEditor.CurrentEditor.DisableInput = false;
            LevelEditor.CurrentEditor.Background.Active = true;
            LevelEditor.CurrentEditor.HUD.RenderActive = true;

            _connectMode = false;
            _connectSelectList = null;

            foreach (IShowItem item in LevelEditor.CurrentEditor.Objects)
            {
                item.Active = true;
            }

            Reload(_object);
        }
    }
}