using NetsphereScnTool.Scene;
using NetsphereScnTool.Scene.Chunks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.Collections;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
#if PROBUILDER 
using UnityEngine.ProBuilder;
#endif
using System.Linq;

namespace AevenScnTool.IO
{
	public static class ScnFileImporter
	{
		public static Dictionary<SceneChunk, GameObject> createdObjects = new Dictionary<SceneChunk, GameObject>();

		public static void BuildFromContainer(SceneContainer container, GameObject sceneObj, bool identityMatrix = false)
		{
			List < TreeItem < SceneChunk >> rootItems = GetRootItems(container);

			createdObjects.Clear();
			foreach (var item in rootItems)
			{
				BuildTreeItem(item, container, sceneObj, identityMatrix);
			}
		}


		public static List<TreeItem<SceneChunk>> GetRootItems(SceneContainer container)
		{
			List<SceneChunk> chunks = new();
			foreach (var item in container)
			{
				chunks.Add(item);
			}

			List<TreeItem<SceneChunk>> items = GetChildItems(chunks, null, container.Header.Name);
			
			return items;
		}

		static List<TreeItem<SceneChunk>> GetChildItems(List<SceneChunk> chunks, TreeItem<SceneChunk> parentItem, string parentName)
		{
			var items = new List<TreeItem<SceneChunk>>();
			for (int i = 0; i < chunks.Count; i++)
			{
				SceneChunk chunk = chunks[i];
				if (parentItem == null)
				{
					if (chunk.SubName != parentName && chunk.SubName != string.Empty)
						continue;
				}
				else
				{
					if (chunk.SubName != parentName)
						continue;
				}
				
				var treeItem = new TreeItem<SceneChunk>
				{
					item = chunk,
					parent = parentItem
				};
				items.Add(treeItem);
			}
			foreach (var item in items)
			{
				chunks.Remove(item.item);
			}

			foreach (var item in items)
			{
				var children = GetChildItems(chunks, item, item.item.Name);

				item.childs = children;
			}
			return items;
		}

		static void BuildTreeItem(TreeItem<SceneChunk> treeItem, SceneContainer container, GameObject parent, bool identityMatrix)
		{
			GameObject go = BuildFromChunk(treeItem, container.fileInfo.Directory, parent, identityMatrix);
			createdObjects.Add(treeItem.item, go);
			foreach (var child in treeItem.childs)
			{
				BuildTreeItem(child, container, go, identityMatrix);
			}
		}

		static GameObject BuildFromChunk(TreeItem<SceneChunk> treeItem, DirectoryInfo di, GameObject parent, bool identityMatrix)
		{
			return treeItem.item.ChunkType switch
			{
				ChunkType.Box => CreateBox(treeItem.item as BoxChunk, parent),
				ChunkType.ModelData => CreateModel(treeItem.item as ModelChunk, di, parent, identityMatrix),
				ChunkType.Bone => CreateBone(treeItem.item as BoneChunk, parent),
				ChunkType.SkyDirect1 => CreateSkyDirect1(treeItem.item as SkyDirect1Chunk, parent),
				ChunkType.BoneSystem => CreateBoneSystem(treeItem.item as BoneSystemChunk, parent),
				ChunkType.Shape => CreateShape(treeItem.item as ShapeChunk, parent),
				_ => null,
			};
		}


		static GameObject CreateModel(ModelChunk model, DirectoryInfo di, GameObject parent, bool identityMatrix)
		{
			GameObject go = CreateGameObject(model);
			go.transform.SetParent(parent.transform);

			go.transform.localPosition = model.Matrix.GetPosition() / ScnToolData.Instance.scale;
			go.transform.localRotation = model.Matrix.rotation;
			go.transform.localScale = model.Matrix.lossyScale;
			if (identityMatrix)
			{
				go.transform.position = Vector3.zero;
				go.transform.rotation = Quaternion.identity;
				go.transform.localScale = Vector3.one;
			}

			Mesh mesh = ModelChunkImporter.CreateMesh(model);

			Material mat = ScnToolData.GetMatFromShader(model.Shader);

			TextureReference tr = go.AddComponent<TextureReference>();
			tr.renderFlags = model.Shader;

			Material[] mats = ModelChunkImporter.SetFaceData(mesh, model, di, tr);
			if (mats.Length == 0)
			{
				mats = new Material[1];
			}

			if (model.WeightBone.Count > 0)
			{
				ModelChunkImporter.SetSkinnedMesh(go, model, mesh, mats);
			}
			else if (model.Name.StartsWith("oct_") && ScnToolData.Instance.importCollisionAsVisual == false)
			{
				ModelChunkImporter.SetOctMesh(go, model, mesh);
			}
			else
			{
				MeshFilter mf = go.AddComponent<MeshFilter>();
				MeshRenderer mr = go.AddComponent<MeshRenderer>();
				mr.materials = mats;
				mf.mesh = mesh;
			}
			int isAnim = ModelChunkImporter.ModelAnimationIsTransform(model.Animation);
			if (isAnim == 1)
			{
				go.transform.localPosition = model.Animation[0].transformKeyData2.TransformKey.Translation / ScnToolData.Instance.scale;
				go.transform.localRotation = model.Animation[0].transformKeyData2.TransformKey.Rotation;
				go.transform.localScale = model.Animation[0].transformKeyData2.TransformKey.Scale;

			}
			else if (isAnim == 2)
			{
				go.transform.localPosition = model.Animation[0].transformKeyData2.TransformKey.Translation / ScnToolData.Instance.scale;
				go.transform.localRotation = model.Animation[0].transformKeyData2.TransformKey.Rotation;
				go.transform.localScale = model.Animation[0].transformKeyData2.TransformKey.Scale;

				tr.transparency = model.Animation[0].transformKeyData2.AlphaKeys[0].Alpha;
			}
			else if(isAnim == 3){
				go.transform.localPosition = model.Animation[0].transformKeyData2.TransformKey.Translation / ScnToolData.Instance.scale;
				go.transform.localRotation = model.Animation[0].transformKeyData2.TransformKey.Rotation;
				go.transform.localScale = model.Animation[0].transformKeyData2.TransformKey.Scale;

				go.AddComponent<S4Animations>().FromModelAnimation(model.Animation);
			}
			else if (isAnim == -1)
			{
				//We do Nothing!!
			}
			else
			{
				go.AddComponent<S4Animations>().FromModelAnimation(model.Animation);
			}

			return go;
		}

		static GameObject CreateBox(BoxChunk box, GameObject parent)
		{
			GameObject go = CreateGameObject(box);
			go.transform.SetParent(parent.transform);
			go.transform.localPosition = box.Matrix.GetPosition() / ScnToolData.Instance.scale;
			go.transform.localRotation = box.Matrix.rotation;
			go.transform.localScale = box.Matrix.lossyScale;
			
			go.AddComponent<BoxCollider>().size = box.Size / ScnToolData.Instance.scale * 2;
			if (go.name.Contains("jump_dir") || go.name.Contains("jump_char"))
			{
				go.AddComponent<Jumppad>();
			}
			if (go.name.Contains("alpha_spawn_pos"))
			{
				go.AddComponent<PointDrawer>().type = PointDrawer.PointType.alpha_spawn_pos;
			}
			if (go.name.Contains("beta_spawn_pos"))
			{
				go.AddComponent<PointDrawer>().type = PointDrawer.PointType.beta_spawn_pos;
			}
			if (go.name.Contains("ball_spawn_pos"))
			{
				go.AddComponent<PointDrawer>().type = PointDrawer.PointType.ball_spawn_pos;
			}
			if (go.name.Contains("alpha_net"))
			{
				go.AddComponent<PointDrawer>().type = PointDrawer.PointType.alpha_net;
			}
			if (go.name.Contains("beta_net"))
			{
				go.AddComponent<PointDrawer>().type = PointDrawer.PointType.beta_net;
			}
			return go;
		}

