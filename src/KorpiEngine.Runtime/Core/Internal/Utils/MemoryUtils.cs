using System.Runtime.InteropServices;

namespace KorpiEngine.Core.Internal.Utils;

public static class MemoryUtils
{
    public static T[] ReadStructArray<T>(byte[] sourceData) where T : unmanaged
    {
        int structSize = Marshal.SizeOf(typeof(T));
        int numStructs = sourceData.Length / structSize;
        T[] structArray = new T[numStructs];

        unsafe
        {
            fixed (byte* bytePtr = sourceData)
            {
                byte* currentPtr = bytePtr;
                for (int i = 0; i < numStructs; i++)
                {
                    structArray[i] = *(T*)currentPtr;
                    currentPtr += structSize;
                }
            }
        }

        return structArray;
    }
    
    
    public static void ReadStructArrayNonAlloc<TStruct>(byte[] sourceData, int structCount, int structSize, IList<TStruct> destination) where TStruct : struct
    {
        IntPtr bufferPtr = Marshal.AllocHGlobal(structSize);
        try
        {
            for (int i = 0; i < structCount; i++)
            {
                int sourceIndex = i * structSize;
                Marshal.Copy(sourceData, sourceIndex, bufferPtr, structSize);

                destination[i] = Marshal.PtrToStructure<TStruct>(bufferPtr);
            }
        }
        finally
        {
            Marshal.FreeHGlobal(bufferPtr);
        }
    }


    /// <summary>
    /// Creates a new byte array with the source data.
    /// </summary>
    public static byte[] WriteStructArray<TStruct>(ArraySegment<TStruct> sourceData) where TStruct : struct
    {
        int structSize = Marshal.SizeOf(typeof(TStruct));

        byte[] destination = new byte[sourceData.Count * structSize];

        WriteStructArray(sourceData, destination, structSize);
        
        return destination;
    }


    /// <summary>
    /// Copies the source data to the destination byte array.
    /// </summary>
    public static void WriteStructArray<TStruct>(ArraySegment<TStruct> sourceData, byte[] destination) where TStruct : struct
    {
        int structSize = Marshal.SizeOf(typeof(TStruct));
        WriteStructArray(sourceData, destination, structSize);
    }


    /// <summary>
    /// Copies the source data to the destination byte array.
    /// </summary>
    public static void WriteStructArray<TStruct>(ArraySegment<TStruct> sourceData, byte[] destination, int structSize) where TStruct : struct
    {
        IntPtr bufferPtr = Marshal.AllocHGlobal(structSize);
        try
        {
            for (int i = 0; i < sourceData.Count; i++)
            {
                Marshal.StructureToPtr(sourceData[i], bufferPtr, false);

                int destinationIndex = i * structSize;
                Marshal.Copy(bufferPtr, destination, destinationIndex, structSize);
            }
        }
        finally
        {
            Marshal.FreeHGlobal(bufferPtr);
        }
    }
}