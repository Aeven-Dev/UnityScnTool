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
using UnityEngine.ProBuilder;
using System.Linq;

namespace AevenScnTool.IO
{
	public static class ScnFileImporter
	{
		public static Dictionary<string, Material> MainMaterials = new Dictionary<string, Material>();
		public static Dictionary<string, Material> SideMaterials = new Dictionary<string, Material>();
		static Dictionary<SceneChunk, GameObject> createdObjects = new Dictionary<SceneChunk, GameObject>();
/*
		public static void BuildFromContainer2(SceneContainer container, GameObject sceneObj)
		{
			createdObjects.Clear();


			foreach (BoneSystemChunk boneSys in container.boneSystems)
			{
				createdObjects.Add(boneSys, CreateBoneSystem(boneSys));
			}

			foreach (BoneChunk bone in container.bones)
			{
				createdObjects.Add(bone, CreateBone(bone));
			}

			foreach (ModelChunk model in container.models)
			{
				createdObjects.Add(model, CreateModel(model, container.fileInfo.Directory));
			}

			foreach (SkyDirect1Chunk skyDirect in container.skyDirect1List) { }

			foreach (BoxChunk box in container.boxes)
			{
				createdObjects.Add(box, CreateBox(box));
			}

			foreach (ShapeChunk shape in container.shapes)
			{
				createdObjects.Add(shape, CreateShape(shape));
			}


			foreach (SceneChunk chunk in container)
			{
				if (createdObjects.ContainsKey(chunk) == false) continue;

				GameObject child = createdObjects[chunk];
				GameObject parent = null;
				if (chunk.SubName == string.Empty || chunk.SubName == sceneObj.name)
				{
					parent = sceneObj;
				}
				else
				{
					foreach (SceneChunk posibleParent in container)
					{
						if (posibleParent.Name == chunk.SubName)
						{
							parent = createdObjects[posibleParent];
							break;
						}
					}
				}
				if (parent == null)
				{
					parent = sceneObj;
				}
				child.transform.SetParent(parent.transform, false);
			}
		}
		*/
		public static void BuildFromContainer(SceneContainer container, GameObject sceneObj)
		{
			List < TreeItem < SceneChunk >> rootItems = GetRootItems(container);
			
			createdObjects.Clear();
			foreach (var item in rootItems)
			{
				BuildTreeItem(item, container, sceneObj);
			}
		}

		static List<TreeItem<SceneChunk>> GetRootItems(SceneContainer container)
		{
			List<SceneChunk> chunks = new();
			foreach (var item in container)
			{
				chunks.Add(item);
			}

			List<TreeItem<SceneChunk>> items = GetChildItems(chunks, null, container.Header.Name);
			//This comment should disapear TT.TT, oh no, a change!
			return items;
		}

		static List<TreeItem<SceneChunk>> GetChildItems(List<SceneChunk> chunks, TreeItem<SceneChunk> parentItem, string parentName)
		{
			var items = new List<TreeItem<SceneChunk>>();
			for(int i = 0; i < chunks.Count; i++)
			{
				SceneChunk chunk = chunks[i];
				if (parentItem == null)
					if (!(chunk.SubName == parentName || chunk.SubName == string.Empty))
						continue;
				else
					if (chunk.SubName != parentName)
						continue;
					

				var treeItem = new TreeItem<SceneChunk>
				{
					item = chunk,
					parent = parentItem
				};
				var children = GetChildItems(chunks, treeItem, chunk.Name);

				treeItem.childs = children;

				items.Add(treeItem);

			}
			return items;
		}

		static void BuildTreeItem(TreeItem<SceneChunk> treeItem, SceneContainer container, GameObject parent)
		{
			GameObject go = BuildFromChunk(treeItem, container.fileInfo.Directory, parent);
			createdObjects.Add(treeItem.item, go);
			foreach (var child in treeItem.childs)
			{
				BuildTreeItem(child, container, go);
			}
		}

		static GameObject BuildFromChunk(TreeItem<SceneChunk> treeItem, DirectoryInfo di, GameObject parent)
		{
			return treeItem.item.ChunkType switch
			{
				ChunkType.Box => CreateBox(treeItem.item as BoxChunk, parent),
				ChunkType.ModelData => CreateModel(treeItem.item as ModelChunk, di, parent),
				ChunkType.Bone => CreateBone(treeItem.item as BoneChunk, parent),
				ChunkType.SkyDirect1 => CreateSkyDirect1(treeItem.item as SkyDirect1Chunk, parent),
				ChunkType.BoneSystem => CreateBoneSystem(treeItem.item as BoneSystemChunk, parent),
				ChunkType.Shape => CreateShape(treeItem.item as ShapeChunk, parent),
				_ => null,
			};
		}


