using KorpiEngine.Entities;

namespace KorpiEngine.Core.Tests;

[TestFixture]
[TestOf(typeof(Transform))]
public class TransformTest
{
    private class Vector3Comparer : IEqualityComparer<Vector3>
    {
        public bool Equals(Vector3 x, Vector3 y)
        {
            return x.AlmostEquals(y, 0.01f);
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
            return x.AlmostEquals(y, 0.01f);
        }

        
        public int GetHashCode(Quaternion obj)
        {
            return obj.GetHashCode();
        }
    }
    
    
    private class QuaternionRotationComparer : IEqualityComparer<Quaternion>
    {
        public bool Equals(Quaternion x, Quaternion y)
        {
            return x.AlmostEquals(y, 0.01f) || x.AlmostEquals(-y, 0.01f);
        }

        
        public int GetHashCode(Quaternion obj)
        {
            return obj.GetHashCode();
        }
    }
    
    
#region POSITION

    [Test]
    public void Position_Set_Get_ReturnsCorrectValue()
    {
        Entity e = new(null, null);
        Transform component = e.Transform;
        Vector3 expectedPosition = new(1, 2, 3);

        component.Position = expectedPosition;

        Assert.That(component.Position, Is.EqualTo(expectedPosition).Using(new Vector3Comparer()));
    }

    [Test]
    public void Position_Set_Get_ReturnsCorrectValue_Negative()
    {
        Entity e = new(null, null);
        Transform component = e.Transform;
        Vector3 expectedPosition = new(-1, -2, -3);

        component.Position = expectedPosition;

        Assert.That(component.Position, Is.EqualTo(expectedPosition).Using(new Vector3Comparer()));
    }

    [Test]
    public void LocalPosition_Set_Get_ReturnsCorrectValue()
    {
        Entity e = new(null, null);
        Transform component = e.Transform;
        Vector3 expectedPosition = new(1, 2, 3);

        component.LocalPosition = expectedPosition;

        Assert.That(component.LocalPosition, Is.EqualTo(expectedPosition).Using(new Vector3Comparer()));
    }

    [Test]
    public void LocalPosition_Set_Get_ReturnsCorrectValue_Negative()
    {
        Entity e = new(null, null);
        Transform component = e.Transform;
        Vector3 expectedPosition = new(-1, -2, -3);

        component.LocalPosition = expectedPosition;

        Assert.That(component.LocalPosition, Is.EqualTo(expectedPosition).Using(new Vector3Comparer()));
    }

#endregion


#region ROTATION

    [Test]
    public void Rotation_Set_Get_ReturnsCorrectValue()
    {
        Entity e = new(null, null);
        Transform component = e.Transform;
        Quaternion expectedRotation = Quaternion.CreateFromEulerAnglesDegrees(new Vector3(15, 90, 45));

        component.Rotation = expectedRotation;
        
        Assert.That(component.Rotation, Is.EqualTo(expectedRotation).Using(new QuaternionComparer()));
    }


    [Test]
    public void Rotation_Set_Get_ReturnsCorrectValue_Negative()
    {
        Entity e = new(null, null);
        Transform component = e.Transform;
        Quaternion expectedRotation = Quaternion.CreateFromEulerAnglesDegrees(new Vector3(-15, -90, -45));

        component.Rotation = expectedRotation;
        
        Assert.That(component.Rotation, Is.EqualTo(expectedRotation).Using(new QuaternionComparer()));
    }


    [Test]
    public void LocalRotation_Set_Get_ReturnsCorrectValue()
    {
        Entity e = new(null, null);
        Transform component = e.Transform;
        Quaternion expectedRotation = Quaternion.CreateFromEulerAnglesDegrees(new Vector3(15, 90, 45));

        component.LocalRotation = expectedRotation;
        
        Assert.That(component.LocalRotation, Is.EqualTo(expectedRotation).Using(new QuaternionComparer()));
    }


    [Test]
    public void LocalRotation_Set_Get_ReturnsCorrectValue_Negative()
    {
        Entity e = new(null, null);
        Transform component = e.Transform;
        Quaternion expectedRotation = Quaternion.CreateFromEulerAnglesDegrees(new Vector3(-15, -90, -45));

        component.LocalRotation = expectedRotation;
        
        Assert.That(component.LocalRotation, Is.EqualTo(expectedRotation).Using(new QuaternionComparer()));
    }

#endregion


#region SCALE