		static GameObject CreateBone(BoneChunk bone, GameObject parent)
		{
			GameObject go = CreateGameObject(bone);
			go.transform.SetParent(parent.transform);
			var b = go.AddComponent<Bone>();

			int isAnim = BoneAnimationIsTransform(bone.Animation);
			if (isAnim == 1)
			{
				go.transform.localPosition = bone.Animation[0].TransformKeyData.TransformKey.Translation / ScnToolData.Instance.scale;
				go.transform.localRotation = bone.Animation[0].TransformKeyData.TransformKey.Rotation;
				go.transform.localScale = bone.Animation[0].TransformKeyData.TransformKey.Scale;
			}
			else if (isAnim == 2)
			{
				go.transform.localPosition = bone.Animation[0].TransformKeyData.TransformKey.Translation / ScnToolData.Instance.scale;
				go.transform.localRotation = bone.Animation[0].TransformKeyData.TransformKey.Rotation;
				go.transform.localScale = bone.Animation[0].TransformKeyData.TransformKey.Scale;

				var s4a = go.AddComponent<S4Animations>();
				s4a.FromBoneAnimation(bone.Animation);
				b.s4Animations = s4a;
			}
			else if (isAnim == -1)
			{
				//We do Nothing!!
			}
			else
			{
				var s4a = go.AddComponent<S4Animations>();
				s4a.FromBoneAnimation(bone.Animation);
				b.s4Animations = s4a;
			}

			return go;
		}

		static GameObject CreateBoneSystem(BoneSystemChunk boneSys, GameObject parent)
		{
			GameObject go = CreateGameObject(boneSys);
			go.transform.SetParent(parent.transform);
			go.AddComponent<Bonesystem>();
			return go;
		}

		static GameObject CreateShape(ShapeChunk shape, GameObject parent)
		{
			GameObject go = CreateGameObject(shape);
			go.transform.SetParent(parent.transform);
			go.transform.localPosition = shape.Matrix.GetPosition() / ScnToolData.Instance.scale;
			go.transform.localRotation = shape.Matrix.rotation;

			LineRenderer lr = go.AddComponent<LineRenderer>();
			lr.widthMultiplier = 0.01f;

			Vector3[] points = new Vector3[shape.Unk.Count * 2];
			for (int i = 0; i < shape.Unk.Count; i++)
			{
				points[i * 2] = shape.Unk[i].Item1 / ScnToolData.Instance.scale;
				points[i * 2 + 1] = shape.Unk[i].Item2 / ScnToolData.Instance.scale;
			}
			lr.useWorldSpace = false;
			lr.SetPositions(points);
			return go;

		}

		static GameObject CreateSkyDirect1(SkyDirect1Chunk chunk, GameObject parent)
		{
			GameObject go = CreateGameObject(chunk);
			go.transform.SetParent(parent.transform);

			TextMesh light = go.AddComponent<TextMesh>();

			light.text = $"Color 1: {chunk.color1}\nColor 2: {chunk.color2}\nColor 3: {chunk.color3}\nColor 4: {chunk.color4}\nColor 5: {chunk.color5}\nColor 6: {chunk.color6}\n";

			return go;

		}

		static GameObject CreateGameObject(SceneChunk chunk)
		{
			GameObject go = new GameObject(chunk.Name);
			go.transform.position = chunk.Matrix.GetPosition() / ScnToolData.Instance.scale;
			go.transform.rotation = chunk.Matrix.rotation;
			go.transform.localScale = chunk.Matrix.lossyScale;

			return go;
		}



		static int BoneAnimationIsTransform(List<BoneAnimation> anims)
		{
			if (anims.Count == 0)
			{
				return -1;
			}
			else if (anims.Count > 1)
			{
				return 0;
			}
			else if (anims.Count == 1)
			{
				var anim = anims[0];
				if (anim.TransformKeyData.TransformKey.TKey.Count != 0)
				{
					return 0;
				}
				if (anim.TransformKeyData.TransformKey.RKey.Count != 0)
				{
					return 0;
				}
				if (anim.TransformKeyData.TransformKey.SKey.Count != 0)
				{
					return 0;
				}
				if (anim.TransformKeyData.AlphaKeys.Count != 0)
				{
					return 2;
				}
				return 1;
			}
			return 0;
		}

		public static Texture2D ParseTextureDXT(byte[] ddsBytes)
		{
			try
			{

				byte a = ddsBytes[84];
				byte b = ddsBytes[85];
				byte c = ddsBytes[86];
				byte d = ddsBytes[87];

				string format = System.Text.Encoding.ASCII.GetString(new byte[] { a, b, c, d });
				//Debug.Log(format);
				TextureFormat textureFormat = TextureFormat.DXT1;

				if (format == "DXT3" || format == "DXT5")
				{
					textureFormat = TextureFormat.DXT5;
				}
				byte ddsSizeCheck = ddsBytes[4];
				if (ddsSizeCheck != 124)
				{
					Debug.Log("Invalid DDS DXTn texture. Unable to read. Oh no, looks like the file wasnt a dds, or maybe it's corrupted, check it out!");  //this header byte should be 124 for DDS image files
					return Texture2D.whiteTexture;
				}
				int height = ddsBytes[13] * 256 + ddsBytes[12];
				int width = ddsBytes[17] * 256 + ddsBytes[16];

				int DDS_HEADER_SIZE = 128;
				byte[] dxtBytes = new byte[ddsBytes.Length - DDS_HEADER_SIZE];
				Buffer.BlockCopy(ddsBytes, DDS_HEADER_SIZE, dxtBytes, 0, ddsBytes.Length - DDS_HEADER_SIZE);


				Texture2D texture = new Texture2D(width, height, textureFormat, false);
				texture.LoadRawTextureData(dxtBytes);
				texture.Apply();

				Color[] pixels = texture.GetPixels(0);

				for (int i = 0; i < height / 2; i++)
				{
					for (int j = 0; j < width; j++)
					{
						int alpha = i * width + j;
						int beta = (height - 1 - i) * width + j;
						if(beta >= pixels.Length || beta < 0){
							Debug.Log("Beta was " + beta + " from heigh: " + height + " minus 1 minus: " + i + " times: " + width + " plus: " + j + " and length is " + dxtBytes.Length);
						}
						Color buffer = pixels[alpha];
						pixels[alpha] = pixels[beta];
						pixels[beta] = buffer;
					}
				}

				Texture2D finalTexture = new Texture2D(width, height);

				finalTexture.SetPixels(pixels);
				finalTexture.Apply();

				return finalTexture;
			}
			catch (Exception e)
			{
				Debug.LogError("Oopsie daisy! Something went wrong!\n\n" + e.Message);
				Debug.LogError(e.StackTrace);
				return Texture2D.whiteTexture;
			}
		}

		public static Texture2D LoadTGA(string fileName)
		{
			using (var imageFile = File.OpenRead(fileName))
			{
				return LoadTGA(imageFile);
			}
		}

		public static Texture2D LoadTGA(Stream TGAStream)
		{
			try
			{
				using (BinaryReader r = new BinaryReader(TGAStream))
				{

					// Skip some header info we don't care about.
					// Even if we did care, we have to move the stream seek point to the beginning,
					// as the previous method in the workflow left it at the end.
					r.BaseStream.Seek(12, SeekOrigin.Begin);

					short width = r.ReadInt16();
					short height = r.ReadInt16();
					int bitDepth = r.ReadByte();

					// Skip a byte of header information we don't care about.
					r.BaseStream.Seek(1, SeekOrigin.Current);

					Texture2D tex = new Texture2D(width, height);
					Color32[] pulledColors = new Color32[width * height];

					if (bitDepth == 32)
					{
						for (int i = 0; i < width * height; i++)
						{
							byte red = r.ReadByte();
							byte green = r.ReadByte();
							byte blue = r.ReadByte();
							byte alpha = r.ReadByte();

							pulledColors[i] = new Color32(blue, green, red, alpha);
						}
					}
					else if (bitDepth == 24)
					{
						for (int i = 0; i < width * height; i++)
						{
							byte red = r.ReadByte();
							byte green = r.ReadByte();
							byte blue = r.ReadByte();

							pulledColors[i] = new Color32(blue, green, red, 1);
						}
					}
					else
					{
						Debug.Log("TGA texture had non 32/24 bit depth.");
						return Texture2D.whiteTexture;
					}

					tex.SetPixels32(pulledColors);
					tex.Apply();
					return tex;

				}
			}
			catch (Exception e)
			{
				Debug.LogError("Oopsie daisy! Something went wrong!\n\n" + e.Message);
				return Texture2D.whiteTexture;
			}
			
		}