		static GameObject CreateModel(ModelChunk model, DirectoryInfo di, GameObject parent)
		{
			GameObject go = CreateGameObject(model);
			go.transform.SetParent(parent.transform);


			Mesh mesh = CreateMesh(model);

			Material mat = GetMatFromShader(model.Shader);

			TextureReference tr = go.AddComponent<TextureReference>();
			tr.renderFlags = model.Shader;

			Material[] mats = SetFaceData(mesh, model, di, tr);

			if (model.WeightBone.Count > 0)
			{
				SetSkinnedMesh(go, model, mesh, mats);
			}
			else if (model.Name.StartsWith("oct_"))
			{
				SetOctMesh(go, model, mesh);
			}
			else
			{

				MeshFilter mf = go.AddComponent<MeshFilter>();
				MeshRenderer mr = go.AddComponent<MeshRenderer>();
				mr.materials = mats;
				mf.mesh = mesh;
			}

			go.AddComponent<S4Animations>().FromModelAnimation(model.Animation);

			return go;
		}

		static GameObject CreateBox(BoxChunk box, GameObject parent)
		{
			GameObject go = CreateGameObject(box);
			go.transform.SetParent(parent.transform);

			go.AddComponent<BoxCollider>().size = box.Size;
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

			var s4a = go.AddComponent<S4Animations>();
			s4a.FromBoneAnimation(bone.Animation);
			go.AddComponent<Bone>().s4Animations = s4a;
			return go;
		}

		static GameObject CreateBoneSystem(BoneSystemChunk boneSys, GameObject parent)
		{
			GameObject go = CreateGameObject(boneSys);
			go.transform.SetParent(parent.transform);

			return go;
		}

		static GameObject CreateShape(ShapeChunk shape, GameObject parent)
		{
			GameObject go = CreateGameObject(shape);
			go.transform.SetParent(parent.transform);

			LineRenderer lr = go.AddComponent<LineRenderer>();

			Vector3[] points = new Vector3[shape.Unk.Count * 2];
			for (int i = 0; i < shape.Unk.Count; i++)
			{
				points[i * 2] = shape.Unk[i].Item1;
				points[i * 2 + 1] = shape.Unk[i].Item2;
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
			Undo.RegisterCreatedObjectUndo(go, "Created go");
			go.transform.position = chunk.Matrix.GetPosition();
			go.transform.rotation = chunk.Matrix.rotation;
			go.transform.localScale = chunk.Matrix.lossyScale;

			return go;
		}


		static Mesh CreateMesh(ModelChunk model)
		{
			Mesh mesh = new Mesh();

			mesh.vertices = model.Mesh.Vertices.ToArray().Clone() as Vector3[];
			mesh.normals = model.Mesh.Normals.ToArray().Clone() as Vector3[];
			mesh.tangents = model.Mesh.TangentsArray().Clone() as Vector4[];
			mesh.uv = model.Mesh.UV.ToArray().Clone() as Vector2[];
			mesh.uv2 = model.Mesh.UV2.ToArray().Clone() as Vector2[];
			mesh.uv3 = model.Mesh.UV2.ToArray().Clone() as Vector2[];
			mesh.uv4 = model.Mesh.UV2.ToArray().Clone() as Vector2[];

			return mesh;
		}

		static void SetSkinnedMesh(GameObject go, ModelChunk model, Mesh mesh, Material[] mats)
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


			foreach (SceneChunk chunk in createdObjects.Keys)
			{
				if (createdObjects[chunk].name == model.SubName)
				{
					rootBone = createdObjects[chunk];
					break;
				}
			}

			if (rootBone != null)
			{
				smr.rootBone = rootBone.transform;
			}

			smr.ResetBounds();
		}

		static void ExtractBoneWeightData(ModelChunk model, List<Transform> bones, List<Matrix4x4> bindPoses, byte[] bonesPerVertex, List<BoneWeight1>[] weights)
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

