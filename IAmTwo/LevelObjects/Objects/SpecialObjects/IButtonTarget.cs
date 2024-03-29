﻿using IAmTwo.Game;

namespace IAmTwo.LevelObjects.Objects.SpecialObjects
{
    public interface IButtonTarget : IPlaceableObject
    {
        void Activation(PressableButton button, SpecialActor trigger);
        void Collision(PressableButton button, SpecialActor trigger);
        void Reset(PressableButton button, SpecialActor trigger);
    }
}