		public static ScnData LoadModel(string path, bool identityMatrix = false)
		{
			SceneContainer container = SceneContainer.ReadFrom(path);
			var go = new GameObject(container.Header.Name);
			container.fileInfo = new FileInfo(path);
			ScnData sd = go.AddComponent<ScnData>();
			sd.filePath = path;

			BuildFromContainer(container, go, identityMatrix);

			return sd;
		}
	}

	public class TreeItem<T>
	{
		public T item;
		public List<TreeItem<T>> childs;
		public TreeItem<T> parent;
	}

	public static class ModelChunkImporter
	{

		public static Dictionary<string, Material> MainMaterials = new Dictionary<string, Material>();
		public static Dictionary<string, Material> SideMaterials = new Dictionary<string, Material>();

		public static Mesh CreateMesh(ModelChunk model)
		{
			Mesh mesh = new Mesh();

			var verts = model.Mesh.Vertices.ToArray().Clone() as Vector3[];

			for (int i = 0; i < verts.Length; i++)
			{
				verts[i] = verts[i] / ScnToolData.Instance.scale;
			}

			mesh.vertices = verts;

			mesh.normals = model.Mesh.Normals.ToArray().Clone() as Vector3[];
			mesh.tangents = model.Mesh.TangentsArray().Clone() as Vector4[];
			mesh.uv = model.Mesh.UV.ToArray().Clone() as Vector2[];
			mesh.uv2 = model.Mesh.UV2.ToArray().Clone() as Vector2[];

			return mesh;
		}

		public static void SetSkinnedMesh(GameObject go, ModelChunk model, Mesh mesh, Material[] mats)
		{
			SkinnedMeshRenderer smr = go.AddComponent<SkinnedMeshRenderer>();
			smr.sharedMesh = mesh;
			smr.materials = mats;

			//WeightBone
			byte[] bonesPerVertex = new byte[mesh.vertexCount];
			List<BoneWeight1>[] weights = new List<BoneWeight1>[mesh.vertexCount];
			List<Transform> bones = new List<Transform>();
			List<Matrix4x4> bindPoses = new List<Matrix4x4>();

			ExtractBoneWeightData(model, bones, bindPoses, bonesPerVertex, weights);

			for (int i = 0; i < bindPoses.Count; i++)
			{
				var pose = bindPoses[i];
				bindPoses[i] = Matrix4x4.TRS(pose.GetPosition() / ScnToolData.Instance.scale, pose.rotation, pose.lossyScale);
			}
			List<BoneWeight1> w = new List<BoneWeight1>();
			for (int i = 0; i < weights.Length; i++)
			{
				w.AddRange(weights[i]);
			}

			NativeArray<BoneWeight1> w_na = new NativeArray<BoneWeight1>(w.ToArray(), Allocator.Temp);

			NativeArray<byte> bpv = new NativeArray<byte>(bonesPerVertex, Allocator.Temp);

			smr.sharedMesh.SetBoneWeights(bpv, w_na);
			smr.sharedMesh.bindposes = bindPoses.ToArray();
			smr.bones = bones.ToArray();

			GameObject rootBone = null;


			foreach (SceneChunk chunk in ScnFileImporter.createdObjects.Keys)
			{
				if (ScnFileImporter.createdObjects[chunk].name == model.SubName)
				{
					rootBone = ScnFileImporter.createdObjects[chunk];
					break;
				}
			}

			if (rootBone != null)
			{
				smr.rootBone = rootBone.transform;
			}

			smr.ResetBounds();
		}

		public static void ExtractBoneWeightData(ModelChunk model, List<Transform> bones, List<Matrix4x4> bindPoses, byte[] bonesPerVertex, List<BoneWeight1>[] weights)
		{
			for (int i = 0; i < model.WeightBone.Count; i++)
			{
				int boneIndex = GetBoneIndexFromBoneName(model.WeightBone[i].Name, bones);
				bindPoses.Add(model.WeightBone[i].Matrix);

				if (boneIndex == -1)
				{
					Debug.LogWarning($"Bone '{model.WeightBone[i].Name}' couldnt be found. Did you made an oppsie? :P");
					continue;
				}


				for (int j = 0; j < model.WeightBone[i].Weight.Count; j++)
				{
					int index = (int)(model.WeightBone[i].Weight[j].Vertex);
					bonesPerVertex[index]++;
					if (weights[index] == null)
					{
						weights[index] = new List<BoneWeight1>();
					}
					BoneWeight1 bw = new BoneWeight1();
					bw.boneIndex = boneIndex;
					bw.weight = model.WeightBone[i].Weight[j].Weight;
					weights[index].Add(bw);
				}
			}
		}

		public static Material[] SetFaceData(Mesh mesh, ModelChunk model, DirectoryInfo di, TextureReference tr)
		{
			Material mat = ScnToolData.GetMatFromShader(model.Shader);

			Material[] mats = new Material[model.TextureData.Textures.Count];

			if (model.TextureData.Textures.Count > 0)
			{
				mesh.subMeshCount = model.TextureData.Textures.Count;
				int[] tris = model.Mesh.Triangles();

				for (int i = 0; i < tris.Length; i++)
				{
					if (tris[i] >= mesh.vertexCount)
					{
						Debug.Log("Goodness me, some face index are above the vert count X:");
					}
					if (tris[i] < 0)
					{
						Debug.Log("Goodness me, face index are negative D:");
					}
				}

				if (model.TextureData.ExtraUV == 2)
				{
					tr.isNormal = true;
				}

				for (int i = 0; i < model.TextureData.Textures.Count; i++)
				{
					TextureEntry texEntry = model.TextureData.Textures[i];
					mesh.SetTriangles(tris, texEntry.face_offset * 3, texEntry.face_count * 3, i);

					Material mat_x = new Material(mat);
					string mainTex = string.Empty;
					if (texEntry.main_texture != string.Empty)
					{
						mainTex = di.FullName + Path.DirectorySeparatorChar + texEntry.main_texture.Replace(".tga", ".dds");

						if (File.Exists(mainTex))
						{
							mat_x.mainTexture = LoadTexture(mainTex);
						}
						else{
							string localMainTex = Application.dataPath + Path.DirectorySeparatorChar + "S4 Map Textures" + Path.DirectorySeparatorChar + texEntry.main_texture.Replace(".tga", ".dds");
							mainTex = "Assets" +  Path.DirectorySeparatorChar + "S4 Map Textures" + Path.DirectorySeparatorChar + texEntry.main_texture.Replace(".tga", ".dds");
							if(File.Exists(localMainTex)){
								mat_x.mainTexture = LoadTexture(localMainTex);
							}	
						} 
					}

					string sideTex = string.Empty;

					if (texEntry.side_texture != string.Empty)
					{
						sideTex = di.FullName + Path.DirectorySeparatorChar + texEntry.side_texture.Replace(".tga", ".dds");

						Debug.Log("sideTex: " + sideTex);
						Texture2D st = null;
						if (File.Exists(sideTex))
						{
							st = LoadTexture(sideTex);
						}
						else{
							string localSideTex = Application.dataPath + Path.DirectorySeparatorChar + "S4 Map Textures" + Path.DirectorySeparatorChar + texEntry.side_texture.Replace(".tga", ".dds");
							sideTex = "Assets" +  Path.DirectorySeparatorChar + "S4 Map Textures" + Path.DirectorySeparatorChar + texEntry.side_texture.Replace(".tga", ".dds");

							Debug.Log("localSideTex: " + localSideTex);
							if(File.Exists(localSideTex)){
								st = LoadTexture(localSideTex);
								sideTex = localSideTex;
							}	
						}
						if (model.TextureData.ExtraUV == 1)
						{
							mat_x.SetTexture("_DetailAlbedoMap", st);
							mat_x.EnableKeyword("_DETAIL_MULX2");
						}
						else
						{
							mat_x.SetTexture("_BumpMap", st);
							mat_x.EnableKeyword("_NORMALMAP");
						}
					}

					tr.textures.Add(new TextureItem(texEntry.main_texture, mainTex, sideTex));
					mats[i] = mat_x;
				}
			}
			else
			{
				mesh.triangles = model.Mesh.Triangles();
			}

			return mats;
		}

