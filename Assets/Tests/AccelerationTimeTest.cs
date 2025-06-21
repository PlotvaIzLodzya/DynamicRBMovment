using NUnit.Framework;

namespace Tests
{
    public abstract class AccelerationTimeTest : CharacterAccelerationTest
    {
        protected override void AssertAcceleration(CharacterControl character, float elapsedTime)
        {
            Assert.AreEqual(
                elapsedTime,
                character.AccelerationTime,
                0.05f);
        }
    }
}