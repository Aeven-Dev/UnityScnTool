using UnityEngine;
using System.Collections.Generic;

public abstract class AnimationWrapper
{
    public abstract List<string> GetAnimationNames();

    public abstract void ChangeAnimationName(string oldName, string newName);

    public abstract Dictionary<S4Animations, TransformKeyData> GetParts(string name);
    public abstract Dictionary<S4Animations, TransformKeyData> GetCopies(string name);

    public abstract GameObject GetRoot();

    public abstract void AddAnimation();
}

class SingleAnimationWrapper : AnimationWrapper
{
    S4Animations data;
    public SingleAnimationWrapper(S4Animations data)
    {
        this.data = data;
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

    public override Dictionary<S4Animations, TransformKeyData> GetParts(string name)
    {
        var result = new Dictionary<S4Animations, TransformKeyData>();
        foreach (var item in data.animations)
        {
            if (item.Name == name)
            {
                result.Add(data, item.TransformKeyData);
                break;
            }
        }
        return result;
    }
    public override Dictionary<S4Animations, TransformKeyData> GetCopies(string name)
    {
        var result = new Dictionary<S4Animations, TransformKeyData>();
        foreach (var item in data.animations)
        {
            if (item.Name == name)
            {
                foreach (var copy in data.animations)
                {
                    if (item.Copy == copy.Name)
                    {
                        result.Add(data, copy.TransformKeyData);
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

    public override void AddAnimation()
    {
        throw new System.NotImplementedException();
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

    public override Dictionary<S4Animations, TransformKeyData> GetParts(string name)
    {
        Dictionary<S4Animations, TransformKeyData> parts = new Dictionary<S4Animations, TransformKeyData>();
        foreach (var bone in bones)
        {
            foreach (var anim in bone.animations)
            {
                if (anim.Name == name)
                {
                    parts.Add(bone, anim.TransformKeyData);
                    break;
                }
            }
        }
        return parts;
    }
    public override Dictionary<S4Animations, TransformKeyData> GetCopies(string name)
    {
        var result = new Dictionary<S4Animations, TransformKeyData>();
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
                            result.Add(bone, copy.TransformKeyData);
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

    public override void AddAnimation()
    {
        throw new System.NotImplementedException();
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

    public override Dictionary<S4Animations, TransformKeyData> GetParts(string name)
    {
        Dictionary<S4Animations, TransformKeyData> parts = new Dictionary<S4Animations, TransformKeyData>();
        foreach (var obj in objects)
        {
            foreach (var anim in obj.animations)
            {
                if (anim.Name == name)
                {
                    parts.Add(obj, anim.TransformKeyData);
                    break;
                }
            }
        }
        return parts;
    }
    public override Dictionary<S4Animations, TransformKeyData> GetCopies(string name)
    {
        var result = new Dictionary<S4Animations, TransformKeyData>();
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
                            result.Add(obj, copy.TransformKeyData);
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

    public override void AddAnimation()
    {
        throw new System.NotImplementedException();
    }
}