		static Material[] SetFaceData(Mesh mesh, ModelChunk model, DirectoryInfo di, TextureReference tr)
		{
			Material mat = GetMatFromShader(model.Shader);

			Material[] mats = new Material[model.TextureData.Textures.Count];

			if (model.TextureData.Textures.Count > 0)
			{
				mesh.subMeshCount = model.TextureData.Textures.Count;
				for (int i = 0; i < model.TextureData.Textures.Count; i++)
				{
					TextureEntry texEntry = model.TextureData.Textures[i];

					mesh.SetTriangles(model.Mesh.Triangles(), texEntry.FaceOffset * 3, texEntry.FaceCount * 3, i);

					Material mat_x = new Material(mat);

					string mainTex = di.FullName + "\\" + texEntry.FileName.Replace("tga", "dds");
					if (File.Exists(mainTex))
					{
						mat_x.mainTexture = ParseTextureDXT(File.ReadAllBytes(mainTex));
						mat_x.mainTexture.name = texEntry.FileName.Replace("tga", "dds");
					}
					else
					{
						mat_x.mainTexture = Texture2D.whiteTexture;
					}

					string sideTex = di.FullName + "\\" + texEntry.FileName2.Replace("tga", "dds");
					bool normal = false;
					if (sideTex != string.Empty)
					{
						if (File.Exists(sideTex))
						{
							Texture2D st = ParseTextureDXT(File.ReadAllBytes(sideTex));
							st.name = texEntry.FileName2.Replace("tga", "dds");
							if (model.TextureData.ExtraUV == 1)
							{
								mat_x.SetTexture("_DetailAlbedoMap", st);
								mat_x.EnableKeyword("_DETAIL_MULX2");
							}
							else
							{
								mat_x.SetTexture("_BumpMap", st);
								mat_x.EnableKeyword("_NORMALMAP");
								normal = true;
							}
						}
					}
					tr.textures.Add(new TextureItem(texEntry.FileName, mainTex, sideTex, normal));
					mats[i] = mat_x;
				}


			}
			else
			{
				mesh.triangles = model.Mesh.Triangles();
			}

			return mats;
		}

		static int GetBoneIndexFromBoneName(string name, List<Transform> bones)
		{
			GameObject go = null;

			foreach (SceneChunk chunk in createdObjects.Keys)
			{
				if (createdObjects[chunk].name == name)
				{
					go = createdObjects[chunk];
					break;
				}
			}

			if (go == null)
			{
				return -1;
			}
			bones.Add(go.transform);
			return bones.Count - 1;
		}

		static Material GetMatFromShader(RenderFlag shader)
		{
			if (shader.HasFlag(RenderFlag.Transparent))
			{
				return AssetDatabase.LoadAssetAtPath<Material>("Assets/ScnToolByAeven/Editor/Materials/S4_Base_Mat_Transparent.mat");
			}
			if (shader.HasFlag(RenderFlag.Cutout))
			{
				return AssetDatabase.LoadAssetAtPath<Material>("Assets/ScnToolByAeven/Editor/Materials/S4_Base_Mat_Cutout.mat");
			}
			if (shader.HasFlag(RenderFlag.NoLight))
			{
				return AssetDatabase.LoadAssetAtPath<Material>("Assets/ScnToolByAeven/Editor/Materials/S4_Base_Mat_NoLight.mat");
			}
			else
			{
				return AssetDatabase.LoadAssetAtPath<Material>("Assets/ScnToolByAeven/Editor/Materials/S4_Base_Mat_Opaque.mat");
			}
		}

		static void SetOctMesh(GameObject go, ModelChunk model, Mesh mesh)
		{
			MeshCollider mc = go.AddComponent<MeshCollider>();
			CollisionData cd = go.AddComponent<CollisionData>();

			if (Enum.TryParse(model.Name[4..], out GroundFlag result1))
			{
				cd.ground = result1;
			}
			else if (Enum.TryParse(model.Name[4..], out WeaponFlag result2))
			{
				cd.weapon = result2;
			}
			else
			{
				cd.ground = GroundFlag.blast;
			}

			mc.cookingOptions = MeshColliderCookingOptions.EnableMeshCleaning;
			mc.sharedMesh = mesh;
		}