    [Test]
    public void LocalScale_Set_Get_ReturnsCorrectValue()
    {
        Entity e = new(null, null);
        Transform component = e.Transform;
        Vector3 expectedScale = new(1, 2, 3);

        component.LocalScale = expectedScale;
        
        Assert.That(component.LocalScale, Is.EqualTo(expectedScale));
    }

#endregion


#region EULERS

    [Test]
    public void EulerAngles_Set_Get_ReturnsCorrectValue()
    {
        Entity e = new(null, null);
        Transform component = e.Transform;
        Vector3 expectedEulerAngles = new(35, 125, 45);
        Quaternion expectedQuaternion = Quaternion.CreateFromEulerAnglesDegrees(expectedEulerAngles);

        component.EulerAngles = expectedEulerAngles;
        Vector3 actual = component.EulerAngles;
        
        Console.WriteLine($"Expected: {expectedEulerAngles} | Actual: {actual}");
        Assert.Multiple(() =>
        {
            Assert.That(component.Rotation, Is.EqualTo(expectedQuaternion).Using(new QuaternionComparer()));
            Assert.That(Quaternion.CreateFromEulerAnglesDegrees(actual), Is.EqualTo(expectedQuaternion).Using(new QuaternionRotationComparer()));
        });
    }

    [Test]
    public void EulerAngles_Set_Get_ReturnsCorrectValue_Negative()
    {
        Entity e = new(null, null);
        Transform component = e.Transform;
        Vector3 expectedEulerAngles = new(-35, -125, -45);
        Quaternion expectedQuaternion = Quaternion.CreateFromEulerAnglesDegrees(expectedEulerAngles);

        component.EulerAngles = expectedEulerAngles;
        Vector3 actual = component.EulerAngles;
        
        Console.WriteLine($"Expected: {expectedEulerAngles} | Actual: {actual}");
        Assert.Multiple(() =>
        {
            Assert.That(component.Rotation, Is.EqualTo(expectedQuaternion).Using(new QuaternionComparer()));
            Assert.That(Quaternion.CreateFromEulerAnglesDegrees(actual), Is.EqualTo(expectedQuaternion).Using(new QuaternionRotationComparer()));
        });
    }

    [Test]
    public void LocalEulerAngles_Set_Get_ReturnsCorrectValue()
    {
        Entity e = new(null, null);
        Transform component = e.Transform;
        Vector3 expectedEulerAngles = new(35, 125, 45);
        Quaternion expectedQuaternion = Quaternion.CreateFromEulerAnglesDegrees(expectedEulerAngles);

        component.LocalEulerAngles = expectedEulerAngles;
        Vector3 actual = component.LocalEulerAngles;
        
        Console.WriteLine($"Expected: {expectedEulerAngles} | Actual: {actual}");
        Assert.Multiple(() =>
        {
            Assert.That(component.Rotation, Is.EqualTo(expectedQuaternion).Using(new QuaternionComparer()));
            Assert.That(Quaternion.CreateFromEulerAnglesDegrees(actual), Is.EqualTo(expectedQuaternion).Using(new QuaternionRotationComparer()));
        });
    }

    [Test]
    public void LocalEulerAngles_Set_Get_ReturnsCorrectValue_Negative()
    {
        Entity e = new(null, null);
        Transform component = e.Transform;
        Vector3 expectedEulerAngles = new(-35, -125, -45);
        Quaternion expectedQuaternion = Quaternion.CreateFromEulerAnglesDegrees(expectedEulerAngles);

        component.LocalEulerAngles = expectedEulerAngles;
        Vector3 actual = component.LocalEulerAngles;
        
        Console.WriteLine($"Expected: {expectedEulerAngles} | Actual: {actual}");
        Assert.Multiple(() =>
        {
            Assert.That(component.Rotation, Is.EqualTo(expectedQuaternion).Using(new QuaternionComparer()));
            Assert.That(Quaternion.CreateFromEulerAnglesDegrees(actual), Is.EqualTo(expectedQuaternion).Using(new QuaternionRotationComparer()));
        });
    }

#endregion


#region FORWARD UP RIGHT

