using System;
using System.Collections.Generic;
using KorpiEngine.Core.ECS;
using NUnit.Framework;
using OpenTK.Mathematics;

namespace Core.Tests.ECS;

[TestFixture]
[TestOf(typeof(TransformComponent))]
public class TransformComponentTest
{
    #region POSITION ROTATION SCALE

    [Test]
    public void Position_Set_Get_ReturnsCorrectValue()
    {
        TransformComponent component = new();
        Vector3 expectedPosition = new(1, 2, 3);

        component.Position = expectedPosition;

        Assert.That(component.Position, Is.EqualTo(expectedPosition));
    }


    [Test]
    public void Rotation_Set_Get_ReturnsCorrectValue()
    {
        TransformComponent component = new();
        Quaternion expectedRotation = Quaternion.FromEulerAngles(new Vector3(0, 90, 45));

        component.Rotation = expectedRotation;
        
        Assert.That(component.Rotation.ToEulerAngles(), Is.EqualTo(expectedRotation.ToEulerAngles()));
    }


    [Test]
    public void Scale_Set_Get_ReturnsCorrectValue()
    {
        TransformComponent component = new();
        Vector3 expectedScale = new(1, 2, 3);

        component.Scale = expectedScale;
        
        Assert.That(component.Scale, Is.EqualTo(expectedScale));
    }

    #endregion


    #region EULERS

    [Test]
    public void EulerAngles_Set_Get_ReturnsCorrectValue()
    {
        TransformComponent component = new();
        Vector3 expectedEulerAngles = new(35, 125, 45);
        Quaternion expectedRotation = Quaternion.FromEulerAngles(expectedEulerAngles * (MathF.PI / 180.0f));

        component.EulerAngles = expectedEulerAngles;
    
        Assert.That(component.Rotation, Is.EqualTo(expectedRotation).Using(new QuaternionComparer()));
    }
    
    private class QuaternionComparer : IEqualityComparer<Quaternion>
    {
        public bool Equals(Quaternion x, Quaternion y)
        {
            return QuaternionDot(x, y) > 0.999999f;
        }


        private static float QuaternionDot(Quaternion a, Quaternion b)
        {
            return a.X * b.X + a.Y * b.Y + a.Z * b.Z + a.W * b.W;
        }

        
        public int GetHashCode(Quaternion obj)
        {
            return obj.GetHashCode();
        }
    }

    #endregion


    #region FORWARD UP RIGHT

    [Test]
    public void Forward_Get_Default_ReturnsCorrectValue()
    {
        TransformComponent component = new();
        component.Position = new Vector3(1, 2, 3);

        Vector3 expectedForward = new(0, 0, -1);
        
        Assert.That(component.Forward, Is.EqualTo(expectedForward));
    }


    [Test]
    public void Up_Get_Default_ReturnsCorrectValue()
    {
        TransformComponent component = new();
        component.Position = new Vector3(1, 2, 3);

        Vector3 expectedUp = new(0, 1, 0);

        Assert.That(component.Up, Is.EqualTo(expectedUp));
    }


    [Test]
    public void Right_Get_Default_ReturnsCorrectValue()
    {
        TransformComponent component = new();
        component.Position = new Vector3(1, 2, 3);

        Vector3 expectedRight = new(1, 0, 0);

        Assert.That(component.Right, Is.EqualTo(expectedRight));
    }
    

    [Test]
    public void Forward_Get_ReturnsCorrectValue()
    {
        TransformComponent component = new();
        component.Position = new Vector3(1, 2, 3);
        // Rotate 90 degrees counter-clockwise around the Y-axis, when viewed from above
        component.EulerAngles = new Vector3(0, 90, 0);

        Vector3 expectedForward = new(-1, 0, 0);
        
        Assert.That(component.Forward, Is.EqualTo(expectedForward));
    }


    [Test]
    public void Up_Get_ReturnsCorrectValue()
    {
        TransformComponent component = new();
        component.Position = new Vector3(1, 2, 3);
        // Rotate 90 degrees counter-clockwise around the Z-axis, when viewed from behind
        component.EulerAngles = new Vector3(0, 0, 90);

        Vector3 expectedUp = new(-1, 0, 0);

        Assert.That(component.Up, Is.EqualTo(expectedUp));
    }


    [Test]
    public void Right_Get_ReturnsCorrectValue()
    {
        TransformComponent component = new();
        component.Position = new Vector3(1, 2, 3);
        // Rotate 90 degrees counter-clockwise around the Z-axis, when viewed from behind
        component.EulerAngles = new Vector3(0, 0, 90);

        Vector3 expectedRight = new(0, 1, 0);

        Assert.That(component.Right, Is.EqualTo(expectedRight));
    }

    #endregion


    #region CONVERSIONS

    [Test]
    public void ImplicitConversionToMatrix4_ReturnsCorrectMatrix()
    {
        TransformComponent component = new();
        Matrix4 expectedMatrix = Matrix4.Identity;

        Matrix4 resultMatrix = component;

        Assert.That(resultMatrix, Is.EqualTo(expectedMatrix));
    }

    #endregion
}