		static (Material, Material) LoadMaterials(string mainTexturePath, string sideTexturePath, RenderFlag shader, bool isNormal = false)
		{
			string mainMatName = new FileInfo(mainTexturePath).Name;
			string sideMatName = new FileInfo(sideTexturePath).Name;

			Material mainMat;
			if (!MainMaterials.TryGetValue(mainMatName, out mainMat))
			{
				Material mat = GetMatFromShader(shader);
				mainMat = new Material(mat);
				if (File.Exists(mainTexturePath))
				{
					mainMat.mainTexture = ParseTextureDXT(File.ReadAllBytes(mainTexturePath));
					mainMat.mainTexture.name = new FileInfo(mainTexturePath).Name.Replace("tga", "dds");

					MainMaterials.Add(mainMatName, mainMat);
				}
			}

			Material sideMat;
			if (!SideMaterials.TryGetValue(sideMatName, out sideMat))
			{
				Material mat = GetMatFromShader(shader);
				sideMat = new Material(mat);
				if (File.Exists(sideTexturePath))
				{
					Texture2D st = ParseTextureDXT(File.ReadAllBytes(sideTexturePath));
					st.name = new FileInfo(sideTexturePath).Name.Replace(".tga", ".dds");
					if (isNormal)
					{
						sideMat.SetTexture("_DetailAlbedoMap", st);
						sideMat.EnableKeyword("_DETAIL_MULX2");
					}
					else
					{
						sideMat.SetTexture("_BumpMap", st);
						sideMat.EnableKeyword("_NORMALMAP");
					}
					SideMaterials.Add(sideMatName, sideMat);
				}
			}
			return (mainMat, sideMat);
		}

		public static Texture2D ParseTextureDXT(byte[] ddsBytes)
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
				throw new Exception("Invalid DDS DXTn texture. Unable to read. Oh no, looks like the file wasnt a dds, or maybe it's corrupted, check it out!");  //this header byte should be 124 for DDS image files

			int height = ddsBytes[13] * 256 + ddsBytes[12];
			int width = ddsBytes[17] * 256 + ddsBytes[16];

			int DDS_HEADER_SIZE = 128;
			byte[] dxtBytes = new byte[ddsBytes.Length - DDS_HEADER_SIZE];
			Buffer.BlockCopy(ddsBytes, DDS_HEADER_SIZE, dxtBytes, 0, ddsBytes.Length - DDS_HEADER_SIZE);

			Texture2D texture = new Texture2D(width, height, textureFormat, false);
			texture.LoadRawTextureData(dxtBytes);
			texture.Apply();

