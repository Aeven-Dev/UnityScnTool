using BlubLib.IO;
using NetsphereScnTool;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ScnToolByAeven.SeqFile
{
	class Particle : SeqNode
	{
		//Section 1
		int num1;
		int num2;
		string string1;
		int num3;
		int lifeTime;
		//Section 2
		float num4;
		int num5;
		int num6;
		int num7;
		Vector3 vector1;
		Vector3 vector2;
		Vector3 vector3;
		bool ghostTrail;
		int trailParticles;
		Vector3 vector4;
		Vector3 vector5;
		//Section 3
		int miliseconds;
		bool flag1;
		bool flag2;
		float unk1;
		float unk2;
		Vector3 vector6;
		Vector3 vector7;
		Vector3 vector8;
		Vector3 vector9;
		Vector3 vector10;
		float unk3;
		float unk4;
		int num8;
		int num9;
		int num10;
		//Section 4: gradient?
		List<ColorGradient> gradient;
		//Textures
		List<TextureData> textures;

		public void Serialize(Stream stream)
		{
			using (var w = stream.ToBinaryWriter(true))
			{
				w.Write("CActParticle");
				w.Write(1952661827);
				w.Write(num1);
				w.Write(num2);
				w.WriteCString(string1);
				w.Write(num3);
				w.Write(lifeTime);
				w.Write(1952661827);
				w.Write(num4);
				w.Write(num5);
				w.Write(num6);
				w.Write(num7);
				w.Write(vector1);
				w.Write(vector2);
				w.Write(vector3);
				w.Write(ghostTrail);
				w.Write(trailParticles);
				w.Write(vector4);
				w.Write(vector5);
				w.Write(miliseconds);
				w.Write(flag1);
				w.Write(flag2);
				w.Write(unk1);
				w.Write(unk2);
				w.Write(vector6);
				w.Write(vector7);
				w.Write(vector8);
				w.Write(vector9);
				w.Write(vector10);
				w.Write(unk3);
				w.Write(unk4);
				w.Write(num8);
				w.Write(num9);
				w.Write(num10);


				w.Write(gradient.Count);
				for (int i = 0; i < gradient.Count; i++)
				{
					w.Write(gradient[i].influence);
					w.Write(gradient[i].value1);
					w.Write(gradient[i].value2);
					w.Write(gradient[i].value3);
					w.Write(gradient[i].value4);
					w.Write(gradient[i].value5);
					w.Write(gradient[i].value6);
					w.Write(gradient[i].value7);
				}

				w.Write(textures.Count);
				for (int i = 0; i < textures.Count; i++)
				{
					w.Write(textures[i].path);
					w.Write(textures[i].uv_tl);
					w.Write(textures[i].uv_tr);
					w.Write(textures[i].uv_bl);
					w.Write(textures[i].uv_br);
				}
			}
				
		}
		
		public void Deserialize(Stream stream)
		{
			using (var w = stream.ToBinaryReader(true))
			{
				w.ReadInt32();//Separator
				num1 = w.ReadInt32();
				num2 = w.ReadInt32();
				string1 = w.ReadCString();
				num3 = w.ReadInt32();
				lifeTime = w.ReadInt32();
				w.ReadInt32();//Separator
				num4 = w.ReadInt32();
				num5 = w.ReadInt32();
				num6 = w.ReadInt32();
				num7 = w.ReadInt32();
				vector1 = w.ReadVector3();
				vector2 = w.ReadVector3();
				vector3 = w.ReadVector3();
				ghostTrail = w.ReadBoolean();
				trailParticles = w.ReadInt32();
				vector4 = w.ReadVector3();
				vector5 = w.ReadVector3();
				miliseconds = w.ReadInt32();
				flag1 = w.ReadBoolean();
				flag2 = w.ReadBoolean();
				unk1 = w.ReadSingle();
				unk2 = w.ReadSingle();
				vector6 = w.ReadVector3();
				vector7 = w.ReadVector3();
				vector8 = w.ReadVector3();
				vector9 = w.ReadVector3();
				vector10 = w.ReadVector3();
				unk3 = w.ReadSingle();
				unk4 = w.ReadSingle();
				num8 = w.ReadInt32();
				num9 = w.ReadInt32();
				num10 = w.ReadInt32();


				int count = w.ReadInt32();
				gradient.Clear();
				for (int i = 0; i < count; i++)
				{
					var cg = new ColorGradient();
					cg.influence = w.ReadSingle();
					cg.value1 = w.ReadSingle();
					cg.value2 = w.ReadSingle();
					cg.value3 = w.ReadSingle();
					cg.value4 = w.ReadSingle();
					cg.value5 = w.ReadSingle();
					cg.value6 = w.ReadSingle();
					cg.value7 = w.ReadSingle();
					gradient.Add(cg);
				}

				count = w.ReadInt32();
				textures.Clear();
				for (int i = 0; i < count; i++)
				{
					var td = new TextureData();
					td.path = w.ReadCString();
					td.uv_tl = w.ReadVector2();
					td.uv_tr = w.ReadVector2();
					td.uv_bl = w.ReadVector2();
					td.uv_br = w.ReadVector2();
				}
			}
		}
	}

	struct TextureData
	{
		public string path;
		public Vector2 uv_tl;
		public Vector2 uv_tr;
		public Vector2 uv_bl;
		public Vector2 uv_br;
	}
	struct ColorGradient
	{
		 public float influence;
		 public float value1;
		 public float value2;
		 public float value3;
		 public float value4;
		 public float value5;
		 public float value6;
		 public float value7;
	}
}
