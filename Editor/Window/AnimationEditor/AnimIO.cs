using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AevenScnTool
{
	public static class AnimIO
	{
		public static void Export(string animName, AnimationWrapper anim)
		{
			string path = EditorUtility.SaveFilePanelInProject("Where do you save the animation? :o", ScnToolData.GetRandomName(), "anim", "HI! <3");
			if (path == string.Empty)
			{
				return;
			}

			AnimationClip clip = new AnimationClip();
			clip.frameRate = 30f;
			clip.legacy = true;
			var parts = anim.GetParts(animName);
			foreach (var item in parts.Keys)
			{
				string pathToObj = AnimationUtility.CalculateTransformPath(item.transform, anim.GetRoot().transform);
				TransformKeyData tkd = parts[item].TransformKeyData;
				SetTransformCurves(clip, pathToObj, tkd);
			}

			//clip.EnsureQuaternionContinuity();
			AssetDatabase.CreateAsset(clip, path);
			AssetDatabase.SaveAssets();
		}

		static void SetTransformCurves(AnimationClip clip, string pathToObj, TransformKeyData tkd)
		{
			SetPositionCurves(clip, pathToObj, tkd.TransformKey.Translation, tkd.TransformKey.TKey);
			SetRotationCurves(clip, pathToObj, tkd.TransformKey.Rotation, tkd.TransformKey.RKey);
			SetScaleCurves(clip, pathToObj, tkd.TransformKey.Scale, tkd.TransformKey.SKey);
			SetVisibilityCurves(clip, pathToObj, tkd.AlphaKeys);
		}
		static void SetPositionCurves(AnimationClip clip, string pathToObj, Vector3 initial, List<TKey> tKeys)
		{
			AnimationCurve curveX = new AnimationCurve(new Keyframe[] { new Keyframe(0f, initial.x) });
			AnimationCurve curveY = new AnimationCurve(new Keyframe[] { new Keyframe(0f, initial.y) });
			AnimationCurve curveZ = new AnimationCurve(new Keyframe[] { new Keyframe(0f, initial.z) });

			foreach (var key in tKeys)
			{
				float time = (float)key.frame / 1000f;
				curveX.AddKey(new Keyframe(time, key.Translation.x, 0f, 0f, 0f, 0f));
				curveY.AddKey(new Keyframe(time, key.Translation.y, 0f, 0f, 0f, 0f));
				curveZ.AddKey(new Keyframe(time, key.Translation.z, 0f, 0f, 0f, 0f));
			}

			clip.SetCurve(pathToObj, typeof(Transform), "localPosition.x", curveX);
			clip.SetCurve(pathToObj, typeof(Transform), "localPosition.y", curveY);
			clip.SetCurve(pathToObj, typeof(Transform), "localPosition.z", curveZ);
		}
		static void SetRotationCurves(AnimationClip clip, string pathToObj, Quaternion initial, List<RKey> rKeys)
		{
			AnimationCurve curveX = new AnimationCurve(new Keyframe[] { new Keyframe(0f, initial.x) });
			AnimationCurve curveY = new AnimationCurve(new Keyframe[] { new Keyframe(0f, initial.y) });
			AnimationCurve curveZ = new AnimationCurve(new Keyframe[] { new Keyframe(0f, initial.z) });
			AnimationCurve curveW = new AnimationCurve(new Keyframe[] { new Keyframe(0f, initial.w) });

			Quaternion prev = initial;
			foreach (var key in rKeys)
			{
				float time = (float)key.frame / 1000f;
				Quaternion current = key.Rotation;
				curveX.AddKey(new Keyframe(time, current.x, 0f, 0f, 0f, 0f));
				curveY.AddKey(new Keyframe(time, current.y, 0f, 0f, 0f, 0f));
				curveZ.AddKey(new Keyframe(time, current.z, 0f, 0f, 0f, 0f));
				curveW.AddKey(new Keyframe(time, current.w, 0f, 0f, 0f, 0f));
				prev = current;

			}
			clip.SetCurve(pathToObj, typeof(Transform), "localRotation.x", curveX);
			clip.SetCurve(pathToObj, typeof(Transform), "localRotation.y", curveY);
			clip.SetCurve(pathToObj, typeof(Transform), "localRotation.z", curveZ);
			clip.SetCurve(pathToObj, typeof(Transform), "localRotation.w", curveW);
		}
		static void SetEulerRotationCurves(AnimationClip clip, string pathToObj, Quaternion initial, List<RKey> rKeys)
		{
			Vector3 euler = initial.eulerAngles;
			AnimationCurve curveX = new AnimationCurve(new Keyframe[] { new Keyframe(0f, euler.x) });
			AnimationCurve curveY = new AnimationCurve(new Keyframe[] { new Keyframe(0f, euler.y) });
			AnimationCurve curveZ = new AnimationCurve(new Keyframe[] { new Keyframe(0f, euler.z) });


			foreach (var key in rKeys)
			{
				float time = (float)key.frame / 1000f;
				Vector3 eulerKey = key.Rotation.eulerAngles;
				curveX.AddKey(new Keyframe(time, eulerKey.x, 0f, 0f, 0f, 0f));
				curveY.AddKey(new Keyframe(time, eulerKey.y, 0f, 0f, 0f, 0f));
				curveZ.AddKey(new Keyframe(time, eulerKey.z, 0f, 0f, 0f, 0f));
			}
			clip.SetCurve(pathToObj, typeof(Transform), "localEulerAngles.x", curveX);
			clip.SetCurve(pathToObj, typeof(Transform), "localEulerAngles.y", curveY);
			clip.SetCurve(pathToObj, typeof(Transform), "localEulerAngles.z", curveZ);
		}
		static void SetScaleCurves(AnimationClip clip, string pathToObj, Vector3 initial, List<SKey> sKeys)
		{
			AnimationCurve curveX = new AnimationCurve(new Keyframe[] { new Keyframe(0f, initial.x) });
			AnimationCurve curveZ = new AnimationCurve(new Keyframe[] { new Keyframe(0f, initial.y) });
			AnimationCurve curveY = new AnimationCurve(new Keyframe[] { new Keyframe(0f, initial.z) });

			foreach (var key in sKeys)
			{
				float time = (float)key.frame / 1000f;
				curveX.AddKey(new Keyframe(time, key.Scale.x, 0f, 0f, 0f, 0f));
				curveY.AddKey(new Keyframe(time, key.Scale.y, 0f, 0f, 0f, 0f));
				curveZ.AddKey(new Keyframe(time, key.Scale.z, 0f, 0f, 0f, 0f));
			}

			clip.SetCurve(pathToObj, typeof(Transform), "localScale.x", curveX);
			clip.SetCurve(pathToObj, typeof(Transform), "localScale.y", curveY);
			clip.SetCurve(pathToObj, typeof(Transform), "localScale.z", curveZ);
		}
		static void SetVisibilityCurves(AnimationClip clip, string pathToObj, List<FloatKey> fKeys)
		{
			AnimationCurve curve = new AnimationCurve();

			foreach (var key in fKeys)
			{
				curve.AddKey(key.frame, key.Alpha);
			}

			//clip.SetCurve(pathToObj , typeof(Transform), "idk,hehe", curve);
		}

		public static void Import(AnimationWrapper anim)
		{
			string path = EditorUtility.OpenFilePanel("Where do you save the animation? :o", Application.dataPath, ".anim");
			if (path == string.Empty)
			{
				return;
			}
			AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);

			TransformKeyData tkd = new TransformKeyData();
			tkd.TransformKey = new TransformKey();

			string animName = "";

			EditorCurveBinding[] curveBindings = AnimationUtility.GetCurveBindings(clip);
			foreach (var item in curveBindings)
			{
				var part = anim.GetRoot().transform.Find(item.path);
				if (part == null) continue;

				var s4a = part.gameObject.GetComponent<S4Animations>();
				if (s4a == null) continue;

				var a = s4a.GetAnimation(animName);
				if (a == null)
				{
					a = new S4Animation();
					a.Name = animName;
					a.TransformKeyData = new TransformKeyData();
					a.TransformKeyData.TransformKey = new TransformKey();
					a.TransformKeyData.duration = (int)(clip.length * 1000f);
				}

				AnimationCurve curve = AnimationUtility.GetEditorCurve(clip, item);
				switch (item.propertyName)
				{
					case "localPosition.x":
						AddCurvePosX(a, curve);
						break;
					case "localPosition.y":
						AddCurvePosY(a, curve);
						break;
					case "localPosition.z":
						AddCurvePosZ(a, curve);
						break;
					case "localRotation.x":
						AddCurveRotX(a, curve);
						break;
					case "localRotation.y":
						AddCurveRotY(a, curve);
						break;
					case "localRotation.z":
						AddCurveRotZ(a, curve);
						break;
					case "localRotation.w":
						AddCurveRotW(a, curve);
						break;
					case "localScale.x":
						AddCurveScaX(a, curve);
						break;
					case "localScale.y":
						AddCurveScaY(a, curve);
						break;
					case "localScale.z":
						AddCurveScaZ(a, curve);
						break;
					case "idk,hehe":
						AddCurveVis(a, curve);
						break;
					default:
						break;
				}
			}
		}

		static void AddCurvePosX(S4Animation anim, AnimationCurve curve)
		{

			for (int i = 0; i < curve.keys.Length; i++)
			{
				if (anim.TransformKeyData.TransformKey.TKey.Count < curve.keys.Length)
				{
					TKey key = new TKey();
					key.frame = (int)(curve.keys[i].time * 1000f);
					key.Translation.x = curve.keys[i].value;
					anim.TransformKeyData.TransformKey.TKey.Add(key);
				}
				else
				{
					var v = anim.TransformKeyData.TransformKey.TKey[i];
					v.Translation.x = curve.keys[i].value;
					anim.TransformKeyData.TransformKey.TKey[i] = v;
				}
			}
		}
		static void AddCurvePosY(S4Animation anim, AnimationCurve curve)
		{

			for (int i = 0; i < curve.keys.Length; i++)
			{
				if (anim.TransformKeyData.TransformKey.TKey.Count < curve.keys.Length)
				{
					TKey key = new TKey();
					key.frame = (int)(curve.keys[i].time * 1000f);
					key.Translation.y = curve.keys[i].value;
					anim.TransformKeyData.TransformKey.TKey.Add(key);
				}
				else
				{
					var v = anim.TransformKeyData.TransformKey.TKey[i];
					v.Translation.y = curve.keys[i].value;
					anim.TransformKeyData.TransformKey.TKey[i] = v;
				}
			}
		}
		static void AddCurvePosZ(S4Animation anim, AnimationCurve curve)
		{

			for (int i = 0; i < curve.keys.Length; i++)
			{
				if (anim.TransformKeyData.TransformKey.TKey.Count < curve.keys.Length)
				{
					TKey key = new TKey();
					key.frame = (int)(curve.keys[i].time * 1000f);
					key.Translation.z = curve.keys[i].value;
					anim.TransformKeyData.TransformKey.TKey.Add(key);
				}
				else
				{
					var v = anim.TransformKeyData.TransformKey.TKey[i];
					v.Translation.z = curve.keys[i].value;
					anim.TransformKeyData.TransformKey.TKey[i] = v;
				}
			}
		}
		static void AddCurveRotX(S4Animation anim, AnimationCurve curve)
		{

			for (int i = 0; i < curve.keys.Length; i++)
			{
				if (anim.TransformKeyData.TransformKey.RKey.Count < curve.keys.Length)
				{
					RKey key = new RKey();
					key.frame = (int)(curve.keys[i].time * 1000f);
					key.Rotation.x = curve.keys[i].value;
					anim.TransformKeyData.TransformKey.RKey.Add(key);
				}
				else
				{
					var v = anim.TransformKeyData.TransformKey.RKey[i];
					v.Rotation.x = curve.keys[i].value;
					anim.TransformKeyData.TransformKey.RKey[i] = v;
				}
			}
		}
		static void AddCurveRotY(S4Animation anim, AnimationCurve curve)
		{

			for (int i = 0; i < curve.keys.Length; i++)
			{
				if (anim.TransformKeyData.TransformKey.RKey.Count < curve.keys.Length)
				{
					RKey key = new RKey();
					key.frame = (int)(curve.keys[i].time * 1000f);
					key.Rotation.y = curve.keys[i].value;
					anim.TransformKeyData.TransformKey.RKey.Add(key);
				}
				else
				{
					var v = anim.TransformKeyData.TransformKey.RKey[i];
					v.Rotation.y = curve.keys[i].value;
					anim.TransformKeyData.TransformKey.RKey[i] = v;
				}
			}
		}
		static void AddCurveRotZ(S4Animation anim, AnimationCurve curve)
		{

			for (int i = 0; i < curve.keys.Length; i++)
			{
				if (anim.TransformKeyData.TransformKey.RKey.Count < curve.keys.Length)
				{
					RKey key = new RKey();
					key.frame = (int)(curve.keys[i].time * 1000f);
					key.Rotation.z = curve.keys[i].value;
					anim.TransformKeyData.TransformKey.RKey.Add(key);
				}
				else
				{
					var v = anim.TransformKeyData.TransformKey.RKey[i];
					v.Rotation.z = curve.keys[i].value;
					anim.TransformKeyData.TransformKey.RKey[i] = v;
				}
			}
		}
		static void AddCurveRotW(S4Animation anim, AnimationCurve curve)
		{

			for (int i = 0; i < curve.keys.Length; i++)
			{
				if (anim.TransformKeyData.TransformKey.RKey.Count < curve.keys.Length)
				{
					RKey key = new RKey();
					key.frame = (int)(curve.keys[i].time * 1000f);
					key.Rotation.w = curve.keys[i].value;
					anim.TransformKeyData.TransformKey.RKey.Add(key);
				}
				else
				{
					var v = anim.TransformKeyData.TransformKey.RKey[i];
					v.Rotation.w = curve.keys[i].value;
					anim.TransformKeyData.TransformKey.RKey[i] = v;
				}
			}
		}
		static void AddCurveScaX(S4Animation anim, AnimationCurve curve)
		{

			for (int i = 0; i < curve.keys.Length; i++)
			{
				if (anim.TransformKeyData.TransformKey.SKey.Count < curve.keys.Length)
				{
					SKey key = new SKey();
					key.frame = (int)(curve.keys[i].time * 1000f);
					key.Scale.x = curve.keys[i].value;
					anim.TransformKeyData.TransformKey.SKey.Add(key);
				}
				else
				{
					var v = anim.TransformKeyData.TransformKey.SKey[i];
					v.Scale.x = curve.keys[i].value;
					anim.TransformKeyData.TransformKey.SKey[i] = v;
				}
			}
		}
		static void AddCurveScaY(S4Animation anim, AnimationCurve curve)
		{

			for (int i = 0; i < curve.keys.Length; i++)
			{
				if (anim.TransformKeyData.TransformKey.SKey.Count < curve.keys.Length)
				{
					SKey key = new SKey();
					key.frame = (int)(curve.keys[i].time * 1000f);
					key.Scale.y = curve.keys[i].value;
					anim.TransformKeyData.TransformKey.SKey.Add(key);
				}
				else
				{
					var v = anim.TransformKeyData.TransformKey.SKey[i];
					v.Scale.y = curve.keys[i].value;
					anim.TransformKeyData.TransformKey.SKey[i] = v;
				}
			}
		}
		static void AddCurveScaZ(S4Animation anim, AnimationCurve curve)
		{

			for (int i = 0; i < curve.keys.Length; i++)
			{
				if (anim.TransformKeyData.TransformKey.SKey.Count < curve.keys.Length)
				{
					SKey key = new SKey();
					key.frame = (int)(curve.keys[i].time * 1000f);
					key.Scale.z = curve.keys[i].value;
					anim.TransformKeyData.TransformKey.SKey.Add(key);
				}
				else
				{
					var v = anim.TransformKeyData.TransformKey.SKey[i];
					v.Scale.z = curve.keys[i].value;
					anim.TransformKeyData.TransformKey.SKey[i] = v;
				}
			}
		}
		static void AddCurveVis(S4Animation anim, AnimationCurve curve)
		{
			for (int i = 0; i < curve.keys.Length; i++)
			{
				FloatKey key = new FloatKey();
				key.frame = (int)(curve.keys[i].time * 1000f);
				key.Alpha = curve.keys[i].value;
				anim.TransformKeyData.AlphaKeys.Add(key);
			}
		}
	}
}