		static Texture2D LoadTexture(string path )
		{
			if (path.EndsWith(".tga"))
			{
				if (File.Exists(path))
				{
					return ScnFileImporter.LoadTGA(path);
				}
				else
				{
					path = path.Replace(".tga", ".dds");

					if (File.Exists(path))
					{
						return ScnFileImporter.ParseTextureDXT(File.ReadAllBytes(path));
					}
				}
			}
			else if (path.EndsWith(".dds"))
			{
				if (File.Exists(path))
				{
					return ScnFileImporter.ParseTextureDXT(File.ReadAllBytes(path));
				}
				else
				{
					path = path.Replace(".dds", ".tga");

					if (File.Exists(path))
					{
						return ScnFileImporter.LoadTGA(path);
					}
				}
			}
			return Texture2D.whiteTexture;
		}

		public static int GetBoneIndexFromBoneName(string name, List<Transform> bones)
		{
			GameObject go = null;

			foreach (SceneChunk chunk in ScnFileImporter.createdObjects.Keys)
			{
				if (ScnFileImporter.createdObjects[chunk].name == name)
				{
					go = ScnFileImporter.createdObjects[chunk];
					break;
				}
			}

			if (go == null) return -1;

			bones.Add(go.transform);
			return bones.Count - 1;
		}



		public static void SetOctMesh(GameObject go, ModelChunk model, Mesh mesh)
		{
			MeshCollider mc = go.AddComponent<MeshCollider>();
			CollisionData cd = go.AddComponent<CollisionData>();

			string oct_name = model.Name[4..];
			if (oct_name == "NONE")
			{
				cd.ground = GroundFlag.blast;
			}
			else if (Double.TryParse(oct_name, out _))
			{
				cd.ground = GroundFlag.blast;
			}
			else if (Enum.TryParse(oct_name, out GroundFlag result1))
			{
				cd.ground = result1;
			}
			else if (Enum.TryParse(oct_name, out WeaponFlag result2))
			{
				cd.weapon = result2;
			}
			else
			{
				cd.ground = GroundFlag.blast;
			}

			mc.cookingOptions = MeshColliderCookingOptions.EnableMeshCleaning;
			if (mesh.vertices.Length > 0)
			{
				mc.sharedMesh = mesh;
			}
		}

		public static int ModelAnimationIsTransform(List<ModelAnimation> anims)
		{
			if (anims.Count == 0)
			{
				return -1;
			}
			else if (anims.Count > 1)
			{
				return 0;
			}
			else if (anims.Count == 1)
			{
				var anim = anims[0];
				if (anim.transformKeyData2.TransformKey.TKey.Count != 0)
				{
					return 0;
				}
				if (anim.transformKeyData2.TransformKey.RKey.Count != 0)
				{
					return 0;
				}
				if (anim.transformKeyData2.TransformKey.SKey.Count != 0)
				{
					return 0;
				}
				if (anim.transformKeyData2.MorphKeys.Count != 0)
				{
					return 0;
				}
				if (anim.transformKeyData2.AlphaKeys.Count == 1)
				{
					if (anim.transformKeyData2.AlphaKeys[0].Alpha != 1f)
					{
						return 2;
					}
				}
				if (anim.transformKeyData2.AlphaKeys.Count != 0)
				{
					return 3;
				}
				return 1;
			}
			return 0;
		}
	}

	public static class AnimationImporter
	{
		public static List<TreeItem<(string Object_Name, List<S4Animation> anims)>> LoadAnimations(string path)
		{
			SceneContainer sc = SceneContainer.ReadFrom(path);
			if (sc != null)
			{
				return LoadAnimations(sc);
			}
			return null;
		}
		public static List<TreeItem<(string Object_Name, List<S4Animation> anims)>> LoadAnimations(SceneContainer container)
		{
			List<TreeItem<SceneChunk>> root = ScnFileImporter.GetRootItems(container);
			List<TreeItem<(string Object_Name, List<S4Animation>)>> anims = new();
			for (int i = 0; i < root.Count; i++)
			{
				anims.Add(Transcript(root[i]));
			}
			return anims;

		}

		static TreeItem<(string Object_Name, List<S4Animation> anims)> Transcript(TreeItem<SceneChunk> node)
		{
			TreeItem<(string Object_Name, List<S4Animation> anims)> anim_node = new();
			anim_node.item.Object_Name = node.item.Name;
			anim_node.item.anims = new();
			if (node.item is ModelChunk mc)
			{
				for (int i = 0; i < mc.Animation.Count; i++)
				{
					anim_node.item.anims.Add(new S4Animation(mc.Animation[i]));
				}
			}
			else if (node.item is BoneChunk bc)
			{
				for (int i = 0; i < bc.Animation.Count; i++)
				{
					anim_node.item.anims.Add(new S4Animation(bc.Animation[i]));
				}
			}
			for (int i = 0; i < node.childs.Count; i++)
			{
				var child = Transcript(node.childs[i]);
				anim_node.childs.Add(child);
				child.parent = anim_node;
			}
			return anim_node;
		}
	}

	public static class ScnFileExporter
	{
		static List<string> usedNames = new List<string>();

		public static Dictionary<int,(string name,Texture2D tex)> lightmaps = new Dictionary<int,(string name, Texture2D tex)>();

		public static string lightmapName = "";

		public static SceneContainer CreateContainerFromScenes(string name, ScnData[] scenes)
		{
			usedNames.Clear();
			lightmaps.Clear();
			SceneContainer container = new SceneContainer();
			container.Header.Name = name;
			foreach (var scene in scenes)
			{
				lightmapName = scene.lightmapName;
				CreateChunksFromChildren(scene.transform,scene.transform, container, null);
				S4Armature armature = scene.GetComponent<S4Armature>();
				if(armature != null){
					armature.WriteAnimationsToSceneContainer(container);
				}
			}
			lightmapName = "";

			return container;
		}

		public static void CreateChunksFromChildren(Transform parent, Transform relativeParent, SceneContainer container, SceneChunk parentChunk)
		{
			try{
				for (int i = 0; i < parent.childCount; i++)
				{
					Transform child = parent.GetChild(i);
					if (!child.gameObject.activeInHierarchy)
					{
						continue;
					}
	
					SceneChunk childChunk = CreateChunk(child, container, parentChunk, relativeParent);
	
					Transform relative;
	
					if (childChunk != null)
					{
						relative = child;
					}
					else
					{
						relative = relativeParent;
						childChunk = parentChunk;
					}
	
					CreateChunksFromChildren(child, relative, container, childChunk);
					EditorUtility.DisplayProgressBar($"Parsing unity scene! OwO", $"{child.name}", (float)i/(float)parent.childCount);
				}
            	EditorUtility.ClearProgressBar();
			}
			catch(Exception e){
				EditorUtility.ClearProgressBar();
				Debug.LogError(e.StackTrace);
				throw e;
			}

		}

