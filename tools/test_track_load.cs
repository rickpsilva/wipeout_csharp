#!/usr/bin/env dotnet-script

// Test script to verify track loading
// Run with: dotnet script tools/test_track_load.cs

using System;
using System.IO;

// Read big-endian integers
static short ReadI16BE(byte[] bytes, ref int p)
{
    short value = (short)((bytes[p] << 8) | bytes[p + 1]);
    p += 2;
    return value;
}

static int ReadI32BE(byte[] bytes, ref int p)
{
    int value = (bytes[p] << 24) | (bytes[p + 1] << 16) | (bytes[p + 2] << 8) | bytes[p + 3];
    p += 4;
    return value;
}

var trvPath = "/home/rick/workspace/wipeout_csharp/assets/wipeout/track01/track.trv";
var trfPath = "/home/rick/workspace/wipeout_csharp/assets/wipeout/track01/track.trf";

Console.WriteLine($"Reading {trvPath}");
var trvBytes = File.ReadAllBytes(trvPath);
Console.WriteLine($"File size: {trvBytes.Length} bytes ({trvBytes.Length / 16} vertices)");

int p = 0;
int vertexCount = 0;
while (p + 16 <= trvBytes.Length && vertexCount < 10)
{
    int x = ReadI32BE(trvBytes, ref p);
    int y = ReadI32BE(trvBytes, ref p);
    int z = ReadI32BE(trvBytes, ref p);
    p += 4; // padding
    
    Console.WriteLine($"Vertex {vertexCount}: X={x}, Y={y}, Z={z}");
    vertexCount++;
}

Console.WriteLine($"\nTotal vertices: {trvBytes.Length / 16}");

Console.WriteLine($"\n\nReading {trfPath}");
var trfBytes = File.ReadAllBytes(trfPath);
Console.WriteLine($"File size: {trfBytes.Length} bytes ({trfBytes.Length / 20} faces)");

p = 0;
int faceCount = 0;
while (p + 20 <= trfBytes.Length && faceCount < 10)
{
    short v0 = ReadI16BE(trfBytes, ref p);
    short v1 = ReadI16BE(trfBytes, ref p);
    short v2 = ReadI16BE(trfBytes, ref p);
    short v3 = ReadI16BE(trfBytes, ref p);
    
    short nx = ReadI16BE(trfBytes, ref p);
    short ny = ReadI16BE(trfBytes, ref p);
    short nz = ReadI16BE(trfBytes, ref p);
    
    byte tex = trfBytes[p++];
    byte flags = trfBytes[p++];
    
    uint color = (uint)((trfBytes[p] << 24) | (trfBytes[p+1] << 16) | (trfBytes[p+2] << 8) | trfBytes[p+3]);
    p += 4;
    
    Console.WriteLine($"Face {faceCount}: v=[{v0},{v1},{v2},{v3}] n=[{nx/4096.0f:F3},{ny/4096.0f:F3},{nz/4096.0f:F3}] tex={tex} flags={flags:X2} color={color:X8}");
    faceCount++;
}

Console.WriteLine($"\nTotal faces: {trfBytes.Length / 20}");
