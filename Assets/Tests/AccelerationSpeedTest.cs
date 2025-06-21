using NUnit.Framework;
using UnityEngine;

namespace Tests
{
    public abstract class AccelerationSpeedTest : CharacterAccelerationTest
    {
        protected override void AssertAcceleration(CharacterControl character, float elapsedTime)
        {
            Assert.AreEqual(
                character.MaxSpeed,
                Mathf.Abs(character.Speed),
                0.5f);
        }
    }
}