		static SceneChunk CreateChunk(Transform child, SceneContainer container, SceneChunk parentChunk, Transform relativeParent)
		{
			Bone bone = child.GetComponent<Bone>();
			if (bone) return CreateBoneChunk(bone, container, parentChunk);
			
			Bonesystem bonesystem = child.GetComponent<Bonesystem>();
			if (bonesystem) return CreateBoneSystemChunk(bonesystem, container, parentChunk);

			SkinnedMeshRenderer smr = child.GetComponent<SkinnedMeshRenderer>();
			if (smr) return ModelChunkExporter.CreateModelChunkSkinned(smr, container, parentChunk, relativeParent);

			MeshRenderer mr = child.GetComponent<MeshRenderer>();
			if (mr)
			{
				var mesh = ModelChunkExporter.CreateModelChunk(mr, container, parentChunk, relativeParent);
				CollisionData cd_mr = child.GetComponent<CollisionData>();
				if (cd_mr)
				{
					ModelChunkExporter.CreateCollisionChunk(cd_mr, container, mesh);
				}
				return mesh;
			}

			CollisionData cd = child.GetComponent<CollisionData>();
			if (cd) return ModelChunkExporter.CreateCollisionChunk(cd, container, parentChunk);

			BoxCollider bc = child.GetComponent<BoxCollider>();
			if (bc) return CreateBoxChunk(bc, container, parentChunk);

			LineRenderer lr = child.GetComponent<LineRenderer>();
			if (lr) return CreateShapeChunks(lr, container);

			return null;
		}

		static BoneChunk CreateBoneChunk(Bone bone, SceneContainer container, SceneChunk parent)
		{
			BoneChunk b = MakeChunk<BoneChunk>(container, bone.name);
			while (ValidateName(b.Name) == false)
			{
				b.Name = b.Name + ScnToolData.GetRandomName();
			}
			b.SubName = parent != null ? parent.Name : container.Header.Name;

			Vector3 position = bone.transform.localPosition * ScnToolData.Instance.scale;
			Quaternion rotation = bone.transform.localRotation;
			Vector3 scale = bone.transform.localScale;
			if (parent == null)
			{
				position = bone.transform.position * ScnToolData.Instance.scale;
				rotation = bone.transform.rotation;
				scale = bone.transform.lossyScale;
			}

			b.Matrix = Matrix4x4.TRS(
				position,
				rotation,
				scale);

			S4Animations s4a = bone.s4Animations;
			if (s4a)
			{
				b.Animation = s4a.ToBoneAnimation();
			}
			else
			{
				b.Animation = new List<BoneAnimation>();
				BoneAnimation ma = new BoneAnimation();
				ma.TransformKeyData = new TransformKeyData();
				ma.Name = ScnToolData.Instance.main_animation_name;
				ma.TransformKeyData.TransformKey = new TransformKey();
				ma.TransformKeyData.TransformKey.Translation = position;
				ma.TransformKeyData.TransformKey.Rotation = rotation;
				ma.TransformKeyData.TransformKey.Scale = scale;
				b.Animation.Add(ma);
			}

			return b;
		}

		static BoneSystemChunk CreateBoneSystemChunk(Bonesystem rootBone, SceneContainer container, SceneChunk parent)
		{
			BoneSystemChunk boneSys = MakeChunk<BoneSystemChunk>(container, rootBone.name);
			while (ValidateName(boneSys.Name) == false)
			{
				boneSys.Name = boneSys.Name + ScnToolData.GetRandomName();
			}

			if (boneSys.Name != "BONESYSTEM")
			{
				Debug.LogWarning("Your bonesystem isnt named 'BONESYSTEM'! as far as i know all bonesystems are called like that so if you're encountering errors then checkthat~~~!", rootBone.gameObject);
			}
			boneSys.SubName = parent != null ? parent.Name : container.Header.Name;

			boneSys.Matrix = Matrix4x4.TRS(
				rootBone.transform.position * ScnToolData.Instance.scale,
				rootBone.transform.rotation,
				rootBone.transform.lossyScale);

			return boneSys;
		}

		static BoxChunk CreateBoxChunk(BoxCollider bc, SceneContainer container, SceneChunk parentChunk)
		{
			string boxName = bc.name;
			var pd = bc.GetComponent<PointDrawer>();
			if (pd)
			{

			}

			BoxChunk box = MakeChunk<BoxChunk>(container, boxName);
			while (ValidateName(box.Name) == false)
			{
				box.Name = box.Name + ScnToolData.GetRandomName();
			}

			if (parentChunk != null)
			{
				if (parentChunk.ChunkType == ChunkType.Bone || parentChunk.ChunkType == ChunkType.Box)
				{
					box.SubName = parentChunk.Name;
					box.Matrix = Matrix4x4.TRS(bc.transform.localPosition * ScnToolData.Instance.scale, bc.transform.localRotation, bc.transform.localScale);
					box.Size = bc.size * ScnToolData.Instance.scale / 2;
					return box;
				}
			}

			box.SubName = container.Header.Name;
			box.Matrix = Matrix4x4.TRS(bc.transform.position * ScnToolData.Instance.scale, bc.transform.rotation, bc.transform.lossyScale);
			box.Size = bc.size * ScnToolData.Instance.scale / 2;
			return box;
		}

		static ShapeChunk CreateShapeChunks(LineRenderer lr, SceneContainer container)
		{
			if (lr.positionCount % 2 != 0)
			{
				Debug.LogWarning("Your LineRenderer had an odd number of positions! This cant be, it's suposed to be in pairs! We're gonna have to ignore it this time, fix it please~~~!", lr.gameObject);
				return MakeChunk<ShapeChunk>(container, lr.name + "_empty"); ;
			}
			ShapeChunk shape = MakeChunk<ShapeChunk>(container, lr.name);
			while (ValidateName(shape.Name) == false)
			{
				shape.Name = shape.Name + ScnToolData.GetRandomName();
			}


			if (lr.transform.parent != null) shape.SubName = lr.transform.parent.name;

			shape.Matrix = Matrix4x4.TRS(lr.transform.localPosition * ScnToolData.Instance.scale, lr.transform.localRotation, lr.transform.localScale);

			for (int i = 0; i < lr.positionCount; i += 2)
			{
				shape.Unk.Add(Tuple.Create(lr.GetPosition(i) * ScnToolData.Instance.scale, lr.GetPosition(i + 1) * ScnToolData.Instance.scale));
			}

			return shape;
		}

		public static bool ValidateName(string name)
		{
			if (name.StartsWith("oct_"))
			{
				return true;
			}
			if (usedNames.Contains(name))
			{
				Debug.LogWarning($"Oh boy! you're using the name '{name}' somewhere else already! That could be a very big issue so im going to rename it inside the scene file!");
				return false;
			}
			else
			{
				usedNames.Add(name);
				return true;
			}
		}

		public static T MakeChunk<T>(SceneContainer container, string name) where T : SceneChunk
		{
			T chunk = (T)Activator.CreateInstance(typeof(T), new object[] { container });
			container.Add(chunk);
			chunk.Name = name;

			return chunk;
		}


