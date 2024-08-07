﻿using System.Collections.Generic;
using KorpiEngine.Core.API;
using KorpiEngine.Core.EntityModel.SpatialHierarchy;
using NUnit.Framework;

namespace Core.Tests.ECS;

[TestFixture]
[TestOf(typeof(Transform))]
public class TransformTest
{
    private class Vector3Comparer : IEqualityComparer<Vector3>
    {
        public bool Equals(Vector3 x, Vector3 y)
        {
            return Vector3.Approximately(x, y, 0.001f);
        }

        
        public int GetHashCode(Vector3 obj)
        {
            return obj.GetHashCode();
        }
    }
    
    
    private class QuaternionComparer : IEqualityComparer<Quaternion>
    {
        public bool Equals(Quaternion x, Quaternion y)
        {
            return Quaternion.Approximately(x, y, 0.01f);
        }

        
        public int GetHashCode(Quaternion obj)
        {
            return obj.GetHashCode();
        }
    }
    
    
    #region POSITION ROTATION SCALE

    [Test]
    public void Position_Set_Get_ReturnsCorrectValue()
    {
        Transform component = new();
        Vector3 expectedPosition = new(1, 2, 3);

        component.Position = expectedPosition;

        Assert.That(component.Position, Is.EqualTo(expectedPosition).Using(new Vector3Comparer()));
    }


    [Test]
    public void Rotation_Set_Get_ReturnsCorrectValue()
    {
        Transform component = new();
        Quaternion expectedRotation = Quaternion.Euler(new Vector3(0, 90, 45));

        component.Rotation = expectedRotation;
        
        Assert.That(component.Rotation, Is.EqualTo(expectedRotation).Using(new QuaternionComparer()));
    }


    [Test]
    public void Scale_Set_Get_ReturnsCorrectValue()
    {
        Transform component = new();
        Vector3 expectedScale = new(1, 2, 3);

        component.LocalScale = expectedScale;
        
        Assert.That(component.LocalScale, Is.EqualTo(expectedScale));
    }

    #endregion


    #region EULERS

    [Test]
    public void EulerAngles_Set_Get_ReturnsCorrectValue()
    {
        Transform component = new();
        Vector3 expectedEulerAngles = new(35, 125, 45);

        component.EulerAngles = expectedEulerAngles;
    
        Assert.That(component.EulerAngles, Is.EqualTo(expectedEulerAngles).Using(new Vector3Comparer()));
        
        // Also check if the rotation is correct
        Assert.That(component.Rotation, Is.EqualTo(Quaternion.Euler(expectedEulerAngles)).Using(new QuaternionComparer()));
    }

    #endregion


    #region FORWARD UP RIGHT

    [Test]
    public void Forward_Get_Default_ReturnsCorrectValue()
    {
        Transform component = new()
        {
            Position = new Vector3(1, 2, 3)
        };

        Vector3 expectedForward = Vector3.Forward;
        
        Assert.That(component.Forward, Is.EqualTo(expectedForward));
    }
    

    [Test]
    public void Forward_Get_ReturnsCorrectValue()
    {
        Transform component = new()
        {
            Position = new Vector3(1, 2, 3),

            // Rotate 90 degrees clockwise around the Y-axis, when viewed from above
            EulerAngles = new Vector3(0, 90, 0)
        };

        Vector3 expectedForward = Vector3.Right;
        
        Assert.That(component.Forward, Is.EqualTo(expectedForward).Using(new Vector3Comparer()));
    }


    [Test]
    public void Up_Get_Default_ReturnsCorrectValue()
    {
        Transform component = new()
        {
            Position = new Vector3(1, 2, 3)
        };

        Vector3 expectedUp = Vector3.Up;

        Assert.That(component.Up, Is.EqualTo(expectedUp));
    }


    [Test]
    public void Up_Get_ReturnsCorrectValue()
    {
        Transform component = new()
        {
            Position = new Vector3(1, 2, 3),

            // Rotate 90 degrees clockwise around the Z-axis, when viewed from front
            EulerAngles = new Vector3(0, 0, 90)
        };

        Vector3 expectedUp = Vector3.Left;

        Assert.That(component.Up, Is.EqualTo(expectedUp).Using(new Vector3Comparer()));
    }


    [Test]
    public void Right_Get_Default_ReturnsCorrectValue()
    {
        Transform component = new()
        {
            Position = new Vector3(1, 2, 3)
        };

        Vector3 expectedRight = Vector3.Right;

        Assert.That(component.Right, Is.EqualTo(expectedRight));
    }


    [Test]
    public void Right_Get_ReturnsCorrectValue()
    {
        Transform component = new()
        {
            Position = new Vector3(1, 2, 3),

            // Rotate 90 degrees clockwise around the Z-axis, when viewed from front
            EulerAngles = new Vector3(0, 0, 90)
        };

        Vector3 expectedRight = Vector3.Up;

        Assert.That(component.Right, Is.EqualTo(expectedRight).Using(new Vector3Comparer()));
    }

    #endregion


    #region CONVERSIONS

    [Test]
    public void ImplicitConversionToMatrix4x4_ReturnsCorrectMatrix()
    {
        Transform component = new();
        Matrix4x4 expectedMatrix = Matrix4x4.Identity;

        Matrix4x4 resultMatrix = component.LocalToWorldMatrix;

        Assert.That(resultMatrix, Is.EqualTo(expectedMatrix));
    }

    #endregion
}