using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using NetsphereScnTool.Scene;
using NetsphereScnTool.Scene.Chunks;
using System.IO;



namespace AevenScnTool
{
    [ExecuteInEditMode]
    [AddComponentMenu("S4 scn/S4 Armature")]
    [RequireComponent(typeof(Animation))]
    public class S4Armature : MonoBehaviour
    {
        public new Animation animation;

        [Button("Compile Animations! \\o/")] public ButtonAction CompileAnimations;
        [Button("Save Animations! [º]")] public ButtonAction SaveAnimations;
        public string AnimationNameToCompile;
        [Button("Compile Named Animation! ^^")] public ButtonAction CompileAnimationFromName;

        private void OnEnable()
        {
            animation = GetComponent<Animation>();
            CompileAnimations = new ButtonAction(ReadAnimationFromHierarchy);
            SaveAnimations = new ButtonAction(SaveAnimationsToFolder);
            CompileAnimationFromName = new ButtonAction(ReadNamedAnimFromHierarchy);
        }
        public void ReadAnimationFromHierarchy(){
            try{
                //Iterate over the tree of objects
                RecursivelyReadAnimations(transform);
                EditorUtility.ClearProgressBar();
            }
            catch(Exception e){
                EditorUtility.ClearProgressBar();
                Debug.LogError(e.StackTrace);
                throw e;
            }

        }

        public void ReadNamedAnimFromHierarchy(){
            try{
                //Iterate over the tree of objects
                RecursivelyReadNamedAnimation(transform,AnimationNameToCompile);
                EditorUtility.ClearProgressBar();
            }
            catch(Exception e){
                EditorUtility.ClearProgressBar();
                Debug.LogError(e.StackTrace);
                throw e;
            }
        }

        void RecursivelyReadAnimations(Transform item){
            S4Animations s4anims = item.GetComponent<S4Animations>();
            if (s4anims != null)//Does the object have an S4 animation?
            {
                for(int i = 0; i < s4anims.animations.Count; i++)
                {  
                    S4Animation anim = s4anims.animations[i];
                    
                    EditorUtility.DisplayProgressBar($"Compiling animations! <3", $"{item.name}/{anim.Name}", 0.5f);
                    
                    //For each animation an object has key the corresponding animation clip
                    AnimationState animState = animation[anim.Name];//Get animation state
                    if(animState == null){//If animation state is null then create new animation clip
                        var clip = CreateNewClip(anim.Name);
                        animation.AddClip(clip, anim.Name);
                        animState = animation[anim.Name];
                    }
                    SetKeyFrames(item, anim, animState.clip);
                    animState.clip.EnsureQuaternionContinuity();

                    RemoveS4KeyFrames(anim);
                }
                for(int i = s4anims.animations.Count-1; i >= 0; i--)
                {
                    var anim = s4anims.animations[i];
                    //Remove the animation from the s4 object if it doesnt have morphkeys
                    RemoveAnimIfEmpty(s4anims,anim);
                }
                RemoveComponentIfEmpty(s4anims);
            }
            foreach (Transform child in item)
            {
                RecursivelyReadAnimations(child);
            }
        }

        void RecursivelyReadNamedAnimation(Transform item, string name){
            S4Animations s4anims = item.GetComponent<S4Animations>();
            if (s4anims != null)//Does the object have an S4 animation?
            {
                for(int i = 0; i < s4anims.animations.Count; i++)
                {
                    S4Animation anim = s4anims.animations[i];
                    if(anim.Name != name) continue;
                        
                    
                    EditorUtility.DisplayProgressBar($"Compiling animations! <3", $"{item.name}/{anim.Name}", 0.5f);
                    
                    //For each animation an object has key the corresponding animation clip
                    AnimationState animState = animation[anim.Name];//Get animation state
                    if(animState == null){//If animation state is null then create new animation clip
                        var clip = CreateNewClip(anim.Name);
                        animation.AddClip(clip, anim.Name);
                        animState = animation[anim.Name];
                    }
                    SetKeyFrames(item, anim, animState.clip);
                    animState.clip.EnsureQuaternionContinuity();

                    RemoveS4KeyFrames(anim);
                }
                for(int i = s4anims.animations.Count-1; i >= 0; i--)
                {
                    var anim = s4anims.animations[i];
                    if(anim.Name != name) continue;
                    //Remove the animation from the s4 object if it doesnt have morphkeys
                    RemoveAnimIfEmpty(s4anims,anim);
                }
                RemoveComponentIfEmpty(s4anims);
            }
            foreach (Transform child in item)
            {
                RecursivelyReadNamedAnimation(child,name);
            }
        }

        AnimationClip CreateNewClip(string name){
            AnimationClip clip = new AnimationClip();
	    	clip.frameRate = 60f;
	    	clip.legacy = true;
            clip.name = name;
            return clip;
        }

        void SetKeyFrames(Transform item, S4Animation anim, AnimationClip clip){
            string pathToObj = AnimationUtility.CalculateTransformPath(item, transform);
	    	TransformKeyData tkd = anim.TransformKeyData;
            if(tkd == null){
                return;
                
            }
            float transparency = 1;
            TextureReference tr = item.GetComponent<TextureReference>();
            if(tr){
                transparency = tr.transparency;
            }
	    	AnimIO.SetTransformCurves(clip, pathToObj, tkd, transparency);
        }