		public static byte[] WriteTextureDXT(Texture2D texture)
		{
			var colors = texture.GetPixels32();
			byte[] bytes = new byte[colors.Length * 4];
			for (int i = 0; i < colors.Length; i++)
			{
				int bi = i * 4;
				bytes[bi] = colors[i].r;
				bytes[bi + 1] = colors[i].g;
				bytes[bi + 2] = colors[i].b;
				bytes[bi + 3] = colors[i].a;
			}

			byte[] magic = System.Text.Encoding.ASCII.GetBytes(new char[] { 'D', 'D', 'S', ' ' });
			int Size = 124;
			byte[] Flags = StrToByteArray("00000000");
			int Height = texture.height;
			int Width = texture.width;
			int PitchOrLinearSize = 0;
			int Depth = 0;
			int MipMapCount = 1;
			byte[] Reserved1 = new byte[44];//[11];
											//pixel format
			int pfSize = 32;
			var pfFlags = StrToByteArray("41000000");
			byte[] pfFourCC = System.Text.Encoding.ASCII.GetBytes(new char[] { 'D', 'X', 'T', '1' });
			if (texture.format == TextureFormat.DXT1)
			{
				pfFourCC = System.Text.Encoding.ASCII.GetBytes(new char[] { 'D', 'X', 'T', '1' });
			}
			else if (texture.format == TextureFormat.DXT5)
			{
				pfFourCC = System.Text.Encoding.ASCII.GetBytes(new char[] { 'D', 'X', 'T', '5' });
			}
			int pfRGBBitCount = 32;
			byte[] pfRBitMask = StrToByteArray("ff000000");
			byte[] pfGBitMask = StrToByteArray("00ff0000");
			byte[] pfBBitMask = StrToByteArray("0000ff00");
			byte[] pfABitMask = StrToByteArray("000000ff");

			var Caps = StrToByteArray("00010000");
			int Caps2 = 0;
			int Caps3 = 0;
			int Caps4 = 0;
			int Reserved2 = 0;

			List<byte> dxtBytes = new List<byte>();
			dxtBytes.AddRange(magic);
			dxtBytes.AddRange(BitConverter.GetBytes(Size));
			dxtBytes.AddRange(Flags);
			dxtBytes.AddRange(BitConverter.GetBytes(Height));
			dxtBytes.AddRange(BitConverter.GetBytes(Width));
			dxtBytes.AddRange(BitConverter.GetBytes(PitchOrLinearSize));
			dxtBytes.AddRange(BitConverter.GetBytes(Depth));
			dxtBytes.AddRange(BitConverter.GetBytes(MipMapCount));
			dxtBytes.AddRange(Reserved1);
			dxtBytes.AddRange(BitConverter.GetBytes(pfSize));
			dxtBytes.AddRange(pfFlags);
			dxtBytes.AddRange(pfFourCC);
			dxtBytes.AddRange(BitConverter.GetBytes(pfRGBBitCount));
			dxtBytes.AddRange(pfRBitMask);
			dxtBytes.AddRange(pfGBitMask);
			dxtBytes.AddRange(pfBBitMask);
			dxtBytes.AddRange(pfABitMask);
			dxtBytes.AddRange(Caps);
			dxtBytes.AddRange(BitConverter.GetBytes(Caps2));
			dxtBytes.AddRange(BitConverter.GetBytes(Caps3));
			dxtBytes.AddRange(BitConverter.GetBytes(Caps4));
			dxtBytes.AddRange(BitConverter.GetBytes(Reserved2));
			dxtBytes.AddRange(bytes);

			return dxtBytes.ToArray();

			static byte[] StrToByteArray(string str)
			{
				List<byte> hexres = new List<byte>();
				for (int i = 0; i < str.Length; i += 2)
					hexres.Add(Convert.ToByte(str.Substring(i, 2), 16));

				return hexres.ToArray();
			}
		}
	}

	public static class ModelChunkExporter
	{

		public static ModelChunk CreateCollisionChunk(CollisionData cd, SceneContainer container, SceneChunk parentChunk)
		{
			if (cd.ground == GroundFlag.blast)//Dealing with the special case of breakables
			{
				string blast_name = cd.name;
				if(blast_name.StartsWith("oct_") == false){
					blast_name = "oct_" + blast_name;
				}
				return CreateModelChunkFromMeshBlast(
					cd.transform,
					cd.GetComponent<MeshCollider>().sharedMesh,
					blast_name, parentChunk);
			}
			else if (cd.ground != GroundFlag.NONE)
			{
				Mesh mesh = cd.GetComponent<MeshCollider>().sharedMesh;
				if (mesh)
				{
					CreateModelChunkFromMesh(
						cd.transform,
						mesh,
						"oct_" + cd.ground.ToString().Replace("wire", "@").Replace("hash", "#"));
				}
				else
				{
					Debug.LogWarning("Oh No! this collider has no mesh!", cd);
				}
			}

			if (cd.weapon != WeaponFlag.NONE)
			{
				Mesh mesh = cd.GetComponent<MeshCollider>().sharedMesh;
				if (mesh)
				{
					CreateModelChunkFromMesh(cd.transform, mesh, "oct_" + cd.weapon.ToString());
				}
				else
				{
					Debug.LogWarning("Oh No! this collider has no mesh!", cd);
				}

			}

			return null;

			ModelChunk CreateModelChunkFromMesh(Transform transform, Mesh mesh, string name)
			{
				ModelChunk model = ScnFileExporter.MakeChunk<ModelChunk>(container, name);
				model.SubName = container.Header.Name;
				model.Matrix = Matrix4x4.TRS(
					transform.position * ScnToolData.Instance.scale,
					transform.rotation,
					transform.lossyScale);
				model.Shader = RenderFlag.None;

				model.Animation = GetTransformAnimation(transform.position, transform.rotation, transform.lossyScale);

				SetMesh(model.Mesh, mesh, obj: cd.gameObject);


				model.TextureData.Version = 0.2000000029802322f;
				model.TextureData.ExtraUV = 0;

				return model;
			}

			ModelChunk CreateModelChunkFromMeshBlast(Transform transform, Mesh mesh, string name, SceneChunk parentChunk)
			{
				ModelChunk model = ScnFileExporter.MakeChunk<ModelChunk>(container, name);
				while (ScnFileExporter.ValidateName(model.Name) == false)
				{
					model.Name = model.Name + ScnToolData.GetRandomName();
				}
				model.SubName = parentChunk != null ? parentChunk.Name : container.Header.Name;
				model.Matrix = Matrix4x4.TRS(
					transform.localPosition * ScnToolData.Instance.scale,
					transform.localRotation,
					transform.localScale);

				model.Shader = RenderFlag.None;
				SetMesh(model.Mesh, mesh, obj: cd.gameObject);

				model.TextureData.Version = 0.2000000029802322f;
				model.TextureData.ExtraUV = 0;

				model.Animation = new List<ModelAnimation>();
				ModelAnimation ma = new ModelAnimation();
				ma.transformKeyData2 = new TransformKeyData2();
				ma.Name = ScnToolData.Instance.main_animation_name;
				ma.transformKeyData2.TransformKey = new TransformKey();
				ma.transformKeyData2.TransformKey.Translation = transform.localPosition * ScnToolData.Instance.scale;
				ma.transformKeyData2.TransformKey.Rotation = transform.localRotation;
				ma.transformKeyData2.TransformKey.Scale = transform.localScale;
				model.Animation.Add(ma);

				return model;
			}
		}

