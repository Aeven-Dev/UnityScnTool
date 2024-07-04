using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting.YamlDotNet.Core.Tokens;

public abstract class AnimationWrapper
{
    public bool isMesh;
    public abstract List<string> GetAnimationNames();

    public abstract void ChangeAnimationName(string oldName, string newName);

    public abstract Dictionary<S4Animations, S4Animation> GetParts(string name);
    public abstract Dictionary<S4Animations, S4Animation> GetCopies(string name);

    public abstract GameObject GetRoot();

    public abstract void AddAnimation( string name );
    public abstract void RemoveAnimation(string name );

    public abstract void SetTotalFrames(string name, int frames);

	public void CopyAnimation(S4Animation from, S4Animation to)
	{
		to.TransformKeyData.duration = from.TransformKeyData.duration;
		from.TransformKeyData.AlphaKeys.ForEach(key => {
			to.TransformKeyData.AlphaKeys.Add(new FloatKey() { frame = key.frame, Alpha = key.Alpha });
		});
		to.TransformKeyData.TransformKey.Translation = from.TransformKeyData.TransformKey.Translation;
		to.TransformKeyData.TransformKey.Rotation = from.TransformKeyData.TransformKey.Rotation;
		to.TransformKeyData.TransformKey.Scale = from.TransformKeyData.TransformKey.Scale;

		to.TransformKeyData.TransformKey.TKey.AddRange(from.TransformKeyData.TransformKey.TKey);
		to.TransformKeyData.TransformKey.RKey.AddRange(from.TransformKeyData.TransformKey.RKey);
		to.TransformKeyData.TransformKey.SKey.AddRange(from.TransformKeyData.TransformKey.SKey);
	}
	public void CopyMorphKeys(S4Animation from, S4Animation to)
	{
		from.MorphKeys.ForEach(key => {
			MorphKey morph = new MorphKey() { frame = key.frame };
			morph.Vertices.AddRange(key.Vertices);
			morph.UVs.AddRange(key.UVs);
			to.MorphKeys.Add(morph);
		});
	}
}

class SingleAnimationWrapper : AnimationWrapper
{
    S4Animations data;
    public SingleAnimationWrapper(S4Animations data, bool isMesh)
    {
        this.data = data;
        this.isMesh = isMesh;
    }

    public override void ChangeAnimationName(string oldName, string newName)
    {
        foreach (var item in data.animations)
        {
            if (item.Name == oldName)
            {
                item.Name = newName;
            }
        }
    }

    public override List<string> GetAnimationNames()
    {
        List<string> names = new List<string>(data.animations.Count);
        foreach (var item in data.animations)
        {
            names.Add(item.Name);
        }
        return names;
    }

    public override Dictionary<S4Animations, S4Animation> GetParts(string name)
    {
        var result = new Dictionary<S4Animations, S4Animation>();
        foreach (var item in data.animations)
        {
            if (item.Name == name)
            {
                result.Add(data, item);
                break;
            }
        }
        return result;
    }
    public override Dictionary<S4Animations, S4Animation> GetCopies(string name)
    {
        var result = new Dictionary<S4Animations, S4Animation>();
        foreach (var item in data.animations)
        {
            if (item.Name == name)
            {
                foreach (var copy in data.animations)
                {
                    if (item.Copy == copy.Name)
                    {
                        result.Add(data, copy);
                        break;
                    }
                }
                break;
            }
        }
        return result;
    }
    public override GameObject GetRoot()
    {
        return data ? data.gameObject : null;
    }

    public override void AddAnimation(string name)
    {
        S4Animation base_anim = null;
        if (data.animations.Count > 0)base_anim = data.animations[0];

        var anim = new S4Animation();
        anim.Name = name;
		if (isMesh)
        {
            anim.TransformKeyData = new TransformKeyData2();
            anim.MorphKeys = new List<MorphKey>();
            if(base_anim != null)
            {
                CopyAnimation(base_anim, anim);
                CopyMorphKeys(base_anim, anim);
			}
        }
		else
		{
            anim.TransformKeyData = new TransformKeyData();
			if (base_anim != null)
			{
				CopyAnimation(base_anim, anim);
			}
		}
        data.animations.Add(anim);
    }
    public override void RemoveAnimation(string name)
    {
        data.animations.Remove(data.animations.Find(x => x.Name == name));
    }

	public override void SetTotalFrames(string name, int frames)
	{
        data.animations.Find(x => x.Name == name).TransformKeyData.duration = frames;
	}
}

class ArmatureAnimationWrapper : AnimationWrapper
{
    S4Animations root;
    S4Animations[] bones;
    public ArmatureAnimationWrapper(S4Animations root)
    {
        this.root = root;
        bones = root.GetComponentsInChildren<S4Animations>();
    }
    public override void ChangeAnimationName(string oldName, string newName)
    {
        foreach (S4Animations bone in bones)
        {
            foreach (var item in bone.animations)
            {
                if (item.Name == oldName)
                {
                    item.Name = newName;
                }
            }
        }
    }
    public override List<string> GetAnimationNames()
    {
        List<string> names = new List<string>();
        foreach (var bone in bones)
        {
            foreach (var anim in bone.animations)
            {
                if (!names.Contains(anim.Name))
                {
                    names.Add(anim.Name);
                }
            }
        }
        return names;
    }