        void RemoveS4KeyFrames(S4Animation anim){
            //Remove Tkeys, Rkeys, Skeys and AlphaKeys
            if (anim.TransformKeyData == null)
            {
                return;
            }
            anim.TransformKeyData.TransformKey.TKey.Clear();
            anim.TransformKeyData.TransformKey.RKey.Clear();
            anim.TransformKeyData.TransformKey.SKey.Clear();
            anim.TransformKeyData.AlphaKeys.Clear();
        }

        void RemoveAnimIfEmpty(S4Animations anims,S4Animation anim){
            //If anim is empty remove anim (No morph)
            if(anim.MorphKeys == null){
                anims.animations.Remove(anim);
            }
            else if(anim.MorphKeys.Count == 0){
                anims.animations.Remove(anim);
            }
        }
        void RemoveComponentIfEmpty(S4Animations anims){
            //If anims is empty, remove component
            if(anims.animations.Count == 0){
                DestroyImmediate(anims);
            }
        }

        public void WriteAnimationsToSceneContainer(SceneContainer container){
            AnimationClip[] anims = AnimationUtility.GetAnimationClips(gameObject);//Get all the animation clips
            int i = 0;
            foreach(AnimationClip clip in anims){
                //For each clip go through each curve
                EditorCurveBinding[] curveBindings = AnimationUtility.GetCurveBindings(clip);

                int dur = AnimIO.UnityFrameToS4(clip.length);
                //Debug.Log("Clip: '" + clip.name + "' is "+ clip.length + "s and becomes " + dur + " frames");

                foreach (var item in curveBindings)
			    {
                    string[] words = item.path.Split("/");
                    string n = words[words.Length-1];//Get the name of the object that this curve is animating

			    	SceneChunk part = container.FindByName(n);//Find the object in the scene container
			    	if (part == null){
                        Debug.LogWarning($"I couldnt find the object called {n} in {this.name} as specified in the animation {clip.name} :(");
                        continue;
                    }

                    if(part is ModelChunk modelPart){
                        //Remove default animation
                        if(modelPart.Animation.Count == 1){
                            if(modelPart.Animation[0].Name == ScnToolData.Instance.main_animation_name){
                                modelPart.Animation.RemoveAt(0);
                            }
                        }
                        //Find the ModelAnimation of the same name
                        ModelAnimation ma = null;
                        foreach(var anim in modelPart.Animation){
                            if(anim.Name == clip.name){
                                ma = anim;
                            }
                        }
                        if(ma == null){//If not found, create a new one
                            ma = new ModelAnimation();
			    		    ma.Name = clip.name;
			    		    ma.transformKeyData2 = new TransformKeyData2();
			    		    ma.transformKeyData2.TransformKey = new TransformKey();
			    		    ma.transformKeyData2.duration = dur;
                            modelPart.Animation.Add(ma);
                        }
                        AnimationCurve curve = AnimationUtility.GetEditorCurve(clip, item);
                        AnimIO.AddPropertyToKeyData(ma.transformKeyData2, curve, item);
                    }
                    else if(part is BoneChunk bonePart){
                        //Remove default animation
                        if(bonePart.Animation.Count == 1){
                            if(bonePart.Animation[0].Name == ScnToolData.Instance.main_animation_name){
                                bonePart.Animation.RemoveAt(0);
                            }
                        }
                        //Find the ModelAnimation of the same name
                        BoneAnimation ba = null;
                        foreach(var anim in bonePart.Animation){
                            if(anim.Name == clip.name){
                                ba = anim;
                            }
                        }
                        if(ba == null){//If not found, create a new one
                            ba = new BoneAnimation();
			    		    ba.Name = clip.name;
			    		    ba.TransformKeyData = new TransformKeyData();
			    		    ba.TransformKeyData.TransformKey = new TransformKey();
			    		    ba.TransformKeyData.duration = dur;
                            bonePart.Animation.Add(ba);
                        }
                        AnimationCurve curve = AnimationUtility.GetEditorCurve(clip, item);
                        AnimIO.AddPropertyToKeyData(ba.TransformKeyData, curve, item);
                        //Debug.Log(ba.TransformKeyData.TransformKey.TKey.Count);
                    }
                    
                    
			    }
                EditorUtility.DisplayProgressBar($"Writing animations! <3", $"{clip.name}", i/anims.Length);
                i++;
            }
            EditorUtility.ClearProgressBar();
        }


        void SaveAnimationsToFolder(){
            string path = EditorUtility.SaveFolderPanel("Save Animations!", "","");

            if (path == string.Empty)
            {
                return;
            }
            AnimationClip[] anims = AnimationUtility.GetAnimationClips(gameObject);
            foreach (AnimationClip clip in anims)
            {
                path = path.Remove(0,path.IndexOf("Asset"));
                AssetDatabase.CreateAsset(clip, path + Path.DirectorySeparatorChar + clip.name + ".anim");
                AssetDatabase.SaveAssets();
            }
        }
    }
}