		public static ModelChunk CreateModelChunk(MeshRenderer mr, SceneContainer container, SceneChunk parentChunk, Transform relativeParent)
		{
			ModelChunk model = ScnFileExporter.MakeChunk<ModelChunk>(container, mr.name);
			while (ScnFileExporter.ValidateName(model.Name) == false)
			{
				model.Name = model.Name + ScnToolData.GetRandomName();
			}


			Mesh mesh;

			TextureReference tr = mr.GetComponent<TextureReference>();
			MeshFilter mf = mr.GetComponent<MeshFilter>();

#if PROBUILDER
			ProBuilderMesh pbm = mr.GetComponent<ProBuilderMesh>();
			if (pbm)
			{
				mesh = GetMeshFromPBM(pbm, tr?tr.hasLightmap:false);
			}
			else
#endif
			if (mf)
			{
				mesh = mf.sharedMesh;
			}
			else
			{
				mesh = new Mesh();
				Debug.LogWarning("Boy Oh boy! Girl Oh Girl! My Sweet summerchild, you're lacking a MeshFilter or a ProBuilderMesh! That's so crazy! How did this happen? anyway, make sure this object is fine!", mr);
			}

			if (tr)
			{
				model.Shader = tr.renderFlags;
				bool uv2 = tr.hasLightmap;
				//Debug.Log("Has lightmap: " + uv2);
				SetMesh(model.Mesh, mesh, tr.flipUvHorizontal, mr.gameObject, uv2: uv2);
			}
			else
			{
				model.Shader = RenderFlag.None;
				SetMesh(model.Mesh, mesh, false, mr.gameObject, uv2: false);



				Debug.LogError("Goodness me! You have a mesh without a texture reference! That will make the mesh have so much problems!", mr.gameObject);
			}


			for (int i = 0; i < model.Mesh.UV2.Count; i++)
			{
				float x = (model.Mesh.UV2[i].x * mr.lightmapScaleOffset.x) + mr.lightmapScaleOffset.z;
				float y = (model.Mesh.UV2[i].y * mr.lightmapScaleOffset.y) + mr.lightmapScaleOffset.w;
				model.Mesh.UV2[i] = new Vector2(x, y);
			}

			if (tr)
			{
				if (ScnToolData.Instance.uv_flipHorizontal_lm ^ tr.flipUvHorizontal_lm)
				{
					for (int i = 0; i < model.Mesh.UV2.Count; i++)
					{
						model.Mesh.UV2[i] = new Vector2(1-model.Mesh.UV2[i].x, model.Mesh.UV2[i].y);
					}
				}

				SetTextureData(model.TextureData, mesh, tr);
			}
			
			(Vector3 position, Quaternion rotation, Vector3 scale) = DealWithModelParenting(mr.transform, relativeParent, model, parentChunk, container);

			S4Animations s4a = mr.GetComponent<S4Animations>();
			if (s4a)
			{
				if (s4a is ParametricAnim s4ap)
				{
					model.Animation = s4ap.ToModelAnimation();					
				}
				else{
					model.Animation = s4a.ToModelAnimation();
				}
			}
			else
			{
				model.Animation = new List<ModelAnimation>();
				ModelAnimation ma = new ModelAnimation();
				ma.transformKeyData2 = new TransformKeyData2();
				ma.Name = ScnToolData.Instance.main_animation_name;
				ma.transformKeyData2.TransformKey.Translation = position;
				ma.transformKeyData2.TransformKey.Rotation = rotation;
				ma.transformKeyData2.TransformKey.Scale = scale;
				if (tr.transparency != 1f)
				{
					ma.transformKeyData2.AlphaKeys.Add(new FloatKey() { Alpha = tr.transparency, frame = 0 });
				}
				model.Animation.Add(ma);
			}

			return model;
		}

		public static ModelChunk CreateModelChunkSkinned(SkinnedMeshRenderer smr, SceneContainer container, SceneChunk parentChunk, Transform relativeParent)
		{
			ModelChunk model = ScnFileExporter.MakeChunk<ModelChunk>(container, smr.name);
			while (ScnFileExporter.ValidateName(model.Name) == false)
			{
				model.Name = model.Name + ScnToolData.GetRandomName();
			}
			if (smr.transform.parent != null) model.SubName = smr.transform.parent.name;


			(Vector3 position, Quaternion rotation, Vector3 scale) = DealWithModelParenting(smr.transform, relativeParent, model, parentChunk, container);

			Mesh mesh = smr.sharedMesh;


			TextureReference tr = smr.GetComponent<TextureReference>();
			if (tr)
			{
				model.Shader = tr.renderFlags;
				SetMesh(model.Mesh, mesh, tr.flipUvHorizontal, smr.gameObject);
			}
			else{
				model.Shader = RenderFlag.None;
				SetMesh(model.Mesh, mesh, false, smr.gameObject);
			}

			SetTextureData(model.TextureData, mesh, tr);

			foreach (var transform in smr.bones)
			{
				WeightBone wb = new WeightBone();
				wb.Name = transform.name;
				model.WeightBone.Add(wb);
			}
			for (int i = 0; i < smr.sharedMesh.bindposes.Length; i++)
			{
				var pose = smr.sharedMesh.bindposes[i];
				model.WeightBone[i].Matrix = Matrix4x4.TRS( pose.GetPosition() * ScnToolData.Instance.scale, pose.rotation,pose.lossyScale);
			}
			NativeArray<byte> bpv = smr.sharedMesh.GetBonesPerVertex();
			NativeArray<BoneWeight1> bw = smr.sharedMesh.GetAllBoneWeights();
			int counter = 0;
			for (int i = 0; i < bpv.Length; i++)
			{
				for (byte j = 0; j < bpv[i]; j++)
				{
					WeightData wd = new WeightData();
					wd.Vertex = (uint)i;
					wd.Weight = bw[counter].weight;
					model.WeightBone[bw[counter].boneIndex].Weight.Add(wd);
					counter++;
				}
			}
			S4Animations s4a = smr.GetComponent<S4Animations>();
			if (s4a)
			{
				model.Animation = s4a.ToModelAnimation();
				if (model.Animation.Count == 0)
				{
					Debug.LogWarning("Oh my! This model has no animation Data! That's gonna me the model have a position of zero and a scale of 1!", s4a);
				}
				else
				{
					model.Animation[0].transformKeyData2.TransformKey.Translation *= ScnToolData.Instance.scale;
				}
			}
			else
			{
				model.Animation = new List<ModelAnimation>();
				ModelAnimation ma = new ModelAnimation();
				ma.transformKeyData2 = new TransformKeyData2();
				ma.Name = ScnToolData.Instance.main_animation_name;
				ma.transformKeyData2.TransformKey = new TransformKey();
				ma.transformKeyData2.TransformKey.Translation = position;
				ma.transformKeyData2.TransformKey.Rotation = rotation;
				ma.transformKeyData2.TransformKey.Scale = scale;
				model.Animation.Add(ma);
			}

			return model;
		}

		static void SetMesh(MeshData meshData, Mesh mesh, bool flipUvHorizontal = false, UnityEngine.Object obj = null, bool uv2 = false)
		{
			if (mesh == null)
			{
				meshData.Vertices = new List<Vector3>();
				meshData.Normals = new List<Vector3>();
				meshData.UV = new List<Vector2>();
				meshData.UV2 = new List<Vector2>();
				Debug.Log("Goodness me! this object has an empty mesh!", obj);

				return;
			}
			var verts = new List<Vector3>(mesh.vertices);
			for (int i = 0; i < verts.Count; i++)
			{
				verts[i] *= ScnToolData.Instance.scale;
			}

			meshData.Vertices = verts;
			meshData.Normals = new List<Vector3>(mesh.normals);
			meshData.UV = new List<Vector2>(mesh.uv);

			
			if (uv2)
			{
				meshData.UV2 = new List<Vector2>(mesh.uv2);
			}

			if (ScnToolData.Instance.uv_flipHorizontal ^ flipUvHorizontal)
			{
				for (int i = 0; i < meshData.UV.Count; i++)
				{
					meshData.UV[i] = new Vector2(-meshData.UV[i].x, meshData.UV[i].y);
				}
			}


			meshData.SetTangents(mesh.tangents);
			meshData.SetTriangles(mesh.triangles);
		}