    public override Dictionary<S4Animations, S4Animation> GetParts(string name)
    {
        Dictionary<S4Animations, S4Animation> parts = new();
        foreach (var bone in bones)
        {
            foreach (var anim in bone.animations)
            {
                if (anim.Name == name)
                {
                    parts.Add(bone, anim);
                    break;
                }
            }
        }
        return parts;
    }
    public override Dictionary<S4Animations, S4Animation> GetCopies(string name)
    {
        var result = new Dictionary<S4Animations, S4Animation>();
        foreach (var bone in bones)
        {
            foreach (var anim in bone.animations)
            {
                if (anim.Name == name)
                {
                    foreach (var copy in bone.animations)
                    {
                        if (anim.Copy == copy.Name)
                        {
                            result.Add(bone, copy);
                            break;
                        }
                    }
                    break;
                }
            }
        }
        return result;
    }
    public override GameObject GetRoot()
    {
        return root ? root.gameObject : null;
    }


    public override void AddAnimation(string name)
    {
        foreach (var bone in bones)
		{
			S4Animation base_anim = null;
			if (bone.animations.Count > 0) base_anim = bone.animations[0];

			var anim = new S4Animation();
            anim.Name = name;
            if (isMesh)
            {
                anim.TransformKeyData = new TransformKeyData2();
                anim.MorphKeys = new List<MorphKey>();

				if (base_anim != null)
				{
					CopyAnimation(base_anim, anim);
					CopyMorphKeys(base_anim, anim);
				}
			}
            else
            {
                anim.TransformKeyData = new TransformKeyData();

				if (base_anim != null)
				{
					CopyAnimation(base_anim, anim);
				}
			}
            bone.animations.Add(anim);
        }
        
    }
    public override void RemoveAnimation(string name)
    {
        foreach (var bone in bones)
        {
            bone.animations.Remove(bone.animations.Find(x => x.Name == name));
        }
    }
    public override void SetTotalFrames(string name, int frames)
    {
		foreach (var data in bones)
		{
            data.animations.Find(x => x.Name == name).TransformKeyData.duration = frames;
        }
    }
}

class SceneAnimationWrapper : AnimationWrapper
{
    ScnData root;
    S4Animations[] objects;
    public SceneAnimationWrapper(ScnData root)
    {
        this.root = root;
        objects = root.GetComponentsInChildren<S4Animations>();
    }

    public override void ChangeAnimationName(string oldName, string newName)
    {
        foreach (S4Animations obj in objects)
        {
            foreach (var item in obj.animations)
            {
                if (item.Name == oldName)
                {
                    item.Name = newName;
                }
            }
        }
    }
    public override List<string> GetAnimationNames()
    {
        List<string> names = new List<string>();
        foreach (var obj in objects)
        {
            foreach (var anim in obj.animations)
            {
                if (!names.Contains(anim.Name))
                {
                    names.Add(anim.Name);
                }
            }
        }
        return names;
    }

    public override Dictionary<S4Animations, S4Animation> GetParts(string name)
    {
        Dictionary<S4Animations, S4Animation> parts = new();
        foreach (var obj in objects)
        {
            foreach (var anim in obj.animations)
            {
                if (anim.Name == name)
                {
                    parts.Add(obj, anim);
                    break;
                }
            }
        }
        return parts;
    }
    public override Dictionary<S4Animations, S4Animation> GetCopies(string name)
    {
        var result = new Dictionary<S4Animations, S4Animation>();
        foreach (var obj in objects)
        {
            foreach (var anim in obj.animations)
            {
                if (anim.Name == name)
                {
                    foreach (var copy in obj.animations)
                    {
                        if (anim.Copy == copy.Name)
                        {
                            result.Add(obj, copy);
                            break;
                        }
                    }
                    break;
                }
            }
        }
        return result;
    }
    public override GameObject GetRoot()
    {
        return root ? root.gameObject : null;
    }


    public override void AddAnimation(string name)
    {
        foreach (var bone in objects)
		{
			S4Animation base_anim = null;
			if (bone.animations.Count > 0) base_anim = bone.animations[0];

			var anim = new S4Animation();
            anim.Name = name;
            if (isMesh)
            {
                anim.TransformKeyData = new TransformKeyData2();
                anim.MorphKeys = new List<MorphKey>();

				if (base_anim != null)
				{
					CopyAnimation(base_anim, anim);
					CopyMorphKeys(base_anim, anim);
				}
			}
            else
            {
                anim.TransformKeyData = new TransformKeyData();

				if (base_anim != null)
				{
					CopyAnimation(base_anim, anim);
				}
			}
            bone.animations.Add(anim);
        }

    }
    public override void RemoveAnimation(string name)
    {
        foreach (var bone in objects)
        {
            bone.animations.Remove(bone.animations.Find(x => x.Name == name));
        }
    }
    public override void SetTotalFrames(string name, int frames)
    {
        foreach (var data in objects)
        {
            data.animations.Find(x => x.Name == name).TransformKeyData.duration = frames;
        }
    }
}