    [Test]
    public void Forward_Get_Default_ReturnsCorrectValue()
    {
        Entity e = new(null, null);
        Transform component = e.Transform;
        component.Position = new Vector3(1, 2, 3);

        Vector3 expected = Vector3.Forward;
        Vector3 actual = component.Forward;

        Assert.That(actual, Is.EqualTo(expected));
    }
    

    [Test]
    public void Forward_Get_ReturnsCorrectValue()
    {
        Entity e = new(null, null);
        Transform component = e.Transform;
        component.Position = new Vector3(1, 2, 3);
        // Rotate 90 degrees CCW around the Y-axis, when viewed from above
        component.EulerAngles = new Vector3(0, 90, 0);

        Vector3 expected = Vector3.Left;
        Vector3 actual = component.Forward;

        Assert.That(actual, Is.EqualTo(expected).Using(new Vector3Comparer()));
    }


    [Test]
    public void Up_Get_Default_ReturnsCorrectValue()
    {
        Entity e = new(null, null);
        Transform component = e.Transform;
        component.Position = new Vector3(1, 2, 3);

        Vector3 expected = Vector3.Up;
        Vector3 actual = component.Up;

        Assert.That(actual, Is.EqualTo(expected));
    }


    [Test]
    public void Up_Get_ReturnsCorrectValue()
    {
        Entity e = new(null, null);
        Transform component = e.Transform;
        component.Position = new Vector3(1, 2, 3);
        // Rotate 90 degrees CCW around the X-axis, when viewed from the front
        component.EulerAngles = new Vector3(90, 0, 0);

        Vector3 expected = Vector3.Backward;
        Vector3 actual = component.Up;

        Assert.That(actual, Is.EqualTo(expected).Using(new Vector3Comparer()));
    }


    [Test]
    public void Right_Get_Default_ReturnsCorrectValue()
    {
        Entity e = new(null, null);
        Transform component = e.Transform;
        component.Position = new Vector3(1, 2, 3);

        Vector3 expected = Vector3.Right;
        Vector3 actual = component.Right;

        Assert.That(actual, Is.EqualTo(expected));
    }


    [Test]
    public void Right_Get_ReturnsCorrectValue()
    {
        Entity e = new(null, null);
        Transform component = e.Transform;
        component.Position = new Vector3(1, 2, 3);
        // Rotate 90 degrees CCW around the Z-axis, when viewed from the front
        component.EulerAngles = new Vector3(0, 0, 90);

        Vector3 expectedRight = Vector3.Up;

        Assert.That(component.Right, Is.EqualTo(expectedRight).Using(new Vector3Comparer()));
    }

#endregion


#region CONVERSIONS

    [Test]
    public void ImplicitConversionToMatrix4x4_ReturnsCorrectMatrix_Identity()
    {
        Entity e = new(null, null);
        Transform component = e.Transform;
        Matrix4x4 expectedMatrix = Matrix4x4.Identity;

        Matrix4x4 resultMatrix = component.LocalToWorldMatrix;

        Assert.That(resultMatrix, Is.EqualTo(expectedMatrix));
    }

    [Test]
    public void ImplicitConversionToMatrix4x4_ReturnsCorrectMatrix()
    {
        Entity e = new(null, null);
        Transform component = e.Transform;
        component.Position = new Vector3(-123.6f, 83.1f, 432.2f);
        component.EulerAngles = new Vector3(12.4f, -63.9f, 137.7f);
        component.LocalScale = new Vector3(1.5f, 2.5f, 3.5f);
        Matrix4x4 expectedMatrix = new Matrix4x4(
            -0.48808908f, 1.9998544f, -1.7646828f, 0f,
            -0.44412678f, -1.4814868f, -2.621884f, 0f,
            -1.3470414f, -0.23617618f, 1.5038674f, 0f,
            -558.7704f, -472.36893f, 650.2077f, 1f
        );

        Matrix4x4 resultMatrix = component.LocalToWorldMatrix;

        Console.WriteLine(resultMatrix);
        Assert.That(resultMatrix, Is.EqualTo(expectedMatrix));
    }

#endregion
}