		static void SetTextureData(TextureData textureData, Mesh mesh, TextureReference textures)
		{
			textureData.Version = 0.2000000029802322f;
			textureData.ExtraUV = 0;
			if (textures == null)return;
			
			if (textures.isNormal){
				textureData.ExtraUV = 2;
			}
			else if (textures.hasLightmap)
			{
				if (mesh.uv2.Length == 0)
				{
					textureData.ExtraUV = 0;
					Debug.LogWarning($"Oh gosh! {textures.gameObject.name} is set to have lightmaps, however the UV2 array is empty!" +
					"Either define those in your modeling program or enable 'Generate Lightmap UVs' in the import settings!!");
				}
				else{
					textureData.ExtraUV = 1;
				}
			}	

			if (textures.textures.Count > mesh.subMeshCount)
			{
				Debug.Log($"Oh no! {textures.gameObject.name} has more textures in texture reference than submeshes in it's mesh!", textures.gameObject);
			}
			else if (textures.textures.Count < mesh.subMeshCount)
			{
				Debug.Log($"Oh no! {textures.gameObject.name} has more submeshes in it's mesh than textures intexture reference!", textures.gameObject);
			}

			for (int i = 0; i < mesh.subMeshCount; i++)
			{
				TextureEntry te = new TextureEntry();
				te.face_count = mesh.GetSubMesh(i).indexCount / 3;
				te.face_offset = mesh.GetSubMesh(i).indexStart / 3;

				if (i >= textures.textures.Count)
				{
					te.main_texture = "MissingTexture.tga";
				}
				else
				{
					var ti = textures.textures[i];
					//Main Texture
					if (ti == null)
					{
						te.main_texture = "NullTexture.tga";
					}
					else if (ti.mainTexturePath != string.Empty)
					{
						te.main_texture = new FileInfo(ti.mainTexturePath).Name;
					}
					else
					{
						te.main_texture = "EmptyTexture.tga";
					}

					//Side Texture
					if (textures.isNormal)
					{
						if (ti.sideTexturePath != string.Empty)
						{
							te.side_texture = new FileInfo(ti.sideTexturePath).Name;
						}
					}
					else
					{
						if (!textures.ignoreLightmaps)
						{
							if (ti.sideTexturePath != string.Empty)
							{
								te.side_texture = new FileInfo(ti.sideTexturePath).Name;
							}
							else
							{
								var mr = textures.GetComponent<MeshRenderer>();
								if (mr != null && mr.lightmapIndex != -1 && mr.lightmapIndex < LightmapSettings.lightmaps.Length)
								{
									var lm = LightmapSettings.lightmaps[mr.lightmapIndex].lightmapColor;


									string textureName = lm.name;
									if (ScnFileExporter.lightmapName != string.Empty)
									{
										textureName = ScnFileExporter.lightmapName + "_" + mr.lightmapIndex;
									}

									ScnFileExporter.lightmaps[mr.lightmapIndex] = (textureName, lm);

									te.side_texture = textureName + ".tga";
								}
							}
						}
					}
				}
				textureData.Textures.Add(te);
			}
		}


		static (Vector3, Quaternion, Vector3) DealWithModelParenting(Transform transform, Transform relativeParent, SceneChunk chunk, SceneChunk parentChunk, SceneContainer container)
		{
			Vector3 position = transform.position * ScnToolData.Instance.scale;
			Quaternion rotation = transform.rotation;
			Vector3 scale = transform.lossyScale;
			chunk.SubName = container.Header.Name;
			if (parentChunk != null)
			{
				if (parentChunk.ChunkType == ChunkType.Bone || parentChunk.ChunkType == ChunkType.BoneSystem ||
					(parentChunk.ChunkType == ChunkType.ModelData/* &&
					transform.name.StartsWith("oct_")
					&& !transform.name.StartsWith("oct_land") &&
					!transform.name.StartsWith("oct_weapon")*/))
				{
					chunk.SubName = parentChunk.Name;
					Transform oldParent = transform.transform.parent;
					transform.transform.SetParent(relativeParent, true);

					position = transform.transform.localPosition * ScnToolData.Instance.scale;
					rotation = transform.transform.localRotation;
					scale = transform.transform.localScale;

					transform.transform.SetParent(oldParent, true);
				}
			}

			chunk.Matrix = Matrix4x4.TRS(position, rotation, scale);

			return (position, rotation, scale);
		}

#if PROBUILDER
		static Mesh GetMeshFromPBM(ProBuilderMesh pbm, bool hasLightmap)
		{
			Mesh mesh = new Mesh();

			SetMeshValues(pbm, mesh, hasLightmap);

			IList<Face> faces = pbm.faces;

			Dictionary<int, List<Face>> facesByMat = GetFacesByMat(faces);

			mesh.subMeshCount = facesByMat.Count;

			List<int> indices = new List<int>();
			Dictionary<int, int> triangleCount = new Dictionary<int, int>();

			GetParametersOut(facesByMat, indices, triangleCount);

			SetTriangles(mesh, triangleCount, indices, new List<int>(facesByMat.Keys));

			return mesh;

			void SetMeshValues(ProBuilderMesh pbm, Mesh mesh, bool hasLightmap)
			{
				Vertex[] verts = pbm.GetVertices();
				Vector3[] vertices = new Vector3[verts.Length];
				Vector3[] normals = new Vector3[verts.Length];
				Vector2[] uv = new Vector2[verts.Length];
				Vector2[] uv2 = new Vector2[verts.Length];
				Vector4[] tangents = new Vector4[verts.Length];

				for (int i = 0; i < verts.Length; i++)
				{
					vertices[i] = verts[i].position;
					normals[i] = verts[i].normal;
					uv[i] = verts[i].uv0;
					uv2[i] = verts[i].uv2;
					tangents[i] = verts[i].tangent;
				}

				mesh.vertices = vertices;
				mesh.normals = normals;
				mesh.uv = uv;
				mesh.tangents = tangents;
				if (hasLightmap)
				{
					mesh.uv2 = uv2;
				}
			}

			Dictionary<int, List<Face>> GetFacesByMat(IList<Face> faces)
			{
				Dictionary<int, List<Face>> facesByMat = new Dictionary<int, List<Face>>();
				for (int i = 0; i < faces.Count; i++)
				{
					int submesh = faces[i].submeshIndex;
					if (!facesByMat.ContainsKey(submesh))
					{
						facesByMat.Add(submesh, new List<Face>());
					}
					facesByMat[submesh].Add(faces[i]);
				}

				return facesByMat;
			}

			void GetParametersOut(Dictionary<int, List<Face>> facesByMat, List<int> indices, Dictionary<int, int> triangleCount)
			{
				List<int> keys = new List<int>(facesByMat.Keys);
				for (int i = 0; i < keys.Count; i++)//submeshes
				{
					triangleCount.Add(keys[i], facesByMat[keys[i]].Count);
					foreach (Face face in facesByMat[keys[i]])//faces in this submesh
					{
						if (face.IsQuad())
						{
							triangleCount[keys[i]] += 1;

							int[] quadIndex = face.ToQuad();

							indices.Add(quadIndex[0]);
							indices.Add(quadIndex[1]);
							indices.Add(quadIndex[2]);

							indices.Add(quadIndex[0]);
							indices.Add(quadIndex[2]);
							indices.Add(quadIndex[3]);
						}
						else
						{
							indices.AddRange(face.indexes);
						}
					}
				}
			}

			void SetTriangles(Mesh mesh, Dictionary<int, int> triangleCount, List<int> indices, List<int> keys)
			{
				int trianglesStart = 0;
				for (int i = 0; i < keys.Count; i++)
				{
					int trianglesLength = triangleCount[keys[i]] * 3;
					mesh.SetTriangles(indices, trianglesStart, trianglesLength, i);

					trianglesStart += trianglesLength;
				}
			}
		}

#endif

		static List<ModelAnimation> GetTransformAnimation(Vector3 position, Quaternion rotation, Vector3 scale)
		{
			var a = new List<ModelAnimation>();
			var anim = new ModelAnimation();
			anim.Name = ScnToolData.Instance.main_animation_name;
			anim.transformKeyData2 = new TransformKeyData2();
			anim.transformKeyData2.TransformKey = new TransformKey();
			
			anim.transformKeyData2.TransformKey.Translation = position * ScnToolData.Instance.scale;
			anim.transformKeyData2.TransformKey.Rotation = rotation;
			anim.transformKeyData2.TransformKey.Scale = scale;

			a.Add(anim);
			return a;
		}
	}
}
