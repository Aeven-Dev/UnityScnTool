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
			clip.frameRate = 60f;
			clip.legacy = false;
			var parts = anim.GetParts(animName);
			foreach (var item in parts.Keys)
			{
				string pathToObj = AnimationUtility.CalculateTransformPath(item.transform, anim.GetRoot().transform);
				TransformKeyData tkd = parts[item].TransformKeyData;
				SetTransformCurves(clip, pathToObj, tkd,1);
			}

			//clip.EnsureQuaternionContinuity();
			AssetDatabase.CreateAsset(clip, path);
			AssetDatabase.SaveAssets();
		}

		public static void SetTransformCurves(AnimationClip clip, string pathToObj, TransformKeyData tkd, float transparency)
		{
			SetPositionCurves(clip, pathToObj, tkd.TransformKey.Translation, tkd.TransformKey.TKey);
			SetRotationCurves(clip, pathToObj, tkd.TransformKey.Rotation, tkd.TransformKey.RKey);
			SetScaleCurves(clip, pathToObj, tkd.TransformKey.Scale, tkd.TransformKey.SKey);
			SetVisibilityCurves(clip, pathToObj, tkd.AlphaKeys, transparency);
		}
		static void SetPositionCurves(AnimationClip clip, string pathToObj, Vector3 initial, List<TKey> tKeys)
		{
			AnimationCurve curveX = new AnimationCurve(new Keyframe[] { new Keyframe(0f, initial.x) });
			AnimationCurve curveY = new AnimationCurve(new Keyframe[] { new Keyframe(0f, initial.y) });
			AnimationCurve curveZ = new AnimationCurve(new Keyframe[] { new Keyframe(0f, initial.z) });

			foreach (var key in tKeys)
			{
				float time = S4FrameToUnity(key.frame);
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
				float time = S4FrameToUnity(key.frame);
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
				float time = S4FrameToUnity(key.frame);
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
				float time = S4FrameToUnity(key.frame);
				curveX.AddKey(new Keyframe(time, key.Scale.x, 0f, 0f, 0f, 0f));
				curveY.AddKey(new Keyframe(time, key.Scale.y, 0f, 0f, 0f, 0f));
				curveZ.AddKey(new Keyframe(time, key.Scale.z, 0f, 0f, 0f, 0f));
			}

			clip.SetCurve(pathToObj, typeof(Transform), "localScale.x", curveX);
			clip.SetCurve(pathToObj, typeof(Transform), "localScale.y", curveY);
			clip.SetCurve(pathToObj, typeof(Transform), "localScale.z", curveZ);
		}
		static void SetVisibilityCurves(AnimationClip clip, string pathToObj, List<FloatKey> fKeys, float transparency)
		{
			AnimationCurve curve = new AnimationCurve(new Keyframe[] { new Keyframe(0f, transparency) });

			foreach (var key in fKeys)
			{
				curve.AddKey(key.frame, key.Alpha);
			}

			clip.SetCurve(pathToObj , typeof(TextureReference), "transparency", curve);
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
			var root = anim.GetRoot();

			EditorCurveBinding[] curveBindings = AnimationUtility.GetCurveBindings(clip);
			foreach (var item in curveBindings)
			{
				var part = root.transform.Find(item.path);
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
				AddPropertyToKeyData(a.TransformKeyData, curve, item);
			}
		}

		public static void AddPropertyToKeyData(TransformKeyData tkd, AnimationCurve curve, EditorCurveBinding item){
			
			switch (item.propertyName)
			{
				case "m_LocalPosition.x":
					AddCurvePosX(tkd, curve);
					break;
				case "m_LocalPosition.y":
					AddCurvePosY(tkd, curve);
					break;
				case "m_LocalPosition.z":
					AddCurvePosZ(tkd, curve);
					break;
				case "m_LocalRotation.x":
					AddCurveRotX(tkd, curve);
					break;
				case "m_LocalRotation.y":
					AddCurveRotY(tkd, curve);
					break;
				case "m_LocalRotation.z":
					AddCurveRotZ(tkd, curve);
					break;
				case "m_LocalRotation.w":
					AddCurveRotW(tkd, curve);
					break;
				case "m_LocalScale.x":
					AddCurveScaX(tkd, curve);
					break;
				case "m_LocalScale.y":
					AddCurveScaY(tkd, curve);
					break;
				case "m_LocalScale.z":
					AddCurveScaZ(tkd, curve);
					break;
				case "transparency":
					AddCurveVis(tkd, curve);
					break;
				default:
					Debug.Log(item.propertyName);
					break;
			}
		}

		static void AddCurvePosX(TransformKeyData tkd, AnimationCurve curve)
		{
			if(curve.keys.Length == 1){
				tkd.TransformKey.Translation.x = curve.keys[0].value * ScnToolData.Instance.scale;
				return;
			}
			else if(curve.keys.Length > 1){
				tkd.TransformKey.Translation.x = curve.keys[0].value * ScnToolData.Instance.scale;
			}
			for (int i = 0; i < curve.keys.Length; i++)
			{
				if (tkd.TransformKey.TKey.Count < curve.keys.Length)
				{
					TKey key = new TKey();
					key.frame = UnityFrameToS4(curve.keys[i].time);
					key.Translation.x = curve.keys[i].value * ScnToolData.Instance.scale;
					tkd.TransformKey.TKey.Add(key);
				}
				else
				{
					var v = tkd.TransformKey.TKey[i];
					v.Translation.x = curve.keys[i].value * ScnToolData.Instance.scale;
					tkd.TransformKey.TKey[i] = v;
				}
			}
		}
		static void AddCurvePosY(TransformKeyData tkd, AnimationCurve curve)
		{
			if(curve.keys.Length == 1){
				tkd.TransformKey.Translation.y = curve.keys[0].value * ScnToolData.Instance.scale;
				return;
			}
			else if(curve.keys.Length > 1){
				tkd.TransformKey.Translation.y = curve.keys[0].value * ScnToolData.Instance.scale;
			}
			for (int i = 0; i < curve.keys.Length; i++)
			{
				if (tkd.TransformKey.TKey.Count < curve.keys.Length)
				{
					TKey key = new TKey();
					key.frame = UnityFrameToS4(curve.keys[i].time);
					key.Translation.y = curve.keys[i].value * ScnToolData.Instance.scale;
					tkd.TransformKey.TKey.Add(key);
				}
				else
				{
					var v = tkd.TransformKey.TKey[i];
					v.Translation.y = curve.keys[i].value * ScnToolData.Instance.scale;
					tkd.TransformKey.TKey[i] = v;
				}
			}
		}
		static void AddCurvePosZ(TransformKeyData tkd, AnimationCurve curve)
		{
			if(curve.keys.Length == 1){
				tkd.TransformKey.Translation.z = curve.keys[0].value * ScnToolData.Instance.scale;
				return;
			}
			else if(curve.keys.Length > 1){
				tkd.TransformKey.Translation.z = curve.keys[0].value * ScnToolData.Instance.scale;
			}
			for (int i = 0; i < curve.keys.Length; i++)
			{
				if (tkd.TransformKey.TKey.Count < curve.keys.Length)
				{
					TKey key = new TKey();
					key.frame = UnityFrameToS4(curve.keys[i].time);
					key.Translation.z = curve.keys[i].value * ScnToolData.Instance.scale;
					tkd.TransformKey.TKey.Add(key);
				}
				else
				{
					var v = tkd.TransformKey.TKey[i];
					v.Translation.z = curve.keys[i].value * ScnToolData.Instance.scale;
					tkd.TransformKey.TKey[i] = v;
				}
			}
		}
		static void AddCurveRotX(TransformKeyData tkd, AnimationCurve curve)
		{
			if(curve.keys.Length == 1){
				tkd.TransformKey.Rotation.x = curve.keys[0].value;
				return;
			}
			else if(curve.keys.Length > 1){
				tkd.TransformKey.Rotation.x = curve.keys[0].value;
			}

			for (int i = 0; i < curve.keys.Length; i++)
			{
				if (tkd.TransformKey.RKey.Count < curve.keys.Length)
				{
					RKey key = new RKey();
					key.frame = UnityFrameToS4(curve.keys[i].time);
					key.Rotation.x = curve.keys[i].value;
					tkd.TransformKey.RKey.Add(key);
				}
				else
				{
					var v = tkd.TransformKey.RKey[i];
					v.Rotation.x = curve.keys[i].value;
					tkd.TransformKey.RKey[i] = v;
				}
			}
		}
		static void AddCurveRotY(TransformKeyData tkd, AnimationCurve curve)
		{
			if(curve.keys.Length == 1){
				tkd.TransformKey.Rotation.y = curve.keys[0].value;
				return;
			}
			else if(curve.keys.Length > 1){
				tkd.TransformKey.Rotation.y = curve.keys[0].value;
			}
			for (int i = 0; i < curve.keys.Length; i++)
			{
				if (tkd.TransformKey.RKey.Count < curve.keys.Length)
				{
					RKey key = new RKey();
					key.frame = UnityFrameToS4(curve.keys[i].time);
					key.Rotation.y = curve.keys[i].value;
					tkd.TransformKey.RKey.Add(key);
				}
				else
				{
					var v = tkd.TransformKey.RKey[i];
					v.Rotation.y = curve.keys[i].value;
					tkd.TransformKey.RKey[i] = v;
				}
			}
		}
		static void AddCurveRotZ(TransformKeyData tkd, AnimationCurve curve)
		{
			if(curve.keys.Length == 1){
				tkd.TransformKey.Rotation.z = curve.keys[0].value;
				return;
			}
			else if(curve.keys.Length > 1){
				tkd.TransformKey.Rotation.z = curve.keys[0].value;
			}

			for (int i = 0; i < curve.keys.Length; i++)
			{
				if (tkd.TransformKey.RKey.Count < curve.keys.Length)
				{
					RKey key = new RKey();
					key.frame = UnityFrameToS4(curve.keys[i].time);
					key.Rotation.z = curve.keys[i].value;
					tkd.TransformKey.RKey.Add(key);
				}
				else
				{
					var v = tkd.TransformKey.RKey[i];
					v.Rotation.z = curve.keys[i].value;
					tkd.TransformKey.RKey[i] = v;
				}
			}
		}
		static void AddCurveRotW(TransformKeyData tkd, AnimationCurve curve)
		{
			if(curve.keys.Length == 1){
				tkd.TransformKey.Rotation.w = curve.keys[0].value;
				return;
			}
			else if(curve.keys.Length > 1){
				tkd.TransformKey.Rotation.w = curve.keys[0].value;
			}

			for (int i = 0; i < curve.keys.Length; i++)
			{
				if (tkd.TransformKey.RKey.Count < curve.keys.Length)
				{
					RKey key = new RKey();
					key.frame = UnityFrameToS4(curve.keys[i].time);
					key.Rotation.w = curve.keys[i].value;
					tkd.TransformKey.RKey.Add(key);
				}
				else
				{
					var v = tkd.TransformKey.RKey[i];
					v.Rotation.w = curve.keys[i].value;
					tkd.TransformKey.RKey[i] = v;
				}
			}
		}
		static void AddCurveScaX(TransformKeyData tkd, AnimationCurve curve)
		{
			if(curve.keys.Length == 1){
				tkd.TransformKey.Scale.x = curve.keys[0].value;
				return;
			}
			else if(curve.keys.Length > 1){
				tkd.TransformKey.Scale.x = curve.keys[0].value;
			}

			for (int i = 0; i < curve.keys.Length; i++)
			{
				if (tkd.TransformKey.SKey.Count < curve.keys.Length)
				{
					SKey key = new SKey();
					key.frame = UnityFrameToS4(curve.keys[i].time);
					key.Scale.x = curve.keys[i].value;
					tkd.TransformKey.SKey.Add(key);
				}
				else
				{
					var v = tkd.TransformKey.SKey[i];
					v.Scale.x = curve.keys[i].value;
					tkd.TransformKey.SKey[i] = v;
				}
			}
		}
		static void AddCurveScaY(TransformKeyData tkd, AnimationCurve curve)
		{
			if(curve.keys.Length == 1){
				tkd.TransformKey.Scale.y = curve.keys[0].value;
				return;
			}
			else if(curve.keys.Length > 1){
				tkd.TransformKey.Scale.y = curve.keys[0].value;
			}

			for (int i = 0; i < curve.keys.Length; i++)
			{
				if (tkd.TransformKey.SKey.Count < curve.keys.Length)
				{
					SKey key = new SKey();
					key.frame = UnityFrameToS4(curve.keys[i].time);
					key.Scale.y = curve.keys[i].value;
					tkd.TransformKey.SKey.Add(key);
				}
				else
				{
					var v = tkd.TransformKey.SKey[i];
					v.Scale.y = curve.keys[i].value;
					tkd.TransformKey.SKey[i] = v;
				}
			}
		}
		static void AddCurveScaZ(TransformKeyData tkd, AnimationCurve curve)
		{
			if(curve.keys.Length == 1){
				tkd.TransformKey.Scale.z = curve.keys[0].value;
				return;
			}
			else if(curve.keys.Length > 1){
				tkd.TransformKey.Scale.z = curve.keys[0].value;
			}

			for (int i = 0; i < curve.keys.Length; i++)
			{
				if (tkd.TransformKey.SKey.Count < curve.keys.Length)
				{
					SKey key = new SKey();
					key.frame = UnityFrameToS4(curve.keys[i].time);
					key.Scale.z = curve.keys[i].value;
					tkd.TransformKey.SKey.Add(key);
				}
				else
				{
					var v = tkd.TransformKey.SKey[i];
					v.Scale.z = curve.keys[i].value;
					tkd.TransformKey.SKey[i] = v;
				}
			}
		}
		static void AddCurveVis(TransformKeyData tkd, AnimationCurve curve)
		{
			for (int i = 0; i < curve.keys.Length; i++)
			{
				FloatKey key = new FloatKey();
				key.frame = UnityFrameToS4(curve.keys[i].time);
				key.Alpha = curve.keys[i].value;
				tkd.AlphaKeys.Add(key);
			}
		}

		static float S4FrameToUnity(int frame){
			return (float)frame / 1000f;
		}
		static int UnityFrameToS4(float frame){
			return (int)(frame * 1000f);
		}
	}
}