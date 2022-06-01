using System;

namespace NetsphereScnTool.Scene
{
    [Flags]
    public enum Shader
    {
        None = 0,
        NoLight = 1,
        Transparent = 2,
        Cutout = 4,
        NoCulling = 8,
        Billboard = 16,
        Flare = 32,
        ZWriteOff = 64,
        Shader = 128,
        NoFog = 512,
        Unknown = 1024,
        NoMipmap = 2048,
        VertexAnim = 4096,
		Shadow = 8192,
		Glow = 16384,
        Water = 32768,
        Distortion = 65536,
        Dark = 131072
    }

    public enum ChunkType : uint
    {
        Box = 0x25ADF0D1, // fumbi, spawns, deadzones
        ModelData = 0x081098F8, // Game::CActorGeometry
        Bone = 0x6D411AD1, // CoreLib::Scene::CBone
        SkyDirect1 = 0xC3E8BE62,
        BoneSystem = 0x5E74333F,
        Shape = 0xADEE38A2
    }

    public enum ParentGrade : uint
    {
        Father,
        Child,
        Grandson
    }
}