			return (texture);
		}
	}

	class TreeItem<T>
	{
		public T item;
		public List<TreeItem<T>> childs;
		public TreeItem<T> parent;
	}

	public static class ScnFileExporter
	{
		static List<string> usedNames = new List<string>();

		public static SceneContainer CreateContainerFromScenes(FileInfo fileInfo, ScnData[] scenes)
		{
			usedNames.Clear();
			SceneContainer container = new SceneContainer();
			container.Header.Name = fileInfo.Name;
			foreach (var scene in scenes)
			{
				CreateChunksFromChildren(scene.transform,scene.transform, container, null);
			}

			return container;
		}

		public static void CreateChunksFromChildren(Transform parent, Transform relativeParent, SceneContainer container, SceneChunk parentChunk)
		{
			for (int i = 0; i < parent.childCount; i++)
			{
				Transform child = parent.GetChild(i);

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
			}
		}

		static SceneChunk CreateChunk(Transform child, SceneContainer container, SceneChunk parentChunk, Transform relativeParent)
		{
			Bone bone = child.GetComponent<Bone>();
			if (bone) return CreateBoneChunk(bone, container, parentChunk);
			
			Bonesystem bonesystem = child.GetComponent<Bonesystem>();
			if (bone) return CreateBoneSystemChunk(bonesystem, container, parentChunk);

			SkinnedMeshRenderer smr = child.GetComponent<SkinnedMeshRenderer>();
			if (smr) return CreateModelChunkSkinned(smr, container, parentChunk, relativeParent);

			MeshRenderer mr = child.GetComponent<MeshRenderer>();
			if (mr)
			{
				var mesh = CreateModelChunk(mr, container, parentChunk, relativeParent);
				CollisionData cd_mr = child.GetComponent<CollisionData>();
				if (cd_mr)
				{
					CreateCollisionChunk(cd_mr, container, mesh);
				}
				return mesh;
			}

			CollisionData cd = child.GetComponent<CollisionData>();
			if (cd) return CreateCollisionChunk(cd, container, parentChunk);

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

			Vector3 position = bone.transform.localPosition;
			Quaternion rotation = bone.transform.localRotation;
			Vector3 scale = bone.transform.localScale;
			if (parent == null)
			{
				position = bone.transform.position * ScnToolData.Instance.scale;
				rotation = bone.transform.rotation;
				scale = bone.transform.lossyScale * ScnToolData.Instance.scale;
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
				rootBone.transform.lossyScale * ScnToolData.Instance.scale);

			return boneSys;
		}

		static ModelChunk CreateCollisionChunk(CollisionData cd, SceneContainer container, SceneChunk parentChunk)
		{
			if (cd.ground == GroundFlag.blast)//Dealing with the special case of breakables
			{
				return CreateModelChunkFromMeshBlast(
					cd.transform,
					cd.GetComponent<MeshCollider>().sharedMesh,
					"oct_" + cd.name, parentChunk);
			}
			else if (cd.ground != GroundFlag.NONE)
			{
				Mesh mesh = cd.GetComponent<MeshCollider>().sharedMesh;
				if (mesh)
				{
					return CreateModelChunkFromMesh(
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
					return CreateModelChunkFromMesh(cd.transform, mesh, "oct_" + cd.weapon.ToString());
				}
				else
				{
					Debug.LogWarning("Oh No! this collider has no mesh!", cd);
				}

			}

			return null;

			ModelChunk CreateModelChunkFromMesh(Transform transform, Mesh mesh, string name)
			{
				ModelChunk model = MakeChunk<ModelChunk>(container, name);
				model.SubName = container.Header.Name;
				model.Matrix = Matrix4x4.TRS(
					transform.position * ScnToolData.Instance.scale,
					transform.rotation,
					transform.lossyScale * ScnToolData.Instance.scale);
				model.Shader = RenderFlag.None;
				model.Animation = new List<ModelAnimation>();
				model.Animation.Add(new ModelAnimation());
				model.Animation[0].Name = ScnToolData.Instance.main_animation_name;
				model.Animation[0].TransformKeyData2 = new TransformKeyData2();
				model.Animation[0].TransformKeyData2.TransformKey = new TransformKey();

				model.Animation[0].TransformKeyData2.TransformKey.Translation = transform.position * ScnToolData.Instance.scale;
				model.Animation[0].TransformKeyData2.TransformKey.Rotation = transform.rotation;
				model.Animation[0].TransformKeyData2.TransformKey.Scale = transform.lossyScale * ScnToolData.Instance.scale;

				SetMesh(model.Mesh, mesh);

				return model;
			}

			ModelChunk CreateModelChunkFromMeshBlast(Transform transform, Mesh mesh, string name, SceneChunk parentChunk)
			{
				ModelChunk model = MakeChunk<ModelChunk>(container, name);
				while (ValidateName(model.Name) == false)
				{
					model.Name = model.Name + ScnToolData.GetRandomName();
				}
				model.SubName = parentChunk != null? parentChunk.Name : container.Header.Name;
				model.Matrix = Matrix4x4.TRS(
					transform.localPosition,
					transform.localRotation,
					transform.localScale);

				model.Shader = RenderFlag.None;
				SetMesh(model.Mesh, mesh);

				model.TextureData.Version = 0.2000000029802322f;
				model.TextureData.ExtraUV = (mesh.uv2.Length != 0) ? (uint)1 : (uint)0;

				model.Animation = new List<ModelAnimation>();
				ModelAnimation ma = new ModelAnimation();
				ma.TransformKeyData2 = new TransformKeyData2();
				ma.Name = ScnToolData.Instance.main_animation_name;
				ma.TransformKeyData2.TransformKey = new TransformKey();
				ma.TransformKeyData2.TransformKey.Translation = transform.localPosition;
				ma.TransformKeyData2.TransformKey.Rotation = transform.localRotation;
				ma.TransformKeyData2.TransformKey.Scale = transform.localScale;
				model.Animation.Add(ma);

				return model;
			}
		}

		static ModelChunk CreateModelChunk(MeshRenderer mr, SceneContainer container, SceneChunk parentChunk, Transform relativeParent)
		{
			ModelChunk model = MakeChunk<ModelChunk>(container, mr.name);
			while (ValidateName(model.Name) == false)
			{
				model.Name = model.Name + ScnToolData.GetRandomName();
			}
			

			MeshFilter mf = mr.GetComponent<MeshFilter>();
			ProBuilderMesh pbm = mr.GetComponent<ProBuilderMesh>();
			Mesh mesh;

			if (pbm)
			{
				mesh = GetMeshFromPBM(pbm, false);
			}
			else if (mf)
			{
				mesh = mr.GetComponent<MeshFilter>().sharedMesh;
			}
			else
			{
				mesh = new Mesh();
				Debug.LogWarning("Boy Oh boy! Girl Oh Girl! My Sweet summerchild, you're lacking a MeshFilter or a ProBuilderMesh! That's so crazy! How did this happen? anyway, make sure this object is fine!", mr);
			}

			TextureReference tr = mr.GetComponent<TextureReference>();

			model.Shader = tr.renderFlags;
			SetMesh(model.Mesh, mesh, tr.flipUvVertical, tr.flipUvHorizontal);

			SetTextureData(model.TextureData, mesh, tr);

			(Vector3 position, Quaternion rotation, Vector3 scale) = DealWithModelParenting(mr.transform, relativeParent, model, parentChunk, container);

			S4Animations s4a = mr.GetComponent<S4Animations>();
			if (s4a)
			{
				model.Animation = s4a.ToModelAnimation();
			}
			else
			{
				model.Animation = new List<ModelAnimation>();
				ModelAnimation ma = new ModelAnimation();
				ma.TransformKeyData2 = new TransformKeyData2();
				ma.Name = ScnToolData.Instance.main_animation_name;
				ma.TransformKeyData2.TransformKey = new TransformKey();
				ma.TransformKeyData2.TransformKey.Translation = position;
				ma.TransformKeyData2.TransformKey.Rotation = rotation;
				ma.TransformKeyData2.TransformKey.Scale = scale;
				model.Animation.Add(ma);
			}

			return model;
		}

		static ModelChunk CreateModelChunkSkinned(SkinnedMeshRenderer smr, SceneContainer container, SceneChunk parentChunk, Transform relativeParent)
		{
			ModelChunk model = MakeChunk<ModelChunk>(container, smr.name);
			while (ValidateName(model.Name) == false)
			{
				model.Name = model.Name + ScnToolData.GetRandomName();
			}
			if (smr.transform.parent != null) model.SubName = smr.transform.parent.name;


			(Vector3 position, Quaternion rotation, Vector3 scale) = DealWithModelParenting(smr.transform,relativeParent,model,parentChunk,container);

			Mesh mesh = smr.GetComponent<MeshFilter>().sharedMesh;


			TextureReference tr = smr.GetComponent<TextureReference>();

			model.Shader = tr.renderFlags;
			SetMesh(model.Mesh, mesh, tr.flipUvVertical, tr.flipUvHorizontal);

			SetTextureData(model.TextureData, mesh, tr);

			foreach (var transform in smr.bones)
			{
				WeightBone wb = new WeightBone();
				wb.Name = transform.name;
				model.WeightBone.Add(wb);
			}
			for (int i = 0; i < smr.sharedMesh.bindposes.Length; i++)
			{
				model.WeightBone[i].Matrix = smr.sharedMesh.bindposes[i];
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
					model.Animation[0].TransformKeyData2.TransformKey.Translation *= ScnToolData.Instance.scale;
					model.Animation[0].TransformKeyData2.TransformKey.Scale *= ScnToolData.Instance.scale;
				}
			}
			else
			{
				model.Animation = new List<ModelAnimation>();
				ModelAnimation ma = new ModelAnimation();
				ma.TransformKeyData2 = new TransformKeyData2();
				ma.Name = ScnToolData.Instance.main_animation_name;
				ma.TransformKeyData2.TransformKey = new TransformKey();
				ma.TransformKeyData2.TransformKey.Translation = position;
				ma.TransformKeyData2.TransformKey.Rotation = rotation;
				ma.TransformKeyData2.TransformKey.Scale = scale;
				model.Animation.Add(ma);
			}

			return model;
		}

		static BoxChunk CreateBoxChunk(BoxCollider bc, SceneContainer container, SceneChunk parentChunk)
		{
			BoxChunk box = MakeChunk<BoxChunk>(container, bc.name);
			while (ValidateName(box.Name) == false)
			{
				box.Name = box.Name + ScnToolData.GetRandomName();
			}

			if (parentChunk != null)
			{
				if (parentChunk.ChunkType == ChunkType.Bone || parentChunk.ChunkType == ChunkType.Box)
				{
					box.SubName = parentChunk.Name;
					box.Matrix = Matrix4x4.TRS(bc.transform.localPosition, bc.transform.localRotation, bc.transform.localScale);
					box.Size = bc.size;
					return box;
				}
			}

			box.SubName = container.Header.Name;
			box.Matrix = Matrix4x4.TRS(bc.transform.position * ScnToolData.Instance.scale, bc.transform.rotation, bc.transform.lossyScale);
			box.Size = bc.size * ScnToolData.Instance.scale;
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

			shape.Matrix = Matrix4x4.TRS(lr.transform.localPosition, lr.transform.localRotation, lr.transform.localScale);

			for (int i = 0; i < lr.positionCount; i += 2)
			{
				shape.Unk.Add(Tuple.Create(lr.GetPosition(i), lr.GetPosition(i + 1)));
			}

			return shape;
		}


		static (Vector3,Quaternion,Vector3) DealWithModelParenting(Transform transform, Transform relativeParent, SceneChunk chunk, SceneChunk parentChunk, SceneContainer container)
		{
			Vector3 position = transform.position * ScnToolData.Instance.scale;
			Quaternion rotation = transform.rotation;
			Vector3 scale = transform.lossyScale * ScnToolData.Instance.scale;
			chunk.SubName = container.Header.Name;
			if (parentChunk != null)
			{
				if (parentChunk.ChunkType == ChunkType.Bone ||
					(parentChunk.ChunkType == ChunkType.ModelData/* &&
					transform.name.StartsWith("oct_")
					&& !transform.name.StartsWith("oct_land") &&
					!transform.name.StartsWith("oct_weapon")*/))
				{
					chunk.SubName = parentChunk.Name;
					Transform oldParent = transform.transform.parent;
					transform.transform.SetParent(relativeParent, true);

					position = transform.transform.localPosition;
					rotation = transform.transform.localRotation;
					scale = transform.transform.localScale;

					transform.transform.SetParent(oldParent, true);
				}
			}

			chunk.Matrix = Matrix4x4.TRS(position, rotation, scale);

			return (position,rotation,scale);
		}

		static void SetMesh(MeshData meshData, Mesh mesh, bool flipUvVertical = false, bool flipUvHorizontal = false)
		{
			meshData.Vertices = new List<Vector3>(mesh.vertices);
			meshData.Normals = new List<Vector3>(mesh.normals);
			meshData.UV = new List<Vector2>(mesh.uv);
			meshData.UV2 = new List<Vector2>(mesh.uv2);
			if (ScnToolData.Instance.uv_flipVertical ^ flipUvVertical)
			{
				for (int i = 0; i < meshData.UV.Count; i++)
				{
					meshData.UV[i] = new Vector2(meshData.UV[i].x, -meshData.UV[i].y);
				}
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
			textureData.ExtraUV = (mesh.uv2.Length != 0) ? (uint)1 : (uint)0;
			for (int i = 0; i < textures.textures.Count; i++)
			{
				TextureEntry te = new TextureEntry();
				if (textures.textures[i] != null)
				{
					if (textures.textures[i].mainTexturePath != string.Empty) te.FileName = new FileInfo(textures.textures[i].mainTexturePath).Name;
					if (textures.textures[i].sideTexturePath != string.Empty) te.FileName2 = new FileInfo(textures.textures[i].sideTexturePath).Name;
				}
				//Set normal texture too
				if (i >= mesh.subMeshCount)
				{
					te.FaceCount = 0;
					te.FaceOffset = mesh.triangles.Length / 3;
				}
				else
				{
					te.FaceCount = mesh.GetSubMesh(i).indexCount / 3;
					te.FaceOffset = mesh.GetSubMesh(i).indexStart / 3;
				}
				textureData.Textures.Add(te);
			}
		}


		static bool ValidateName(string name)
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

		static T MakeChunk<T>(SceneContainer container, string name) where T : SceneChunk
		{
			T chunk = (T)Activator.CreateInstance(typeof(T), new object[] { container });
			container.Add(chunk);
			chunk.Name = name;

			return chunk;
		}
	}
}